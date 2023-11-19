using Microsoft.AspNetCore.Mvc;
using ObjectDetectionServer.Models;

namespace ObjectDetectionServer.Services
{
    public class ObjectDetectionService : IObjectDetectionService
    {
        public List<ObjectImageDTO> GetAllObjects(byte[] image) 
        {
            //return new List<ObjectImageDto>()
            //{
            //    new ObjectImageDto
            //    {
            //        ImageWithObject = new byte[] {1, 2 ,3, 4},
            //        ClassName = "2",
            //        Metrics = 123,
            //    }
            //};



        }
    }
}
