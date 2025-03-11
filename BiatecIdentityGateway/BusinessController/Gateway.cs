using BiatecIdentity;
using Google.Protobuf;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace BiatecIdentityGateway.BusinessController
{
    /// <summary>
    /// Gateway business controller
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="options"></param>
    public class Gateway(
        ILogger<Gateway> logger,
        IOptions<Model.Config> options
            )
    {
        private readonly ILogger<Gateway> _logger = logger;
        private readonly IOptions<Model.Config> _options = options;

        /// <summary>
        /// Store document using helpers
        /// 
        /// Split the document in shamir way with merkle tree and distribute shares to helpers. All helpers must be available in order to successfuly store the document.
        /// 
        /// </summary>
        /// <param name="documentRawBytes"></param>
        /// <param name="identity"></param>
        /// <returns>docId if successful stored</returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> StoreDocumentAsync(byte[] documentRawBytes, string identity, string updateDocId = "")
        {
            var gatewaySigPublicKey = Google.Protobuf.ByteString.FromBase64(_options.Value.GatewaySignaturePublicKeyB64);
            var gatewaySigPrivateKey = Google.Protobuf.ByteString.FromBase64(_options.Value.GatewaySignaturePrivateKeyB64);
            var gatewayEncPublicKey = Google.Protobuf.ByteString.FromBase64(_options.Value.GatewayEncryptionPublicKeyB64);
            var gatewayEncPrivateKey = Google.Protobuf.ByteString.FromBase64(_options.Value.GatewayEncryptionPrivateKeyB64);

            using var channel = GrpcChannel.ForAddress("http://localhost:50051");
            var client = new DerecCrypto.DeRecCryptographyService.DeRecCryptographyServiceClient(channel);

            ulong n = Convert.ToUInt64(_options.Value.Helpers.Count);
            var t = n - 1;

            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] randomBytes = new byte[16];
            rng.GetBytes(randomBytes);

            var shares = await client.VSSShareAsync(new DerecCrypto.VSSShareRequest()
            {
                Message = ByteString.CopyFrom(documentRawBytes),
                N = n,
                T = t,
                Rand = ByteString.CopyFrom(randomBytes),
            });
            var index = -1;
            var docId = Guid.NewGuid().ToString();
            if (!string.IsNullOrEmpty(updateDocId))
            {
                docId = updateDocId;
            }
            foreach (var helper in _options.Value.Helpers)
            {
                index++;
                var doc = new BiatecIdentity.PostDocumentUnsigned()
                {
                    Docid = Google.Protobuf.ByteString.CopyFromUtf8(docId),
                    Identity = Google.Protobuf.ByteString.CopyFromUtf8(identity),
                    Share = shares.Shares[index].ToByteString()
                };
                var docBytes = doc.ToByteString();
                var signed = await client.SignSignAsync(new DerecCrypto.SignSignRequest()
                {
                    Message = docBytes,
                    SecretKey = gatewaySigPrivateKey
                });
                var docSigned = new BiatecIdentity.PostDocumentSignedRequest()
                {
                    Document = docBytes,
                    Signature = signed.Signature
                };
                var encryptedSigned = await client.EncryptEncryptAsync(new DerecCrypto.EncryptEncryptRequest()
                {
                    Message = docSigned.ToByteString(),
                    PublicKey = ByteString.FromBase64(helper.HelperEncryptionPublicKeyB64)
                });
                var encryptedSignedBytes = encryptedSigned.Ciphertext.ToByteArray();

                using var httpClient = new HttpClient();
                var helperClient = new BiatecIdentityHelper.HelperClient(helper.Host, httpClient);
                var result = await helperClient.StoreDocumentAsync(encryptedSignedBytes);

                var decryptedStoredResponse = await client.EncryptDecryptAsync(new DerecCrypto.EncryptDecryptRequest() { Ciphertext = ByteString.CopyFrom(result), SecretKey = gatewayEncPrivateKey });
                var decryptedStoredResponseMsg = BiatecIdentity.PostDocumentSignedResponse.Parser.ParseFrom(decryptedStoredResponse.Message);
                var isSuccess = decryptedStoredResponseMsg.IsSuccess;
                if (!isSuccess) throw new Exception("One of the sharer did not report successful storage of data");

                var decryptedStoredResponseCheckSign = await client.SignVerifyAsync(new DerecCrypto.SignVerifyRequest() { Message = ByteString.CopyFrom(BitConverter.GetBytes(isSuccess)), PublicKey = ByteString.FromBase64(helper.HelperSignaturePublicKeyB64), Signature = decryptedStoredResponseMsg.Signature });

                if (!decryptedStoredResponseCheckSign.Valid) throw new Exception("Invalid signature received after the storage of data from one of the helpers.");

            }
            return docId;
        }

        public async Task<byte[]> GetDocumentAsync(string docId, string identity)
        {
            var gatewaySigPublicKey = Google.Protobuf.ByteString.FromBase64(_options.Value.GatewaySignaturePublicKeyB64);
            var gatewaySigPrivateKey = Google.Protobuf.ByteString.FromBase64(_options.Value.GatewaySignaturePrivateKeyB64);
            var gatewayEncPublicKey = Google.Protobuf.ByteString.FromBase64(_options.Value.GatewayEncryptionPublicKeyB64);
            var gatewayEncPrivateKey = Google.Protobuf.ByteString.FromBase64(_options.Value.GatewayEncryptionPrivateKeyB64);

            using var channel = GrpcChannel.ForAddress("http://localhost:50051");
            var client = new DerecCrypto.DeRecCryptographyService.DeRecCryptographyServiceClient(channel);

            var req = new BiatecIdentity.GetDocumentUnsigned()
            {
                Docid = Google.Protobuf.ByteString.CopyFromUtf8(docId),
                Identity = Google.Protobuf.ByteString.CopyFromUtf8(identity)
            };
            var reqBytes = req.ToByteString();
            var signedReq = await client.SignSignAsync(new DerecCrypto.SignSignRequest()
            {
                Message = reqBytes,
                SecretKey = gatewaySigPrivateKey
            });
            var reqSigned = new BiatecIdentity.PostDocumentSignedRequest()
            {
                Document = reqBytes,
                Signature = signedReq.Signature
            };
            var shares = new Google.Protobuf.Collections.RepeatedField<DerecCrypto.VSSShare>();
            foreach (var helper in _options.Value.Helpers)
            {
                try
                {
                    var encryptedSignedReq = await client.EncryptEncryptAsync(new DerecCrypto.EncryptEncryptRequest()
                    {
                        Message = reqSigned.ToByteString(),
                        PublicKey = ByteString.FromBase64(helper.HelperEncryptionPublicKeyB64)
                    });
                    var encryptedSignedReqBytes = encryptedSignedReq.Ciphertext.ToByteArray();


                    using var httpClient = new HttpClient();
                    var helperClient = new BiatecIdentityHelper.HelperClient(helper.Host, httpClient);
                    var result = await helperClient.GetDocumentAsync(encryptedSignedReqBytes);

                    var encryptedResult = await client.EncryptDecryptAsync(new DerecCrypto.EncryptDecryptRequest()
                    {
                        Ciphertext = ByteString.CopyFrom(result),
                        SecretKey = gatewayEncPrivateKey
                    });

                    var getDocumentSignedResponse = BiatecIdentity.GetDocumentSignedResponse.Parser.ParseFrom(encryptedResult.Message);
                    if (getDocumentSignedResponse.Result.Status == StatusEnum.Ok)
                    {
                        var getDocumentSignedResponseCheckSign = await client.SignVerifyAsync(new DerecCrypto.SignVerifyRequest() { Message = getDocumentSignedResponse.Document, PublicKey = ByteString.FromBase64(helper.HelperSignaturePublicKeyB64), Signature = getDocumentSignedResponse.Signature });
                        if (getDocumentSignedResponseCheckSign.Valid)
                        {
                            var share = getDocumentSignedResponse.Document.ToByteArray();
                            shares.Add(DerecCrypto.VSSShare.Parser.ParseFrom(share));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"One of the helpers ({helper.Host}) has the issue - {ex.Message}");
                }
            }

            var request = new DerecCrypto.VSSRecoverRequest();
            request.Shares.AddRange(shares);
            var recovery = await client.VSSRecoverAsync(request);

            return recovery.Message.ToByteArray();
        }


        public async Task<string[]> GetUserDocumentsAsync(string identity)
        {
            var gatewaySigPublicKey = Google.Protobuf.ByteString.FromBase64(_options.Value.GatewaySignaturePublicKeyB64);
            var gatewaySigPrivateKey = Google.Protobuf.ByteString.FromBase64(_options.Value.GatewaySignaturePrivateKeyB64);
            var gatewayEncPublicKey = Google.Protobuf.ByteString.FromBase64(_options.Value.GatewayEncryptionPublicKeyB64);
            var gatewayEncPrivateKey = Google.Protobuf.ByteString.FromBase64(_options.Value.GatewayEncryptionPrivateKeyB64);

            using var channel = GrpcChannel.ForAddress("http://localhost:50051");
            var client = new DerecCrypto.DeRecCryptographyService.DeRecCryptographyServiceClient(channel);

            var req = new BiatecIdentity.GetUserDocumentsUnsigned()
            {
                Identity = Google.Protobuf.ByteString.CopyFromUtf8(identity)
            };
            var reqBytes = req.ToByteString();
            var signedReq = await client.SignSignAsync(new DerecCrypto.SignSignRequest()
            {
                Message = reqBytes,
                SecretKey = gatewaySigPrivateKey
            });
            var reqSigned = new BiatecIdentity.GetUserDocumentsSignedRequest()
            {
                Document = reqBytes,
                Signature = signedReq.Signature
            };
            var shares = new Google.Protobuf.Collections.RepeatedField<DerecCrypto.VSSShare>();
            var documents = new ConcurrentBag<string>();
            foreach (var helper in _options.Value.Helpers)
            {
                try
                {
                    var encryptedSignedReq = await client.EncryptEncryptAsync(new DerecCrypto.EncryptEncryptRequest()
                    {
                        Message = reqSigned.ToByteString(),
                        PublicKey = ByteString.FromBase64(helper.HelperEncryptionPublicKeyB64)
                    });
                    var encryptedSignedReqBytes = encryptedSignedReq.Ciphertext.ToByteArray();


                    using var httpClient = new HttpClient();
                    var helperClient = new BiatecIdentityHelper.HelperClient(helper.Host, httpClient);
                    var result = await helperClient.GetDocumentAsync(encryptedSignedReqBytes);

                    var encryptedResult = await client.EncryptDecryptAsync(new DerecCrypto.EncryptDecryptRequest()
                    {
                        Ciphertext = ByteString.CopyFrom(result),
                        SecretKey = gatewayEncPrivateKey
                    });

                    var getUserDocumentsSignedResponse = BiatecIdentity.GetUserDocumentsSignedResponse.Parser.ParseFrom(encryptedResult.Message);
                    var responseWithDataWithoutSignature = new BiatecIdentity.GetUserDocumentsSignedResponse();
                    responseWithDataWithoutSignature.Documents.AddRange(getUserDocumentsSignedResponse.Documents);
                    if (getUserDocumentsSignedResponse.Result.Status == StatusEnum.Ok)
                    {
                        var getDocumentSignedResponseCheckSign = await client.SignVerifyAsync(new DerecCrypto.SignVerifyRequest() { Message = responseWithDataWithoutSignature.ToByteString(), PublicKey = ByteString.FromBase64(helper.HelperSignaturePublicKeyB64), Signature = getUserDocumentsSignedResponse.Signature });
                        if (getDocumentSignedResponseCheckSign.Valid)
                        {
                            foreach (var doc in getUserDocumentsSignedResponse.Documents)
                            {
                                var docName = doc.ToStringUtf8();
                                if (!documents.Contains(docName))
                                {
                                    documents.Add(docName);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"One of the helpers ({helper.Host}) has the issue - {ex.Message}");
                }
            }

            return documents.OrderBy(d => d).ToArray();
        }
    }
}
