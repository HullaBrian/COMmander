using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

// TODO: Implement process filtering

namespace ETW
{
    class Program
    {
        private static readonly Guid RpcProviderGuid = new Guid("{6AD52B32-D609-4BE9-AE07-CE8DAE937E39}");
        private const string SessionName = "COMmander-RPC-ETW-Session";

        static void Main(string[] args)
        {
            if (!IsAdministrator())
            {
                Console.WriteLine("This program must be run as an administrator to collect ETW events.");
                Console.WriteLine("Please restart the application with administrative privileges.");
                return;
            }

            Console.WriteLine("[+] Loading configuration rules...");
            List<Filter.Rule> rules = Filter.Filter.Load();
            Console.WriteLine("[+] Loaded configuration rules!");

            Console.WriteLine($"[+] Starting ETW session for provider: Microsoft-Windows-RPC ({RpcProviderGuid})");
            Console.WriteLine($"[+] Session Name: {SessionName}");
            Console.WriteLine("Press Ctrl+C to stop listening.\n\n");

            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                Console.WriteLine("\nStopping ETW session...");
                cts.Cancel();
            };

            TraceEventSession session = null;
            ETWTraceEventSource source = null;

            try
            {
                session = new TraceEventSession(SessionName, null);
                session.EnableProvider(RpcProviderGuid, TraceEventLevel.Verbose, ulong.MaxValue);

                source = new ETWTraceEventSource(SessionName, TraceEventSourceType.Session);
                source.Dynamic.All += delegate (TraceEvent data)
                {
                    object interfaceUuidObj = null;
                    object procNumObj = null;
                    object endpoint = null;
                    bool foundInterfaceUuid = false;
                    bool foundProcNum = false;
                    bool foundendpoint = false;

                    for (int i = 0; i < data.PayloadNames.Length; i++)
                    {
                        if (string.Equals(data.PayloadNames[i], "InterfaceUuid", StringComparison.OrdinalIgnoreCase))
                        {
                            interfaceUuidObj = data.PayloadValue(i);
                            foundInterfaceUuid = true;
                        }
                        else if (string.Equals(data.PayloadNames[i], "ProcNum", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(data.PayloadNames[i], "ProcedureNumber", StringComparison.OrdinalIgnoreCase)) // ProcNum or ProcedureNumber
                        {
                            procNumObj = data.PayloadValue(i);
                            foundProcNum = true;
                        } else if (string.Equals(data.PayloadNames[i], "Endpoint", StringComparison.OrdinalIgnoreCase))
                        {
                            endpoint = data.PayloadValue(i);
                            foundendpoint = true;
                        }

                        if (foundInterfaceUuid && foundProcNum && foundendpoint) break;
                    }

                    // Only print if we found the relevant information
                    if (foundInterfaceUuid && foundProcNum)
                    {
                        if (interfaceUuidObj == null)
                        {
                            return;
                        }
                        var interface_uuid = interfaceUuidObj is Guid guidValue ? guidValue.ToString() : interfaceUuidObj;

                        foreach (Filter.Rule rule in rules)
                        {
                            if (Filter.Filter.EvaluateRule(rule, interface_uuid, procNumObj, endpoint))
                            {
                                if (procNumObj != null)
                                {
                                    Console.WriteLine($"[!] Rule {rule.Name} triggered by {getProcessNameFromPID(data.ProcessID)}(PID: {data.ProcessID})");
                                    for (int i = 0; i < data.PayloadNames.Length; i++)
                                    {
                                        Console.WriteLine($"\t{data.PayloadNames[i]} - {data.PayloadValue(i)}");
                                    }
                                } else
                                {
                                    Console.WriteLine($"[!] Rule {rule.Name} triggered");
                                }

                            }
                        }
                    }
                };

                Task.Run(() =>
                {
                    try
                    {
                        source.Process();
                    }
                    catch (ObjectDisposedException)
                    {
                        // Session likely stopped, ignore.
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during event processing: {ex.Message}");
                    }
                });

                cts.Token.WaitHandle.WaitOne();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
                Console.WriteLine("Make sure you are running this application as an Administrator.");
                Console.WriteLine("Also, ensure no other tools are conflicting with the ETW session name.");
                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine("Cleaning up ETW session...");
                if (source != null)
                {
                    source.StopProcessing();
                    source.Dispose();
                }
                if (session != null)
                {
                    session.Dispose();
                }
                Console.WriteLine("Session stopped and disposed. Exiting.");
            }
        }

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