using System.ComponentModel;
using System.Security.AccessControl;
using System.Security.Principal;
using Eulg.Update.Service.ServiceSecurity;

namespace Eulg.Update.Service
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        private void serviceInstaller1_AfterInstall(object sender, System.Configuration.Install.InstallEventArgs e)
        {
            SetServiceRights();
        }
        private void SetServiceRights()
        {
            ServiceSecurity.ServiceSecurity serviceSecurity = Eulg.Update.Service.ServiceSecurity.Service.GetAccessControl(serviceInstaller1.ServiceName);
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            serviceSecurity.AddAccessRule(new ServiceAccessRule(sid, ServiceRights.Start, AccessControlType.Allow));
            serviceSecurity.AddAccessRule(new ServiceAccessRule(sid, ServiceRights.Stop, AccessControlType.Allow));
            serviceSecurity.AddAccessRule(new ServiceAccessRule(sid, ServiceRights.Read, AccessControlType.Allow));
            serviceSecurity.AddAccessRule(new ServiceAccessRule(sid, ServiceRights.SendUserDefinedControl, AccessControlType.Allow));
            //serviceSecurity.AddAccessRule(new ServiceAccessRule(sid, ServiceRights.QueryConfiguration, AccessControlType.Allow));
            //serviceSecurity.AddAccessRule(new ServiceAccessRule(sid, ServiceRights.QueryStatus, AccessControlType.Allow));
            Eulg.Update.Service.ServiceSecurity.Service.SetAccessControl(serviceInstaller1.ServiceName, serviceSecurity);
        }
    }
}
