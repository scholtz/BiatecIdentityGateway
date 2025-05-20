using Algorand.Algod;
using Algorand;
using Algorand.Algod.Model;
using BiatecIdentityGateway.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using AlgorandAuthentication;
using AlgorandAuthenticationV2;
using BiatecIdentityGateway.Generated;

namespace BiatecIdentityGateway.BusinessController
{
    public class DocumentVerification
    {
        /// <summary>
        /// config
        /// </summary>
        private readonly ILogger<DocumentVerification> _logger;
        private readonly IOptions<Model.Config> _options;
        private readonly IOptions<AlgorandAuthenticationOptionsV2> _chains;
        private readonly Gateway _gateway;
        private readonly Account _account;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="options"></param>
        public DocumentVerification(ILogger<DocumentVerification> logger, IOptions<Model.Config> options, IOptions<AlgorandAuthenticationOptionsV2> chains, Gateway gateway)
        {
            _logger = logger;
            _options = options;
            _chains = chains;
            _gateway = gateway;
            _account = AlgorandARC76AccountDotNet.ARC76.GetAccount(_options.Value.Account);

            logger.LogInformation($"Processing account: {_account.Address.EncodeAsString()}");
        }
        /// <summary>
        /// Validator can check the document and mark is as valid or invalid. If validationFailureReason is set, the docment is invalid.
        /// </summary>
        /// <param name="docId"></param>
        /// <param name="validationFailureReason"></param>
        /// <param name="authUserId"></param>
        /// <returns></returns>
        public async Task<bool> ConfirmValidityOfDocument(
            string userId,
            BiatecIdentityProviderProxy.Structs.IdentityInfo userData,
            string validationFailureReason,
            string verifierUserId
            )
        {
            foreach (var item in _options.Value.Apps)
            {
                foreach (var app in item.Value)
                {
                    if (!_chains.Value.AllowedNetworks.ContainsKey(item.Key))
                    {
                        _logger.LogError($"Chain is missing: {item.Key} - {app.Value}");
                        continue;
                    }
                    var chain = _chains.Value.AllowedNetworks[item.Key];

                    var httpClient = HttpClientConfigurator.ConfigureHttpClient(chain.Server, chain.Token);

                    byte[] box = new byte[] { (byte)'i' }.Concat(new Address(userId).Bytes).ToArray();

                    DefaultApi algodApiInstance = new DefaultApi(httpClient);
                    var contract = new BiatecIdentityProviderProxy(algodApiInstance, app.Value);
                    await contract.SetInfo(new Address(userId), userData, _account, 1000, _tx_boxes: new List<BoxRef>(){
                        new BoxRef()
                        {
                            App = 0,
                            Name = box,

                        }
                    });
                    _logger.LogInformation($"User info updated {userId} @ : {item.Key} - {app.Value}");
                }
            }
            var docId = "kyc-form.json";
            var verificationForm = new VerificationForm()
            {
                Id = docId,
                UserId = userId,
                ValidationFailureReason = validationFailureReason,
                VerifierUserId = verifierUserId,
                VerifiedAt = DateTimeOffset.Now,
                IdentityInfo = userData
            };
            var ret = await _gateway.StoreDocumentAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(verificationForm)), _account.Address.EncodeAsString(), $"{userId}-kyc-validation.json");

            return string.IsNullOrEmpty(ret);
        }
    }
}
