using BiT.Central.Core.Mvc;
using BiT.Central.Core.Mvc.Query;
using CognitiveLibrary.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CognitiveLibrary.Interfaces
{
    public interface ISearchInterface
    {
        public Task <PagedListPage<SearchItem>> SearchItem(string input, PaginationQueryParameters pagination, SortingQueryParameters sorting, List<Filter> filters);
    }
}
