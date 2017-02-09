using System;
using System.Net;
using System.DirectoryServices.Protocols;
using System.Linq;

namespace System.DirectoryServices.Protocols.Services
{
    public class DirectoryEntry : IDisposable
    {
        #region private fields
        private string _path = "";
        private NetworkCredential _credentials;
        private AuthenticationTypes _authenticationType = AuthenticationTypes.Secure;
        private bool _userNameIsNull = false;
        private bool _passwordIsNull = false;
        private bool _disposed = false;
        private LdapConnection _ldapConnection;
        #endregion


        #region Constructors
        public DirectoryEntry()
        {

        }

        public DirectoryEntry(string path) : this()
        {
            Path = path;
        }

        public DirectoryEntry(string path, string username, string password, AuthenticationTypes authenticationType) : this(path)
        {
            // Store credentials separate from _ldapConnection since LdapConnection.Credential is read-only
            _credentials = new NetworkCredential(username, password);

            if (username == null)
                _userNameIsNull = true;

            if (password == null)
                _passwordIsNull = true;

            _authenticationType = authenticationType;
        }
        #endregion


        #region Public Properties
        internal LdapConnection Connection
        {
            get
            {
                Bind();
                return _ldapConnection;
            }
        }

        public string Name
        {
            get
            {
                // Bind();
                // TODO - What is ADSI doing in its .Name function?
                return RelativeDistinguishedName;
            }
        }

        // TODO : Should probably cache these
        public string DistinguishedName => Path?.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        public string RelativeDistinguishedName => DistinguishedName?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

        public AuthenticationTypes AuthenticationType
        {
            get
            {
                return _authenticationType;
            }
            set
            {
                if (_authenticationType == value)
                    return;

                _authenticationType = value;
                Unbind();
            }
        }

        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                if (value == null)
                    value = "";

                if (string.Equals(_path, value, StringComparison.CurrentCultureIgnoreCase))
                    return;

                _path = value;
                Unbind();
            }
        }

        public string Username
        {
            get
            {
                if (_credentials == null || _userNameIsNull)
                    return null;

                return _credentials.UserName;
            }
            set
            {
                if (value == GetUsername())
                    return;

                if (_credentials == null)
                {
                    _credentials = new NetworkCredential();
                    _passwordIsNull = true;
                }

                if (value == null)
                    _userNameIsNull = true;
                else
                    _userNameIsNull = false;

                _credentials.UserName = value;

                Unbind();
            }
        }
        #endregion


        private void Bind()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            if (_ldapConnection == null)
            {
                _ldapConnection = new LdapConnection(GetDomainFromPath());
                if (_credentials != null)
                {
                    _ldapConnection.Credential = _credentials;
                }
                _ldapConnection.SessionOptions.AutoReconnect = true; // Do we want this?
                // Should I check _ldapConnection.SessionOptions.HostReachable
            }
        }
        
        private string GetDomainFromPath()
        {
            var domainComponents = Path?.Split(new[] { ',', ':', '/' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(rdn => rdn.StartsWith("DC=", StringComparison.OrdinalIgnoreCase))
                        .Select(rdn => rdn.Substring(3).Trim())
                        ?? Enumerable.Empty<string>();
            return string.Join(".", domainComponents);
        }

        private void Unbind()
        {
            _ldapConnection?.Dispose();
            _ldapConnection = null;
        }

        internal string GetUsername()
        {
            if (_credentials == null || _userNameIsNull)
                return null;

            return _credentials.UserName;
        }

        internal string GetPassword()
        {
            if (_credentials == null || _passwordIsNull)
                return null;

            return _credentials.Password;
        }

        internal NetworkCredential GetCredentials()
        {
            return _credentials;
        }


        protected void Dispose(bool disposing)
        {
            // no managed object to free

            // free own state (unmanaged objects)
            if (!_disposed)
            {
                Unbind();
                _disposed = true;
            }
        }

        ~DirectoryEntry()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
