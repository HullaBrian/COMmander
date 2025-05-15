using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                Console.ReadKey();
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
                Console.WriteLine("Stopping ETW session...");
                cts.Cancel();
            };

            TraceEventSession session = null;
            ETWTraceEventSource source = null;

            try
            {
                session = new TraceEventSession(SessionName, null);
                session.EnableProvider(RpcProviderGuid, TraceEventLevel.Verbose, ulong.MaxValue);

                source = new ETWTraceEventSource(SessionName, TraceEventSourceType.Session);

                // Subscribe to specific events or all events and filter
                // RpcClientCallStart (Event ID 5) and RpcServerCallStart (Event ID 6) usually have this info.
                source.Dynamic.All += delegate (TraceEvent data)
                {
                    // We are interested in events that contain Interface UUID and ProcNum (Opnum)
                    // These are typically RpcClientCallStart (ID 5) and RpcServerCallStart (ID 6)
                    // Let's check if the payload names exist before trying to access them.

                    object interfaceUuidObj = null;
                    object procNumObj = null;
                    bool foundInterfaceUuid = false;
                    bool foundProcNum = false;

                    // Try to get payload data by common names
                    // The actual names can sometimes vary slightly or depend on the event version.
                    // Common names: "InterfaceUuid", "ProcNum"
                    // You might need to inspect raw event data if these don't work for all desired events.

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
                        }

                        if (foundInterfaceUuid && foundProcNum) break;
                    }

                    // Only print if we found the relevant information
                    if (foundInterfaceUuid && foundProcNum)
                    {
                        foreach (Filter.Rule rule in rules)
                        {
                            if (string.Equals(rule.InterfaceUUID, interfaceUuidObj) && string.Equals(rule.OpNum, procNumObj))
                            {
                                Console.WriteLine($"[!] Alert '{rule.Name}' with id '{rule.ID}' detected!");
                            }
                        }
                        //Console.WriteLine("--------------------------------------------------");
                        //Console.WriteLine($"EVENT: {data.ProviderName}/{data.EventName} (ID: {data.ID})");
                        //Console.WriteLine($"Timestamp: {data.TimeStamp.ToLocalTime()}");
                        //Console.WriteLine($"Process: {data.ProcessName} (PID: {data.ProcessID})"); // This is the Process ID

                        //if (interfaceUuidObj != null)
                        //{
                        //    // InterfaceUUID is typically a Guid
                        //    if (interfaceUuidObj is Guid guidValue)
                        //    {
                        //        Console.WriteLine($"Interface UUID: {guidValue}");
                        //    }
                        //    else
                        //    {
                        //        Console.WriteLine($"Interface UUID: {interfaceUuidObj}"); // Print as string if not Guid
                        //    }
                        //}

                        //if (procNumObj != null)
                        //{
                        //    // ProcNum is typically an integer (ushort or uint)
                        //    Console.WriteLine($"Opnum: {procNumObj}");
                        //}
                        // Console.WriteLine("--------------------------------------------------\n");
                    }
                    // Optional: You can log other RPC events too, or filter specifically for Event ID 5 or 6
                    // else if (data.ID == 5 || data.ID == 6) // If specifically targeting these events
                    // {
                    //     Console.WriteLine($"DEBUG: Event {data.ID} ({data.EventName}) from PID {data.ProcessID} did not have expected InterfaceUuid/ProcNum fields or they were null.");
                    //     // You could print all payload names here to help debug:
                    //     // for (int i = 0; i < data.PayloadNames.Length; i++)
                    //     // {
                    //     //    Console.WriteLine($"  Available Field: {data.PayloadNames[i]} = {data.PayloadString(i)}");
                    //     // }
                    // }
                };

                Task.Run(() =>
                {
                    try
                    {
                        source.Process();
                    }
                    catch (ObjectDisposedException)
                    {
                        // Session was likely stopped, ignore.
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
    }
}