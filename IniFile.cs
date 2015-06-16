using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace hcGate
{
    class IniFile
    {
        [DllImport("kernel32.dll")]
        public static extern int GetModuleFileName(IntPtr hModule, StringBuilder buffer, int bufferSize);

        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(
            string section, string key, string defaultValue,
            StringBuilder buffer, int bufferSize,
            string fileName);

        private const int _bufferSize = 260;

        public string _file;

        public IniFile(string iniFile)
        {
            var exeFullName = new StringBuilder(_bufferSize);
            GetModuleFileName(IntPtr.Zero, exeFullName, _bufferSize);
            _file = Path.Combine(Path.GetDirectoryName(exeFullName.ToString()), iniFile);
        }

        public string ReadValue(string section, string key, string defaultValue)
        {
            var returnValue = new StringBuilder(_bufferSize);
            GetPrivateProfileString(section, key, defaultValue, returnValue, _bufferSize, _file);
            return returnValue.ToString();
        }
    }
}
