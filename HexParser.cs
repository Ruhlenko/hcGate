using System;
using System.Globalization;
using System.Text;

namespace hcGate
{
    static class HexParser
    {

        public static Byte GetByte(byte[] buffer, int index)
        {
            Byte data = 0;
            try
            {
                data = Byte.Parse(Encoding.ASCII.GetString(buffer, index, sizeof(Byte) * 2), NumberStyles.HexNumber);
            }
            catch { }
            return data;
        }

        public static Int16 GetInt16(byte[] buffer, int index)
        {
            Int16 data = 0;
            try
            {
                data = Int16.Parse(Encoding.ASCII.GetString(buffer, index, sizeof(Int16) * 2), NumberStyles.HexNumber);
            }
            catch { }
            return data;
        }

        public static UInt16 GetUInt16(byte[] buffer, int index)
        {
            UInt16 data = 0;
            try
            {
                data = UInt16.Parse(Encoding.ASCII.GetString(buffer, index, sizeof(UInt16) * 2), NumberStyles.HexNumber);
            }
            catch { }
            return data;
        }

        public static Int32 GetInt32(byte[] buffer, int index)
        {
            Int32 data = 0;
            try
            {
                data = Int32.Parse(Encoding.ASCII.GetString(buffer, index, sizeof(Int32) * 2), NumberStyles.HexNumber);
            }
            catch { }
            return data;
        }

        public static UInt32 GetUInt32(byte[] buffer, int index)
        {
            UInt32 data = 0;
            try
            {
                data = UInt32.Parse(Encoding.ASCII.GetString(buffer, index, sizeof(UInt32) * 2), NumberStyles.HexNumber);
            }
            catch { }
            return data;
        }

    }
}
