using CognitiveLibrary.Model;
using CognitiveLibrary.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CognitiveLibrary.Interfaces
{
    public interface IStoreInterface
    {
        public Task<FilterdResult> GenerateFilterdResult(ReadResultResponse readResultResponse, string pictureName, int pictrueId);
        public Task StoreAsJson(ReadResultResponse resultResponse, string fileName, int pictureId);
        public void StoreFailedPicture(PostResponse postResponse, int pictureId);
        public List<string> GetAnalysedFileNames();
        public List<string> GetUntreatedFileNames(List<string> analysedFileNames, List<string> apiFileNames);
        public Task<AnalysisResultEntity> StoreResultResponse(PictureInfoForm form, ReadResultResponse readResultResponse, TableService tableService, BlobService blobService);
    }
}
