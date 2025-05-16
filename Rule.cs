using System;

namespace Filter
{
    class Rule
    {
        public String Name, InterfaceUUID, OpNum, Endpoint;

        public Rule(String Name, String InterfaceUUID, String OpNum, String Endpoint)
        {
            this.Name = Name;
            this.InterfaceUUID = InterfaceUUID;
            this.OpNum = OpNum;
            this.Endpoint = Endpoint;
        }

    }
}
