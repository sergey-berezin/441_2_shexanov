using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryANN
{
    public class ProcessedImageInfo
    {
        public string fileName = string.Empty;
        public int classNumber;
        public string className = string.Empty;
        public double leftUpperCornerX;
        public double leftUpperCornerY;
        public double width;
        public double height;
        public Image<Rgb24> detectedObjectImage;

        public ProcessedImageInfo(string fileName, int classNumber, 
            string className, double leftUpperCornerX,
            double leftUpperCornerY, double width, double height, 
            Image<Rgb24> detectedObjectImage)
        {
            this.fileName = fileName;
            this.classNumber = classNumber;
            this.className = className;
            this.leftUpperCornerX = leftUpperCornerX;
            this.leftUpperCornerY = leftUpperCornerY;
            this.width = width;
            this.height = height;
            this.detectedObjectImage = detectedObjectImage;
        }

        public void SaveAsJpeg()
        {
            detectedObjectImage.SaveAsJpeg(fileName);
        }

        public override string ToString()
        {
            return fileName + ", " + classNumber.ToString() + ", " + className +
                ", " + leftUpperCornerX.ToString().Replace(',', '.') + ", " + leftUpperCornerY.ToString().Replace(',', '.') + 
                ", " + width.ToString().Replace(',', '.') + ", " + height.ToString().Replace(',', '.');
        }
    }
}
