using System;

namespace CognitiveLibrary.Model
{
    public class ReadResultResponse
    {
        public string? Status { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? LastUpdatedDateTime { get; set; }
        public AnalyseResult? AnalyzeResult { get; set; }
    }
}
