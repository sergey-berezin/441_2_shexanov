using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectDetectionWPF.ViewModel.Data
{
    public class ImageDbContent
    {
        public string Name { get; set; }
        public List<string> ClassNames { get; set; }
        public byte[] LoadedImage { get; set; }
        public byte[] ImageWithObjects { get; set; }
    }
}
