using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMmanderService
{
    class Rule
    {
        public String Name, InterfaceUUID, OpNum, Endpoint, NetworkAddress, ProcessName;

        public Rule(String Name, String InterfaceUUID, String OpNum, String Endpoint, String NetworkAddress, String ProcessName)
        {
            this.Name = Name;
            this.InterfaceUUID = InterfaceUUID;
            this.OpNum = OpNum;
            this.Endpoint = Endpoint;
            this.NetworkAddress = NetworkAddress;
            this.ProcessName = ProcessName;
        }
    }
}
