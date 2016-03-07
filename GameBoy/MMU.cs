using System;
using System.IO;
using System.Text;

namespace GameBoy
{
    internal class MMU
    {
        private byte[] Rom, Ram, VideoRam, ExternalRam, ZeroRam;
        private byte interrupt_flags;

        public string title;
        public bool color, superGameBoyEnabled, forJapan;
        public byte cartridgeType, cartSize, ramSize; //Todo: Change these to enum? Split out cartridge information?

        internal MMU(string filename)
        {
            // Setup RAM
            Ram = new byte[0x2000];
            VideoRam = new byte[0x2000];
            ExternalRam = null;
            ZeroRam = new byte[128];

            // Load ROM & ROM infpo
            Rom = File.ReadAllBytes(filename);

            byte[] temp = new byte[15];
            Array.Copy(Rom, 0x0134, temp, 0, 15);
            title = Encoding.ASCII.GetString(temp).Replace("\0", "");

            color = Rom[0x0143] == 0x80;
            superGameBoyEnabled = Rom[0x0146] == 0x03;
            cartridgeType = Rom[0x0147];
            cartSize = Rom[0x0148];
            ramSize = Rom[0x0149];
            forJapan = Rom[0x14A] == 0x0;

            // Setup special registers
            interrupt_flags = 0;
        }

        public string getViewableRom()
        {
            string romFormatted = "";

            for (int i = 0; i <= Rom.Length; i += 16)
            {
                int segLen = Math.Min(16, Rom.Length - i);

                string str = "";
                str += i.ToString("D4") + "   ";
                for (int j = 0; j < segLen; j++)
                    str += Rom[i + j].ToString("X2") + " ";
                str += "   ";
                byte[] tmp = new byte[segLen];
                Array.Copy(Rom, i, tmp, 0, segLen);
                
                for (int k = 0; k < segLen; k++)
                {
                    byte b = Rom[i + k];
                    if (b >= 32 && b <= 126)
                    {
                        str += (char)b;
                    }
                    else
                    {
                        str += ".";
                    }
                }

                str += Environment.NewLine;

                romFormatted += str;
            }

            return romFormatted;
        }

        internal byte ReadByte(ushort address)
        {
            switch(address & 0xF000)
            {
                // Rom0
                case 0x0000:
                case 0x1000:
                case 0x2000:
                case 0x3000:
                    return Rom[address];

                // Switchable ROM bank
                case 0x4000:
                case 0x5000:
                case 0x6000:
                case 0x7000:
                    // No banking
                    return Rom[address];

                // Video ram
                case 0x8000:
                case 0x9000:
                    return VideoRam[address - 0x8000];

                // Switchable RAM bank
                case 0xA000:
                case 0xB000:
                    return ExternalRam[address - 0xA000];

                // Internal RAM
                case 0xC000:
                case 0xD000:
                    return Ram[address - 0xC000];

                // Internal RAM Mirror
                case 0xE000:
                    return Ram[address - 0xE000];

                case 0xF000:
                    switch (address & 0x0F00)
                    {
                        // Internal RAM mirror
                        case 0x0000: case 0x0100: case 0x0200: case 0x0300:
                        case 0x0400: case 0x0500: case 0x0600: case 0x0700:
                        case 0x0800: case 0x0900: case 0x0A00: case 0x0B00:
                        case 0x0C00: case 0x0D00:
                            return Ram[address - 0xE000];

                        // Sprite Attribute Memory (OAM)
                        case 0x0E00:
                            throw new NotImplementedException("Sprite Attribe Memory (OAM)");

                        case 0x0F00:
                            // I/O Ports
                            if (address < 0xFF80)
                            {
                                throw new NotImplementedException("I/O Ports");
                            }
                            // Internal/Zerop Page
                            else
                            {
                                return ZeroRam[address - 0xFF80];
                            }

                        default:
                            throw new NotImplementedException("CRITICAL READ ERROR?");
                    }

                default:
                    throw new NotImplementedException("CRITICAL READ ERROR?");
            }
        }

        internal void WriteByte(ushort address, byte data)
        {
            switch (address & 0xF000)
            {
                // Rom0
                case 0x0000:
                case 0x1000:
                case 0x2000:
                case 0x3000:
                    Rom[address] = data;
                    break;

                // Switchable ROM bank
                case 0x4000:
                case 0x5000:
                case 0x6000:
                case 0x7000:
                    // No banking
                    Rom[address] = data;
                    break;

                // Video ram
                case 0x8000:
                case 0x9000:
                    VideoRam[address - 0x8000] = data;
                    break;

                // Switchable RAM bank
                case 0xA000:
                case 0xB000:
                    ExternalRam[address - 0xA000] = data;
                    break;

                // Internal RAM
                case 0xC000:
                case 0xD000:
                    Ram[address - 0xC000] = data;
                    break;

                // Internal RAM Mirror
                case 0xE000:
                    Ram[address - 0xE000] = data;
                    break;

                case 0xF000:
                    switch (address & 0x0F00)
                    {
                        // Internal RAM mirror
                        case 0x0000: case 0x0100: case 0x0200: case 0x0300:
                        case 0x0400: case 0x0500: case 0x0600: case 0x0700:
                        case 0x0800: case 0x0900: case 0x0A00: case 0x0B00:
                        case 0x0C00: case 0x0D00:
                            Ram[address - 0xE000] = data;
                            break;

                        // Sprite Attribute Memory (OAM)
                        case 0x0E00:
                            throw new NotImplementedException("Sprite Attribe Memory (OAM)");

                        case 0x0F00:
                            // I/O Ports
                            if (address < 0xFF80)
                            {
                                if (address == 0xFF05) break; // TODO - Timer Counter
                                if (address == 0xFF06) break; // TODO - Timer Modulo
                                if (address == 0xFF07) break; // TODO - Timer Control
                                if (address == 0xFF0F) interrupt_flags = data; break;
                                
                                throw new NotImplementedException("I/O Ports");
                            }
                            // Internal/Zero Page
                            else
                            {
                                ZeroRam[address - 0xFF80] = data;
                                break;
                            }

                        default:
                            throw new NotImplementedException("CRITICAL READ ERROR?");
                    }
                    break;

                default:
                    throw new NotImplementedException("CRITICAL READ ERROR?");
            }
        }

        internal ushort ReadWord(ushort address)
        {
            return (ushort)(ReadByte(address) << 8 | ReadByte(++address));
        }

        internal void WriteWord(ushort address, ushort data)
        {
            WriteByte(address, (byte)(data >> 8));
            WriteByte(++address, (byte)data);
        }
    }
}
