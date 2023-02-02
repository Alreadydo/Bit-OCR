using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using CognitiveLibrary.Interfaces;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using CognitiveLibrary.Model;
using System.Collections.Generic;
using BiT.Central.Core.Mvc;
using BiT.Central.Core.Mvc.Query;
using BiT.Central.Core.Extensions;

namespace CognitiveLibrary
{
    public class SearchService : ISearchInterface
    {
        private static ILogger logger;
        private readonly IConfiguration Configuration;
        public SearchService(IConfiguration configuration, ILogger Ilogger = null)
        {
            logger = Ilogger;
            Configuration = configuration;
        }

        public async Task<PagedListPage<SearchItem>> SearchItem(string input, PaginationQueryParameters pagination, SortingQueryParameters sorting, List<Filter> filters)
        {
            string indexName = Configuration["AzureSearch:IndexName"];
            Uri endpoint = new Uri(Configuration["AzureSearch:APIEndpoint"]);
            string key = Configuration["AzureSearch:Key"];
            AzureKeyCredential azureKeyCredential = new AzureKeyCredential(key);
            SearchOptions searchOptions = new SearchOptions()
            {
                IncludeTotalCount = true,
                SearchMode = SearchMode.All,
                OrderBy = { "search.score() desc" }
                
            };
            SearchClient searchClient = new SearchClient(endpoint, indexName, azureKeyCredential);
            SearchResults<SearchDocument> response = await searchClient.SearchAsync<SearchDocument>(
                input,searchOptions);
   
            logger?.Information($"Amount of results: {response.GetResults().Count()}");
            if (response.GetResults() != null)
            {
                List<SearchItem> searchItemList = new List<SearchItem>();
                foreach (SearchResult<SearchDocument> result in response.GetResults())
                {
                     
                    SearchDocument document = result.Document;
                  
                    SearchItem searchItem = new SearchItem
                    {
                        Id = int.Parse(document.GetString("Id")),
                        Score = result.Score,
                        PictureName = (string)document["metadata_storage_name"]
                    };
                    Console.WriteLine("Id:"+ searchItem.Id);
                    Console.WriteLine("Score:"+ result.Score);
                   
                    searchItemList.Add(searchItem);
                }

                return searchItemList.AsQueryable().Query(pagination, sorting, filters);
            }
            else return null;
        }

    }
}
