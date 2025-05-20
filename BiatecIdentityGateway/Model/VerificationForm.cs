using BiatecIdentityGateway.Generated;

namespace BiatecIdentityGateway.Model
{
    public class VerificationForm
    {
        /// <summary>
        /// Id of the document
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// User who filled in the KYC form
        /// </summary>
        public string UserId { get; set; } = string.Empty;
        /// <summary>
        /// Validation failure reason
        /// </summary>
        public string ValidationFailureReason { get; set; } = string.Empty;
        /// <summary>
        /// Verifier user id
        /// </summary>
        public string VerifierUserId { get; set; } = string.Empty;
        /// <summary>
        /// Verified at
        /// </summary>
        public DateTimeOffset VerifiedAt { get; set; }
        /// <summary>
        /// User info as it is meant to be stored in blockchain
        /// </summary>
        public BiatecIdentityProviderProxy.Structs.IdentityInfo IdentityInfo { get; set; } = null;
    }
}
