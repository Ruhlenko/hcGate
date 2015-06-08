using System;
using System.Globalization;
using System.ServiceProcess;
using System.Text;

namespace hcGate
{

    static class Defaults
    {
        public const string SettingsFileName = "hcGate.conf";

        public const string ServiceName = "hcGate Service";
        public const ServiceStartMode ServiceStartType = ServiceStartMode.Manual;
        public const int TcpPort = 7231;

        public const int IoBufferSize = 255;
        
        public const string ComPortName = "COM1";
        public const int ComPortBaud = 115200;
        public const string ComPortDelimiter = "\n";

        public const double WatchdogTimerInterval = 5000;
    }

    static class CheckSum
    {

        public static byte Sum8(string _str)
        {
            byte result = 0;
            for (int i = 0; i < _str.Length; i++)
                result += (byte)_str[i];
            return result;
        }

        public static byte Sum8(byte[] _buffer, int _size)
        {
            byte result = 0;
            for (int i = 0; i < _size; i++)
                result += _buffer[i];
            return result;
        }

    }

    static class HexParser
    {

        public static Byte GetByte(byte[] _buffer, int _index)
        {
            Byte _data = 0;
            try
            {
                _data = Byte.Parse(Encoding.ASCII.GetString(_buffer, _index, sizeof(Byte) * 2), NumberStyles.HexNumber);
            }
            catch { }
            return _data;
        }

        public static Int16 GetInt16(byte[] _buffer, int _index)
        {
            Int16 _data = 0;
            try
            {
                _data = Int16.Parse(Encoding.ASCII.GetString(_buffer, _index, sizeof(Int16) * 2), NumberStyles.HexNumber);
            }
            catch { }
            return _data;
        }

        public static Int32 GetInt32(byte[] _buffer, int _index)
        {
            Int32 _data = 0;
            try
            {
                _data = Int32.Parse(Encoding.ASCII.GetString(_buffer, _index, sizeof(Int32) * 2), NumberStyles.HexNumber);
            }
            catch { }
            return _data;
        }

        public static UInt32 GetUInt32(byte[] _buffer, int _index)
        {
            UInt32 _data = 0;
            try
            {
                _data = UInt32.Parse(Encoding.ASCII.GetString(_buffer, _index, sizeof(UInt32) * 2), NumberStyles.HexNumber);
            }
            catch { }
            return _data;
        }

    }

}
