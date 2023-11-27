namespace ObjectDetectionServer.Models
{
    public class ObjectImageDTO
    {
        public byte[] ImageWithObject { get; set; }
        public string ClassName { get; set; }
        public double Confidence { get; set; }
    }
}
