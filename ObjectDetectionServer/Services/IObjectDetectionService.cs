using ObjectDetectionServer.Models;

namespace ObjectDetectionServer.Services
{
    public interface IObjectDetectionService
    {
        public Task<List<ObjectImageDTO>> GetAllObjects(byte[] image);
    }
}
