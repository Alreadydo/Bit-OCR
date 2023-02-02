using System.Collections.Generic;

namespace CognitiveLibrary.Model
{
    public class FilterdResult
    {
        public int Id { get; set; }
        public string PictureName { get; set; }
        public string Text { get; set; }
        public List<ResultDetail>? ResultDetails { get; set; }
    }
}
