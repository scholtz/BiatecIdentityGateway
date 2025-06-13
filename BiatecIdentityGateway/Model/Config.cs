using System.Collections.Generic;

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
        /// <summary>
        /// List of biatec verifiers accounts
        /// </summary>
        public List<string> Admins { get; set; } = [];
        /// <summary>
        /// Biatec Identity App Id at specific blockchain
        /// 
        /// Example:
        /// "Apps": {
        ///  "wGHE2Pwdvd7S12BL5FaOP20EGYesN73ktiC1qzkkit8=": {
        ///    "BiatecIdentity": 1,
        ///    "BiatecConfig": 1
        ///  },
        ///  "SGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiI=": {
        ///    "BiatecIdentity": 1,
        ///    "BiatecConfig": 1
        ///  },
        ///  "NbFPTiXlg5yw4FcZLqpoxnEPZjrfxb471aNSHp/e1Yw=": {
        ///    "BiatecIdentity": 1,
        ///    "BiatecConfig": 1
        ///  }
        ///}
        /// </summary>
        public Dictionary<string, Dictionary<string, ulong>> Apps { get; set; } = new();
        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; } = String.Empty;
        /// <summary>
        /// Account
        /// </summary>
        public string Account { get; set; } = String.Empty;
    }
}
