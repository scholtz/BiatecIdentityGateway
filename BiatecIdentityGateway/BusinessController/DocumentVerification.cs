using Algorand.Algod.Model;
using BiatecIdentityGateway.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace BiatecIdentityGateway.BusinessController
{
    public class DocumentVerification
    {
        /// <summary>
        /// config
        /// </summary>
        private readonly IOptions<Model.Config> _options;
        private readonly Gateway _gateway;
        private readonly Account _account;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="options"></param>
        public DocumentVerification(ILogger<DocumentVerification> logger, IOptions<Model.Config> options, Gateway gateway)
        {
            _options = options;
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
        public async Task<bool> ConfirmValidityOfDocument(string userId, string validationFailureReason, string verifierUserId)
        {
            var docId = "kyc-form.json";
            var verificationForm = new VerificationForm()
            {
                Id = docId,
                UserId = userId,
                ValidationFailureReason = validationFailureReason,
                VerifierUserId = verifierUserId,
                VerifiedAt = DateTimeOffset.Now
            };
            var ret = await _gateway.StoreDocumentAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(verificationForm)), _account.Address.EncodeAsString(), $"{userId}-kyc-validation.json");

            return string.IsNullOrEmpty(ret);
        }
    }
}
