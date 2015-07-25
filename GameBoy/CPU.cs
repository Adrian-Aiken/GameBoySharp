using System;

namespace GameBoy
{
    internal class CPU
    {
        // External CPU Info
        internal int cycles;
        public bool halted;
        public bool interrupts;

        private MMU mmu;
        [Flags] enum Flags { Zero = 0x80, Subtract = -0x40, HalfCarry = 0x20, Carry = 0x10 }

        // Registers
        private Flags f;
        private byte b, c, d, e, h, l, a;
        private ushort sp, pc;
        private byte diTimer, eiTimer;

        // Combination registers
        private ushort bc
        {
            get { return (ushort)(b << 8 | c); }
            set { byte[] v = BitConverter.GetBytes(value); b = v[0]; c = v[1]; }
        }
        private ushort de
        {
            get { return (ushort)(d << 8 | e); }
            set { byte[] v = BitConverter.GetBytes(value); d = v[0]; e = v[1]; }
        }
        private ushort hl
        {
            get { return (ushort)(h << 8 | l); }
            set { byte[] v = BitConverter.GetBytes(value); h = v[0]; l = v[1]; }
        }
        private ushort af
        {
            get { return (ushort)(a << 8 | (byte)f); }
            set { byte[] v = BitConverter.GetBytes(value); a = v[0]; f = (Flags)v[1]; }
        }

        // Misc & temp
        byte t;
        ushort utmp;

        /// <summary>
        /// Constructor for the CPU. Sets up the CPU.
        /// </summary>
        /// <param name="mmu">Memory Management Unit</param>
        public CPU(MMU mmu)
        {
            this.mmu = mmu;
            Reset();
        }

        /// <summary>
        /// Resets the state of the CPU
        /// </summary>
        public void Reset()
        {
            b = 0; c = 0; d = 0; e = 0;
            h = 0; l = 0; a = 0; f = 0;
            pc = 0x0100;
            sp = 0xFFFE;
            cycles = 0;
            halted = false;
            interrupts = true;
            diTimer = 0; eiTimer = 0;
        }

        /// <summary>
        /// Executes the next CPU instruction
        /// </summary>
        public void Execute()
        {
            byte instruction = mmu.ReadByte(pc++);

            #region Opcodes
            switch (instruction)
            {
            #region 8-Bit Loads

                #region Load Immediate
                case 0x06: cycles = 2; b = mmu.ReadByte(pc++); break;
                case 0x0E: cycles = 2; c = mmu.ReadByte(pc++); break;
                case 0x16: cycles = 2; d = mmu.ReadByte(pc++); break;
                case 0x1E: cycles = 2; e = mmu.ReadByte(pc++); break;
                case 0x26: cycles = 2; h = mmu.ReadByte(pc++); break;
                case 0x2E: cycles = 2; l = mmu.ReadByte(pc++); break;
                #endregion

                #region Load Register
                case 0x40: cycles = 1; b = b; break;
                case 0x41: cycles = 1; b = c; break;
                case 0x42: cycles = 1; b = d; break;
                case 0x43: cycles = 1; b = e; break;
                case 0x44: cycles = 1; b = h; break;
                case 0x45: cycles = 1; b = l; break;
                case 0x47: cycles = 1; b = a; break;

                case 0x48: cycles = 1; c = b; break;
                case 0x49: cycles = 1; c = c; break;
                case 0x4A: cycles = 1; c = d; break;
                case 0x4B: cycles = 1; c = e; break;
                case 0x4C: cycles = 1; c = h; break;
                case 0x4D: cycles = 1; c = l; break;
                case 0x4F: cycles = 1; c = a; break;

                case 0x50: cycles = 1; d = b; break;
                case 0x51: cycles = 1; d = c; break;
                case 0x52: cycles = 1; d = d; break;
                case 0x53: cycles = 1; d = e; break;
                case 0x54: cycles = 1; d = h; break;
                case 0x55: cycles = 1; d = l; break;
                case 0x57: cycles = 1; d = a; break;

                case 0x58: cycles = 1; e = b; break;
                case 0x59: cycles = 1; e = c; break;
                case 0x5A: cycles = 1; e = d; break;
                case 0x5B: cycles = 1; e = e; break;
                case 0x5C: cycles = 1; e = h; break;
                case 0x5D: cycles = 1; e = l; break;
                case 0x5F: cycles = 1; e = a; break;

                case 0x60: cycles = 1; h = b; break;
                case 0x61: cycles = 1; h = c; break;
                case 0x62: cycles = 1; h = d; break;
                case 0x63: cycles = 1; h = e; break;
                case 0x64: cycles = 1; h = h; break;
                case 0x65: cycles = 1; h = l; break;
                case 0x67: cycles = 1; h = a; break;

                case 0x68: cycles = 1; l = b; break;
                case 0x69: cycles = 1; l = c; break;
                case 0x6A: cycles = 1; l = d; break;
                case 0x6B: cycles = 1; l = e; break;
                case 0x6C: cycles = 1; l = h; break;
                case 0x6D: cycles = 1; l = l; break;
                case 0x6F: cycles = 1; l = a; break;

                case 0x78: cycles = 1; a = b; break;
                case 0x79: cycles = 1; a = c; break;
                case 0x7A: cycles = 1; a = d; break;
                case 0x7B: cycles = 1; a = e; break;
                case 0x7C: cycles = 1; a = h; break;
                case 0x7D: cycles = 1; a = l; break;
                case 0x7F: cycles = 1; a = a; break;
                #endregion

                #region Load from Memory
                case 0x46: cycles = 2; b = mmu.ReadByte(hl); break;
                case 0x4E: cycles = 2; c = mmu.ReadByte(hl); break;
                case 0x56: cycles = 2; d = mmu.ReadByte(hl); break;
                case 0x5E: cycles = 2; e = mmu.ReadByte(hl); break;
                case 0x66: cycles = 2; h = mmu.ReadByte(hl); break;
                case 0x6E: cycles = 2; l = mmu.ReadByte(hl); break;

                case 0x0A: cycles = 2; a = mmu.ReadByte(bc); break;
                case 0x1A: cycles = 2; a = mmu.ReadByte(de); break;
                case 0x7E: cycles = 2; a = mmu.ReadByte(hl); break;
                case 0xFA: cycles = 4; a = mmu.ReadByte(mmu.ReadWord(pc)); pc += 2; break;
                case 0x3E: cycles = 2; a = mmu.ReadByte(pc++); break;

                case 0x2A: cycles = 2; a = mmu.ReadByte(hl); hl += 1; break;
                case 0x3A: cycles = 2; a = mmu.ReadByte(hl); hl -= 1; break;

                case 0xF0: cycles = 3; a = mmu.ReadByte((ushort)(0xFF00 + mmu.ReadByte(pc++))); break;
                case 0xF2: cycles = 2; a = mmu.ReadByte((ushort)(0xFF00 + c)); break;
                #endregion

                #region Load to Memory
                case 0x70: cycles = 2; mmu.WriteByte(hl, b); break;
                case 0x71: cycles = 2; mmu.WriteByte(hl, c); break;
                case 0x72: cycles = 2; mmu.WriteByte(hl, d); break;
                case 0x73: cycles = 2; mmu.WriteByte(hl, e); break;
                case 0x74: cycles = 2; mmu.WriteByte(hl, h); break;
                case 0x75: cycles = 2; mmu.WriteByte(hl, l); break;

                case 0x02: cycles = 2; mmu.WriteByte(bc, a); break;
                case 0x12: cycles = 2; mmu.WriteByte(de, a); break;
                case 0x77: cycles = 2; mmu.WriteByte(hl, a); break;
                case 0xEA: cycles = 4; mmu.WriteByte(mmu.ReadByte(pc), a); pc += 2; break;

                case 0x22: cycles = 2; mmu.WriteByte(hl, a); hl += 1; break;
                case 0x32: cycles = 2; mmu.WriteByte(hl, a); hl -= 1; break;

                case 0xE0: cycles = 3; mmu.WriteByte((ushort)(0xFF00 + mmu.ReadByte(pc++)), a); break;
                case 0xE2: cycles = 2; mmu.WriteByte((ushort)(0xFF00 + c), a); break;

                case 0x36: cycles = 3; mmu.WriteByte(hl, mmu.ReadByte(pc++)); break;
                #endregion

            #endregion

            #region 16-Bit Loads
                
                #region Load Immediate
                case 0x01: cycles = 3; bc = mmu.ReadWord(mmu.ReadWord(pc)); pc += 2; break;
                case 0x11: cycles = 3; de = mmu.ReadWord(mmu.ReadWord(pc)); pc += 2; break;
                case 0x21: cycles = 3; hl = mmu.ReadWord(mmu.ReadWord(pc)); pc += 2; break;
                case 0x31: cycles = 3; sp = mmu.ReadWord(mmu.ReadWord(pc)); pc += 2; break;
                #endregion

                #region Stack
                case 0xF8: cycles = 3; hl = mmu.ReadWord((ushort)(sp + mmu.ReadByte(pc++))); break;
                case 0xF9: cycles = 2; sp = hl; break;

                case 0x08: cycles = 5; mmu.WriteWord(mmu.ReadWord(pc), sp); pc += 2; break;

                // PUSH
                case 0xC5: cycles = 4; mmu.WriteWord(sp, bc); sp -= 2; break;
                case 0xD5: cycles = 4; mmu.WriteWord(sp, de); sp -= 2; break;
                case 0xE5: cycles = 4; mmu.WriteWord(sp, hl); sp -= 2; break;
                case 0xF5: cycles = 4; mmu.WriteWord(sp, af); sp -= 2; break;

                // POP
                case 0xC1: cycles = 3; bc = mmu.ReadWord(sp); sp += 2; break;
                case 0xD1: cycles = 3; de = mmu.ReadWord(sp); sp += 2; break;
                case 0xE1: cycles = 3; hl = mmu.ReadWord(sp); sp += 2; break;
                case 0xF1: cycles = 3; af = mmu.ReadWord(sp); sp += 2; break;
                #endregion

            #endregion

            #region 8-bit ALU

                #region Add
                case 0x80: cycles = 1; a = add(a, b); break;
                case 0x81: cycles = 1; a = add(a, c); break;
                case 0x82: cycles = 1; a = add(a, d); break;
                case 0x83: cycles = 1; a = add(a, e); break;
                case 0x84: cycles = 1; a = add(a, h); break;
                case 0x85: cycles = 1; a = add(a, l); break;
                case 0x87: cycles = 1; a = add(a, a); break;

                case 0x86: cycles = 2; a = add(a, mmu.ReadByte(hl)); break;
                case 0xC6: cycles = 2; a = add(a, mmu.ReadByte(pc++)); break;
                #endregion

                #region Add Carry
                case 0x88: cycles = 1; a = addCarry(a, b); break;
                case 0x89: cycles = 1; a = addCarry(a, c); break;
                case 0x8A: cycles = 1; a = addCarry(a, d); break;
                case 0x8B: cycles = 1; a = addCarry(a, e); break;
                case 0x8C: cycles = 1; a = addCarry(a, h); break;
                case 0x8D: cycles = 1; a = addCarry(a, l); break;
                case 0x8F: cycles = 1; a = addCarry(a, a); break;

                case 0x8E: cycles = 2; a = addCarry(a, mmu.ReadByte(hl)); break;
                case 0xCE: cycles = 2; a = addCarry(a, mmu.ReadByte(pc++)); break;
                #endregion

                #region Subtract
                case 0x90: cycles = 1; a = subtract(a, b); break;
                case 0x91: cycles = 1; a = subtract(a, c); break;
                case 0x92: cycles = 1; a = subtract(a, d); break;
                case 0x93: cycles = 1; a = subtract(a, e); break;
                case 0x94: cycles = 1; a = subtract(a, h); break;
                case 0x95: cycles = 1; a = subtract(a, l); break;
                case 0x97: cycles = 1; a = subtract(a, a); break;

                case 0x96: cycles = 2; a = subtract(a, mmu.ReadByte(hl)); break;
                case 0xD6: cycles = 2; a = subtract(a, mmu.ReadByte(pc++)); break;
                #endregion

                #region Subtract Carry
                case 0x98: cycles = 1; a = subtractCarry(a, b); break;
                case 0x99: cycles = 1; a = subtractCarry(a, c); break;
                case 0x9A: cycles = 1; a = subtractCarry(a, d); break;
                case 0x9B: cycles = 1; a = subtractCarry(a, e); break;
                case 0x9C: cycles = 1; a = subtractCarry(a, h); break;
                case 0x9D: cycles = 1; a = subtractCarry(a, l); break;
                case 0x9F: cycles = 1; a = subtractCarry(a, a); break;

                case 0x9E: cycles = 2; a = subtractCarry(a, mmu.ReadByte(hl)); break;
                case 0xDE: cycles = 2; a = subtractCarry(a, mmu.ReadByte(pc++)); break;
                #endregion

                #region And
                case 0xA0: cycles = 1; a &= b; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xA1: cycles = 1; a &= c; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xA2: cycles = 1; a &= d; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xA3: cycles = 1; a &= e; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xA4: cycles = 1; a &= h; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xA5: cycles = 1; a &= l; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xA7: cycles = 1; a &= a; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;

                case 0xA6: cycles = 2; a &= mmu.ReadByte(hl); f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xE6: cycles = 2; a &= mmu.ReadByte(pc++); f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                #endregion

                #region Or
                case 0xB0: cycles = 1; a |= b; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xB1: cycles = 1; a |= c; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xB2: cycles = 1; a |= d; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xB3: cycles = 1; a |= e; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xB4: cycles = 1; a |= h; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xB5: cycles = 1; a |= l; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xB7: cycles = 1; a |= a; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;

                case 0xB6: cycles = 2; a |= mmu.ReadByte(hl); f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xF6: cycles = 2; a |= mmu.ReadByte(pc++); f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                #endregion

                #region Xor
                case 0xA8: cycles = 1; a ^= b; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xA9: cycles = 1; a ^= c; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xAA: cycles = 1; a ^= d; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xAB: cycles = 1; a ^= e; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xAC: cycles = 1; a ^= h; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xAD: cycles = 1; a ^= l; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xAF: cycles = 1; a ^= a; f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;

                case 0xAE: cycles = 2; a ^= mmu.ReadByte(hl); f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                case 0xEE: cycles = 2; a ^= mmu.ReadByte(pc++); f &= (~Flags.Zero); f |= a == 0 ? Flags.Zero : 0; break;
                #endregion

                #region Compare
                case 0xB8: cycles = 1; subtract(a, b); break;
                case 0xB9: cycles = 1; subtract(a, c); break;
                case 0xBA: cycles = 1; subtract(a, d); break;
                case 0xBB: cycles = 1; subtract(a, e); break;
                case 0xBC: cycles = 1; subtract(a, h); break;
                case 0xBD: cycles = 1; subtract(a, l); break;
                case 0xBF: cycles = 1; subtract(a, a); break;

                case 0xBE: cycles = 2; subtract(a, mmu.ReadByte(hl)); break;
                case 0xFE: cycles = 2; subtract(a, mmu.ReadByte(pc++)); break;
                #endregion

                #region Increment
                case 0x04: cycles = 1; b = add(b, 1); break;
                case 0x0C: cycles = 1; c = add(c, 1); break;
                case 0x14: cycles = 1; d = add(d, 1); break;
                case 0x1C: cycles = 1; e = add(e, 1); break;
                case 0x24: cycles = 1; h = add(h, 1); break;
                case 0x2C: cycles = 1; l = add(l, 1); break;
                case 0x3C: cycles = 1; a = add(a, 1); break;

                case 0x34: cycles = 3; mmu.WriteByte(hl, add(mmu.ReadByte(hl), 1)); break;
                #endregion

                #region Decrement
                case 0x05: cycles = 1; b = subtract(b, 1); break;
                case 0x0D: cycles = 1; c = subtract(c, 1); break;
                case 0x15: cycles = 1; d = subtract(d, 1); break;
                case 0x1D: cycles = 1; e = subtract(e, 1); break;
                case 0x25: cycles = 1; h = subtract(h, 1); break;
                case 0x2D: cycles = 1; l = subtract(l, 1); break;
                case 0x3D: cycles = 1; a = subtract(a, 1); break;

                case 0x35: cycles = 3; mmu.WriteByte(hl, subtract(mmu.ReadByte(hl), 1)); break;
                #endregion

            #endregion

            #region 16-Bit Arithmetic

                #region Add
                case 0x09: cycles = 2; hl = add(hl, bc); break;
                case 0x19: cycles = 2; hl = add(hl, de); break;
                case 0x29: cycles = 2; hl = add(hl, hl); break;
                case 0x39: cycles = 2; hl = add(hl, sp); break;

                case 0xE8: cycles = 4; sp = add(sp, mmu.ReadByte(pc++)); break;
                #endregion

                #region Increase
                case 0x03: cycles = 2; bc++; break;
                case 0x13: cycles = 2; de++; break;
                case 0x23: cycles = 2; hl++; break;
                case 0x33: cycles = 2; sp++; break;
                #endregion

                #region Decrease
                case 0x0B: cycles = 2; bc--; break;
                case 0x1B: cycles = 2; de--; break;
                case 0x2B: cycles = 2; hl--; break;
                case 0x3B: cycles = 2; sp--; break;
                #endregion

            #endregion

            #region Miscellaneous

                // NOP
                case 0x00: cycles = 1; break;
                
                // Decimal Adjust register A
                case 0x27:
                    cycles = 1;
                    byte lower = (byte)(a | 0x0F);
                    if (lower > 9)
                    {
                        a += 10;
                        f = (a == 0) ? Flags.Zero : 0;
                    }
                    break;

                // Complement register A
                case 0x2F:
                    cycles = 1;
                    a = (byte)~a;
                    f = Flags.Subtract | Flags.HalfCarry;
                    break;

                // Set Carry Flag
                case 0x37:
                    cycles = 1;
                    f |= Flags.Carry;
                    break;

                // Complement carry flag
                case 0x3F:
                    cycles = 1;
                    f = (f & Flags.Zero) | ~(f & Flags.Carry);
                    break;

                // HALT
                case 0x76:
                    cycles = 1;
                    halted = true;
                    break;

                // STOP
                case 0x10:
                    cycles = 1;
                    if (mmu.ReadByte(pc) == 0x00)
                    {
                        pc++; halted = true;
                        // Dunno what to do from here.
                    }
                    break;

                // Disable Interrupt
                case 0xF3:
                    cycles = 1;
                    diTimer = 1;
                    break;

                // Enable Interrupt
                case 0xFB:
                    cycles = 1;
                    eiTimer = 1;
                    break;

                // Rotate Left
                case 0x07: cycles = 1; a = rotateLeft(a); break;

                // Rotate Left through Carry
                case 0x17: cycles = 1; a = rotateLeft(a); if (f.HasFlag(Flags.Carry)) a++; break;

                // Rotate Right
                case 0x0F: cycles = 1; a = rotateRight(a); break;

                // Rotate Right through Carry
                case 0x1F: cycles = 1; a = rotateRight(a); if (f.HasFlag(Flags.Carry)) a += 0x80; break;

                // Restart
                case 0xC7: cycles = 8; mmu.WriteWord(sp++, pc); pc = 0x00; break;
                case 0xCF: cycles = 8; mmu.WriteWord(sp++, pc); pc = 0x08; break;
                case 0xD7: cycles = 8; mmu.WriteWord(sp++, pc); pc = 0x10; break;
                case 0xDF: cycles = 8; mmu.WriteWord(sp++, pc); pc = 0x18; break;
                case 0xE7: cycles = 8; mmu.WriteWord(sp++, pc); pc = 0x20; break;
                case 0xEF: cycles = 8; mmu.WriteWord(sp++, pc); pc = 0x28; break;
                case 0xF7: cycles = 8; mmu.WriteWord(sp++, pc); pc = 0x30; break;
                case 0xFF: cycles = 8; mmu.WriteWord(sp++, pc); pc = 0x38; break;

            #endregion

            #region Jumps
                // Jump
                case 0xC3:
                    cycles = 3;
                    pc = (ushort)(mmu.ReadByte(pc++) | ((mmu.ReadByte(pc++)) << 8));
                    break;

                case 0xE9:
                    cycles = 1;
                    pc = mmu.ReadByte(hl); break;
                case 0x18:
                    cycles = 2;
                    pc = (ushort)((short)pc + (short)(unchecked((sbyte)mmu.ReadByte(pc++))));
                    break;

                // Jump if flag
                case 0xC2:
                    cycles = 3;
                    utmp = (ushort)(mmu.ReadByte(pc++) | ((mmu.ReadByte(pc++)) << 8));
                    if (!f.HasFlag(Flags.Zero)) pc = utmp;
                    break;
                case 0xCA:
                    cycles = 3;
                    utmp = (ushort)(mmu.ReadByte(pc++) | ((mmu.ReadByte(pc++)) << 8));
                    if (f.HasFlag(Flags.Zero)) pc = utmp;
                    break;
                case 0xD2:
                    cycles = 3;
                    utmp = (ushort)(mmu.ReadByte(pc++) | ((mmu.ReadByte(pc++)) << 8));
                    if (!f.HasFlag(Flags.Carry)) pc = utmp;
                    break;
                case 0xDA:
                    cycles = 3;
                    utmp = (ushort)(mmu.ReadByte(pc++) | ((mmu.ReadByte(pc++)) << 8));
                    if (f.HasFlag(Flags.Carry)) pc = utmp;
                    break;

                // Jump Relative if flag
                case 0x20:
                    cycles = 3;
                    utmp = (ushort)((short)pc + (short)(unchecked((sbyte)mmu.ReadByte(pc++))));
                    if (!f.HasFlag(Flags.Zero)) pc = utmp;
                    break;
                case 0x28:
                    cycles = 3;
                    utmp = (ushort)((short)pc + (short)(unchecked((sbyte)mmu.ReadByte(pc++))));
                    if (f.HasFlag(Flags.Zero)) pc = utmp;
                    break;
                case 0x30:
                    cycles = 3;
                    utmp = (ushort)((short)pc + (short)(unchecked((sbyte)mmu.ReadByte(pc++))));
                    if (!f.HasFlag(Flags.Carry)) pc = utmp;
                    break;
                case 0x38:
                    cycles = 3;
                    utmp = (ushort)((short)pc + (short)(unchecked((sbyte)mmu.ReadByte(pc++))));
                    if (f.HasFlag(Flags.Carry)) pc = utmp;
                    break;
                    
                // Call
                case 0xCD:
                    mmu.WriteWord(sp++, (ushort)(pc + 1));
                    pc = (ushort)(mmu.ReadByte(pc++) | ((mmu.ReadByte(pc++)) << 8));
                    break;

                // Call if flag
                case 0xC4:
                    cycles = 3;
                    utmp = (ushort)(mmu.ReadByte(pc++) | ((mmu.ReadByte(pc++)) << 8));
                    if (!f.HasFlag(Flags.Zero))
                    {
                        mmu.WriteWord(sp++, (ushort)(pc + 1));
                        pc = utmp;
                    }
                    break;
                case 0xCC:
                    cycles = 3;
                    utmp = (ushort)(mmu.ReadByte(pc++) | ((mmu.ReadByte(pc++)) << 8));
                    if (f.HasFlag(Flags.Zero))
                    {
                        mmu.WriteWord(sp++, (ushort)(pc + 1));
                        pc = utmp;
                    }
                    break;
                case 0xD4:
                    cycles = 3;
                    utmp = (ushort)(mmu.ReadByte(pc++) | ((mmu.ReadByte(pc++)) << 8));
                    if (!f.HasFlag(Flags.Carry))
                    {
                        mmu.WriteWord(sp++, (ushort)(pc + 1));
                        pc = utmp;
                    }
                    break;
                case 0xDC:
                    cycles = 3;
                    utmp = (ushort)(mmu.ReadByte(pc++) | ((mmu.ReadByte(pc++)) << 8));
                    if (f.HasFlag(Flags.Carry))
                    {
                        mmu.WriteWord(sp++, (ushort)(pc + 1));
                        pc = utmp;
                    }
                    break;

                // Return
                case 0xC9:
                    cycles = 2;
                    pc = mmu.ReadWord(sp--);
                    break;

                // Return if Flag
                case 0xC0:
                    cycles = 2;
                    if (!f.HasFlag(Flags.Zero)) pc = mmu.ReadWord(sp--);
                    break;
                case 0xC8:
                    cycles = 2;
                    if (f.HasFlag(Flags.Zero)) pc = mmu.ReadWord(sp--);
                    break;
                case 0xD0:
                    cycles = 2;
                    if (!f.HasFlag(Flags.Carry)) pc = mmu.ReadWord(sp--);
                    break;
                case 0xD8:
                    cycles = 2;
                    if (f.HasFlag(Flags.Carry)) pc = mmu.ReadWord(sp--);
                    break;

                // Return & enable interrupts
                case 0xD9:
                    cycles = 2;
                    pc = mmu.ReadWord(sp--);
                    eiTimer = 1;
                    break;

            #endregion

                #region CB subset
                case 0xCB:
                switch (mmu.ReadByte(pc++))
                {
                    #region Swap
                    case 0x30: cycles = 2; b = swapByte(b); break;
                    case 0x31: cycles = 2; c = swapByte(c); break;
                    case 0x32: cycles = 2; d = swapByte(d); break;
                    case 0x33: cycles = 2; e = swapByte(e); break;
                    case 0x34: cycles = 2; h = swapByte(h); break;
                    case 0x35: cycles = 2; l = swapByte(l); break;
                    case 0x37: cycles = 2; a = swapByte(a); break;

                    case 0x36: cycles = 4; mmu.WriteByte(hl, swapByte(mmu.ReadByte(hl))); break;
                    #endregion

                    #region Rotates
                    case 0x00: cycles = 2; b = rotateLeft(b); break;
                    case 0x01: cycles = 2; c = rotateLeft(c); break;
                    case 0x02: cycles = 2; d = rotateLeft(d); break;
                    case 0x03: cycles = 2; e = rotateLeft(e); break;
                    case 0x04: cycles = 2; h = rotateLeft(h); break;
                    case 0x05: cycles = 2; l = rotateLeft(l); break;
                    case 0x07: cycles = 2; a = rotateLeft(a); break;

                    case 0x06: cycles = 4; mmu.WriteByte(hl, rotateLeft(mmu.ReadByte(hl))); break;

                    case 0x10: cycles = 2; b = rotateLeft(b); if (f.HasFlag(Flags.Carry)) a++; break;
                    case 0x11: cycles = 2; c = rotateLeft(c); if (f.HasFlag(Flags.Carry)) a++; break;
                    case 0x12: cycles = 2; d = rotateLeft(d); if (f.HasFlag(Flags.Carry)) a++; break;
                    case 0x13: cycles = 2; e = rotateLeft(e); if (f.HasFlag(Flags.Carry)) a++; break;
                    case 0x14: cycles = 2; h = rotateLeft(h); if (f.HasFlag(Flags.Carry)) a++; break;
                    case 0x15: cycles = 2; l = rotateLeft(l); if (f.HasFlag(Flags.Carry)) a++; break;
                    case 0x17: cycles = 2; a = rotateLeft(a); if (f.HasFlag(Flags.Carry)) a++; break;

                    case 0x16: cycles = 4; t = rotateLeft(mmu.ReadByte(hl)); if (f.HasFlag(Flags.Carry)) t++; mmu.WriteByte(hl, t); break;

                    case 0x08: cycles = 2; b = rotateRight(b); break;
                    case 0x09: cycles = 2; c = rotateRight(c); break;
                    case 0x0A: cycles = 2; d = rotateRight(d); break;
                    case 0x0B: cycles = 2; e = rotateRight(e); break;
                    case 0x0C: cycles = 2; h = rotateRight(h); break;
                    case 0x0D: cycles = 2; l = rotateRight(l); break;
                    case 0x0F: cycles = 2; a = rotateRight(a); break;

                    case 0x0E: cycles = 4; mmu.WriteByte(hl, rotateRight(mmu.ReadByte(hl))); break;

                    case 0x18: cycles = 2; b = rotateRight(b); if (f.HasFlag(Flags.Carry)) a++; break;
                    case 0x19: cycles = 2; c = rotateRight(c); if (f.HasFlag(Flags.Carry)) a++; break;
                    case 0x1A: cycles = 2; d = rotateRight(d); if (f.HasFlag(Flags.Carry)) a++; break;
                    case 0x1B: cycles = 2; e = rotateRight(e); if (f.HasFlag(Flags.Carry)) a++; break;
                    case 0x1C: cycles = 2; h = rotateRight(h); if (f.HasFlag(Flags.Carry)) a++; break;
                    case 0x1D: cycles = 2; l = rotateRight(l); if (f.HasFlag(Flags.Carry)) a++; break;
                    case 0x1F: cycles = 2; a = rotateRight(a); if (f.HasFlag(Flags.Carry)) a++; break;

                    case 0x1E: cycles = 4; t = rotateRight(mmu.ReadByte(hl)); if (f.HasFlag(Flags.Carry)) t++; mmu.WriteByte(hl, t); break;

                    case 0x20: cycles = 2; f = 0; if ((b & 0x80) != 0) f |= Flags.Carry; b <<= 1; if (b == 0) f |= Flags.Zero; break;
                    case 0x21: cycles = 2; f = 0; if ((c & 0x80) != 0) f |= Flags.Carry; c <<= 1; if (c == 0) f |= Flags.Zero; break;
                    case 0x22: cycles = 2; f = 0; if ((d & 0x80) != 0) f |= Flags.Carry; d <<= 1; if (d == 0) f |= Flags.Zero; break;
                    case 0x23: cycles = 2; f = 0; if ((e & 0x80) != 0) f |= Flags.Carry; e <<= 1; if (e == 0) f |= Flags.Zero; break;
                    case 0x24: cycles = 2; f = 0; if ((h & 0x80) != 0) f |= Flags.Carry; h <<= 1; if (h == 0) f |= Flags.Zero; break;
                    case 0x25: cycles = 2; f = 0; if ((l & 0x80) != 0) f |= Flags.Carry; l <<= 1; if (l == 0) f |= Flags.Zero; break;
                    case 0x27: cycles = 2; f = 0; if ((a & 0x80) != 0) f |= Flags.Carry; a <<= 1; if (a == 0) f |= Flags.Zero; break;

                    case 0x26: cycles = 4; t = mmu.ReadByte(hl); f = 0; if ((t & 0x80) != 0) f |= Flags.Carry; mmu.WriteByte(hl, (byte)(t << 1)); if (b == 0) f |= Flags.Zero; break;

                    case 0x28: cycles = 2; t = b; f = 0; if ((b & 0x01) != 0) f |= Flags.Carry; b >>= 1; b |= (byte)(t & 0x80); if (b == 0) f |= Flags.Zero; break;
                    case 0x29: cycles = 2; t = c; f = 0; if ((c & 0x01) != 0) f |= Flags.Carry; c >>= 1; c |= (byte)(t & 0x80); if (c == 0) f |= Flags.Zero; break;
                    case 0x2A: cycles = 2; t = d; f = 0; if ((d & 0x01) != 0) f |= Flags.Carry; d >>= 1; d |= (byte)(t & 0x80); if (d == 0) f |= Flags.Zero; break;
                    case 0x2B: cycles = 2; t = e; f = 0; if ((e & 0x01) != 0) f |= Flags.Carry; e >>= 1; e |= (byte)(t & 0x80); if (e == 0) f |= Flags.Zero; break;
                    case 0x2C: cycles = 2; t = h; f = 0; if ((h & 0x01) != 0) f |= Flags.Carry; h >>= 1; h |= (byte)(t & 0x80); if (h == 0) f |= Flags.Zero; break;
                    case 0x2D: cycles = 2; t = l; f = 0; if ((l & 0x01) != 0) f |= Flags.Carry; l >>= 1; l |= (byte)(t & 0x80); if (l == 0) f |= Flags.Zero; break;
                    case 0x2F: cycles = 2; t = a; f = 0; if ((a & 0x01) != 0) f |= Flags.Carry; a >>= 1; a |= (byte)(t & 0x80); if (a == 0) f |= Flags.Zero; break;

                    case 0x2E: cycles = 4; t = mmu.ReadByte(hl); f = 0; if ((t & 0x01) != 0) f |= Flags.Carry; t >>= 1; mmu.WriteByte(hl, (byte)(t | (mmu.ReadByte(hl) & 0x80))); if (t == 0) f |= Flags.Zero; break;

                    case 0x38: cycles = 2; f = 0; if ((b & 0x01) != 0) f |= Flags.Carry; b >>= 1; if (b == 0) f |= Flags.Zero; break;
                    case 0x39: cycles = 2; f = 0; if ((c & 0x01) != 0) f |= Flags.Carry; c >>= 1; if (c == 0) f |= Flags.Zero; break;
                    case 0x3A: cycles = 2; f = 0; if ((d & 0x01) != 0) f |= Flags.Carry; d >>= 1; if (d == 0) f |= Flags.Zero; break;
                    case 0x3B: cycles = 2; f = 0; if ((e & 0x01) != 0) f |= Flags.Carry; e >>= 1; if (e == 0) f |= Flags.Zero; break;
                    case 0x3C: cycles = 2; f = 0; if ((h & 0x01) != 0) f |= Flags.Carry; h >>= 1; if (h == 0) f |= Flags.Zero; break;
                    case 0x3D: cycles = 2; f = 0; if ((l & 0x01) != 0) f |= Flags.Carry; l >>= 1; if (l == 0) f |= Flags.Zero; break;
                    case 0x3F: cycles = 2; f = 0; if ((a & 0x01) != 0) f |= Flags.Carry; a >>= 1; if (a == 0) f |= Flags.Zero; break;

                    case 0x3E: cycles = 4; t = mmu.ReadByte(hl); f = 0; if ((t & 0x01) != 0) f |= Flags.Carry; t >>= 1; mmu.WriteByte(hl, t); if (t == 0) f |= Flags.Zero; break;
                    #endregion

                    #region Bit operations
                    // Test
                    case 0x40: cycles = 2; testBit(b, 0); break;
                    case 0x41: cycles = 2; testBit(c, 0); break;
                    case 0x42: cycles = 2; testBit(d, 0); break;
                    case 0x43: cycles = 2; testBit(e, 0); break;
                    case 0x44: cycles = 2; testBit(h, 0); break;
                    case 0x45: cycles = 2; testBit(l, 0); break;
                    case 0x47: cycles = 2; testBit(a, 0); break;

                    case 0x46: cycles = 4; testBit(mmu.ReadByte(hl), 0); break;

                    case 0x48: cycles = 2; testBit(b, 1); break;
                    case 0x49: cycles = 2; testBit(c, 1); break;
                    case 0x4A: cycles = 2; testBit(d, 1); break;
                    case 0x4B: cycles = 2; testBit(e, 1); break;
                    case 0x4C: cycles = 2; testBit(h, 1); break;
                    case 0x4D: cycles = 2; testBit(l, 1); break;
                    case 0x4F: cycles = 2; testBit(a, 1); break;

                    case 0x4E: cycles = 4; testBit(mmu.ReadByte(hl), 1); break;

                    case 0x50: cycles = 2; testBit(b, 2); break;
                    case 0x51: cycles = 2; testBit(c, 2); break;
                    case 0x52: cycles = 2; testBit(d, 2); break;
                    case 0x53: cycles = 2; testBit(e, 2); break;
                    case 0x54: cycles = 2; testBit(h, 2); break;
                    case 0x55: cycles = 2; testBit(l, 2); break;
                    case 0x57: cycles = 2; testBit(a, 2); break;

                    case 0x56: cycles = 4; testBit(mmu.ReadByte(hl), 2); break;

                    case 0x58: cycles = 2; testBit(b, 3); break;
                    case 0x59: cycles = 2; testBit(c, 3); break;
                    case 0x5A: cycles = 2; testBit(d, 3); break;
                    case 0x5B: cycles = 2; testBit(e, 3); break;
                    case 0x5C: cycles = 2; testBit(h, 3); break;
                    case 0x5D: cycles = 2; testBit(l, 3); break;
                    case 0x5F: cycles = 2; testBit(a, 3); break;

                    case 0x5E: cycles = 4; testBit(mmu.ReadByte(hl), 3); break;

                    case 0x60: cycles = 2; testBit(b, 4); break;
                    case 0x61: cycles = 2; testBit(c, 4); break;
                    case 0x62: cycles = 2; testBit(d, 4); break;
                    case 0x63: cycles = 2; testBit(e, 4); break;
                    case 0x64: cycles = 2; testBit(h, 4); break;
                    case 0x65: cycles = 2; testBit(l, 4); break;
                    case 0x67: cycles = 2; testBit(a, 4); break;

                    case 0x66: cycles = 4; testBit(mmu.ReadByte(hl), 4); break;

                    case 0x68: cycles = 2; testBit(b, 5); break;
                    case 0x69: cycles = 2; testBit(c, 5); break;
                    case 0x6A: cycles = 2; testBit(d, 5); break;
                    case 0x6B: cycles = 2; testBit(e, 5); break;
                    case 0x6C: cycles = 2; testBit(h, 5); break;
                    case 0x6D: cycles = 2; testBit(l, 5); break;
                    case 0x6F: cycles = 2; testBit(a, 5); break;

                    case 0x6E: cycles = 4; testBit(mmu.ReadByte(hl), 5); break;

                    case 0x70: cycles = 2; testBit(b, 6); break;
                    case 0x71: cycles = 2; testBit(c, 6); break;
                    case 0x72: cycles = 2; testBit(d, 6); break;
                    case 0x73: cycles = 2; testBit(e, 6); break;
                    case 0x74: cycles = 2; testBit(h, 6); break;
                    case 0x75: cycles = 2; testBit(l, 6); break;
                    case 0x77: cycles = 2; testBit(a, 6); break;

                    case 0x76: cycles = 4; testBit(mmu.ReadByte(hl), 6); break;

                    case 0x78: cycles = 2; testBit(b, 7); break;
                    case 0x79: cycles = 2; testBit(c, 7); break;
                    case 0x7A: cycles = 2; testBit(d, 7); break;
                    case 0x7B: cycles = 2; testBit(e, 7); break;
                    case 0x7C: cycles = 2; testBit(h, 7); break;
                    case 0x7D: cycles = 2; testBit(l, 7); break;
                    case 0x7F: cycles = 2; testBit(a, 7); break;

                    case 0x7E: cycles = 4; testBit(mmu.ReadByte(hl), 7); break;

                    // Reset
                    case 0x80: cycles = 2; b = resetBit(b, 0); break;
                    case 0x81: cycles = 2; c = resetBit(c, 0); break;
                    case 0x82: cycles = 2; d = resetBit(d, 0); break;
                    case 0x83: cycles = 2; e = resetBit(e, 0); break;
                    case 0x84: cycles = 2; h = resetBit(h, 0); break;
                    case 0x85: cycles = 2; l = resetBit(l, 0); break;
                    case 0x87: cycles = 2; a = resetBit(a, 0); break;

                    case 0x86: cycles = 4; mmu.WriteByte(hl, resetBit(mmu.ReadByte(hl), 0)); break;

                    case 0x88: cycles = 2; b = resetBit(b, 1); break;
                    case 0x89: cycles = 2; c = resetBit(c, 1); break;
                    case 0x8A: cycles = 2; d = resetBit(d, 1); break;
                    case 0x8B: cycles = 2; e = resetBit(e, 1); break;
                    case 0x8C: cycles = 2; h = resetBit(h, 1); break;
                    case 0x8D: cycles = 2; l = resetBit(l, 1); break;
                    case 0x8F: cycles = 2; a = resetBit(a, 1); break;

                    case 0x8E: cycles = 4; mmu.WriteByte(hl, resetBit(mmu.ReadByte(hl), 1)); break;

                    case 0x90: cycles = 2; b = resetBit(b, 2); break;
                    case 0x91: cycles = 2; c = resetBit(c, 2); break;
                    case 0x92: cycles = 2; d = resetBit(d, 2); break;
                    case 0x93: cycles = 2; e = resetBit(e, 2); break;
                    case 0x94: cycles = 2; h = resetBit(h, 2); break;
                    case 0x95: cycles = 2; l = resetBit(l, 2); break;
                    case 0x97: cycles = 2; a = resetBit(a, 2); break;

                    case 0x96: cycles = 4; mmu.WriteByte(hl, resetBit(mmu.ReadByte(hl), 2)); break;

                    case 0x98: cycles = 2; b = resetBit(b, 3); break;
                    case 0x99: cycles = 2; c = resetBit(c, 3); break;
                    case 0x9A: cycles = 2; d = resetBit(d, 3); break;
                    case 0x9B: cycles = 2; e = resetBit(e, 3); break;
                    case 0x9C: cycles = 2; h = resetBit(h, 3); break;
                    case 0x9D: cycles = 2; l = resetBit(l, 3); break;
                    case 0x9F: cycles = 2; a = resetBit(a, 3); break;

                    case 0x9E: cycles = 4; mmu.WriteByte(hl, resetBit(mmu.ReadByte(hl), 3)); break;

                    case 0xA0: cycles = 2; b = resetBit(b, 4); break;
                    case 0xA1: cycles = 2; c = resetBit(c, 4); break;
                    case 0xA2: cycles = 2; d = resetBit(d, 4); break;
                    case 0xA3: cycles = 2; e = resetBit(e, 4); break;
                    case 0xA4: cycles = 2; h = resetBit(h, 4); break;
                    case 0xA5: cycles = 2; l = resetBit(l, 4); break;
                    case 0xA7: cycles = 2; a = resetBit(a, 4); break;

                    case 0xA6: cycles = 4; mmu.WriteByte(hl, resetBit(mmu.ReadByte(hl), 4)); break;

                    case 0xA8: cycles = 2; b = resetBit(b, 5); break;
                    case 0xA9: cycles = 2; c = resetBit(c, 5); break;
                    case 0xAA: cycles = 2; d = resetBit(d, 5); break;
                    case 0xAB: cycles = 2; e = resetBit(e, 5); break;
                    case 0xAC: cycles = 2; h = resetBit(h, 5); break;
                    case 0xAD: cycles = 2; l = resetBit(l, 5); break;
                    case 0xAF: cycles = 2; a = resetBit(a, 5); break;

                    case 0xAE: cycles = 4; mmu.WriteByte(hl, resetBit(mmu.ReadByte(hl), 5)); break;

                    case 0xB0: cycles = 2; b = resetBit(b, 6); break;
                    case 0xB1: cycles = 2; c = resetBit(c, 6); break;
                    case 0xB2: cycles = 2; d = resetBit(d, 6); break;
                    case 0xB3: cycles = 2; e = resetBit(e, 6); break;
                    case 0xB4: cycles = 2; h = resetBit(h, 6); break;
                    case 0xB5: cycles = 2; l = resetBit(l, 6); break;
                    case 0xB7: cycles = 2; a = resetBit(a, 6); break;

                    case 0xB6: cycles = 4; mmu.WriteByte(hl, resetBit(mmu.ReadByte(hl), 6)); break;

                    case 0xB8: cycles = 2; b = resetBit(b, 7); break;
                    case 0xB9: cycles = 2; c = resetBit(c, 7); break;
                    case 0xBA: cycles = 2; d = resetBit(d, 7); break;
                    case 0xBB: cycles = 2; e = resetBit(e, 7); break;
                    case 0xBC: cycles = 2; h = resetBit(h, 7); break;
                    case 0xBD: cycles = 2; l = resetBit(l, 7); break;
                    case 0xBF: cycles = 2; a = resetBit(a, 7); break;

                    case 0xBE: cycles = 4; mmu.WriteByte(hl, resetBit(mmu.ReadByte(hl), 7)); break;

                    // Set
                    case 0xC0: cycles = 2; b = setBit(b, 0); break;
                    case 0xC1: cycles = 2; c = setBit(c, 0); break;
                    case 0xC2: cycles = 2; d = setBit(d, 0); break;
                    case 0xC3: cycles = 2; e = setBit(e, 0); break;
                    case 0xC4: cycles = 2; h = setBit(h, 0); break;
                    case 0xC5: cycles = 2; l = setBit(l, 0); break;
                    case 0xC7: cycles = 2; a = setBit(a, 0); break;

                    case 0xC6: cycles = 4; mmu.WriteByte(hl, setBit(mmu.ReadByte(hl), 0)); break;

                    case 0xC8: cycles = 2; b = setBit(b, 1); break;
                    case 0xC9: cycles = 2; c = setBit(c, 1); break;
                    case 0xCA: cycles = 2; d = setBit(d, 1); break;
                    case 0xCB: cycles = 2; e = setBit(e, 1); break;
                    case 0xCC: cycles = 2; h = setBit(h, 1); break;
                    case 0xCD: cycles = 2; l = setBit(l, 1); break;
                    case 0xCF: cycles = 2; a = setBit(a, 1); break;

                    case 0xCE: cycles = 4; mmu.WriteByte(hl, setBit(mmu.ReadByte(hl), 1)); break;

                    case 0xD0: cycles = 2; b = setBit(b, 2); break;
                    case 0xD1: cycles = 2; c = setBit(c, 2); break;
                    case 0xD2: cycles = 2; d = setBit(d, 2); break;
                    case 0xD3: cycles = 2; e = setBit(e, 2); break;
                    case 0xD4: cycles = 2; h = setBit(h, 2); break;
                    case 0xD5: cycles = 2; l = setBit(l, 2); break;
                    case 0xD7: cycles = 2; a = setBit(a, 2); break;

                    case 0xD6: cycles = 4; mmu.WriteByte(hl, setBit(mmu.ReadByte(hl), 2)); break;

                    case 0xD8: cycles = 2; b = setBit(b, 3); break;
                    case 0xD9: cycles = 2; c = setBit(c, 3); break;
                    case 0xDA: cycles = 2; d = setBit(d, 3); break;
                    case 0xDB: cycles = 2; e = setBit(e, 3); break;
                    case 0xDC: cycles = 2; h = setBit(h, 3); break;
                    case 0xDD: cycles = 2; l = setBit(l, 3); break;
                    case 0xDF: cycles = 2; a = setBit(a, 3); break;

                    case 0xDE: cycles = 4; mmu.WriteByte(hl, setBit(mmu.ReadByte(hl), 3)); break;

                    case 0xE0: cycles = 2; b = setBit(b, 4); break;
                    case 0xE1: cycles = 2; c = setBit(c, 4); break;
                    case 0xE2: cycles = 2; d = setBit(d, 4); break;
                    case 0xE3: cycles = 2; e = setBit(e, 4); break;
                    case 0xE4: cycles = 2; h = setBit(h, 4); break;
                    case 0xE5: cycles = 2; l = setBit(l, 4); break;
                    case 0xE7: cycles = 2; a = setBit(a, 4); break;

                    case 0xE6: cycles = 4; mmu.WriteByte(hl, setBit(mmu.ReadByte(hl), 4)); break;

                    case 0xE8: cycles = 2; b = setBit(b, 5); break;
                    case 0xE9: cycles = 2; c = setBit(c, 5); break;
                    case 0xEA: cycles = 2; d = setBit(d, 5); break;
                    case 0xEB: cycles = 2; e = setBit(e, 5); break;
                    case 0xEC: cycles = 2; h = setBit(h, 5); break;
                    case 0xED: cycles = 2; l = setBit(l, 5); break;
                    case 0xEF: cycles = 2; a = setBit(a, 5); break;

                    case 0xEE: cycles = 4; mmu.WriteByte(hl, setBit(mmu.ReadByte(hl), 5)); break;

                    case 0xF0: cycles = 2; b = setBit(b, 6); break;
                    case 0xF1: cycles = 2; c = setBit(c, 6); break;
                    case 0xF2: cycles = 2; d = setBit(d, 6); break;
                    case 0xF3: cycles = 2; e = setBit(e, 6); break;
                    case 0xF4: cycles = 2; h = setBit(h, 6); break;
                    case 0xF5: cycles = 2; l = setBit(l, 6); break;
                    case 0xF7: cycles = 2; a = setBit(a, 6); break;

                    case 0xF6: cycles = 4; mmu.WriteByte(hl, setBit(mmu.ReadByte(hl), 6)); break;

                    case 0xF8: cycles = 2; b = setBit(b, 7); break;
                    case 0xF9: cycles = 2; c = setBit(c, 7); break;
                    case 0xFA: cycles = 2; d = setBit(d, 7); break;
                    case 0xFB: cycles = 2; e = setBit(e, 7); break;
                    case 0xFC: cycles = 2; h = setBit(h, 7); break;
                    case 0xFD: cycles = 2; l = setBit(l, 7); break;
                    case 0xFF: cycles = 2; a = setBit(a, 7); break;

                    case 0xFE: cycles = 4; mmu.WriteByte(hl, setBit(mmu.ReadByte(hl), 7)); break;
                    #endregion

                    default: throw new NotImplementedException();
                }
                break;
            #endregion

                default: throw new NotImplementedException();
            }
            #endregion

            if (diTimer >= 0) { diTimer--; if (diTimer == 0) interrupts = false; }
            if (eiTimer >= 0) { eiTimer--; if (eiTimer == 0) interrupts = true; }
        }

        private byte add(byte left, byte right)
        {
            int result = left + right;
            f = 0;
            f |= result == 0 ? Flags.Zero : 0;
            f |= (left & right & 0x8) == 0 ? 0 : Flags.HalfCarry;
            f |= result > 0xFF ? Flags.Carry : 0;
            return (byte)result;
        }

        private ushort add(ushort left, ushort right)
        {
            int result = left + right;
            f = 0;
            f |= result == 0 ? Flags.Zero : 0;
            f |= (left & right & 0x800) == 0 ? 0 : Flags.HalfCarry;
            f |= result > 0xFF ? Flags.Carry : 0;
            return (ushort)result;
        }

        private byte addCarry(byte left, byte right)
        {
            int result = left + right + (f & Flags.Carry) == 0 ? 0 : 1;
            f = 0;
            f |= result == 0 ? Flags.Zero : 0;
            f |= (left & right & 0x10) == 0 ? 0 : Flags.HalfCarry;
            f |= result > 0xFF ? Flags.Carry : 0;
            return (byte)result;
        }

        private byte subtract(byte left, byte right)
        {
            byte result = (byte)(left - right);
            f = 0;
            f |= result == 0 ? Flags.Zero : 0;
            f |= Flags.Subtract;
            f |= (left & 0xF) > (right & 0xF) ? 0 : Flags.HalfCarry;
            f |= result <= left ? 0 : Flags.Carry;
            return result;
        }

        private byte subtractCarry(byte left, byte right)
        {
            byte result = (byte)(((f & Flags.Carry) == 0 ? 0 : 1) + left - right);
            f = 0;
            f |= result == 0 ? Flags.Zero : 0;
            f |= Flags.Subtract;
            f |= (left & 0xF) > (right & 0xF) ? 0 : Flags.HalfCarry;
            f |= result <= left ? 0 : Flags.Carry;
            return result;
        }

        private byte swapByte(byte value)
        {
            uint val = value;
            byte toReturn = (byte)((value << 4) | (value >> 4));
            f = 0; f |= toReturn == 0 ? Flags.Zero : 0;
            return toReturn;
        }

        private byte rotateLeft(byte value)
        {
            f = 0;
            f |= (value & 0x80) == 0 ? 0 : Flags.Carry;
            value <<= 1;
            f |= value == 0 ? Flags.Zero : 0;
            return value;
        }

        private byte rotateRight(byte value)
        {
            f = 0;
            f |= (value & 0x01) == 0 ? 0 : Flags.Carry;
            value >>= 1;
            f |= value == 0 ? Flags.Zero : 0;
            return value;
        }

        private void testBit(byte b, int bit)
        {
            var value = (b & (1 << bit)) != 0;
            var tmp = f;
            f |= 0;
            f |= (tmp & Flags.Carry);
            if (!value) f |= Flags.Zero;
        }

        private byte setBit(byte b, int bit)
        {
            return (byte)(b | (1 << bit));
        }

        private byte resetBit(byte b, int bit)
        {
            return (byte)(b & ~(1 << bit));
        }
    }
}
