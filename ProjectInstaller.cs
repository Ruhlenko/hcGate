using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace hcGate
{
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        private ServiceInstaller serviceInstaller;
        private ServiceProcessInstaller processInstaller;

        public ProjectInstaller()
        {
            processInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;
            serviceInstaller.StartType = Defaults.ServiceStartType;
            serviceInstaller.ServiceName = Defaults.ServiceName;

            Installers.Add(serviceInstaller);
            Installers.Add(processInstaller);
        }
    }
}
