using System.Drawing;

namespace Assesment.Models.Dtos
{
    public class DetectedObject
    {
        public int ClassId { get; set; }
        public string Label { get; set; }
        public float Confidence { get; set; }
        public Rectangle BoundingBox { get; set; }

    }
}
