using CognitiveLibrary.Interfaces;
using CognitiveLibrary.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CognitiveLibrary
{
    public class RequestAnalyseService : IRequestAnalysisInterface
    {
        private static ILogger logger;
        private readonly IConfiguration _configuration;
        public RequestAnalyseService(IConfiguration configuration, ILogger Ilogger=null )
        {
            logger = Ilogger;
            _configuration = configuration;
        }

        public async Task<PostResponse> BadRequestAnalyse(HttpResponseMessage httpResponseMessage, int pictureId, string fileName)
        {
            string body = await httpResponseMessage.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(body);
            logger?.Warning($"400 BadRequest returnd by {pictureId}");
            var storeService = new StoreService(_configuration);
            var tableService = new TableService(_configuration, logger);
            PostResponse postResponse = new PostResponse()
            {
                PictureId = pictureId,
                Status = "Failed",
                Message = $"Bad request:{errorResponse.Error.Code}"
            };
            storeService.StoreFailedPicture(postResponse, pictureId);
            tableService.PostTableEntity(pictureId, fileName, "null", "Failed");
            return postResponse;
        }

        public PostResponse SuccessfulRequestAnalyse(HttpResponseMessage httpResponseMessage, int pictureId)
        {
            HttpResponseHeaders responseHeader = httpResponseMessage.Headers;
            string resultUrl = responseHeader.GetValues("Operation-Location").First();
            PostResponse postResponse = new PostResponse()
            {
                PictureId = pictureId,
                Status = "Success",
                Message = resultUrl
            };
            return postResponse;
        }
    }
}

