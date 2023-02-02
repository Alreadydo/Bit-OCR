using BiT.Central.Core.Logging;
using CognitiveLibrary.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CognitiveLibrary
{
    public class CognitiveAnalysisService : ICognitiveInterface
    {

        private static ILogger logger = new BitCentralLoggerFactory().Build(null, null, null, false);
        private readonly IConfiguration _configuration;
        private RequestAnalyseService _requestAnalyseService;

        public CognitiveAnalysisService(IConfiguration configuration, RequestAnalyseService requestAnalyseService)
        {
            if (requestAnalyseService == null)
            {
                throw new ArgumentNullException(nameof(requestAnalyseService));
            }


            this._configuration = configuration;
            this._requestAnalyseService = requestAnalyseService;
        }

        public async Task<ReadResultResponse> GetResultAsJson(string url)
        {
            ReadResultResponse readResultResponse;
            do
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _configuration["CognitiveServices:OCR:Ocp-Apim-Subscription-Key"]);
                HttpResponseMessage responseMessage;
                responseMessage = await httpClient.GetAsync(url);
                string body = await responseMessage.Content.ReadAsStringAsync();
                readResultResponse = JsonConvert.DeserializeObject<ReadResultResponse>(body);
            } while (readResultResponse?.AnalyzeResult == null);

            return readResultResponse;
        }

        public async Task<PostResponse> PostReadApiRequest(PictureInfoForm pictureInfo)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _configuration["CognitiveServices:OCR:Ocp-Apim-Subscription-Key"]);
            UrlObject urlObject = new UrlObject()
            {
                Url = $"{pictureInfo.FileUrl}{_configuration["PhotoStorage:SasToken"]}"
            };
            var json = JsonConvert.SerializeObject(urlObject);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(_configuration["CognitiveServices:OCR:ReadApiUrl"], content);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return _requestAnalyseService.BadRequestAnalyse(response, pictureInfo.Id, pictureInfo.FileName).Result;
                }
                else
                {
                    logger?.Error("Too Many Request at the same time, Start post request once again");
                    Thread.Sleep(5000);
                    await PostReadApiRequest(pictureInfo);
                }
            }

            return _requestAnalyseService.SuccessfulRequestAnalyse(response, pictureInfo.Id);
        }

        public async Task<ReadResultResponse> ProcessImage(PictureInfoForm pictureInfo)
        {
            PostResponse postResponse = await PostReadApiRequest(pictureInfo);
            if (postResponse.Status.Equals("Failed"))
            {
                return null;
            }
            return await GetResultAsJson(postResponse.Message);
        }
    }
}
