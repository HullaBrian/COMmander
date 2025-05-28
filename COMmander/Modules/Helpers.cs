using System;
using System.Diagnostics;

namespace COMmander.Modules
{
    internal class Helpers
    {
        public static bool IsAdministrator()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        public static String getProcessNameFromPID(int pid)
        {
            string processNameFromPid = "N/A (Process may have exited)";
            try
            {
                Process p = Process.GetProcessById(pid);
                processNameFromPid = p.ProcessName;
                p.Dispose(); // Dispose the process object
            }
            catch (ArgumentException)
            {
                // Process with data.ProcessID is not running or has exited
                processNameFromPid = $"N/A (Process ID {pid} not found or exited, ETW original: {pid})";
            }
            catch (Exception ex)
            {
                processNameFromPid = $"N/A (Error fetching name for PID {pid}: {ex.GetType().Name}, ETW original: {pid})";
            }
            return processNameFromPid;
        }
    }
}
