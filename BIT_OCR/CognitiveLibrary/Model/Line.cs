namespace CognitiveLibrary.Model
{
    public class Line
    {
        public int[] BoundingBox { get; set; }
        public string Text { get; set; }
        public Appearance Appearance { get; set; }
        public Word[] Words { get; set; }
    }
}
