using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Protocols.Services
{
    public class SearchResult
    {
        private NetworkCredential _parentCredentials;
        private AuthenticationTypes _parentAuthenticationType;
        private IDictionary<string, IReadOnlyList<object>> _properties = new Dictionary<string, IReadOnlyList<object>>();

        internal SearchResult(NetworkCredential parentCredentials, AuthenticationTypes parentAuthenticationType)
        {
            _parentCredentials = parentCredentials;
            _parentAuthenticationType = parentAuthenticationType;
        }

        public DirectoryEntry GetDirectoryEntry()
        {
            if (_parentCredentials != null)
                return new DirectoryEntry(Path, /*true,*/ _parentCredentials.UserName, _parentCredentials.Password, _parentAuthenticationType);
            else
            {
                DirectoryEntry newEntry = new DirectoryEntry(Path, /* true,*/ null, null, _parentAuthenticationType);
                return newEntry;
            }
        }
        
        public string Path
        {
            get
            {
                return (string)Properties["ADsPath"][0];
            }
        }
        
        public IDictionary<string, IReadOnlyList<object>> Properties
        {
            get
            {
                return _properties;
            }
        }
    }
}
