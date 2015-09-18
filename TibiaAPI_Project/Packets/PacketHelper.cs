using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TibiaAPI_Project.Util;

namespace TibiaAPI_Project.Packets
{
    public class PacketHelper
    {
        IntPtr pSender;
        bool sendCodeWritten = false;

        public bool WriteSocketSendCode()
        {
            IntPtr processHandle = MemoryHelper.OpenProcess(Constants.PROCESS_ALL_ACCESS, false, ProcessHelper.GetProcessId());
            UIntPtr bytesWritten;

            byte[] OpCodes = new byte[]{
	                		0x6A, 0x00,							//push	0						;_flag
			                0xFF, 0x33,							//push	dword ptr [ebx]			;_length
			                0x83, 0xC3, 0x04,					//add	ebx, 4
			                0x53,								//push	ebx						;_buffer
			                0xA1, 0xFF, 0xFF, 0xFF, 0xFF,		//mov	eax, ds:SocketStruct	;_socketstruct
			                0xFF, 0x70, 0x04,					//push	dword ptr [eax+4]		;_socket
			                0xFF, 0x15, 0xFF, 0xFF, 0xFF, 0xFF,	//call	dword ptr ds:Send		;call send
			                0xC3								//retn
		        };

            Array.Copy(BitConverter.GetBytes(Addresses.Addresses.socketStruct), 0, OpCodes, 9, 4);
            Array.Copy(BitConverter.GetBytes(Addresses.Addresses.sendPointer), 0, OpCodes, 18, 4);

            //Allocate memory for writting the bytes
            pSender = MemoryHelper.VirtualAllocEx(processHandle, IntPtr.Zero, (uint)OpCodes.Length,
                Constants.MEM_COMMIT | Constants.MEM_RESERVE, Constants.PAGE_EXECUTE_READWRITE);
            if (pSender != IntPtr.Zero)
            {
                //Write the bytes to memory
                if (MemoryHelper.WriteProcessMemory(processHandle, pSender, OpCodes, (uint)OpCodes.Length, out bytesWritten))
                {
                    sendCodeWritten = true;
                    return true;
                }
                //Free the space in memory
                MemoryHelper.VirtualFreeEx(processHandle, pSender, (UIntPtr)0, Constants.MEM_RELEASE);
                pSender = IntPtr.Zero;
            }
            sendCodeWritten = false;
            return false;
        }
    }
}
