using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TibiaAPI_Project.Util;
using TibiaAPI_Project.Addresses;

namespace TibiaAPI_Project.Packets
{
    public class Packet
    {

        // Send a packet through the client by writing some code in memory and running it.
        // The packet must not contain any header(no length nor Adler checksum) and be unencrypted
        public static bool SendPacketByMemory(Byte[] packet)
        {
            UIntPtr bytesWritten;
            IntPtr processHandle = MemoryHelper.GetProcessHandle();

            byte[] packet_ = new byte[packet.Length + 2];
            Array.Copy(BitConverter.GetBytes(packet.Length), packet_, 2);
            Array.Copy(packet, 0, packet_, 2, packet.Length);

            byte[] encPacket = XTEA.Encrypt(packet_, MemoryHelper.ReadMemoryByteArray(Addresses.Addresses.xteaKey), true);
            uint pSize = (uint)(encPacket.Length + 4);
            byte[] readyPacket = new byte[pSize];
            Array.Copy(BitConverter.GetBytes(encPacket.Length), readyPacket, 4);
            Array.Copy(encPacket, 0, readyPacket, 4, encPacket.Length);


            /*
            NetworkMessage msg = new NetworkMessage(client);
            msg.AddBytes(packet);
            msg.PrepareToSend();
	
            byte[] readyPacket = msg.Packet;
            uint bufferSize=(uint)(4+readyPacket.Length);
            byte[] buffer = new byte[bufferSize];
            Array.Copy(BitConverter.GetBytes(bufferSize), buffer, 4);
            Array.Copy(readyPacket, 0, buffer, 4, readyPacket.Length);
             */

            IntPtr pRemote = MemoryHelper.VirtualAllocEx(processHandle, IntPtr.Zero, pSize,
                                        Constants.MEM_COMMIT | Constants.MEM_RESERVE,
                                        Constants.PAGE_EXECUTE_READWRITE);

            if (pRemote != IntPtr.Zero)
            {
                if (MemoryHelper.WriteProcessMemory(processHandle, (IntPtr)pRemote.ToInt64(), readyPacket, pSize, out bytesWritten))
                {
                    IntPtr threadHandle = MemoryHelper.CreateRemoteThread(processHandle, IntPtr.Zero, 0,
                        (IntPtr)Addresses.Addresses.sendPointer, pRemote, 0, IntPtr.Zero);

                    MemoryHelper.WaitForSingleObject(threadHandle, 0xFFFFFFFF);//INFINITE=0xFFFFFFFF
                    MemoryHelper.CloseHandle(threadHandle);
                    return true;
                }
            }
            return false;
        }
    }


    public static class XTEA
    {
        public static byte[] AddAdlerChecksum(byte[] packet)
        {
            byte[] packet_WithCRC = new byte[packet.Length + 4];
            byte[] packet_WithoutHeader = new byte[packet.Length - 2];
            AdlerChecksum acs = new AdlerChecksum();
            Array.Copy(packet, 2, packet_WithoutHeader, 0, packet_WithoutHeader.Length);
            packet_WithCRC[0] = BitConverter.GetBytes((ushort)(packet.Length + 2))[0];
            packet_WithCRC[1] = BitConverter.GetBytes((ushort)(packet.Length + 2))[1];
            if (acs.MakeForBuff(packet_WithoutHeader))
            {
                Array.Copy(BitConverter.GetBytes(acs.ChecksumValue), 0, packet_WithCRC, 2, 4);
                Array.Copy(packet_WithoutHeader, 0, packet_WithCRC, 6, packet_WithoutHeader.Length);
                return packet_WithCRC;
            }
            else
                return null;
        }

        // Encrypt a packet using XTEA.
        public static byte[] Encrypt(byte[] packet, byte[] key, bool addAdler)
        {
            if (packet.Length == 0)
                return packet;

            uint[] keyprep = key.ToUintArray();

            // Pad the packet with extra bytes for encryption
            int pad = packet.Length % 8;

            byte[] packetprep;

            if (pad == 0)
                packetprep = new byte[packet.Length];
            else
                packetprep = new byte[packet.Length + (8 - pad)];

            Array.Copy(packet, packetprep, packet.Length);

            uint[] payloadprep = packetprep.ToUintArray();

            for (int i = 0; i < payloadprep.Length; i += 2)
            {
                Encode(payloadprep, i, keyprep);
            }

            byte[] encrypted = new byte[packetprep.Length + 2];

            Array.Copy(payloadprep.ToByteArray(), 0, encrypted, 2, packetprep.Length);

            Array.Copy(BitConverter.GetBytes((short)packetprep.Length), 0, encrypted, 0, 2);

            if (addAdler)
            {

                byte[] encrypted_ready = new byte[encrypted.Length + 4];
                Array.Copy(AddAdlerChecksum(encrypted), 0, encrypted_ready, 0, encrypted_ready.Length);
                return encrypted_ready;
            }
            else
                return encrypted;
        }

        public static void Encode(uint[] v, int index, uint[] k)
        {
            uint y = v[index];
            uint z = v[index + 1];
            uint sum = 0;
            uint delta = 0x9e3779b9;
            uint n = 32;

            while (n-- > 0)
            {
                y += (z << 4 ^ z >> 5) + z ^ sum + k[sum & 3];
                sum += delta;
                z += (y << 4 ^ y >> 5) + y ^ sum + k[sum >> 11 & 3];
            }

            v[index] = y;
            v[index + 1] = z;
        }

        private static uint[] ToUintArray(this byte[] bytes)
        {
            uint[] uints = new uint[bytes.Length / 4];

            for (int i = 0; i < uints.Length; i++)
            {
                uints[i] = BitConverter.ToUInt32(bytes, i * 4);
            }

            return uints;
        }

        public static byte[] ToByteArray(this uint[] uints)
        {
            byte[] bytes = new byte[uints.Length * 4];

            for (int i = 0; i < uints.Length; i++)
            {
                Array.Copy(BitConverter.GetBytes(uints[i]), 0, bytes, i * 4, 4);
            }

            return bytes;
        }
    }

    public class AdlerChecksum
    {
        // AdlerBase is Adler-32 checksum algorithm parameter.
        public const uint AdlerBase = 0xFFF1;

        // AdlerStart is Adler-32 checksum algorithm parameter.
        public const uint AdlerStart = 0x0001;

        // AdlerBuff is Adler-32 checksum algorithm parameter.
        public const uint AdlerBuff = 0x0400;

        // Adler-32 checksum value
        private uint m_unChecksumValue = 0;


        // ChecksumValue is property which enables the user
        // to get Adler-32 checksum value for the last calculation
        public uint ChecksumValue
        {
            get
            {
                return m_unChecksumValue;
            }
        }
        // Calculate Adler-32 checksum for buffer
        // Returns true if the checksum values is successfully calculated
        public bool MakeForBuff(byte[] bytesBuff, uint unAdlerCheckSum)
        {
            if (Object.Equals(bytesBuff, null))
            {
                m_unChecksumValue = 0;
                return false;
            }
            int nSize = bytesBuff.GetLength(0);
            if (nSize == 0)
            {
                m_unChecksumValue = 0;
                return false;
            }
            uint unSum1 = unAdlerCheckSum & 0xFFFF;
            uint unSum2 = (unAdlerCheckSum >> 16) & 0xFFFF;
            for (int i = 0; i < nSize; i++)
            {
                unSum1 = (unSum1 + bytesBuff[i]) % AdlerBase;
                unSum2 = (unSum1 + unSum2) % AdlerBase;
            }
            m_unChecksumValue = (unSum2 << 16) + unSum1;
            return true;
        }

        // Calculate Adler-32 checksum for buffer
        // Returns true if the checksum values is successfully calculated
        public bool MakeForBuff(byte[] bytesBuff)
        {
            return MakeForBuff(bytesBuff, AdlerStart);
        }

        // Equals determines whether two files (buffers)
        // have the same checksum value (identical).
        // Returns true if the value of checksum is the same
        // as this instance; otherwise, false
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType())
                return false;
            AdlerChecksum other = (AdlerChecksum)obj;
            return (this.ChecksumValue == other.ChecksumValue);
        }

        // operator== determines whether AdlerChecksum objects are equal.
        // Returns true if the values of its operands are equal
        public static bool operator ==(AdlerChecksum objA, AdlerChecksum objB)
        {
            if (Object.Equals(objA, null) && Object.Equals(objB, null)) return true;
            if (Object.Equals(objA, null) || Object.Equals(objB, null)) return false;
            return objA.Equals(objB);
        }

        // operator!= determines whether AdlerChecksum objects are not equal.
        // Returns true if the values of its operands are not equal
        public static bool operator !=(AdlerChecksum objA, AdlerChecksum objB)
        {
            return !(objA == objB);
        }

        // GetHashCode returns hash code for this instance.
        // hash code of AdlerChecksum
        public override int GetHashCode()
        {
            return ChecksumValue.GetHashCode();
        }

        // ToString is a method for current AdlerChecksum object
        // representation in textual form.
        // Returns current checksum or
        // or "Unknown" if checksum value is unavailable
        public override string ToString()
        {
            if (ChecksumValue != 0)
                return ChecksumValue.ToString();
            return "Unknown";
        }

    }
}

