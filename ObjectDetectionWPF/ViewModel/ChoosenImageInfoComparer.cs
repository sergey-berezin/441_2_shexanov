using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectDetectionWPF.ViewModel
{
    public class ChoosenImageInfoComparer : IComparer<ChoosenImageInfo>
    {
        public int Compare(ChoosenImageInfo? x, ChoosenImageInfo? y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                { 
                    return -1;
                }
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                else
                {
                    if (x.ClassNames.Count != y.ClassNames.Count)
                    {
                        return y.ClassNames.Count.CompareTo(x.ClassNames.Count);
                    }
                    else
                    { 
                        return x.ShortName.CompareTo(y.ShortName);
                    }
                }
            }
        }
    }
}
