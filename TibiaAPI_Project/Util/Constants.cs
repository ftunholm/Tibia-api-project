using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TibiaAPI_Project.Util
{
    public class Constants
    {
        public const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        public const int PROCESS_WM_READ = 0x0010;
        public const int PROCESS_WM_WRITE = 0x0020;
        public const int PROCESS_WM_OPERATION = 0x0008;
        public const uint MEM_COMMIT = 0x1000;
        public const uint MEM_RESERVE = 0x2000;
        public const uint MEM_RELEASE = 0x8000;
        public const int PAGE_EXECUTE_READWRITE = 0x40;
    }
}
