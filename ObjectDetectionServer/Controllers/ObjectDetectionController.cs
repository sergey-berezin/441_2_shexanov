using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ObjectDetectionServer.Models;
using ObjectDetectionServer.Services;

namespace ObjectDetectionServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ObjectDetectionController : ControllerBase
    {

        private readonly IObjectDetectionService objectDetectionService;

        private readonly ILogger<ObjectDetectionService> logger;

        public ObjectDetectionController(IObjectDetectionService _objectDetectionService, ILogger<ObjectDetectionService> _logger)
        {
            objectDetectionService = _objectDetectionService;
            logger = _logger;
        }

        [HttpPost]
        [Route("GetAllObjects")]
        public async Task<IActionResult> GetAllObjects([FromBody] string image64Base)
        {
            try
            {
                var image = Convert.FromBase64String(image64Base);
                var imageObjects = await objectDetectionService.GetAllObjects(image);
                return Ok(imageObjects);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex.Message, ex);
                return StatusCode(500);
            }
        }
    }
}

