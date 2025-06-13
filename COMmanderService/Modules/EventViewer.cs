using System.Diagnostics;
using System.IO;

namespace COMmanderService.Modules
{
    internal class EventViewer
    {
        public static bool CreateIfNotExists()
        {
            if (!EventLog.SourceExists("COMmander"))
            {
                EventLog.CreateEventSource("COMmander", "COMmander");  // If created, the application needs to exit first to register the source
                return true;
            }
            return false;
        }
    }
}
