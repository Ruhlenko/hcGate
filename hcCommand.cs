using System;
using System.Collections.Generic;
using System.Text;

namespace hcGate
{
    class hcCommand : EventArgs
    {
        private byte _id;
        public byte ID { get { return _id; } }

        private string _keys;
        public string Keys { get { return _keys; } }

        public hcCommand(string keys) { _id = 0; _keys = keys; }
        public hcCommand(byte id, string keys) { _id = id; _keys = keys; }
    }
}
