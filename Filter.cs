using System;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace Filter
{
    internal class Filter
    {
        private const string ElementNotPresentValue = "ELEMENT_NOT_PRESENT";

        public static List<Rule> Load()
        {
            String config_file_path = "config.xml";
            List<Rule> rules = new List<Rule>();

            if (!File.Exists(config_file_path))
            {
                Console.WriteLine($"[-] Configuration file '{config_file_path}' not found.");
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
                Console.WriteLine($"[-] Error parsing XML file '{config_file_path}': {ex.Message}");
                Console.ReadKey();
                return rules;
            }

            IEnumerable<XElement> xml_rules = config.Descendants("Rule");
            if (!xml_rules.Any())
            {
                Console.WriteLine("[-] No <Rule> elements found in the XML file.");
                Console.ReadKey();
                return rules;
            }

            Console.WriteLine($"[+] Loading rules from '{config_file_path}'...");
            foreach (var ruleElement in xml_rules)
            {
                string ruleName = ruleElement.Attribute("name")?.Value ?? "N/A";

                XElement interfaceUuidElement = ruleElement.Element("InterfaceUUID");
                string interfaceUuid = interfaceUuidElement == null ? ElementNotPresentValue : interfaceUuidElement.Value;

                XElement opNumElement = ruleElement.Element("OpNum");
                string opNum = opNumElement == null ? ElementNotPresentValue : opNumElement.Value;

                XElement endpointElement = ruleElement.Element("Endpoint");
                string endpoint = endpointElement == null ? ElementNotPresentValue : endpointElement.Value ;

                XElement networkAddress = ruleElement.Element("NetworkAddress");
                string networkaddress = networkAddress == null ? ElementNotPresentValue : networkAddress.Value;

                rules.Add(new Rule(ruleName, interfaceUuid, opNum, endpoint, networkaddress));
                Console.WriteLine($"[+] Registered rule '{ruleName}'");
            }

            Console.WriteLine("[+] All rules loaded.");
            return rules;
        }
        public static bool EvaluateRule(Rule rule, object eventInterfaceUuid, object eventOpNum, object eventEndpoint, object eventNetworkAddress)
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

            return interfaceMatch && opNumMatch && endpointMatch && networkAddressMatch;
        }
    }
}
