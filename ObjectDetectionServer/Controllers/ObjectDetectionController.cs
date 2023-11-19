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
        public IActionResult GetAllObjects([FromBody] byte[] image)
        {
            try
            {
                var imageObjects = objectDetectionService.GetAllObjects(image);
                return Ok(imageObjects);
            }
            //catch(BarrierPostPhaseException ex) // ????????????
            //{
            //    logger.LogError(ex.Message, ex);
            //    return BadRequest(ex.Message);
            //}
            catch (Exception ex)
            {
                logger.LogCritical(ex.Message, ex);
                BadRequest(ex.Message);
                throw new Exception(ex.Message, ex);
            }
        }
    }
}

