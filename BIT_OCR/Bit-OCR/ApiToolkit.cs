using BiT.Central.Core.Mvc;
using BiT.Central.Core.Mvc.Query;
using Bit_OCR.Models.ApiToolkit;
using CognitiveLibrary.Filtering;
using CognitiveLibrary.Model;
using Microsoft.Extensions.Configuration;
namespace Bit_OCR
{
    public class ApiToolkit : BitCentralApiWrapper
    {
        private readonly IConfiguration _configuration;

        public ApiToolkit(BitCentralApiWrapperSettings config, IConfiguration configuration) : base(config)
        {
            AcceptResponseEncoded = true;
            SendRequestEncoded = true;
            //Is this necessary?
            //Client.Timeout = TimeSpan.FromMinutes(10);
            _configuration = configuration;
        }
        public async Task<BitCentralApiResponse<PagedListPage<Picture>>> GetNextPhotos(DateTime startDate, DateTime? endDate=null, IEnumerable<int>? angleId = null, int pageSize = 100, int page = 1)
        {
            List<Filter> filters = new List<Filter>();


            Filter filter2 = new Filter()
            {
                Key = "pictureTaken",
                Value = startDate.ToString("yyyy/MM/dd"),
                Operation = "GREATER_THAN"
            };
            filters.Add(filter2);

            if (endDate != null)
            {
                Filter filter3 = new Filter()
                {
                    Key = "pictureTaken",
                    Value = endDate.Value.ToString("yyyy/MM/dd"),
                    Operation = "LESS_THAN"
                };
                filters.Add(filter3);
            }

            if(angleId?.Any() ?? false)
            {
                Filter filter4 = new Filter()
                {
                    Key = "angleId",
                    Value = string.Join(';', angleId.Select(id => id.ToString())),
                    Operation = "ANY_OF"
                };

                filters.Add(filter4);
            }

            SortingQueryParameters sorting = new SortingQueryParameters()
            {
                SortBy = "Id",
                SortOrder = "ASCENDING"
            };
            
            PaginationQueryParameters pagination = new PaginationQueryParameters()
            {
                Page = page,
                PageSize = pageSize
            };

            var filterOption = new FilterOption();
            var url = $"/pictures";
            url = filterOption.AddFilters(url,filters.ToArray());
            url = filterOption.AddSorting(url, sorting);
            url = filterOption.AddPagination(url, pagination);
            return await TryGetObjectAsync<PagedListPage<Picture>>(url);
        }

        public async Task<List<PictureInfoForm>> GetPictureUrls(int page, DateTime startDate, DateTime? endDate, int pageSize, IEnumerable<int>? angleId)
        {
            var pictures = new List<PictureInfoForm>();
            var baseUrl = _configuration["PhotoStorage:BaseUrl"];
            try
            {
                
                var response = await GetNextPhotos(startDate, endDate, angleId, pageSize, page);
                var result = response.Result;
                var pages = result.TotalPages;
                foreach (var item in result.Items)
                {
                    PictureInfoForm pictureInfo = new PictureInfoForm()
                    {
                        Id = item.Id,
                        FileName = item.FileName,
                        FileUrl = $"{baseUrl}{item.FileName}.png{_configuration["BiT:Central:PhotoApi:SasToken"]}"
                    };
                    pictures.Add(pictureInfo);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return pictures;
        }

        public static List<string> GetApiFileNames(List<PictureInfoForm> pictureInfos)
        {
            List<string> fileNames = new List<string>();
            foreach (PictureInfoForm pictureInfo in pictureInfos)
            {
                fileNames.Add(pictureInfo.FileName);
            }
            return fileNames;
        }

        public async Task<int> GetTotalPages(DateTime startDate, DateTime? endDate, int pageSize, IEnumerable<int>? angleId)
        {
            var response = await GetNextPhotos(startDate, endDate, angleId, pageSize);
            Console.WriteLine("TotaalItems"+response.Result.TotalItems);
            return response.Result.TotalPages;
        }
    }
}
