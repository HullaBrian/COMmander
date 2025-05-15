using System;

namespace Filter
{
    class Rule
    {
        public String ID, Name, InterfaceUUID, OpNum, Endpoint;

        public Rule(String ID, String Name, String InterfaceUUID, String OpNum, String Endpoint)
        {
            this.ID = ID;
            this.Name = Name;
            this.InterfaceUUID = InterfaceUUID;
            this.OpNum = OpNum;
            this.Endpoint = Endpoint;
        }

    }
}
