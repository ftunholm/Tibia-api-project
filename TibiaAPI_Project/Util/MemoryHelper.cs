using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TibiaAPI_Project.Util
{
    public class MemoryHelper
    {

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(int hProcess,
          int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
            uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess,
            IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);


        public static Int32 ReadMemoryInt32(Int32 address)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[4];

            IntPtr handle = OpenProcess(Constants.PROCESS_WM_READ, false, ProcessHelper.GetProcessId());
            ReadProcessMemory((int)handle, address, buffer, buffer.Length, ref bytesRead);

            return BitConverter.ToInt32(buffer, 0);
        }

        public static byte[] ReadMemoryByteArray(Int32 address)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[4];

            IntPtr handle = OpenProcess(Constants.PROCESS_WM_READ, false, ProcessHelper.GetProcessId());
            ReadProcessMemory((int)handle, address, buffer, buffer.Length, ref bytesRead);

            return buffer;
        }
        public static IntPtr GetProcessHandle()
        {
            return OpenProcess(Constants.PROCESS_ALL_ACCESS, false, ProcessHelper.GetProcessId());
        }

    }


}
