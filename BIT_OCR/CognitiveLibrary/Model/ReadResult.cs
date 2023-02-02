using System;

namespace CognitiveLibrary.Model
{
    [Serializable]
    public class ReadResult
    {
        public int Page { get; set; }
        public float Angle { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Unit { get; set; }
        public Line[] Lines { get; set; }
    }
}
