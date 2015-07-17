using System;
using System.Globalization;
using System.Text;

namespace hcGate
{
    static class HexParser
    {
        public static Byte GetByte(byte[] buffer, int index)
        {
            Byte data;

            Byte.TryParse(
                Encoding.ASCII.GetString(buffer, index, sizeof(Byte) * 2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out data);

            return data;
        }

        public static Int16 GetInt16(byte[] buffer, int index)
        {
            Int16 data;

            Int16.TryParse(
                Encoding.ASCII.GetString(buffer, index, sizeof(Int16) * 2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out data);

            return data;
        }

        public static UInt16 GetUInt16(byte[] buffer, int index)
        {
            UInt16 data;

            UInt16.TryParse(
                Encoding.ASCII.GetString(buffer, index, sizeof(UInt16) * 2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out data);

            return data;
        }

        public static Int32 GetInt32(byte[] buffer, int index)
        {
            Int32 data;

            Int32.TryParse(
                Encoding.ASCII.GetString(buffer, index, sizeof(Int32) * 2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out data);

            return data;
        }

        public static UInt32 GetUInt32(byte[] buffer, int index)
        {
            UInt32 data;

            UInt32.TryParse(
                Encoding.ASCII.GetString(buffer, index, sizeof(UInt32) * 2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out data);

            return data;
        }
    }
}
