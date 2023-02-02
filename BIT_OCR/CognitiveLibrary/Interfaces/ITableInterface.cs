using BiT.Central.Core.Mvc;
using BiT.Central.Core.Mvc.Query;
using CognitiveLibrary.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CognitiveLibrary.Interfaces
{
    public interface ITableInterface
    {
        public void PostTableEntity(int pictureId, string pictureName, string analyseResultPath, string status);
        public Task<AnalysisResultEntity> GetTableEntity(string id);
        public Task DeleteTableEntity(string id);
        public Task<PagedListPage<AnalysisResultEntity>> GetAllTableEntities(PaginationQueryParameters pagination, SortingQueryParameters sorting, List<Filter> filters);
        public Task UpdateTableEntity(AnalysisResultEntity analysisResultEntity);
        public Task<AnalysisResultEntity> UpsertTableEntity(AnalysisResultEntity analyseResultEntity);
    }
}
