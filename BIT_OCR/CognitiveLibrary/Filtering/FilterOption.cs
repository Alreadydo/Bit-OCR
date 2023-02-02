using BiT.Central.Core.Mvc.Query;
using CognitiveLibrary.Interfaces;
using System.Linq;
using System.Web;
namespace CognitiveLibrary.Filtering
{
    public class FilterOption : FilteringInterface
    {
        public string AddFilters(string url, params Filter[] filters)
        {
            if (filters != null)
            {
                for (int i = 0; i < filters.Count(); i++)
                {
                    string startCharacter = url.Contains('?') ? "&" : "?";
                    url += $"{startCharacter}filters[{i}][key]={HttpUtility.UrlEncode(filters.ElementAt(i).Key)}";
                    url += $"&filters[{i}][value]={HttpUtility.UrlEncode(filters.ElementAt(i).Value)}";
                    url += $"&filters[{i}][operation]={HttpUtility.UrlEncode(filters.ElementAt(i).Operation)}";
                }
            }
            return url;
        }

        public string AddPagination(string url, PaginationQueryParameters pagination)
        {
            if (pagination != null)
            {
                string startCharacter = url.Contains('?') ? "&" : "?";
                url += $"{startCharacter}page={pagination.Page}&pageSize={pagination.PageSize}";
            }
            return url;
        }

        public string AddSorting(string url, SortingQueryParameters sorting)
        {
            if (sorting != null)
            {
                string startCharacter = url.Contains('?') ? "&" : "?";
                url += $"{startCharacter}sortBy={HttpUtility.UrlEncode(sorting.SortBy)}&sortOrder={HttpUtility.UrlEncode(sorting.SortOrder)}";
            }
            return url;
        }
    }
}
