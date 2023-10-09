using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ObjectDetectionWPF.ViewModel
{
    public class ChoosenImageInfo
    {
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public string NameOfProcessedFile { get; set; }
        //public Image<Rgb> Image { get; set; }
        public ImageSharpImageSource<Rgb24> ImageSource { get; set; }

        public List<string> ClassNames { get; set; }

        public ChoosenImageInfo(string fullName, string shortname, string nameOfProcessedFile, List<string> classNames, ImageSharpImageSource<Rgb24> imageSource)
        {
            FullName = fullName;
            ShortName = shortname;
            NameOfProcessedFile = nameOfProcessedFile;
            ClassNames = classNames;
            ImageSource = imageSource;
        }
    }
}
