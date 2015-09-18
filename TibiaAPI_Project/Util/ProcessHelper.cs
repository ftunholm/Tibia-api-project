using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TibiaAPI_Project.Util
{
    public class ProcessHelper
    {
        public static int GetProcessId()
        {
            return System.Diagnostics.Process.GetProcessesByName("Tibia").FirstOrDefault().Id;
        }
        public static Process GetProcess()
        {
            return System.Diagnostics.Process.GetProcessesByName("Tibia").FirstOrDefault();
        }
    }
}
