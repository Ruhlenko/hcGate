namespace hcGate
{
    class hcCache<T>
    {
        private T _defaultData;
        private T[] _data;

        private int _count = 0;
        public int Count { get { return _count; } }

        public hcCache(int size, T defaultData)
        {
            _defaultData = defaultData;
            _data = new T[size];
            Reset();
        }

        public T Read(int id)
        {
            if (id < 0) return _defaultData;
            if (id >= _data.Length) return _defaultData;

            if (id >= _count) _count = id + 1;

            return _data[id];
        }

        public void Write(int id, T data)
        {
            if (id < 0) return;
            if (id >= _data.Length) return;

            if (id >= _count) _count = id + 1;

            _data[id] = data;
        }

        public void Reset()
        {
            for (var i = 0; i < _data.Length; i++)
                _data[i] = _defaultData;
        }

    }
}
