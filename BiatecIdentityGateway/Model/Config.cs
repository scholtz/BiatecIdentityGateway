namespace BiatecIdentityGateway.Model
{
    public class Config
    {
        /// <summary>
        /// List of helpers
        /// </summary>
        public List<Helper> Helpers { get; set; } = [];
        /// <summary>
        /// Gateway s public key in base64
        /// </summary>
        public string GatewaySignaturePublicKeyB64 { get; set; } = string.Empty;
        /// <summary>
        /// Gateway s private key in base64
        /// </summary>
        public string GatewaySignaturePrivateKeyB64 { get; set; } = string.Empty;
        /// <summary>
        /// Gateway e public key in base64
        /// </summary>
        public string GatewayEncryptionPublicKeyB64 { get; set; } = string.Empty;
        /// <summary>
        /// Gateway e private key in base64
        /// </summary>
        public string GatewayEncryptionPrivateKeyB64 { get; set; } = string.Empty;
    }
}
