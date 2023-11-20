using LibraryANN;
using Microsoft.AspNetCore.Mvc;
using ObjectDetectionServer.Models;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace ObjectDetectionServer.Services
{
    public class ObjectDetectionService : IObjectDetectionService
    {
        private ObjectDetection objectDetection = new ObjectDetection();

        public ObjectDetectionService()
        {
            objectDetection.SessionInitialization();
        }

        public async Task<List<ObjectImageDTO>> GetAllObjects(byte[] image) 
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

            var task = await objectDetection.GetInfoAsync(image, new CancellationToken());
            
            var imagesDtos = new List<ObjectImageDTO>();

            foreach(var imageInfo in task)
            {
                var byteImage = ImageConvertor<Rgb24>.ToByteArray(ImageConvertor<Rgb24>.RemoveBlackStripes(imageInfo.DetectedObjectImage));

                imagesDtos.Add(new ObjectImageDTO()
                {
                    ImageWithObject = byteImage,
                    ClassName = imageInfo.ClassName,
                    Confidence = imageInfo.Confidence,
                });
            }
            return imagesDtos;
        }
    }
}
