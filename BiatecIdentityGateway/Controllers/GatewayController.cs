using BiatecIdentityGateway.BusinessController;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiatecIdentityGateway.Controllers
{
    [ApiController]
    [Authorize]
    public class GatewayController : ControllerBase
    {
        private readonly ILogger<GatewayController> _logger;
        private readonly Gateway _gateway;

        public GatewayController(
            ILogger<GatewayController> logger,
            Gateway gateway
            )
        {
            _logger = logger;
            _gateway = gateway;
        }

        /// <summary>
        /// Loads the document from helpers
        /// </summary>
        /// <param name="docId">Document id</param>
        /// <returns>byte[] of the document</returns>
        [Route("/v1/document/{docId}")]
        [HttpGet]
        public Task<byte[]> GetDocument([FromRoute] string docId)
        {
            _logger.LogInformation($"GetDocument {docId}");
            return _gateway.GetDocumentAsync(docId, User?.Identity?.Name ?? throw new Exception("Unathorized"));
        }
        /// <summary>
        /// Stores the document
        /// </summary>
        /// <param name="data">Encrypted by helper public key, signed with Gateway private key</param>
        /// <returns>True if document has been stored</returns>
        [Route("/v1/document")]
        [HttpPost]
        public Task<string> StoreDocument([FromBody] byte[] data)
        {
            _logger.LogInformation($"Document {data.Length}");
            return _gateway.StoreDocumentAsync(data, User?.Identity?.Name ?? throw new Exception("Unathorized"));
        }
    }
}
