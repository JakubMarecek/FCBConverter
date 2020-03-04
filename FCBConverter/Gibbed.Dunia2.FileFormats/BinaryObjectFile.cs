/* Copyright (c) 2012 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

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
using Gibbed.IO;

namespace Gibbed.Dunia2.FileFormats
{
    public class BinaryObjectFile
    {
        public const uint Signature = 0x4643626E; // 'FCbn' FarCry Binary N???

        public ushort Version = 2;
        public HeaderFlags Flags = HeaderFlags.None;
        public BinaryObject Root;

        public static Dictionary<string, uint> pointersFields;
        public static Dictionary<string, uint> pointersObjects;
        public static uint offsetsObjectsNum = 0;
        public static uint xxx = 0;

        public void Serialize(Stream output)
        {
            if (this.Version != 2)
            {
                throw new FormatException("unsupported file version");
            }

            if (this.Flags != HeaderFlags.None)
            {
                throw new FormatException("unsupported file flags");
            }

            pointersFields = new Dictionary<string, uint>();
            pointersObjects = new Dictionary<string, uint>();

            var endian = Endian.Little;
            using (var data = new MemoryStream())
            {/*
                uint totalObjectCount = 0, totalValueCount = 0;

                if (matHeaders != null && matHeaders.Length > 0)
                {
                    data.WriteValueU32(SignatureMAT, endian);
                    data.Write(matHeaders, 0, 16);
                }
                else
                    data.WriteValueU32(Signature, endian);
                data.WriteValueU16(this.Version, endian);
                data.WriteValueEnum<HeaderFlags>(this.Flags, endian);
                data.WriteValueU32(totalObjectCount, endian);
                data.WriteValueU32(totalValueCount, endian);

                uint objectCount = 0;
                this.Root.Serialize(data,ref totalObjectCount,
                                    ref totalValueCount,
                                    endian,
                                    ref objectCount);
                data.Flush();
                data.Position = 0;

                output.WriteFromStream(data, data.Length);
                */

                uint totalObjectCount = 1, totalValueCount = 0;

                this.Root.Serialize(data, ref totalObjectCount,
                                    ref totalValueCount,
                                    endian);
                data.Flush();
                data.Position = 0;

                output.WriteValueU32(Signature, endian);
                output.WriteValueU16(this.Version, endian);
                output.WriteValueEnum<HeaderFlags>(this.Flags, endian);
                output.WriteValueU32(totalObjectCount, endian);
                output.WriteValueU32(totalValueCount, endian);
                output.WriteFromStream(data, data.Length);
            }
        }

        public void Deserialize(Stream input)
        {
            var magic = input.ReadValueU32(Endian.Little);
            if (magic != Signature) // FCbn
            {
                throw new FormatException("invalid header magic");
            }

            var endian = Endian.Little;

            var version = input.ReadValueU16(endian);
            if (version != 2)
            {
                throw new FormatException("unsupported file version");
            }

            var flags = input.ReadValueEnum<HeaderFlags>(endian);
            if (flags != HeaderFlags.None)
            {
                throw new FormatException("unsupported file flags");
            }

            var totalObjectCount = input.ReadValueU32(endian);
            var totalValueCount = input.ReadValueU32(endian);

            var pointers = new List<BinaryObject>();

            this.Version = version;
            this.Flags = flags;
            this.Root = BinaryObject.Deserialize(null, input, pointers, endian);
        }

        [Flags]
        public enum HeaderFlags : ushort
        {
            None = 0,

            Debug = 1 << 0, // "Not Stripped"
        }
    }
}
