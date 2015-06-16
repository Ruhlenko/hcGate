using System.ServiceProcess;

namespace hcGate
{
    static class Defaults
    {
        public const string SettingsFileName = "hcGate.ini";

        public const string ServiceName = "hcGate Service";
        public const ServiceStartMode ServiceStartType = ServiceStartMode.Manual;
        public const int TcpPort = 7231;

        public const int IoBufferSize = 255;

        public const string ComPortName = "COM1";
        public const int ComPortBaud = 115200;
        public const string ComPortDelimiter = "\n";

        public const double WatchdogTimerInterval = 5000;

        public const int DataCacheSize = 1024;
    }
}
