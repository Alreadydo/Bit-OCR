using Azure;
using Azure.Data.Tables;
using BiT.Central.Core.Extensions;
using BiT.Central.Core.Mvc;
using BiT.Central.Core.Mvc.Query;
using CognitiveLibrary.Interfaces;
using CognitiveLibrary.Model;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CognitiveLibrary
{
    public class TableService : ITableInterface
    {
        private static ILogger _logger;
        private TableClient _tableClient;
        public TableService(IConfiguration configuration, ILogger Ilogger = null)
        {
            _logger = Ilogger;
            _tableClient = new TableClient(configuration["AzureTable:ConnectionString"], configuration["AzureTable:TableName"]);
        }


        public async Task<PagedListPage<AnalysisResultEntity>> GetAllTableEntities(PaginationQueryParameters pagination, SortingQueryParameters sorting, List<Filter> filters)
        {
            //TODO: Fix pagination/sorting/filtering NoSql
            var entities = _tableClient.Query<AnalysisResultEntity>()
                .ToList()
                .AsQueryable()
                .Query(pagination, sorting, filters);
            return entities;
        }

        public async Task<AnalysisResultEntity> GetTableEntity(string id)
        {
            try
            {
                return await _tableClient.GetEntityAsync<AnalysisResultEntity>("analyseResult", id);
            }
            catch (RequestFailedException e)
            {
                _logger?.Error(e.ToString());
                return null;
            }
        }

        public void PostTableEntity(int pictureId, string pictureName, string analyseResultPath, string status)
        {
            //Probably the partitionkey could be configured better
            var entity = new TableEntity(partitionKey: "analyseResult", rowKey: pictureId.ToString())
            {
                {"Id", pictureId },
                {"PictureName",pictureName},
                {"AnalyseResultPath", analyseResultPath},
                {"Status", status}
            };
            _tableClient.UpsertEntity(entity);
        }

        public async Task UpdateTableEntity(AnalysisResultEntity analysisResultEntity)
        {
            await _tableClient.UpdateEntityAsync(analysisResultEntity, ifMatch: new ETag("*"), mode: TableUpdateMode.Replace);
        }
        public async Task DeleteTableEntity(string id)
        {
            await _tableClient.DeleteEntityAsync("analyseResult", id);
        }

        public async Task<AnalysisResultEntity> UpsertTableEntity(AnalysisResultEntity analyseResultEntity)
        {
            await _tableClient.UpsertEntityAsync(analyseResultEntity);
            return analyseResultEntity;
        }
        
        public async Task<List<PictureInfoForm>> GetUnanalysedPictures(List<PictureInfoForm> pictures)
        {
            string filter = string.Join("or ", pictures.Select(picture => $"RowKey eq '{picture.Id}'").ToList());
            filter = $"(PartitionKey eq 'analyseResult') and ({filter})";
            var entities = _tableClient.Query<AnalysisResultEntity>(filter).ToList();

            return pictures.Where(picture =>
               !entities.Any(entity => picture.Id == entity.Id)
            ).ToList();
        }
    }
}
