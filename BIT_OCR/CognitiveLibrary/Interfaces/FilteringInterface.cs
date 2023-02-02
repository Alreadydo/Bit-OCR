using BiT.Central.Core.Mvc.Query;

namespace CognitiveLibrary.Interfaces
{
    public interface FilteringInterface
    {
        public string AddFilters(string url, params Filter[] filters);
        public string AddPagination(string url, PaginationQueryParameters pagination);
        public string AddSorting(string url, SortingQueryParameters sorting);
    }
}
