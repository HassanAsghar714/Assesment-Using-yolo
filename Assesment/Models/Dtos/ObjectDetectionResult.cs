namespace Assesment.Models.Dtos
{
    public class ObjectDetectionResult
    {
        public List<DetectedObject> DetectedObjects { get; set; }
        public double MatchPercentage { get; set; }
    }
}
