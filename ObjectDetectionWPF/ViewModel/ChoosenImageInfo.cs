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
        //public string FullName { get; set; }
        public string ShortName { get; set; }
        //public string NameOfProcessedFile { get; set; }
        
        public ImageSharpImageSource<Rgb24> LoadedImageSource { get; set; }
        public ImageSharpImageSource<Rgb24> ImageWithObjectsSource { get; set; }

        public List<string> ClassNames { get; set; }

        public ChoosenImageInfo(string shortname, 
            List<string> classNames,
            ImageSharpImageSource<Rgb24> loadedImage,
            ImageSharpImageSource<Rgb24> imageWithObjects)
        {
            //FullName = fullName;
            ShortName = shortname;
            //NameOfProcessedFile = nameOfProcessedFile;
            ClassNames = classNames;
            LoadedImageSource = loadedImage;
            ImageWithObjectsSource = imageWithObjects;
        }
    }
}
