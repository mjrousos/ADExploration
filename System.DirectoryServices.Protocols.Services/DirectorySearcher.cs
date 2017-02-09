using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Protocols.Services
{
    public class DirectorySearcher : IDisposable
    {
        #region private fields
        private const string defaultFilter = "(objectClass=*)";

        private DirectoryEntry _searchRoot;
        private string _filter = defaultFilter;
        private List<string> _propertiesToLoad;
        private bool _disposed = false; // true: if a temporary entry inside Searcher has been created
        private bool _rootEntryAllocated = false;
        private SearchScope _scope = SearchScope.Subtree;
        private bool _scopeSpecified = false;
        private string _assertDefaultNamingContext = null;
        #endregion


        #region Constructors
        public DirectorySearcher() : this(null, defaultFilter, null, SearchScope.Subtree)
        {
            _scopeSpecified = false;
        }

        public DirectorySearcher(DirectoryEntry searchRoot) : this(searchRoot, defaultFilter, null, SearchScope.Subtree)
        {
            _scopeSpecified = false;
        }

        public DirectorySearcher(DirectoryEntry searchRoot, string filter) : this(searchRoot, filter, null, SearchScope.Subtree)
        {
            _scopeSpecified = false;
        }

        public DirectorySearcher(DirectoryEntry searchRoot, string filter, string[] propertiesToLoad) : this(searchRoot, filter, propertiesToLoad, SearchScope.Subtree)
        {
            _scopeSpecified = false;
        }

        public DirectorySearcher(string filter) : this(null, filter, null, SearchScope.Subtree)
        {
            _scopeSpecified = false;
        }

        public DirectorySearcher(string filter, string[] propertiesToLoad) : this(null, filter, propertiesToLoad, SearchScope.Subtree)
        {
            _scopeSpecified = false;
        }

        public DirectorySearcher(string filter, string[] propertiesToLoad, SearchScope scope) : this(null, filter, propertiesToLoad, scope)
        {
        }

        public DirectorySearcher(DirectoryEntry searchRoot, string filter, string[] propertiesToLoad, SearchScope scope)
        {
            _searchRoot = searchRoot;
            _filter = filter;
            if (propertiesToLoad != null)
                PropertiesToLoad.AddRange(propertiesToLoad);
            this.SearchScope = scope;
        }
        #endregion


        #region Public properties
        public string Filter
        {
            get
            {
                return _filter;
            }
            set
            {
                if (value == null || value.Length == 0)
                    value = defaultFilter;
                _filter = value;
            }
        }
        
        public List<string> PropertiesToLoad
        {
            get
            {
                if (_propertiesToLoad == null)
                {
                    _propertiesToLoad = new List<string>();
                }
                return _propertiesToLoad;
            }
        }

        public DirectoryEntry SearchRoot
        {
            get
            {
                if (_searchRoot == null)
                {
#if FALSE // TODO
                    // get the default naming context. This should be the default root for the search.
                    DirectoryEntry rootDSE = new DirectoryEntry("LDAP://RootDSE", true, null, null, AuthenticationTypes.Secure);

                    //SECREVIEW: Searching the root of the DS will demand browse permissions
                    //                     on "*" or "LDAP://RootDSE".
                    string defaultNamingContext = (string)rootDSE.Properties["defaultNamingContext"][0];
                    rootDSE.Dispose();

                    _searchRoot = new DirectoryEntry("LDAP://" + defaultNamingContext, true, null, null, AuthenticationTypes.Secure);
                    _rootEntryAllocated = true;
                    _assertDefaultNamingContext = "LDAP://" + defaultNamingContext;
#endif // FALSE
                }
                return _searchRoot;
            }
            set
            {
                if (_rootEntryAllocated)
                    _searchRoot.Dispose();
                _rootEntryAllocated = false;

                _assertDefaultNamingContext = null;
                _searchRoot = value;
            }
        }

        public SearchScope SearchScope
        {
            get
            {
                return _scope;
            }
            set
            {
                if (value < SearchScope.Base || value > SearchScope.Subtree)
                    throw new ArgumentException($"Invalid Scope value ({(int)value})", "value");

                _scope = value;

                _scopeSpecified = true;
            }
        }
        #endregion

        #region Public methods
        public SearchResult FindOne() => FindAll(1).FirstOrDefault();

        public SearchResultCollection FindAll() => FindAll(-1);

        public SearchResultCollection FindAll(int count)
        {
            // TODO - Do something with count

            if (!PropertiesToLoad.Contains("ADsPath"))
            {
                PropertiesToLoad.Add("ADsPath");
            }

            SearchRequest req = new SearchRequest(SearchRoot.DistinguishedName, Filter, SearchScope, PropertiesToLoad.ToArray());

            // Use the SearchRoot's connection since it should already exist.
            // TODO - Learn more about AD to understand if it's better to create a new connection or to use an existing one.
            var res = SearchRoot.Connection.SendRequest(req) as SearchResponse;

            if (res.ResultCode != ResultCode.Success)
            {
                throw new InvalidOperationException($"Error connecting to Active Directory: {res.ResultCode.ToString()}: {res.ErrorMessage}");
            }

            return new SearchResultCollection(SearchRoot, res.Entries, PropertiesToLoad); 
        }
        #endregion

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_rootEntryAllocated)
                    _searchRoot.Dispose();
                _rootEntryAllocated = false;
                _disposed = true;
            }
        }
    }
}
