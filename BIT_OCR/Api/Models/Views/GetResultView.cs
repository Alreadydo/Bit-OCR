using BiT.Central.OCR.Api.Models.Views;
using CognitiveLibrary.Model;

namespace BiT.Central.OCR.Api.Models
{
    public class GetResultView
    {
        public AnalyseResultView TableEntityView { get; set; }
        public FilterdResult FilterResult { get; set; }
    }
}
