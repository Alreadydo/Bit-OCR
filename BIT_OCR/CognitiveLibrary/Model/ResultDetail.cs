namespace CognitiveLibrary.Model
{
    public class ResultDetail
    {
        public string? Text { get; set; }
        public float? Confidence { get; set; }
        public int[]? BoundingBox { get; set; }
    }
}
