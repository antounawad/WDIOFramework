using System.ServiceModel;
using System.ServiceProcess;
using Eulg.Update.Shared;

namespace Eulg.Update.Service
{
    public partial class EulgUpdateService : ServiceBase
    {
        private ServiceHost _serviceHost;
        private const string Address = "net.pipe://localhost/EULGUpdate/Updater";

        public EulgUpdateService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _serviceHost?.Close();
            _serviceHost = new ServiceHost(typeof (Updater));
            var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            _serviceHost.AddServiceEndpoint(typeof (IUpdater), binding, Address);
            _serviceHost.Open();
        }

        protected override void OnStop()
        {
            if (_serviceHost != null)
            {
                _serviceHost.Close();
                _serviceHost = null;
            }
        }
    }
}