using ObjectDetectionServer.Models;

namespace ObjectDetectionServer.Services
{
    public interface IObjectDetectionService
    {
        public List<ObjectImageDTO> GetAllObjects(byte[] image); 
    }
}
