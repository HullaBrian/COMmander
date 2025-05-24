using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace COMmanderService.Modules
{
    internal class Filter
    {
        private const string ElementNotPresentValue = "ELEMENT_NOT_PRESENT";

        public static List<Rule> Load()
        {
            String config_file_path = "C:\\Program Files\\COMmander\\config.xml";
            List<Rule> rules = new List<Rule>();

            if (!File.Exists(config_file_path))
            {
                EventLog.WriteEntry("COMmander", $"Configuration file '{config_file_path}' not found.", System.Diagnostics.EventLogEntryType.Error, 4, 1);
                Console.ReadKey();
                return rules;
            }

            XDocument config;
            try
            {
                config = XDocument.Load(config_file_path);
            }
            catch (System.Xml.XmlException ex)
            {
                EventLog.WriteEntry("COMmander", $"Error parsing XML file '{config_file_path}': {ex.Message}", System.Diagnostics.EventLogEntryType.Error, 4, 1);
                Console.ReadKey();
                return rules;
            }

            IEnumerable<XElement> xml_rules = config.Descendants("Rule");
            if (!xml_rules.Any())
            {
                EventLog.WriteEntry("COMmander", "No Rule elements found in the XML file.", System.Diagnostics.EventLogEntryType.Information, 4, 1);
                Console.ReadKey();
                return rules;
            }

            foreach (var ruleElement in xml_rules)
            {
                string ruleName = ruleElement.Attribute("name")?.Value ?? "N/A";

                XElement interfaceUuidElement = ruleElement.Element("InterfaceUUID");
                string interfaceUuidStr = interfaceUuidElement == null ? ElementNotPresentValue : interfaceUuidElement.Value;

                XElement opNumElement = ruleElement.Element("OpNum");
                string opNumStr = opNumElement == null ? ElementNotPresentValue : opNumElement.Value;

                XElement endpointElement = ruleElement.Element("Endpoint");
                string endpointStr = endpointElement == null ? ElementNotPresentValue : endpointElement.Value;

                XElement networkAddress = ruleElement.Element("NetworkAddress");
                string networkAddressStr = networkAddress == null ? ElementNotPresentValue : networkAddress.Value;

                XElement processName = ruleElement.Element("ProcessName");
                string processNameStr = processName == null ? ElementNotPresentValue : processName.Value;

                rules.Add(new Rule(ruleName, interfaceUuidStr, opNumStr, endpointStr, networkAddressStr, processNameStr));
                EventLog.WriteEntry("COMmander", $"Registered rule '{ruleName}'", System.Diagnostics.EventLogEntryType.Information, 3, 1);
            }

            return rules;
        }
        public static bool EvaluateRule(Rule rule, object eventInterfaceUuid, object eventOpNum, object eventEndpoint, object eventNetworkAddress, int processID)
        {
            string eventInterfaceUuidStr = Convert.ToString(eventInterfaceUuid);
            string eventOpNumStr = Convert.ToString(eventOpNum);
            string eventEndpointStr = Convert.ToString(eventEndpoint);
            string eventNetworkAddressStr = Convert.ToString(eventNetworkAddress);

            bool interfaceMatch = rule.InterfaceUUID == ElementNotPresentValue ||
                                  string.Equals(rule.InterfaceUUID, eventInterfaceUuidStr, StringComparison.OrdinalIgnoreCase);
            bool opNumMatch = rule.OpNum == ElementNotPresentValue ||
                              string.Equals(rule.OpNum, eventOpNumStr, StringComparison.OrdinalIgnoreCase);
            bool endpointMatch = rule.Endpoint == ElementNotPresentValue ||
                                 string.Equals(rule.Endpoint, eventEndpointStr, StringComparison.OrdinalIgnoreCase);
            bool networkAddressMatch = rule.NetworkAddress == ElementNotPresentValue || string.Equals(rule.NetworkAddress, eventNetworkAddressStr, StringComparison.OrdinalIgnoreCase);
            bool staticMatches = interfaceMatch && opNumMatch && endpointMatch && networkAddressMatch;

            if (staticMatches && rule.ProcessName != ElementNotPresentValue)  // Only calculate process name during rule evaluation if necessary
            {
                string ProcessName = COMmanderService.Modules.Helpers.GetProcessNameFromPID(processID);
                return string.Equals(rule.ProcessName, ProcessName, StringComparison.OrdinalIgnoreCase);
            }

            return staticMatches;
        }
    }
}
