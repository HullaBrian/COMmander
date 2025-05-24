using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace COMmanderService.Modules
{
    internal class ETW
    {
        private static readonly Guid RpcProviderGuid = new Guid("{6AD52B32-D609-4BE9-AE07-CE8DAE937E39}");
        private const string SessionName = "COMmander-RPC-ETW-Session";
        private TraceEventSession session = null;
        private ETWTraceEventSource source = null;
        private EventLog eventLog = null;

        public void openSession()
        {
            session = new TraceEventSession(SessionName, null);
            session.EnableProvider(RpcProviderGuid, TraceEventLevel.Verbose, ulong.MaxValue);
            source = new ETWTraceEventSource(SessionName, TraceEventSourceType.Session);
        }
        public void closeSession()
        {
            if (source != null)
            {
                source.StopProcessing();
                source.Dispose();
            }
            if (session != null)
            {
                session.Dispose();
            }
            if (eventLog != null)
            {
                eventLog.Dispose();
            }
        }
        public void Monitor(List<Rule> rules)
        {
            try
            {
                eventLog = new EventLog();
                eventLog.Source = "COMmander";

                var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };

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
                        }
                        else if (string.Equals(data.PayloadNames[i], "Endpoint", StringComparison.OrdinalIgnoreCase))
                        {
                            endpoint = data.PayloadValue(i);
                            foundendpoint = true;
                        }
                        else if (string.Equals(data.PayloadNames[i], "NetworkAddress", StringComparison.OrdinalIgnoreCase))
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

                        foreach (Rule rule in rules)
                        {
                            if (Modules.Filter.EvaluateRule(rule, interface_uuid, procNumObj, endpoint, networkaddress, data.ProcessID))
                            {
                                if (procNumObj != null)
                                {
                                    // Console.WriteLine($"[!] Rule {rule.Name} triggered by {Modules.Helpers.GetProcessNameFromPID(data.ProcessID)}(PID: {data.ProcessID})");
                                    eventLog.WriteEntry($"Rule '{rule.Name}' triggered", EventLogEntryType.Warning, 5, 1);
                                    //for (int i = 0; i < data.PayloadNames.Length; i++)
                                    //{
                                    //    Console.WriteLine($"\t{data.PayloadNames[i]} - {data.PayloadValue(i)}");
                                    //}
                                }
                                else
                                {
                                    //Console.WriteLine($"[!] Rule {rule.Name} triggered");
                                    eventLog.WriteEntry($"Rule '{rule.Name}' triggered", EventLogEntryType.Warning, 5, 1);
                                }

                            }
                        }
                    }
                };
                try
                {
                    source.Process();
                }
                catch (ObjectDisposedException)
                {
                    // Session likely stopped, ignore.
                };

                cts.Token.WaitHandle.WaitOne();
            }
            catch (Exception ex)
            {
                eventLog.WriteEntry($"Error during event processing: {ex}", EventLogEntryType.Error, 4, 1);
            }
            closeSession();
        }
    }
}
