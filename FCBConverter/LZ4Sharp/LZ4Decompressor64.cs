using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace LZ4Sharp
{





    /// <summary>
    /// Class for decompressing an LZ4 compressed byte array.
    /// </summary>
    public unsafe class LZ4Decompressor64
    {
        public bool Different { set; get; }

        const int STEPSIZE = 8;
        static byte[] DeBruijnBytePos = new byte[64] { 0, 0, 0, 0, 0, 1, 1, 2, 0, 3, 1, 3, 1, 4, 2, 7, 0, 2, 3, 6, 1, 5, 3, 5, 1, 3, 4, 4, 2, 5, 6, 7, 7, 0, 1, 2, 3, 3, 4, 6, 2, 6, 5, 5, 3, 4, 5, 6, 7, 1, 2, 4, 6, 4, 4, 5, 7, 2, 6, 5, 7, 6, 7, 7 };
        //**************************************
        // Macros
        //**************************************
        readonly sbyte[] m_DecArray = new sbyte[8] { 0, 3, 2, 3, 0, 0, 0, 0 };
        readonly sbyte[] m_Dec2table = new sbyte[8] { 0, 0, 0, -1, 0, 1, 2, 3 };
        // Note : The decoding functions LZ4_uncompress() and LZ4_uncompress_unknownOutputSize()
        //              are safe against "buffer overflow" attack type
        //              since they will *never* write outside of the provided output buffer :
        //              they both check this condition *before* writing anything.
        //              A corrupted packet however can make them *read* within the first 64K before the output buffer.

        /// <summary>
        /// Decompress.
        /// </summary>
        /// <param name="source">compressed array</param>
        /// <param name="dest">This must be the exact length of the decompressed item</param>
        public void DecompressKnownSize(byte[] compressed, byte[] decompressed)
        {
            int len = DecompressKnownSize(compressed, decompressed, decompressed.Length);
            Debug.Assert(len == decompressed.Length);
        }

        public int DecompressKnownSize(byte[] compressed, byte[] decompressedBuffer, int decompressedSize)
        {
            fixed (byte* src = compressed)
            fixed (byte* dst = decompressedBuffer)
                return DecompressKnownSize(src, dst, decompressedSize);
        }

        public int DecompressKnownSize(byte* compressed, byte* decompressedBuffer, int decompressedSize)
        {
            fixed (sbyte* dec = m_DecArray)

            fixed (sbyte* dec2Ptr = m_Dec2table)
            {
                // Local Variables
                byte* ip = (byte*)compressed;
                byte* r;

                byte* op = (byte*)decompressedBuffer;
                byte* oend = op + decompressedSize;
                byte* cpy;

                byte token;
                int len, length;


                // Main Loop
                while (true)
                {
                    // get runLength
                    token = *ip++;
                    if ((length = (token >> LZ4Util.ML_BITS)) == LZ4Util.RUN_MASK) { for (; (len = *ip++) == 255; length += 255) { } length += len; }


                    cpy = op + length;
                    if (cpy > oend - LZ4Util.COPYLENGTH)
                    {
                        if (cpy > oend) goto _output_error;
                        LZ4Util.CopyMemory(op, ip, length);
                        ip += length;
                        break;
                    }

                    do { *(ulong*)op = *(ulong*)ip; op += 8; ip += 8; } while (op < cpy); ; ip -= (op - cpy); op = cpy;


                    // get offset
                    { r = (cpy) - *(ushort*)ip; }; ip += 2;
                    if (r < decompressedBuffer) goto _output_error;

                    // get matchLength
                    if ((length = (int)(token & LZ4Util.ML_MASK)) == LZ4Util.ML_MASK) { for (; *ip == 255; length += 255) { ip++; } length += *ip++; }

                    // copy repeated sequence
                    if (op - r < STEPSIZE)
                    {

                        var dec2 = dec2Ptr[(int)(op - r)];





                        *op++ = *r++;
                        *op++ = *r++;
                        *op++ = *r++;
                        *op++ = *r++;
                        r -= dec[op - r];
                        *(uint*)op = *(uint*)r; op += STEPSIZE - 4;
                        r -= dec2;
                    }
                    else { *(ulong*)op = *(ulong*)r; op += 8; r += 8; ; }
                    cpy = op + length - (STEPSIZE - 4);
                    if (cpy > oend - LZ4Util.COPYLENGTH)
                    {
                        if (cpy > oend) goto _output_error;

                        if (op < (oend - LZ4Util.COPYLENGTH)) do { *(ulong*)op = *(ulong*)r; op += 8; r += 8; } while (op < (oend - LZ4Util.COPYLENGTH)); ;
                        while (op < cpy) *op++ = *r++;
                        op = cpy;
                        if (op == oend) break;
                        continue;
                    }

                    if (op < cpy) do { *(ulong*)op = *(ulong*)r; op += 8; r += 8; } while (op < cpy); ;
                    op = cpy; // correction
                }

                // end of decoding
                return (int)(((byte*)ip) - compressed);

                // write overflow error detected
            _output_error:
                return (int)(-(((byte*)ip) - compressed));
            }
        }

        public byte[] Decompress(byte[] compressed)
        {
            int length = compressed.Length;
            int len;
            byte[] dest;
            const int Multiplier = 4; // Just a number. Determines how fast length should increase.
            do
            {
                length *= Multiplier;
                dest = new byte[length];
                len = Decompress(compressed, dest, compressed.Length);
            }
            while (len < 0 || dest.Length < len);

            byte[] d = new byte[len];
            Buffer.BlockCopy(dest, 0, d, 0, d.Length);
            return d;
        }

        public int Decompress(byte[] compressed, byte[] decompressedBuffer)
        {
            return Decompress(compressed, decompressedBuffer, compressed.Length);
        }

        public int Decompress(byte[] compressedBuffer, byte[] decompressedBuffer, int compressedSize)
        {
            fixed (byte* src = compressedBuffer)
            fixed (byte* dst = decompressedBuffer)
                return Different ? DecompressA(src, dst, compressedSize, decompressedBuffer.Length) : Decompress(src, dst, compressedSize, decompressedBuffer.Length);
        }

        public int Decompress(byte[] compressedBuffer, int compressedPosition, byte[] decompressedBuffer, int decompressedPosition, int compressedSize)
        {
            fixed (byte* src = &compressedBuffer[compressedPosition])
            fixed (byte* dst = &decompressedBuffer[decompressedPosition])
                return Decompress(src, dst, compressedSize, decompressedBuffer.Length);
        }

        public int Decompress(
            byte* compressedBuffer,
            byte* decompressedBuffer,
            int compressedSize,
            int maxDecompressedSize)
        {
            fixed (sbyte* dec = m_DecArray)

            fixed (sbyte* dec2Ptr = m_Dec2table)
            {
                // Local Variables
                byte* ip = (byte*)compressedBuffer;
                byte* iend = ip + compressedSize;
                byte* r;

                byte* op = (byte*)decompressedBuffer;
                byte* oend = op + maxDecompressedSize;
                byte* cpy;

                byte token;
                int length;


                // Main Loop
                while (ip < iend)
                {
                    // get runLength
                    token = *ip++;
                    if ((length = (token >> LZ4Util.ML_BITS)) == LZ4Util.RUN_MASK) { int s = 255; while ((ip < iend) && (s == 255)) { s = *ip++; length += s; } }

                    // copy literals
                    cpy = op + length;
                    if ((cpy > oend - LZ4Util.COPYLENGTH) || (ip + length > iend - LZ4Util.COPYLENGTH))
                    {
                        if (cpy > oend) goto _output_error; // Error : request to write beyond destination buffer
                        if (ip + length > iend) goto _output_error; // Error : request to read beyond source buffer
                        LZ4Util.CopyMemory(op, ip, length);
                        op += length;
                        ip += length;
                        if (ip < iend) goto _output_error; // Error : LZ4 format violation
                        break; //Necessarily EOF
                    }

                    do { *(ulong*)op = *(ulong*)ip; op += 8; ip += 8; } while (op < cpy); ; ip -= (op - cpy); op = cpy;

                    // get offset
                    { r = (cpy) - *(ushort*)ip; }; ip += 2;
                    if (r < decompressedBuffer) goto _output_error;

                    // get matchlength
                    if ((length = (int)(token & LZ4Util.ML_MASK)) == LZ4Util.ML_MASK) { while (ip < iend) { int s = *ip++; length += s; if (s == 255) continue; break; } }

                    // copy repeated sequence
                    if (op - r < STEPSIZE)
                    {

                        var dec2 = dec2Ptr[op - r];




                        *op++ = *r++;
                        *op++ = *r++;
                        *op++ = *r++;
                        *op++ = *r++;
                        r -= dec[op - r];
                        *(uint*)op = *(uint*)r; op += STEPSIZE - 4;
                        r -= dec2;
                    }
                    else { *(ulong*)op = *(ulong*)r; op += 8; r += 8; ; }
                    cpy = op + length - (STEPSIZE - 4);
                    if (cpy > oend - LZ4Util.COPYLENGTH)
                    {
                        if (cpy > oend) goto _output_error;

                        if (op < (oend - LZ4Util.COPYLENGTH)) do { *(ulong*)op = *(ulong*)r; op += 8; r += 8; } while (op < (oend - LZ4Util.COPYLENGTH)); ;
                        while (op < cpy) *op++ = *r++;
                        op = cpy;
                        if (op == oend) goto _output_error; // Check EOF (should never happen, since last 5 bytes are supposed to be literals)
                        continue;
                    }
                    if (op < cpy) do { *(ulong*)op = *(ulong*)r; op += 8; r += 8; } while (op < cpy); ;
                    op = cpy; // correction
                }


                return (int)(((byte*)op) - decompressedBuffer);


            _output_error:
                return (int)(-(((byte*)ip) - compressedBuffer));
            }
        }

        public unsafe int DecompressA(byte* compressedBuffer, byte* decompressedBuffer, int compressedSize, int maxDecompressedSize)
        {
            fixed (sbyte* ptr8 = m_DecArray)
            {
                fixed (sbyte* ptr7 = m_Dec2table)
                {
                    byte* ptr = compressedBuffer;
                    byte* ptr2 = ptr + compressedSize;
                    byte* ptr3 = decompressedBuffer;
                    byte* ptr4 = ptr3 + maxDecompressedSize;

                    int prf = 0;
                    do
                    {
                        prf = *ptr++;
                    }
                    while (prf > 127);

                    while (true)
                    {
                        if (ptr < ptr2)
                        {
                            byte b = *(ptr++);
                            int num;
                            if ((long)(num = b >> 4) == 15)
                            {
                                int num2 = 255;
                                while (ptr < ptr2 && num2 == 255)
                                {
                                    num2 = *(ptr++);
                                    num += num2;
                                }
                            }
                            byte* ptr5 = ptr3 + num;
                            if (ptr5 > ptr4 - 8 || ptr + num > ptr2 - 8)
                            {
                                if (ptr5 > ptr4 || ptr + num > ptr2)
                                {
                                    break;
                                }
                                LZ4Util.CopyMemory(ptr3, ptr, num);
                                ptr3 += num;
                                ptr += num;
                                if (ptr < ptr2)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                do
                                {
                                    *(long*)ptr3 = *(long*)ptr;
                                    ptr3 += 8;
                                    ptr += 8;
                                }
                                while (ptr3 < ptr5);
                                ptr -= ptr3 - ptr5;
                                ptr3 = ptr5;
                                int num3 = *(ushort*)ptr;
                                ptr += 2;
                                if (num3 >= 57344)
                                {
                                    num3 += *(ptr++) << 13;
                                }
                                byte* ptr6 = ptr5 - num3;
                                if (ptr6 < decompressedBuffer)
                                {
                                    break;
                                }
                                if ((long)(num = b & 0xF) == 15)
                                {
                                    while (ptr < ptr2)
                                    {
                                        int num4 = *(ptr++);
                                        num += num4;
                                        if (num4 != 255)
                                        {
                                            break;
                                        }
                                    }
                                }
                                if (ptr3 - ptr6 < 8)
                                {
                                    sbyte b2 = ptr7[ptr3 - ptr6];
                                    *(ptr3++) = *(ptr6++);
                                    *(ptr3++) = *(ptr6++);
                                    *(ptr3++) = *(ptr6++);
                                    *(ptr3++) = *(ptr6++);
                                    ptr6 -= ptr8[ptr3 - ptr6];
                                    *(int*)ptr3 = *(int*)ptr6;
                                    ptr3 += 4;
                                    ptr6 -= b2;
                                }
                                else
                                {
                                    *(long*)ptr3 = *(long*)ptr6;
                                    ptr3 += 8;
                                    ptr6 += 8;
                                }
                                ptr5 = ptr3 + num - 4;
                                if (ptr5 > ptr4 - 8)
                                {
                                    if (ptr5 > ptr4)
                                    {
                                        break;
                                    }
                                    if (ptr3 < ptr4 - 8)
                                    {
                                        do
                                        {
                                            *(long*)ptr3 = *(long*)ptr6;
                                            ptr3 += 8;
                                            ptr6 += 8;
                                        }
                                        while (ptr3 < ptr4 - 8);
                                    }
                                    while (ptr3 < ptr5)
                                    {
                                        *(ptr3++) = *(ptr6++);
                                    }
                                    ptr3 = ptr5;
                                    if (ptr3 == ptr4)
                                    {
                                        break;
                                    }
                                    continue;
                                }
                                if (ptr3 < ptr5)
                                {
                                    do
                                    {
                                        *(long*)ptr3 = *(long*)ptr6;
                                        ptr3 += 8;
                                        ptr6 += 8;
                                    }
                                    while (ptr3 < ptr5);
                                }
                                ptr3 = ptr5;
                                if (ptr2 - ptr < ptr4 - ptr3)
                                {
                                    continue;
                                }
                                long num5 = ptr2 - ptr - (ptr4 - ptr3);
                                LZ4Util.CopyMemory(ptr3, ptr + num5, ptr4 - ptr3);
                            }
                        }
                        return (int)(ptr3 - decompressedBuffer);
                    }
                    return (int)(-(ptr - compressedBuffer));
                }
            }
        }
    }
}
