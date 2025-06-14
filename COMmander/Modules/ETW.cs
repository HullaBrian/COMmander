﻿using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace COMmander.Modules
{
    class Trace
    {
        private static readonly Guid RpcProviderGuid = new Guid("{6AD52B32-D609-4BE9-AE07-CE8DAE937E39}");
        private const string SessionName = "COMmander-RPC-ETW-Session";

        public static void Run()
        {
            if (!Helpers.IsAdministrator())
            {
                Console.WriteLine("This program must be run as an administrator to collect ETW events.");
                Console.WriteLine("Please restart the application with administrative privileges.");
                return;
            }

            Console.WriteLine("[+] Loading configuration rules...");
            List<COMmander.Rule> rules = COMmander.Modules.Filter.Load();
            Console.WriteLine("[+] Loaded configuration rules!\n");

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
                    object networkaddress = null;
                    bool foundInterfaceUuid = false;
                    bool foundProcNum = false;
                    bool foundendpoint = false;
                    bool foundNetworkAddress = false;

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
                        } else if (string.Equals(data.PayloadNames[i], "NetworkAddress", StringComparison.OrdinalIgnoreCase))
                        {
                            networkaddress = data.PayloadValue(i);
                            foundNetworkAddress = true;
                        }

                        if (foundInterfaceUuid && foundProcNum && foundendpoint && foundNetworkAddress) break;
                    }

                    // Only print if we found the relevant information
                    if (foundInterfaceUuid && foundProcNum)
                    {
                        if (interfaceUuidObj == null)
                        {
                            return;
                        }
                        var interface_uuid = interfaceUuidObj is Guid guidValue ? guidValue.ToString() : interfaceUuidObj;

                        foreach (COMmander.Rule rule in rules)
                        {
                            if (COMmander.Modules.Filter.EvaluateRule(rule, interface_uuid, procNumObj, endpoint, networkaddress, data.ProcessID))
                            {
                                if (procNumObj != null)
                                {
                                    Console.WriteLine($"[!] Rule {rule.Name} triggered by {Helpers.getProcessNameFromPID(data.ProcessID)}(PID: {data.ProcessID})");
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
    }
}