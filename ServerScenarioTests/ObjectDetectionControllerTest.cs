using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Newtonsoft.Json;
using ObjectDetectionServer.Models;
using System.Net.Http.Json;

namespace ServerScenarioTests
{
    public class ObjectDetectionControllerTest: IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;

        public ObjectDetectionControllerTest(WebApplicationFactory<Program> _factory)
        {
            factory = _factory;
        }

        [Fact]
        public async Task CasualPostRequestTest()
        {
            var client = factory.CreateClient();
            string base64Image = Convert.ToBase64String(File.ReadAllBytes("..\\..\\..\\..\\ImageAI\\catAndDog.jpg"));
            var response = await client.PostAsJsonAsync("api/ObjectDetection/GetAllObjects", base64Image); 
            var stringResponse = await response.Content.ReadAsStringAsync();
            var objectImages = JsonConvert.DeserializeObject<List<ObjectImageDTO>>(stringResponse);
            Assert.Equal(2, objectImages.Count);
            Assert.Equal("dog", objectImages[0].ClassName);
            Assert.Equal("cat", objectImages[1].ClassName);
        }

        [Fact]
        public async Task InvalidBase64PostRequestTest()
        {
            var client = factory.CreateClient();
            var s = await client.PostAsJsonAsync("api/ObjectDetection/GetAllObjects", "definitely not a base64 string");
            var str = await s.Content.ReadAsStringAsync();
            Assert.Equal(400, (int)s.StatusCode);
            Assert.Equal("The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.", str);
        }

        [Fact]
        public async Task EmptyBodyPostRequestTest()
        {
            var client = factory.CreateClient();
            var s = await client.PostAsJsonAsync("api/ObjectDetection/GetAllObjects", "");
            var str = await s.Content.ReadAsStringAsync();
            Assert.Equal(400, (int)s.StatusCode);
            Assert.Equal("Empty 64base string", str);
        }
    }
}