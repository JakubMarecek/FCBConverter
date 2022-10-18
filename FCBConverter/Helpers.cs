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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

        public static int SearchBytes(byte[] haystack, byte[] needle, int start_index)
        {
            int len = needle.Length;
            int limit = haystack.Length - len;
            for (int i = start_index; i <= limit; i++)
            {
                int k = 0;
                for (; k < len; k++)
                {
                    if (needle[k] != haystack[i + k]) break;
                }
                if (k == len) return i;
            }
            return -1;
        }

        public static int[] SearchBytesMultiple(byte[] haystack, byte[] needle)
        {
            int index = 0;

            List<int> results = new List<int>();

            while (true)
            {
                index = SearchBytes(haystack, needle, index);

                if (index == -1)
                    break;

                results.Add(index);

                index += needle.Length;
            }

            return results.ToArray();
        }

        public struct HideFacesStruct
        {
            public ulong id;
            public ushort start;
            public ushort count;
        }
    }

    /// <summary>
    /// Arguments class
    /// </summary>
    public class Arguments
    {
        // Variables
        private StringDictionary Parameters;

        // Constructor
        public Arguments(string[] Args)
        {
            Parameters = new StringDictionary();
            Regex Spliter = new Regex(@"^-{1,2}|^/|=|:",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            Regex Remover = new Regex(@"^['""]?(.*?)['""]?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            string Parameter = null;
            string[] Parts;

            // Valid parameters forms:
            // {-,/,--}param{ ,=,:}((",')value(",'))
            // Examples: 
            // -param1 value1 --param2 /param3:"Test-:-work" 
            //   /param4=happy -param5 '--=nice=--'
            foreach (string Txt in Args)
            {
                // Look for new parameters (-,/ or --) and a
                // possible enclosed value (=,:)
                Parts = Spliter.Split(Txt, 3);

                switch (Parts.Length)
                {
                    // Found a value (for the last parameter 
                    // found (space separator))
                    case 1:
                        if (Parameter != null)
                        {
                            if (!Parameters.ContainsKey(Parameter))
                            {
                                Parts[0] =
                                    Remover.Replace(Parts[0], "$1");

                                Parameters.Add(Parameter, Parts[0]);
                            }
                            Parameter = null;
                        }
                        // else Error: no parameter waiting for a value (skipped)
                        break;

                    // Found just a parameter
                    case 2:
                        // The last parameter is still waiting. 
                        // With no value, set it to true.
                        if (Parameter != null)
                        {
                            if (!Parameters.ContainsKey(Parameter))
                                Parameters.Add(Parameter, "true");
                        }
                        Parameter = Parts[1];
                        break;

                    // Parameter with enclosed value
                    case 3:
                        // The last parameter is still waiting. 
                        // With no value, set it to true.
                        if (Parameter != null)
                        {
                            if (!Parameters.ContainsKey(Parameter))
                                Parameters.Add(Parameter, "true");
                        }

                        Parameter = Parts[1];

                        // Remove possible enclosing characters (",')
                        if (!Parameters.ContainsKey(Parameter))
                        {
                            Parts[2] = Remover.Replace(Parts[2], "$1");
                            Parameters.Add(Parameter, Parts[2]);
                        }

                        Parameter = null;
                        break;
                }
            }
            // In case a parameter is still waiting
            if (Parameter != null)
            {
                if (!Parameters.ContainsKey(Parameter))
                    Parameters.Add(Parameter, "true");
            }
        }

        // Retrieve a parameter value if it exists 
        // (overriding C# indexer property)
        public string this[string Param]
        {
            get
            {
                return (Parameters[Param]);
            }
        }

        public IEnumerable<DictionaryEntry> AsEnumerable()
        {
            foreach (DictionaryEntry value in Parameters)
            {
                yield return value;
            }
        }
    }
}
