using Microsoft.Win32;
using ObjectDetectionWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectDetectionWPF
{
    public class AppUIFunctions : IUIFunctions
    {
        public List<string> GetFileNames()
        {
            var listOfFileNames = new List<string>();   
            var ofd = new OpenFileDialog()
            {
                Multiselect = true,
                Filter = "Image (*.jpg) | *.jpg",
            };
            if (ofd.ShowDialog() == true)
            {
                foreach (var fileName in ofd.FileNames)
                {
                    listOfFileNames.Add(fileName);
                }
            }
            return listOfFileNames;
        }
    }
}
