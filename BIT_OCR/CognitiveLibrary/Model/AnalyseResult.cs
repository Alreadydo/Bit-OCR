namespace CognitiveLibrary.Model
{
    public class AnalyseResult
    {
        public string Version { get; set; }
        public string ModelVersion { get; set; }
        public ReadResult[] ReadResults { get; set; }
    }
}
