using Microsoft.Extensions.Options;

namespace BiatecIdentityGateway.BusinessController
{
    public class SecurityController
    {
        /// <summary>
        /// config
        /// </summary>
        private readonly IOptions<Model.Config> _options;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="options"></param>
        public SecurityController(IOptions<Model.Config> options)
        {
            _options = options;
        }
        /// <summary>
        /// Returns true if the user is able to see other users sensitive data
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsBiatecVerifier(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return false;
            return _options.Value.Admins.Contains(userId);
        }

    }
}
