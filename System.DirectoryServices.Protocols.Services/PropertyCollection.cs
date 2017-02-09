using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices.Protocols.Services
{
    public class PropertyCollection : IDictionary<string, PropertyValueCollection>
    {
        private const string defaultFilter = "(objectClass=*)";
        private DirectoryEntry _entry;
        private Dictionary<string, PropertyValueCollection> _valueTable;

        public PropertyCollection(DirectoryEntry entry)
        {
            this._entry = entry;

            // System.DirectoryServices seems to query properties on a one-off basis (_entry.AdsObject.GetEx(_propertyName, out var);)
            // but I don't see an S.DS.P equivalent for that, so I just load all properties once a PropertyCollection is needed.
            // It's possible that ADSI does the same thing under the covers, but I haven't looked into it yet.
            PopulateTable();
        }

        private void PopulateTable()
        {
            _valueTable = new Dictionary<string, PropertyValueCollection>();

            var req = new SearchRequest(_entry.DistinguishedName, defaultFilter, SearchScope.Base, null);
            // TODO - Learn more about AD to understand if it's better to create a new connection or to use an existing one.
            //        Probably the right thing to do is to cache connections per-domain in some static store.
            var res = _entry.Connection.SendRequest(req) as SearchResponse;

            if (res.ResultCode != ResultCode.Success)
            {
                throw new InvalidOperationException($"Error connecting to Active Directory: {res.ResultCode.ToString()}: {res.ErrorMessage}");
            }

            var entry = res.Entries?[0];
            if (entry == null)
            {
                throw new InvalidOperationException($"Error retrieving properties for {_entry.DistinguishedName}; entry not found in AD.");
            }

            foreach (DirectoryAttribute attr in entry.Attributes.Values)
            {
                _valueTable.Add(attr.Name/*.ToLowerInvariant()*/, new PropertyValueCollection(_entry, attr));
            }
        }

        public PropertyValueCollection this[string propertyName]
        {
            get
            {
                if (propertyName == null)
                    throw new ArgumentNullException("propertyName");

                var name = propertyName/*.ToLowerInvariant()*/;
                if (_valueTable.ContainsKey(name))
                    return _valueTable[name];
                else
                {
                    var value = new PropertyValueCollection(_entry, propertyName);
                    _valueTable.Add(name, value);
                    return value;
                }
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<string> PropertyNames => Keys;

        public bool Contains(string key) => ContainsKey(key);


        #region IDictionary<string, PropertyValueCollection> implementation
        public IEnumerable<string> Keys => _valueTable.Keys;

        public IEnumerable<PropertyValueCollection> Values => _valueTable.Values;

        public int Count => _valueTable.Count;

        ICollection<string> IDictionary<string, PropertyValueCollection>.Keys => _valueTable.Keys;

        ICollection<PropertyValueCollection> IDictionary<string, PropertyValueCollection>.Values => _valueTable.Values;

        public bool IsReadOnly => true; // For now...

        public bool ContainsKey(string key) => _valueTable.ContainsKey(key);

        public bool TryGetValue(string key, out PropertyValueCollection value) => _valueTable.TryGetValue(key, out value);

        public bool Contains(KeyValuePair<string, PropertyValueCollection> item) => _valueTable.Contains(item);

        public void CopyTo(KeyValuePair<string, PropertyValueCollection>[] array, int arrayIndex) => ((ICollection)_valueTable).CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<string, PropertyValueCollection>> GetEnumerator() => _valueTable.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _valueTable.GetEnumerator();

        public void Add(string key, PropertyValueCollection value)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<string, PropertyValueCollection> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, PropertyValueCollection> item)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
