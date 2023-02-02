using Azure;
using Azure.Data.Tables;
using System;

namespace CognitiveLibrary.Model
{
    public class AnalysisResultEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public int? Id { get; set; }
        public string PictureName { get; set; }
        public string AnalyseResultPath { get; set; }
        public string Status { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

    }
}
