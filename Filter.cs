using System;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace Filter
{
    internal class Filter
    {
        public static List<Rule> Load()
        {
            String config_file_path = "config.xml";

            List<Rule> rules = new List<Rule>();
            XDocument config = XDocument.Load(config_file_path);

            IEnumerable<XElement> xml_rules = config.Descendants("Rule");
            if (!xml_rules.Any())
            {
                Console.WriteLine("[-] No <Rule> elements found in the XML file.");
                Console.ReadKey();
                return rules;
            }

            foreach (var RuleElement in xml_rules)
            {
                string ruleId = RuleElement.Attribute("id")?.Value ?? "-1";
                string ruleName = RuleElement.Attribute("name")?.Value ?? "N/A";
                string interfaceUuid = RuleElement.Element("InterfaceUUID")?.Value;
                string opNum = RuleElement.Element("OpNum")?.Value;
                string endpoint = RuleElement.Element("Endpoint")?.Value;

                //Console.WriteLine($"Rule ID: {ruleId}, Name: {ruleName}");
                //Console.WriteLine($"  InterfaceUUID: {interfaceUuid ?? "Not found"}");
                //Console.WriteLine($"  OpNum: {opNum ?? "Not found"}");
                //Console.WriteLine($"  Endpoint: {endpoint ?? "Not found (or empty)"}");
                //Console.WriteLine("-----------------------------------");
                rules.Add(new Rule(ruleId, ruleName, interfaceUuid, opNum, endpoint));
                Console.WriteLine($"[+] Registered new rule '{ruleName}' with id {ruleId}");
            }

            return rules;
        }
    }
}
