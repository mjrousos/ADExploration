using System.Collections;
using System.Collections.Generic;

namespace System.DirectoryServices.Protocols.Services
{
    public class PropertyValueCollection : IList<object>
    {
        private DirectoryEntry _entry;
        private string _propertyName;
        private List<object> _values;

        public PropertyValueCollection(DirectoryEntry entry, DirectoryAttribute attr) :this(entry, attr?.Name)
        {
            for(int i = 0; i < attr.Count; i++)
            {
                _values.Add(attr[i]);
            }
        }

        public PropertyValueCollection(DirectoryEntry entry, string name)
        {
            _entry = entry;
            _propertyName = name;

            _values = new List<object>();
        }

        public string PropertyName => _propertyName;

        public object Value
        {
            get
            {
                if (Count == 0)
                {
                    return null;
                }
                else if (Count == 1)
                {
                    return _values[0];
                }
                else
                {
                    return _values.ToArray();
                }
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        #region IList<object> implementation
        public object this[int index] { get => _values[index]; set => throw new NotImplementedException(); }

        public int Count => _values.Count;

        public bool IsReadOnly => true; // For now...

        public bool Contains(object item) => _values.Contains(item);

        public void CopyTo(object[] array, int arrayIndex) => _values.CopyTo(array, arrayIndex);

        public IEnumerator<object> GetEnumerator() => _values.GetEnumerator();

        public int IndexOf(object item) => _values.IndexOf(item);

        IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();

        public void Add(object item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(object item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}