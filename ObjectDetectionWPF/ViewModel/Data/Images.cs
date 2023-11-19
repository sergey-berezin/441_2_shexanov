using System.Collections.Generic;

namespace ObjectDetectionWPF.ViewModel.Data
{
    public class Images
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public byte[] LoadedImage { get; set; }
        public byte[] ImageWithObjects { get; set; }
        public virtual ICollection<ClassName> ClassNames { get; set; }
    }
}