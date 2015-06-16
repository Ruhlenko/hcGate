using System.Runtime.InteropServices;
using System.Text;

namespace hcGate
{
    class IniFile
    {
        private static const int _bufferSize = 255;

        public string _file;

        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(
            string section, string key,
            string defaultValue, StringBuilder returnValue,
            int size, string fileName);

        public IniFile(string iniFile)
        {
            _file = iniFile;
        }

        public string ReadValue(string section, string key, string defaultValue)
        {
            StringBuilder returnValue = new StringBuilder(_bufferSize);
            GetPrivateProfileString(section, key, defaultValue, returnValue, _bufferSize, _file);
            return returnValue.ToString();
        }
    }
}
