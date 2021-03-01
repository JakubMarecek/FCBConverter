/* 
 * FCBConverter
 * Copyright (C) 2020  Jakub Mareček (info@jakubmarecek.cz)
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with FCBConverter.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FCBConverter
{
    static class Helpers
    {
        public static List<byte[]> UnpackArray(byte[] buffer, int size)
        {
            int count = BitConverter.ToInt32(buffer, 0);

            var offset = 4;
            var result = new List<byte[]>();

            for (int i = 0; i < count; i++)
            {
                byte[] a = new byte[size];
                Buffer.BlockCopy(buffer, offset, a, 0, size);
                result.Add(a);
                offset += size;
            }

            return result;
        }

        public static List<byte[]> UnpackArraySize(byte[] buffer, int size)
        {
            int count = Convert.ToInt32(Math.Floor(buffer.Length / (decimal)size));

            var offset = 0;
            var result = new List<byte[]>();

            for (int i = 0; i < count; i++)
            {
                byte[] a = new byte[size];
                Buffer.BlockCopy(buffer, offset, a, 0, size);
                result.Add(a);
                offset += size;
            }

            return result;
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static string GetLast(this string source, int tail_length)
        {
            if (tail_length >= source.Length)
                return source;
            return source.Substring(source.Length - tail_length);
        }

        public static bool ContainsCI(this string source, string toCheck)
        {
            return source?.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool StartsWithType(this string source, string toCheck)
        {
            bool starts = source.StartsWith(toCheck);
            if (starts)
            {
                if (source.Length > toCheck.Length)
                {
                    bool secondChar = char.IsUpper(source[toCheck.Length]);
                    return starts && secondChar;
                }
            }
            return false;
        }
    }

    static class StreamExtensions
    {
        /// <summary>
        /// Advances the supplied stream until the given searchBytes are found, without advancing too far (consuming any bytes from the stream after the searchBytes are found).
        /// Regarding efficiency, if the stream is network or file, then MEMORY/CPU optimisations will be of little consequence here.
        /// </summary>
        /// <param name="stream">The stream to search in</param>
        /// <param name="searchBytes">The byte sequence to search for</param>
        /// <returns></returns>
        public static int ScanUntilFound(this Stream stream, byte[] searchBytes)
        {
            // For this class code comments, a common example is assumed:
            // searchBytes are {1,2,3,4} or 1234 for short
            // # means value that is outside of search byte sequence

            byte[] streamBuffer = new byte[searchBytes.Length];
            int nextRead = searchBytes.Length;
            int totalScannedBytes = 0;

            while (true)
            {
                FillBuffer(stream, streamBuffer, nextRead);
                totalScannedBytes += nextRead; //this is only used for final reporting of where it was found in the stream

                if (ArraysMatch(searchBytes, streamBuffer, 0))
                    return totalScannedBytes; //found it

                nextRead = FindPartialMatch(searchBytes, streamBuffer);
            }
        }

        /// <summary>
        /// Check all offsets, for partial match. 
        /// </summary>
        /// <param name="searchBytes"></param>
        /// <param name="streamBuffer"></param>
        /// <returns>The amount of bytes which need to be read in, next round</returns>
        static int FindPartialMatch(byte[] searchBytes, byte[] streamBuffer)
        {
            // 1234 = 0 - found it. this special case is already catered directly in ScanUntilFound            
            // #123 = 1 - partially matched, only missing 1 value
            // ##12 = 2 - partially matched, only missing 2 values
            // ###1 = 3 - partially matched, only missing 3 values
            // #### = 4 - not matched at all

            for (int i = 1; i < searchBytes.Length; i++)
            {
                if (ArraysMatch(searchBytes, streamBuffer, i))
                {
                    // EG. Searching for 1234, have #123 in the streamBuffer, and [i] is 1
                    // Output: 123#, where # will be read using FillBuffer next. 
                    Array.Copy(streamBuffer, i, streamBuffer, 0, searchBytes.Length - i);
                    return i; //if an offset of [i], makes a match then only [i] bytes need to be read from the stream to check if there's a match
                }
            }

            return 4;
        }

        /// <summary>
        /// Reads bytes from the stream, making sure the requested amount of bytes are read (streams don't always fulfill the full request first time)
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <param name="streamBuffer">The buffer to read into</param>
        /// <param name="bytesNeeded">How many bytes are needed. If less than the full size of the buffer, it fills the tail end of the streamBuffer</param>
        static void FillBuffer(Stream stream, byte[] streamBuffer, int bytesNeeded)
        {
            // EG1. [123#] - bytesNeeded is 1, when the streamBuffer contains first three matching values, but now we need to read in the next value at the end 
            // EG2. [####] - bytesNeeded is 4

            var bytesAlreadyRead = streamBuffer.Length - bytesNeeded; //invert
            while (bytesAlreadyRead < streamBuffer.Length)
            {
                bytesAlreadyRead += stream.Read(streamBuffer, bytesAlreadyRead, streamBuffer.Length - bytesAlreadyRead);
            }
        }

        /// <summary>
        /// Checks if arrays match exactly, or with offset. 
        /// </summary>
        /// <param name="searchBytes">Bytes to search for. Eg. [1234]</param>
        /// <param name="streamBuffer">Buffer to match in. Eg. [#123] </param>
        /// <param name="startAt">When this is zero, all bytes are checked. Eg. If this value 1, and it matches, this means the next byte in the stream to read may mean a match</param>
        /// <returns></returns>
        static bool ArraysMatch(byte[] searchBytes, byte[] streamBuffer, int startAt)
        {
            for (int i = 0; i < searchBytes.Length - startAt; i++)
            {
                if (searchBytes[i] != streamBuffer[i + startAt])
                    return false;
            }
            return true;
        }
    }
}
