using BiatecIdentity;
using BiatecIdentityGateway.BusinessController;
using Google.Protobuf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BiatecIdentityGateway.Controllers
{
    [ApiController]
    [Authorize]
    public class GatewayController : ControllerBase
    {
        private readonly ILogger<GatewayController> _logger;
        private readonly Gateway _gateway;
        private readonly SecurityController _securityController;
        private readonly DocumentVerification _documentVerification;

        public GatewayController(
            ILogger<GatewayController> logger,
            Gateway gateway,
            SecurityController securityController,
            DocumentVerification documentVerification
            )
        {
            _logger = logger;
            _gateway = gateway;
            _securityController = securityController;
            _documentVerification = documentVerification;
        }

        /// <summary>
        /// Returns true if the curren authenicated user is registered as biatec verifier
        /// </summary>
        /// <param name="docId">Document id</param>
        /// <returns>byte[] of the document</returns>
        [Route("/v1/is-biatec-verifier")]
        [HttpGet]
        public bool GetIsAdmin()
        {
            _logger.LogInformation($"GetIsAdmin");
            var isAdmin = _securityController.IsBiatecVerifier(User?.Identity?.Name ?? throw new Exception("Unathorized"));
            return isAdmin;
        }
        /// <summary>
        /// Validators can check the KYC
        /// </summary>
        /// <param name="docId">Document id</param>
        /// <returns>byte[] of the document</returns>
        [Route("/v1/validate-document/{userId}")]
        [HttpGet]
        public async Task<bool> ValidateDocument([FromRoute] string userId, [FromBody] string validationFailureReason = "")
        {
            _logger.LogInformation($"GetIsAdmin");
            var isAdmin = _securityController.IsBiatecVerifier(User?.Identity?.Name ?? throw new Exception("Unathorized"));
            if (!isAdmin) throw new Exception("You are not authorized to perform this action");

            return await _documentVerification.ConfirmValidityOfDocument(userId, validationFailureReason, User.Identity.Name);
        }
        /// <summary>
        /// Lists the documents for specific user
        /// 
        /// This action can be performed only by biatec verifiers
        /// </summary>
        /// <param name="docId">Document id</param>
        /// <returns>byte[] of the document</returns>
        [Route("/v1/documents/{userId}")]
        [HttpGet]
        public Task<string[]> GetListOfDocumentsByAdmin([FromRoute] string userId)
        {
            _logger.LogInformation($"GetListOfDocumentsByAdmin {userId}");
            var isAdmin = _securityController.IsBiatecVerifier(User?.Identity?.Name ?? throw new Exception("Unathorized"));
            if (!isAdmin) throw new Exception("You are not authorized to perform this action");

            return _gateway.GetUserDocumentsAsync(userId ?? throw new Exception("User not defined"));
        }
        /// <summary>
        /// List the user documents by user him self
        /// </summary>
        /// <param name="docId">Document id</param>
        /// <returns>byte[] of the document</returns>
        [Route("/v1/documents")]
        [HttpGet]
        public Task<string[]> GetListOfDocumentsByUser()
        {
            _logger.LogInformation($"GetListOfDocumentsByUser");
            return _gateway.GetUserDocumentsAsync(User?.Identity?.Name ?? throw new Exception("Unathorized"));
        }
        /// <summary>
        /// Loads the document from helpers
        /// </summary>
        /// <param name="docId">Document id</param>
        /// <returns>byte[] of the document</returns>
        [Route("/v1/document/binary/{docId}")]
        [HttpGet]
        public Task<byte[]> GetDocumentByteArray([FromRoute] string docId)
        {
            _logger.LogInformation($"GetDocument {docId}");
            return _gateway.GetDocumentAsync(docId, User?.Identity?.Name ?? throw new Exception("Unathorized"));
        }
        /// <summary>
        /// Loads the document from helpers
        /// </summary>
        /// <param name="docId">Document id</param>
        /// <returns>byte[] of the document</returns>
        [Route("/v1/document/utf8/{docId}")]
        [HttpGet]
        public async Task<string> GetDocumentUtf([FromRoute] string docId)
        {
            _logger.LogInformation($"GetDocument {docId}");
            return Encoding.UTF8.GetString(await _gateway.GetDocumentAsync(docId, User?.Identity?.Name ?? throw new Exception("Unathorized")));
        }
        /// <summary>
        /// Loads the document from helpers
        /// </summary>
        /// <param name="docId">Document id</param>
        /// <returns>byte[] of the document</returns>
        [Route("/v1/document/utf8/{userId}/{docId}")]
        [HttpGet]
        public async Task<string> GetDocumentUtfByAdmin([FromRoute] string userId, [FromRoute] string docId)
        {
            _logger.LogInformation($"GetDocumentUtfByAdmin {userId} {docId}");
            var isAdmin = _securityController.IsBiatecVerifier(User?.Identity?.Name ?? throw new Exception("Unathorized"));
            if (!isAdmin) throw new Exception("You are not authorized to perform this action");
            return Encoding.UTF8.GetString(await _gateway.GetDocumentAsync(docId, userId));
        }
        /// <summary>
        /// Loads the document from helpers
        /// </summary>
        /// <param name="docId">Document id</param>
        /// <returns>byte[] of the document</returns>
        [Route("/v1/document/base64/{docId}")]
        [HttpGet]
        public async Task<string> GetDocumentBase64([FromRoute] string docId)
        {
            _logger.LogInformation($"GetDocument {docId}");
            return Convert.ToBase64String(await _gateway.GetDocumentAsync(docId, User?.Identity?.Name ?? throw new Exception("Unathorized")));
        }
        /// <summary>
        /// Stores the document
        /// </summary>
        /// <param name="data">Encrypted by helper public key, signed with Gateway private key</param>
        /// <returns>True if document has been stored</returns>
        [Route("/v1/document/binary")]
        [HttpPost]
        public Task<string> StoreDocumentByteArray([FromBody] byte[] data)
        {
            _logger.LogInformation($"Document {data.Length}");
            return _gateway.StoreDocumentAsync(data, User?.Identity?.Name ?? throw new Exception("Unathorized"));
        }
        /// <summary>
        /// Stores the document
        /// </summary>
        /// <param name="data">Encrypted by helper public key, signed with Gateway private key</param>
        /// <returns>True if document has been stored</returns>
        [Route("/v1/document/download/{docId}")]
        [HttpGet]
        public async Task<IActionResult> GetDocumentDownloadWithContentType([FromRoute] string docId)
        {
            _logger.LogInformation($"Document download {docId}");
            var doc = await _gateway.GetDocumentAsync(docId, User?.Identity?.Name ?? throw new Exception("Unathorized"));
            var fileWithContentType = FileWithContentTypeAndName.Parser.ParseFrom(doc);

            return File(fileWithContentType.Data.ToByteArray(), Encoding.UTF8.GetString(fileWithContentType.ContentType.Span), Encoding.UTF8.GetString(fileWithContentType.FileName.Span));
        }
        /// <summary>
        /// Stores the document
        /// </summary>
        /// <param name="data">Encrypted by helper public key, signed with Gateway private key</param>
        /// <returns>True if document has been stored</returns>
        [Route("/v1/document/utf8")]
        [HttpPost]
        public Task<string> StoreDocumentUtf([FromBody] string data)
        {
            _logger.LogInformation($"Document {data.Length}");
            return _gateway.StoreDocumentAsync(Encoding.UTF8.GetBytes(data), User?.Identity?.Name ?? throw new Exception("Unathorized"));
        }
        /// <summary>
        /// Stores the document
        /// </summary>
        /// <param name="data">Encrypted by helper public key, signed with Gateway private key</param>
        /// <returns>True if document has been stored</returns>
        [Route("/v1/document/base64")]
        [HttpPost]
        public Task<string> StoreDocumentBase64([FromBody] string data)
        {
            _logger.LogInformation($"Document {data.Length}");
            return _gateway.StoreDocumentAsync(Convert.FromBase64String(data), User?.Identity?.Name ?? throw new Exception("Unathorized"));
        }

        /// <summary>
        /// Upload file with multiform post data
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        [Route("/v1/document/upload")]
        public async Task<string> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new Exception("No file provided");
            }
            // Save the file to the server
            using MemoryStream stream = new();
            await file.CopyToAsync(stream);

            var fileName = Path.GetFileName(file.FileName);
            var fileEncoded = new FileWithContentTypeAndName()
            {
                ContentType = ByteString.CopyFrom(Encoding.UTF8.GetBytes(file.ContentType)),
                FileName = ByteString.CopyFrom(Encoding.UTF8.GetBytes(file.FileName)),
                Data = ByteString.CopyFrom(stream.ToArray())
            };

            return await _gateway.StoreDocumentAsync(fileEncoded.ToByteArray(), User?.Identity?.Name ?? throw new Exception("Unathorized"));
        }

        /// <summary>
        /// Stores the document
        /// </summary>
        /// <param name="data">Encrypted by helper public key, signed with Gateway private key</param>
        /// <returns>True if document has been stored</returns>
        [Route("/v1/document/binary/{docId}")]
        [HttpPut]
        public Task<string> StoreDocumentByteArray([FromRoute] string docId, [FromBody] byte[] data)
        {
            _logger.LogInformation($"Document {data.Length}");
            return _gateway.StoreDocumentAsync(data, User?.Identity?.Name ?? throw new Exception("Unathorized"), docId);
        }
        /// <summary>
        /// Stores the document
        /// </summary>
        /// <param name="data">Encrypted by helper public key, signed with Gateway private key</param>
        /// <returns>True if document has been stored</returns>
        [Route("/v1/document/utf8/{docId}")]
        [HttpPut]
        public Task<string> StoreDocumentUtf([FromRoute] string docId, [FromBody] string data)
        {
            _logger.LogInformation($"Document {data.Length}");
            return _gateway.StoreDocumentAsync(Encoding.UTF8.GetBytes(data), User?.Identity?.Name ?? throw new Exception("Unathorized"), docId);
        }
        /// <summary>
        /// Stores the document
        /// </summary>
        /// <param name="data">Encrypted by helper public key, signed with Gateway private key</param>
        /// <returns>True if document has been stored</returns>
        [Route("/v1/document/base64/{docId}")]
        [HttpPut]
        public Task<string> StoreDocumentBase64([FromRoute] string docId, [FromBody] string data)
        {
            _logger.LogInformation($"Document {data.Length}");
            return _gateway.StoreDocumentAsync(Convert.FromBase64String(data), User?.Identity?.Name ?? throw new Exception("Unathorized"), docId);
        }
    }
}
