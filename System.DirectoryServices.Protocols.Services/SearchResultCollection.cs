using System.Collections;
using System.Collections.Generic;

namespace System.DirectoryServices.Protocols.Services
{
    public class SearchResultCollection : IEnumerable<SearchResult>
    {
        private DirectoryEntry _searchRoot;
        private List<SearchResult> _entries;
        private List<string> _propertiesToLoad;

        public SearchResultCollection(DirectoryEntry searchRoot, SearchResultEntryCollection entries, List<string> propertiesToLoad)
        {
            _searchRoot = searchRoot;
            _entries = new List<SearchResult>();
            foreach (SearchResultEntry entry in entries)
            {
                var result = new SearchResult(_searchRoot.GetCredentials(), _searchRoot.AuthenticationType);
                result.Properties["distinguishedName"] = new List<string>(new[] { entry.DistinguishedName });
                foreach (DirectoryAttribute attr in entry.Attributes.Values)
                {
                    var properties = new List<object>();
                    // Use for instead of foreach because DirectoryAttribute's index
                    // property does fancy casting to string as needed.
                    for (int i = 0; i < attr.Count; i++)
                    {
                        properties.Add(attr[i]);
                    }

                    result.Properties[attr.Name] = properties; 
                }
                _entries.Add(result);
            }
            _propertiesToLoad = propertiesToLoad;
        }

        public SearchResult this[int index] => _entries[index];
        public int IndexOf(SearchResult result) => _entries.IndexOf(result);
        public void CopyTo(SearchResult[] results, int index) => _entries.CopyTo(results, index);
        public int Count => _entries.Count;
        public string[] PropertiesLoaded => _propertiesToLoad.ToArray();
        public bool Contains(SearchResult result) => _entries.Contains(result);
        public IEnumerator<SearchResult> GetEnumerator() => _entries.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _entries.GetEnumerator();
    }
}