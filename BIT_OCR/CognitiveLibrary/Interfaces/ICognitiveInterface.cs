using CognitiveLibrary.Model;
using System.Threading.Tasks;

namespace CognitiveLibrary
{
    public interface ICognitiveInterface
    {
        public Task<PostResponse> PostReadApiRequest(PictureInfoForm pictureInfo);
        public Task<ReadResultResponse> GetResultAsJson(string url);
        public Task<ReadResultResponse> ProcessImage(PictureInfoForm pictureInfo);
    }
}
