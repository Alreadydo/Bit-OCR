using BiT.Central.Core.Mvc;
using CognitiveLibrary;
using CognitiveLibrary.Model;
using CognitiveLibrary.Services;
using Microsoft.Extensions.Configuration;
using CommandLine;

namespace Bit_OCR
{
    class Program
    {
        public class Options
        {
            [Option('s', "startDate", Required = true, HelpText = "Set startDate for filter")]
            public DateTime StartDate { get; set; }

            [Option('e', "endDate", Required = true, HelpText = "Set endDate for filter")]
            public DateTime? EndDate { get; set; }

            [Option('a', "angleId", Required =  false, HelpText ="Set angleId for filter")]
            public IEnumerable<int>? AngleId { get; set; }
            
            [Option('p', "pageSize", Required = false, HelpText = "Set pageSize for filter")]
            public int pageSize { get; set; }
        }
        public static async Task Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o=> OcrProces(o).Wait());


        }
        public async static Task OcrProces(Options options)
        {
            string settingsFile = "appsettings.json";
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(settingsFile, optional: true, reloadOnChange: true)
                .Build();

            var photoApiConfig = new BitCentralApiWrapperSettings("BiT:Central:PhotoApi", configuration);
            var apiToolkit = new ApiToolkit(photoApiConfig, configuration);
            var test = options.AngleId;
            var storeService = new StoreService(configuration);
            var blobService = new BlobService(configuration);
            var tableService = new TableService(configuration);
            var requestAnalysisService = new RequestAnalyseService(configuration);
            var cognitiveAnalysisService = new CognitiveAnalysisService(configuration, requestAnalysisService);
            var paralleProces = new ParallelProces(configuration, storeService, blobService, tableService, cognitiveAnalysisService);

            var totalPages = await apiToolkit.GetTotalPages(options.StartDate, options.EndDate, options.pageSize, options.AngleId);
            for (int page = 1; page <= totalPages; page++)
            {
                Console.WriteLine("page:" + page);
                var pictureUrls = await apiToolkit.GetPictureUrls(page, options.StartDate, options.EndDate, options.pageSize, options.AngleId);

                List<PictureInfoForm> pictures = await tableService.GetUnanalysedPictures(pictureUrls);
                Console.WriteLine(pictures.Count());
                await paralleProces.ParallelRequest(pictures);

            }

        }
    }
}