using System;
using System.Collections.Generic;
using System.Text;

namespace hcGate
{
    class hcData : EventArgs, IEquatable<hcData>, IComparable<hcData>
    {

        private int _id = 0;
        public int ID { get { return _id; } }

        private int _data = 0;
        public int Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public hcData() { _id = 0; _data = 0; }
        public hcData(int id) { _id = id; _data = 0; }
        public hcData(int id, int data) { _id = id; _data = data; }

        bool IEquatable<hcData>.Equals(hcData other)
        {
            return this.ID.Equals(other.ID);
        }

        int IComparable<hcData>.CompareTo(hcData other)
        {
            return this.ID.CompareTo(other.ID);
        }

    }
}
