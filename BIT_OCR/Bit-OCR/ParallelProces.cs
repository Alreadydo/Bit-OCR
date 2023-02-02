using CognitiveLibrary;
using CognitiveLibrary.Model;
using CognitiveLibrary.Services;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Bit_OCR
{
    public class ParallelProces
    {
        private ILogger? _logger;
        private readonly IConfiguration _configuration;
        private StoreService _storeService;
        private BlobService _blobService;
        private CognitiveAnalysisService _cognitiveAnalysisService;
        private TableService _tableService;
        
        public ParallelProces(
            IConfiguration configuration, 
            StoreService storeService, 
            BlobService blobService, 
            TableService tableService, 
            CognitiveAnalysisService cognitiveAnalysisService,
            ILogger? logger = null)
        {
            ArgumentNullException.ThrowIfNull(storeService, nameof(storeService));
            ArgumentNullException.ThrowIfNull(blobService, nameof(blobService));
            ArgumentNullException.ThrowIfNull(tableService, nameof(tableService));
            ArgumentNullException.ThrowIfNull(cognitiveAnalysisService, nameof(cognitiveAnalysisService));

            this._logger = logger;
            this._configuration = configuration;
            this._storeService = storeService;
            this._blobService = blobService;
            this._tableService = tableService;
            this._cognitiveAnalysisService = cognitiveAnalysisService;
        }

        public async Task AnalyseProces(PictureInfoForm pictureInfo)
        {
            var fileName = pictureInfo.FileName;
            var result = await _cognitiveAnalysisService.ProcessImage(pictureInfo);
            if (result != null)
            {

                var storageUrl = $"{_configuration["AzureBlob:StorageUrl"]}{fileName}.json";
                _logger?.Information(pictureInfo.Id.ToString());
                //await _storeService.StoreAsJson(result, fileName, pictureInfo.Id);
                var blobStream = _storeService.ObjectToStream(result, fileName, pictureInfo.Id);
                blobStream.Result.Position = 0;
                _logger?.Information("Uploading file to blob storage");
                await _blobService.UploadStreamToBlobStorage(blobStream.Result, fileName);
                _tableService.PostTableEntity(pictureInfo.Id, fileName, storageUrl, result.Status);
                blobStream.Dispose();
            }
        }

        public async Task ParallelRequest(List<PictureInfoForm> fotoUrls)
        {
            _logger?.Information(fotoUrls.Count.ToString());
            int step = 0;
            int stepCount = 100;
            var urls = fotoUrls.Skip(step * stepCount).Take(stepCount);
            while (urls.Any())
            {
                Parallel.ForEach(urls,
                       new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 },
                       url => AnalyseProces(url).Wait());

                step++;
                urls = fotoUrls.Skip(step * stepCount).Take(stepCount);
            }
        }

    }
}
