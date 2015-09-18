using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TibiaAPI_Project.Util;
using TibiaAPI_Project.Addresses;

namespace TibiaAPI_Project.Player
{
    public class Vitals
    {

        public static int GetMaxHitpoint() {
            int maxHp = MemoryHelper.ReadMemoryInt32(Addresses.Addresses.maxHp);
            int xor = MemoryHelper.ReadMemoryInt32(Addresses.Addresses.xor);

            return maxHp^xor;
        }

        public static int GetHitpoint()
        {
            int hp = MemoryHelper.ReadMemoryInt32(Addresses.Addresses.currentHp);
            int xor = MemoryHelper.ReadMemoryInt32(Addresses.Addresses.xor);

            return hp ^ xor;
        }

    }
}
