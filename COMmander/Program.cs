using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COMmander
{
    internal class Program
    {
        public static void printLogo()
        {
            Console.WriteLine(@"
_________  ________      _____                             .___            
\_   ___ \ \_____  \    /     \   _____ _____    ____    __| _/___________ 
/    \  \/  /   |   \  /  \ /  \ /     \\__  \  /    \  / __ |/ __ \_  __ \
\     \____/    |    \/    Y    \  Y Y  \/ __ \|   |  \/ /_/ \  ___/|  | \/
 \______  /\_______  /\____|__  /__|_|  (____  /___|  /\____ |\___  >__|   
        \/         \/         \/      \/     \/     \/      \/    \/       ");
        }
        static void Main(string[] args)
        {
            printLogo();
            COMmander.Modules.Trace.Run();
        }
    }
}
