using System.Collections.Generic;

namespace CognitiveLibrary.Model
{
    public class SearchItems
    {
        public int TotalResults { get; set; }
        public List<SearchItem> Items { get; set; }
    }
}
