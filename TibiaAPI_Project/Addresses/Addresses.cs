using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TibiaAPI_Project.Util;

namespace TibiaAPI_Project.Addresses
{
    public class Addresses
    {


        public Addresses()
        {
            baseAddress = TibiaAPI_Project.Util.ProcessHelper.GetProcess().MainModule.BaseAddress.ToInt32();

            xteaKey = 0x51EBA4 + baseAddress;
            socketStruct = 0x000000 + baseAddress;
            sendPointer = 0x422A58 + baseAddress;
             
            xor = 0x534678 + baseAddress;
            maxHp = 0x6D2024 + baseAddress;
            currentHp = 0x6D2030 + baseAddress;
            maxMana = 0x53467C + baseAddress;
            currentMana = 0x5346A8 + baseAddress;
        }  

        private static Int32 baseAddress;

        public static Int32 xteaKey;
        public static Int32 socketStruct;
        public static Int32 sendPointer;

        public static Int32 xor;
        public static Int32 maxHp;
        public static Int32 currentHp;
        public static Int32 maxMana;
        public static Int32 currentMana;
    }
}
