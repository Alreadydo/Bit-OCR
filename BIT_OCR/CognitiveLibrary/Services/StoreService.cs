using CognitiveLibrary.Interfaces;
using CognitiveLibrary.Model;
using CognitiveLibrary.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveLibrary
{
    public class StoreService : IStoreInterface
    {
        private readonly IConfiguration _configuration;
        public StoreService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<FilterdResult> GenerateFilterdResult(ReadResultResponse readResultResponse, string pictureName, int pictrueId)
        {
            FilterdResult filterdResult = new FilterdResult();
            filterdResult.PictureName = pictureName;
            filterdResult.Id = pictrueId;
            List<ResultDetail> resultDetails = new List<ResultDetail>();

            if (readResultResponse == null)
            {
                filterdResult.ResultDetails = null;
                return filterdResult;
            }

            foreach (var response in readResultResponse.AnalyzeResult.ReadResults)
            {

                for (int i = 0; i < response.Lines.Count(); i++)
                {
                    filterdResult.Text += response.Lines[i].Text;
                    ResultDetail resultDetail = new ResultDetail()
                    {
                        Text = response.Lines[i].Text,
                        BoundingBox = response.Lines[i].BoundingBox
                    };
                    if (response.Lines[i].Appearance == null)
                    {
                        resultDetail.Confidence = null;
                    }
                    else
                    {
                        resultDetail.Confidence = response.Lines[i].Appearance.Style.Confidence;
                    }

                    resultDetails.Add(resultDetail);


                }
            }
            filterdResult.ResultDetails = resultDetails;
            return filterdResult;
        }

        public List<string> GetAnalysedFileNames()
        {
            var files = Directory.GetFiles($"C:analyseresult/filterd").ToList();
            List<string> pictures = new List<string>();
            foreach (var file in files)
            {
                pictures.Add(Path.GetFileNameWithoutExtension(file));
            }
            return pictures;
        }

        public List<string> GetUntreatedFileNames(List<string> analysedFileNames, List<string> apiFileNames)
        {
            var difference = apiFileNames.Except(analysedFileNames).ToList();
            return difference;
        }

        public async Task StoreAsJson(ReadResultResponse resultResponse, string fileName, int pictureId)
        {
            if (resultResponse.AnalyzeResult != null)
            {
                var filterdResult = await GenerateFilterdResult(resultResponse, fileName, pictureId);

                using (var fileWriter = new StreamWriter($"C:analyseresult/filterd/{fileName}.json"))
                {
                    fileWriter.Write(JsonConvert.SerializeObject(filterdResult, Formatting.Indented));
                }
            }
            else
            {
                using (var fileWriter = new StreamWriter($"C:analyseresult/filterd/{fileName}.json"))
                {
                    fileWriter.Write(JsonConvert.SerializeObject(resultResponse, Formatting.Indented));
                }
            }
        }

        public void StoreFailedPicture(PostResponse postResponse, int pictureId)
        {

            using (var fileWriter = new StreamWriter($"C:analyseresult/failed/{pictureId}.json", false))
            {
                fileWriter.Write(JsonConvert.SerializeObject(postResponse, Formatting.Indented));
            }
        }

        public async Task<AnalysisResultEntity> StoreResultResponse(PictureInfoForm form, ReadResultResponse readResultResponse, TableService tableService, BlobService blobService)
        {
            var storageUrl = $"{_configuration["AzureBlob:StorageUrl"]}{form.FileName}.json";
            var blobStream = ObjectToStream(readResultResponse, form.FileName, form.Id);
            blobStream.Result.Position = 0;
            var resultEntity = new AnalysisResultEntity()
            {
                Id = form.Id,
                PictureName = form.FileName,
                AnalyseResultPath = storageUrl,
                Status = readResultResponse.Status
            };
            var entity = await tableService.GetTableEntity(form.Id.ToString());

            if (entity != null)
            {
                resultEntity.PartitionKey = entity.PartitionKey;
                resultEntity.RowKey = resultEntity.Id.ToString();
                await tableService.UpsertTableEntity(resultEntity);
            }
            else
            {
                tableService.PostTableEntity(form.Id, form.FileName, storageUrl, readResultResponse.Status);
            }
            
            await blobService.UploadStreamToBlobStorage(blobStream.Result, form.FileName);
            blobStream.Dispose();

            return resultEntity;
        }

        public async Task<MemoryStream> ObjectToStream(ReadResultResponse readResultResponse, string fileName, int pictureId)
        {
            var filterdResult = await GenerateFilterdResult(readResultResponse, fileName, pictureId);
            var stream = new MemoryStream();
            var json = JsonConvert.SerializeObject(filterdResult, Formatting.Indented);
            var bytes = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(bytes);

            return stream;
        }
    }
}

