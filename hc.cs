using System;
using System.Collections.Generic;
using System.Text;

namespace hcGate
{

    class hcData : EventArgs, IEquatable<hcData>, IComparable<hcData>
    {

        int id = 0;
		public int ID { get { return id; } }

		private int data = 0;
		public int Data
		{
			get { return data; }
			set { data = value; }
		}

		public hcData() { id = 0; data = 0; }
		public hcData(int _id) { id = _id; data = 0; }
        public hcData(int _id, int _data) { id = _id; data = _data; }

        bool IEquatable<hcData>.Equals(hcData _other)
		{
			return this.ID.Equals(_other.ID);
		}

        int IComparable<hcData>.CompareTo(hcData _other)
		{
			return this.ID.CompareTo(_other.ID);
		}

    }



    class hcCommand : EventArgs
    {
        byte id;
        public byte ID { get { return id; } }

        string keys;
        public string Keys { get { return keys; } }

        public hcCommand(string _keys) { id = 0; keys = _keys; }
        public hcCommand(byte _id, string _keys) { id = _id; keys = _keys; }
    }

}
