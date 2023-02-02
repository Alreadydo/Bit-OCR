using CognitiveLibrary.Model;
using System.Net.Http;
using System.Threading.Tasks;

namespace CognitiveLibrary.Interfaces
{
    public interface IRequestAnalysisInterface
    {
        public PostResponse SuccessfulRequestAnalyse(HttpResponseMessage httpResponseMessage, int pictureId);
        public Task<PostResponse> BadRequestAnalyse(HttpResponseMessage httpResponseMessage, int pictureId, string fileName);
    }
}
