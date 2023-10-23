using System.ComponentModel.DataAnnotations.Schema;

namespace ObjectDetectionWPF.ViewModel.Data
{
    public class ClassName
    {
        public long ImageId { get; set; }

        [ForeignKey("ImageId")]
        public virtual Images Images { get; set; }
        public string Name { get; set; }
        public long Count { get; set; }
    }
}