using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectDetectionWPF.ViewModel
{
    public class ChoosenImageInfo
    {
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public string NameOfProcessedFile { get; set; }

        public List<string> ClassNames { get; set; }

        public ChoosenImageInfo(string fullName, string shortname, string nameOfProcessedFile, List<string> classNames)
        {
            FullName = fullName;
            ShortName = shortname;
            NameOfProcessedFile = nameOfProcessedFile;
            ClassNames = classNames;
        }
    }
}
