using BiatecIdentityGateway.BusinessController;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

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
        [Route("/v1/document/{docId}/binary")]
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
        [Route("/v1/document/{docId}/utf")]
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
        [Route("/v1/document/{docId}/base64")]
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
        /// Stores the document
        /// </summary>
        /// <param name="data">Encrypted by helper public key, signed with Gateway private key</param>
        /// <returns>True if document has been stored</returns>
        [Route("/v1/document/{docId}/binary")]
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
        [Route("/v1/document/{docId}/utf8")]
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
        [Route("/v1/document/{docId}/base64")]
        [HttpPut]
        public Task<string> StoreDocumentBase64([FromRoute] string docId, [FromBody] string data)
        {
            _logger.LogInformation($"Document {data.Length}");
            return _gateway.StoreDocumentAsync(Convert.FromBase64String(data), User?.Identity?.Name ?? throw new Exception("Unathorized"), docId);
        }
    }
}
