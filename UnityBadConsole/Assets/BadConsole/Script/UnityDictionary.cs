using UnityEngine;
using System.Collections.Generic;
using System;

namespace bad_log
{
    [Serializable]
    public class UnityDictionary<TKey, TValue>
    {
        [SerializeField]
        private List<TKey> _keys = new List<TKey>();

        [SerializeField]
        private List<TValue> _values = new List<TValue>();

        private Dictionary<TKey, TValue> _cache;

        public Dictionary<TKey, TValue> Cache
        {
            get
            {
                if (_cache == null)
                    BuildCache();
                return _cache;
            }
        }
        public List<TValue> Values
        {
            get { return _values; }
        }
        public List<TKey> Keys
        {
            get { return _keys; }
        }

        public void Add(TKey key, TValue value)
        {
            if (_cache == null)
                BuildCache();

            _cache.Add(key, value);
            _keys.Add(key);
            _values.Add(value);
        }
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_cache == null)
                BuildCache();

            return _cache.TryGetValue(key, out value);
        }
        public bool ContainsKey(TKey key)
        {
            if (_cache == null)
                BuildCache();
            return _cache.ContainsKey(key);
        }
        public bool Remove(TKey key)
        {
            TValue v;
            if (TryGetValue(key, out v))
            {
                _cache.Remove(key);
                _keys.Remove(key);
                _values.Remove(v);
                return true;
            }
            return false;
        }
        public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            if (_cache == null)
                BuildCache();

            return _cache.GetEnumerator();
        }

        public TValue this[TKey key]
        {
            get
            {
                if (_cache == null)
                    BuildCache();

                return _cache[key];
            }
        }

        void BuildCache()
        {
            _cache = new Dictionary<TKey, TValue>();
            for (int i = 0; i != _keys.Count; i++)
            {
                _cache.Add(_keys[i], _values[i]);
            }
        }
    }

}
