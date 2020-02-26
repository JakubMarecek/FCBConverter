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
}
