using COMmanderService.Modules;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace COMmanderService
{
    public partial class Service : ServiceBase
    {
        private static Modules.ETW ETWSession = null;
        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (!Modules.Helpers.IsAdministrator() && !Modules.Helpers.IsSystem())
            {
                Stop();  // Don't run service if not running as administrator or system
            }
            if (EventViewer.CreateIfNotExists())
            {
                Stop();  // stop service if event source was just created
            }
            EventLog.WriteEntry("COMmander", "Service Starting", System.Diagnostics.EventLogEntryType.Information, 1, 1);
            List<Rule> rules = Modules.Filter.Load();
            ETWSession = new Modules.ETW();
            ETWSession.openSession();
            Task.Run(() => ETWSession.Monitor(rules));
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry("COMmander", "Service Stopping", System.Diagnostics.EventLogEntryType.Information, 2, 1);
            ETWSession.closeSession();
        }
    }
}
