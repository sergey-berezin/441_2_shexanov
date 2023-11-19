namespace LibraryANN
{
    public class ProcessedImageInfo
    {
        public string FileName { get; }
        public int ClassNumber { get; }
        public string ClassName {  get; }
        public double LeftUpperCornerX { get; }
        public double LeftUpperCornerY { get; }
        public double Width { get; }
        public double Height { get; }
        public double Confidence {  get; }
        public Image<Rgb24> DetectedObjectImage { get; }

        public ProcessedImageInfo(string fileName, int classNumber,
            string className, double leftUpperCornerX,
            double leftUpperCornerY, double width, double height, double confidence, 
            Image<Rgb24> detectedObjectImage)
        {
            FileName = fileName;
            ClassNumber = classNumber;
            ClassName = className;
            LeftUpperCornerX = leftUpperCornerX;
            LeftUpperCornerY = leftUpperCornerY;
            Width = width;
            Height = height;
            Confidence = confidence;
            DetectedObjectImage = detectedObjectImage;
        }

        public void SaveAsJpeg()
        {
            //var numberOfRetries = 10;
            //var delayOnRetry = 1;

            //var currentTry = 0;
            //while (currentTry < numberOfRetries)
            //{
            //    try
            //    {
            //        DetectedObjectImage.SaveAsJpeg(FileName);
            //        break;
            //    }
            //    catch (IOException)
            //    {
            //        Thread.Sleep(delayOnRetry);
            //        currentTry++;
            //        continue;
            //    }
            //}

            try
            {
                DetectedObjectImage.SaveAsJpeg(FileName);
            }
            catch (IOException)
            {
                
            }

        }

        public override string ToString()
        {
            return FileName + ", " + ClassNumber.ToString() + ", " + ClassName +
                ", " + LeftUpperCornerX.ToString().Replace(',', '.') + ", " + LeftUpperCornerY.ToString().Replace(',', '.') + 
                ", " + Width.ToString().Replace(',', '.') + ", " + Height.ToString().Replace(',', '.');
        }
    }
}
