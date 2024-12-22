namespace BiatecIdentityGateway.Model
{
    public class Helper
    {
        /// <summary>
        /// Helper host
        /// </summary>
        public string Host { get; set; } = string.Empty;
        /// <summary>
        /// Helper e public key in base64
        /// </summary>
        public string HelperEncryptionPublicKeyB64 { get; set; } = string.Empty;

        /// <summary>
        /// Helper s public key in base64
        /// </summary>
        public string HelperSignaturePublicKeyB64 { get; set; } = string.Empty;
    }
}
