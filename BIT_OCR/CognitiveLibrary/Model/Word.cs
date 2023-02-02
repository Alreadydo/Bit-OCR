using System;

namespace CognitiveLibrary.Model
{
    [Serializable]
    public class Word
    {
        public int[] BoundingBox { get; set; }
        public string Text { get; set; }
        public float Confidence { get; set; }
    }
}
