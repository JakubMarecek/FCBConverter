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

using FCBConverter;
using Gibbed.Dunia2.BinaryObjectInfo;
using Gibbed.Dunia2.FileFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Gibbed.Dunia2.ConvertBinaryObject
{
    public static class Exporting
    {
        public static void Export(string outputPath, BinaryObjectFile bof)
        {
            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                CheckCharacters = false,
                OmitXmlDeclaration = false
            };

            using (var writer = XmlWriter.Create(outputPath, settings))
            {
                writer.WriteStartDocument();
                writer.WriteComment(Program.xmlheader);
                writer.WriteComment(Program.xmlheaderfcb);
                writer.WriteComment(Program.xmlheaderfcb2);
                WriteNode(writer, new BinaryObject[0], bof.Root);
                writer.WriteEndDocument();
            }
        }

        internal const uint EntityLibrariesHash = 0xBCDD10B4u; // crc32(EntityLibraries)
        internal const uint EntityLibraryHash = 0xE0BDB3DBu; // crc32(EntityLibrary)
        internal const uint NameHash = 0xFE11D138u; // crc32(Name);
        internal const uint EntityLibraryItemHash = 0x256A1FF9u; // unknown source name
        internal const uint DisLibItemIdHash = 0x8EDB0295u; // crc32(disLibItemId)
        internal const uint EntityHash = 0x0984415Eu; // crc32(Entity)
        internal const uint LibHash = 0xA90F3BCC; // crc32(lib)
        internal const uint LibItemHash = 0x72DE4948; // unknown source name
        internal const uint TextHidNameHash = 0x9D8873F8; // crc32(text_hidName)
        internal const uint NomadObjectTemplatesHash = 0x4C4C4CA4; // crc32(NomadObjectTemplates)
        internal const uint NomadObjectTemplateHash = 0x142371CF; // unknown source name
        internal const uint TemplateHash = 0x6E167DD5; // crc32(Template)

        private static void WriteNode(XmlWriter writer,
                                      IEnumerable<BinaryObject> parentChain,
                                      BinaryObject node)
        {
            var chain = parentChain.Concat(new[] { node });
            /*
            if (def != null &&
                def.ClassFieldHash.HasValue == true)
            {
                if (node.Fields.ContainsKey(def.ClassFieldHash.Value) == true)
                {
                    var hash = FieldTypeDeserializers.Deserialize<uint>(FieldType.UInt32,
                                                                        node.Fields[def.ClassFieldHash.Value]);
                    def = infoManager.GetClassDefinition(hash);
                }
            }*/

            writer.WriteStartElement("object");

            writer.WriteAttributeString("hash", node.NameHash.ToString("X8"));

            if (FCBConverter.Program.listStringsDict.ContainsKey(node.NameHash))
                writer.WriteAttributeString("name", FCBConverter.Program.listStringsDict[node.NameHash]);

            if (node.Fields != null)
            {
                if (Program.isCombinedMoveFile && node.Fields.Count == 2 && node.Fields.ContainsKey(CRC32.Hash("offsetsArray")) && node.Fields.ContainsKey(CRC32.Hash("hashesArray")))
                {
                    FCBConverter.CombinedMoveFile.OffsetsHashesArray.Deserialize(node.Fields);
                }
                else if (Program.isCombinedMoveFile && node.Fields.Count == 3 && node.Fields.ContainsKey(CRC32.Hash("rootNodeIds")) && node.Fields.ContainsKey(CRC32.Hash("resourcePathIds")))
                {
                    FCBConverter.CombinedMoveFile.PerMoveResourceInfo.Deserialize(node.Fields);
                }
                else
                {
                    string prevNodeVal = "";

                    foreach (var kv in node.Fields)
                    {
                        string prefix = "value-";

                        string name = "";
                        if (FCBConverter.Program.listStringsDict.ContainsKey(kv.Key))
                        {
                            name = FCBConverter.Program.listStringsDict[kv.Key];
                        }

                        if (name == "CNH_UncompressedSize")
                            writer.WriteComment("Do not edit CNH_UncompressedSize and CNH_CompressedSize, they will be changed during converting back to FCB.");

                        writer.WriteStartElement("field");
                        writer.WriteAttributeString("hash", kv.Key.ToString("X8"));

                        if (name != "")
                            writer.WriteAttributeString("name", name);

                        string str = Encoding.ASCII.GetString(kv.Value, 0, kv.Value.Length - 1);
                        //str = Regex.Replace(str, @"\p{C}+", string.Empty);

                        string binaryHex = Helpers.ByteArrayToString(kv.Value);
                        bool resetPrevNodeVal = true;
                        bool stringRegex = Regex.IsMatch(str, @"^[a-zA-Z0-9_.:\/\-\x20\\\(\)]+$");

                        byte[] reversed = new byte[kv.Value.Length];
                        for (int i = 0; i < reversed.Length; i++)
                        {
                            reversed[i] = kv.Value[i];
                        }

                        Array.Reverse(reversed);
                        string binaryHexRev = Helpers.ByteArrayToString(reversed);

                        bool isHash32 = (prevNodeVal != "" && CRC32.Hash(prevNodeVal).ToString("x8").ToLower() == binaryHexRev.ToLower());
                        bool isHash64 = (prevNodeVal != "" && CRC64.Hash(prevNodeVal).ToString("x16").ToLower() == binaryHexRev.ToLower());

                        if (name == "data")
                        {
                            var moveBinDataChunk = new FCBConverter.CombinedMoveFile.MoveBinDataChunk(Program.isNewDawn);
                            moveBinDataChunk.Deserialize(writer, kv.Value);
                        }
                        else if (name == "TypeIds")
                        {
                            ReadListHashes(kv.Value, writer, "TypeId");
                        }
                        else if (name == "ResIds")
                        {
                            ReadListFiles(kv.Value, writer, "ResId");
                        }
                        else if (name == "ArchetypeResDepList")
                        {
                            ReadListFiles(node.Fields[CRC32.Hash("ArchetypeResDepList")], writer, "Resource");
                        }
                        else if (name == "hidShapePoints")
                        {
                            byte[] trimmed = kv.Value.Skip(4).ToArray();
                            List<byte[]> unpA = Helpers.UnpackArraySize(trimmed, 12);

                            foreach (byte[] pnt in unpA)
                            {
                                byte[] vecX = pnt.Skip(0).Take(4).ToArray();
                                byte[] vecY = pnt.Skip(4).Take(4).ToArray();
                                byte[] vecZ = pnt.Skip(8).Take(4).ToArray();

                                float vecfX = BitConverter.ToSingle(vecX, 0);
                                float vecfY = BitConverter.ToSingle(vecY, 0);
                                float vecfZ = BitConverter.ToSingle(vecZ, 0);

                                writer.WriteStartElement("Point");
                                writer.WriteString(vecfX.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," +
                                    vecfY.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," +
                                    vecfZ.ToString(System.Globalization.CultureInfo.InvariantCulture));
                                writer.WriteEndElement();
                            }
                        }
                        else if (name == "hidDescriptor")
                        {
                            if (kv.Value[0] == 0)
                            {
                                // legacy, RML format
                                MemoryStream ms = new MemoryStream(kv.Value);

                                var rez = new Gibbed.Dunia2.FileFormats.XmlResourceFile();
                                rez.Deserialize(ms);

                                writer.WriteAttributeString("legacy", "1");

                                Gibbed.Dunia2.ConvertXml.Program.WriteNode(writer, rez.Root);
                            }
                            else
                            {
                                // new format XML
                                XmlReaderSettings settings = new XmlReaderSettings
                                {
                                    IgnoreComments = true,
                                    //IgnoreWhitespace = false,
                                    IgnoreWhitespace = true,
                                    IgnoreProcessingInstructions = true
                                };
                                XmlReader xmlReader = XmlReader.Create(new System.IO.StringReader(str), settings);
                                writer.WriteNode(xmlReader, false);
                            }
                        }
                        else if (name == "CNH_CompressedData" || name == "buffer")
                        {
                            BinaryReader binaryReader = new BinaryReader(new MemoryStream(kv.Value));
                            int len = kv.Value.Length;

                            if (name != "buffer")
                                len = binaryReader.ReadInt32();
                            
                            byte[] bytes = binaryReader.ReadBytes(len);
                            binaryReader.Close();

                            string compressionType = "";

                            if (name != "buffer")
                            {
                                try
                                {
                                    bytes = new LZ4Sharp.LZ4Decompressor64().Decompress(bytes);
                                    compressionType = "LZ4";
                                }
                                catch (Exception)
                                {
                                    int uncompressedSize = BitConverter.ToInt32(node.Fields[CRC32.Hash("CNH_UncompressedSize")], 0);
                                    byte[] bytesOut = new byte[uncompressedSize];

                                    var result = Gibbed.Dunia2.FileFormats.LZO.Decompress(bytes,
                                                                0,
                                                                bytes.Length,
                                                                bytesOut,
                                                                0,
                                                                ref uncompressedSize);

                                    bytes = bytesOut;
                                    compressionType = "LZO";
                                }
                            }

                            File.WriteAllBytes(Program.m_Path + "\\tmp", bytes);
                            Program.ConvertFCB(Program.m_Path + "\\tmp", Program.m_Path + "\\tmpc");

                            XmlReaderSettings settings = new XmlReaderSettings
                            {
                                IgnoreComments = true,
                                //IgnoreWhitespace = false,
                                IgnoreWhitespace = true,
                                IgnoreProcessingInstructions = true
                            };
                            XmlReader xmlReader = XmlReader.Create(Program.m_Path + "\\tmpc", settings);
                            xmlReader.MoveToContent();
                            writer.WriteStartElement(name);
                            writer.WriteAttributeString("CompressionType", compressionType.ToString());
                            writer.WriteNode(xmlReader, false);
                            writer.WriteEndElement();
                            xmlReader.Close();

                            File.Delete(Program.m_Path + "\\tmp");
                            File.Delete(Program.m_Path + "\\tmpc");
                        }
                        // *****************************************************************************************************************
                        // list values
                        // *****************************************************************************************************************
                        /*if ((name.Contains("List") && !stringRegex && !name.Contains("Id") && !name.StartsWithType("is")) || name == "disEntityId" || name == "GameElementId" || name == "TypeId")
                        {
                            writer.WriteAttributeString("type", FieldType.BinHex.GetString());
                            writer.WriteBinHex(kv.Value, 0, kv.Value.Length);
                        }*/

                        // *****************************************************************************************************************
                        // hash32
                        // *****************************************************************************************************************
                        else if (isHash32)
                        {
                            /*if (!Program.aaaa.Contains(prevNodeVal) && name == "name")
                                Program.aaaa.Add(prevNodeVal);*/
                            writer.WriteAttributeString(prefix + "ComputeHash32", prevNodeVal);
                        }
                        // *****************************************************************************************************************
                        // hash64
                        // *****************************************************************************************************************
                        else if (isHash64)
                        {
                            writer.WriteAttributeString(prefix + "ComputeHash64", prevNodeVal);
                        }

                        // *****************************************************************************************************************
                        // enum
                        // *****************************************************************************************************************
                        else if (binaryHex.Length == 8 && name.StartsWithType("sel"))
                        {
                            int v = Int32.Parse(binaryHexRev, NumberStyles.HexNumber);
                            writer.WriteAttributeString(prefix + "Enum", v.ToString());
                        }
                        // *****************************************************************************************************************
                        // id32
                        // *****************************************************************************************************************
                        else if (binaryHex.Length == 8 && name == "TypeId")
                        {
                            uint v = BitConverter.ToUInt32(kv.Value, 0);

                            if (FCBConverter.Program.listStringsDict.ContainsKey(v))
                                writer.WriteAttributeString("value-ComputeHash32", FCBConverter.Program.listStringsDict[v]);
                        }
                        // *****************************************************************************************************************
                        // uint32
                        // *****************************************************************************************************************
                        else if (binaryHex.Length == 8 && name.StartsWithType("snd"))
                        {
                            uint v = BitConverter.ToUInt32(kv.Value, 0);
                            writer.WriteAttributeString(prefix + "UInt32", v.ToString());
                        }
                        // *****************************************************************************************************************
                        // int32
                        // *****************************************************************************************************************
                        else if (binaryHex.Length == 8 && ((binaryHex.GetLast(2) == "00" && !name.StartsWithType("f") && !name.StartsWith("text_") && !name.ContainsCI("name") && name != "String") || name.Contains("locid") || name.StartsWith("loc") || name.StartsWithType("i") || name.StartsWithType("u") || name == "SoundId" || name == "Id"))
                        {
                            int v = Int32.Parse(binaryHexRev, NumberStyles.HexNumber);
                            writer.WriteAttributeString(prefix + "Int32", v.ToString());
                        }
                        // *****************************************************************************************************************
                        // int16
                        // *****************************************************************************************************************
                        else if (binaryHex.Length == 4 && !name.StartsWithType("f"))
                        {
                            int v = Int16.Parse(binaryHexRev, NumberStyles.HexNumber);
                            writer.WriteAttributeString(prefix + "Int16", v.ToString());
                        }
                        // *****************************************************************************************************************
                        // string
                        // *****************************************************************************************************************
                        else if ((stringRegex && binaryHex.Length > 4 && binaryHex.GetLast(2) == "00") || name.StartsWith("text_"))
                        {
                            writer.WriteAttributeString(prefix + "String", str);
                            prevNodeVal = str;
                            resetPrevNodeVal = false;
                        }
                        // *****************************************************************************************************************
                        // hash64 id64
                        // *****************************************************************************************************************
                        else if ((binaryHex.Length == 16 && Regex.IsMatch(binaryHex, "^[0-9a-fA-F]{16}$", RegexOptions.Compiled) && binaryHex != "0000000000000000" && binaryHex != "ffffffffffffffff" && !name.StartsWith("vec") && !name.ContainsCI("value") && !name.StartsWith("text_")) || name == "hash")
                        {
                            if ((binaryHex.GetLast(2) != "00" && !name.ContainsCI("layerid") && !name.StartsWithType("ent") && name != "GameElementId" && !name.EndsWith("Id")) || name == "hash")
                            {
                                writer.WriteAttributeString(prefix + "Hash64", binaryHexRev);
                            }
                            else
                            {
                                //BigInteger a = BigInteger.Parse(binaryHexRev, NumberStyles.AllowHexSpecifier);
                                ulong a = BitConverter.ToUInt64(kv.Value, 0);
                                writer.WriteAttributeString(prefix + "Id64", a.ToString());
                            }
                        }
                        // *****************************************************************************************************************
                        // float32 hash32
                        // *****************************************************************************************************************
                        else if (binaryHex.Length == 8 && Regex.IsMatch(binaryHex, "^[0-9a-fA-F]{8}$", RegexOptions.Compiled) && ((binaryHex != "00000000" && binaryHex != "ffffffff" && binaryHex.GetLast(2) != "00") || name.StartsWithType("f")) && !name.StartsWith("text_")) // && !name.Contains("Id")
                        {
                            float f = BitConverter.ToSingle(kv.Value, 0);
                            if ((!name.ContainsCI("hid_DTCTH_ClassName") && !name.ContainsCI("Name") && !name.ContainsCI("BoneID") && !name.Contains("Type") && !name.ContainsCI("Id") && !name.Contains("sDetail") && !name.Contains("class") && !name.Contains("type")) || f == -1 || name.StartsWithType("f") || name.Contains("Duration") || name.Contains("Multiplier"))
                            {
                                writer.WriteAttributeString(prefix + "Float32", f.ToString(System.Globalization.CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                uint result = uint.Parse(binaryHexRev, System.Globalization.NumberStyles.HexNumber);
                                if (FCBConverter.Program.listStringsDict.ContainsKey(result))
                                    writer.WriteAttributeString(prefix + "ComputeHash32", FCBConverter.Program.listStringsDict[result]);
                                else
                                    writer.WriteAttributeString(prefix + "Hash32", binaryHexRev);
                            }
                        }
                        // *****************************************************************************************************************
                        // bool
                        // *****************************************************************************************************************
                        else if (binaryHex == "00" && (name.StartsWithType("b") || name.StartsWithType("is") || name == "loaddep"))
                        {
                            writer.WriteAttributeString(prefix + "Boolean", "False");
                        }
                        else if (binaryHex == "01" && (name.StartsWithType("b") || name.StartsWithType("is") || name == "loaddep"))
                        {
                            writer.WriteAttributeString(prefix + "Boolean", "True");
                        }
                        // *****************************************************************************************************************
                        // vec4
                        // *****************************************************************************************************************
                        else if (binaryHex.Length == 32 && (name == "Value" || name == "Info" || name.ContainsCI("offset") || name.ContainsCI("position") || name.ContainsCI("rotation") || name.ContainsCI("vector") || name.StartsWith("vec") || name.Contains("value") || name.ContainsCI("color")))
                        {
                            byte[] vecX = kv.Value.Skip(0).Take(4).ToArray();
                            byte[] vecY = kv.Value.Skip(4).Take(4).ToArray();
                            byte[] vecZ = kv.Value.Skip(8).Take(4).ToArray();
                            byte[] vecT = kv.Value.Skip(12).Take(4).ToArray();

                            float vecfX = BitConverter.ToSingle(vecX, 0);
                            float vecfY = BitConverter.ToSingle(vecY, 0);
                            float vecfZ = BitConverter.ToSingle(vecZ, 0);
                            float vecfT = BitConverter.ToSingle(vecT, 0);

                            writer.WriteAttributeString(prefix + "Vector4",
                                vecfX.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," +
                                vecfY.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," +
                                vecfZ.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," +
                                vecfT.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        }
                        // *****************************************************************************************************************
                        // vec3
                        // *****************************************************************************************************************
                        else if (binaryHex.Length == 24 && (binaryHex.Length == 24 || name.ContainsCI("Angle") || name.Contains("Dimensions") || name.ContainsCI("offset") || name.ContainsCI("position") || name.Contains("Pos") || name.ContainsCI("rotation") || name.Contains("Orientation") || name.Contains("Direction") || name.Contains("Multiplier") || name.ContainsCI("vector") || name.ContainsCI("color") || name.ContainsCI("clr") || name.ContainsCI("BBoxMin") || name.ContainsCI("BBoxMax") || name.ContainsCI("BBMin") || name.ContainsCI("BBMax") || name.StartsWith("vec") || name.Contains("value")))
                        {
                            byte[] vecX = kv.Value.Skip(0).Take(4).ToArray();
                            byte[] vecY = kv.Value.Skip(4).Take(4).ToArray();
                            byte[] vecZ = kv.Value.Skip(8).Take(4).ToArray();

                            float vecfX = BitConverter.ToSingle(vecX, 0);
                            float vecfY = BitConverter.ToSingle(vecY, 0);
                            float vecfZ = BitConverter.ToSingle(vecZ, 0);

                            writer.WriteAttributeString(prefix + "Vector3",
                                vecfX.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," +
                                vecfY.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," +
                                vecfZ.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        }
                        // *****************************************************************************************************************
                        // vec2
                        // *****************************************************************************************************************
                        else if (binaryHex.Length == 16 && (name.Contains("offset") || name.Contains("position") || name.Contains("rotation") || name.Contains("vector") || name.StartsWith("vec") || name.Contains("value")))
                        {
                            byte[] vecX = kv.Value.Skip(0).Take(4).ToArray();
                            byte[] vecY = kv.Value.Skip(4).Take(4).ToArray();

                            float vecfX = BitConverter.ToSingle(vecX, 0);
                            float vecfY = BitConverter.ToSingle(vecY, 0);

                            writer.WriteAttributeString(prefix + "Vector2",
                                vecfX.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," +
                                vecfY.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        }
                        // *****************************************************************************************************************
                        // other binhex
                        // *****************************************************************************************************************
                        else if (!name.Contains("offsetsArray") && !name.Contains("hashesArray") && !name.Contains("sizes") && !name.Contains("rootNodeIds") && !name.Contains("resourcePathIds") && !name.Contains("hidIndices") && !name.Contains("hidVertices") && !name.Contains("SectorData") && !name.Contains("ZoneData") && !name.Contains("positions") && !name.Contains("radianceTransferProbes"))
                        {
                            str = Regex.Replace(str, @"\p{C}+", string.Empty).Replace("?", "");
                            if (str != "")
                                writer.WriteAttributeString("strVal", str);

                        }

                        if (name != "data" && name != "ResIds" && name != "TypeIds" && name != "ArchetypeResDepList" && name != "CNH_CompressedData" && name != "buffer" && name != "hidDescriptor" && name != "hidShapePoints")
                        {
                            writer.WriteAttributeString("type", FieldType.BinHex.GetString());
                            writer.WriteBinHex(kv.Value, 0, kv.Value.Length);
                        }

                        if (resetPrevNodeVal) prevNodeVal = "";

                        writer.WriteEndElement();
                    }
                }
            }

            foreach (var childNode in node.Children)
            {
                WriteNode(writer, chain, childNode);
            }

            /*
            if (def == null || def.DynamicNestedClasses == false)
            {
                foreach (var childNode in node.Children)
                {
                    var childDef = def != null ? def.GetObjectDefinition(childNode.NameHash, chain) : null;
                    WriteNode(infoManager, writer, chain, childNode, childDef, null);
                }
            }
            else if (def.DynamicNestedClasses == true)
            {
                foreach (var childNode in node.Children)
                {
                    var childDef = infoManager.GetClassDefinition(childNode.NameHash);
                    WriteNode(infoManager, writer, chain, childNode, childDef, null);
                }
            }*/

            writer.WriteEndElement();
        }

        private static void ReadListFiles(byte[] bytes, XmlWriter xmlWriter, string nodeName)
        {
            var resIds = Helpers.UnpackArray(bytes, Program.isFC2 ? 4 : 8);

            foreach (byte[] fileNameBytes in resIds)
            {
                ulong fileName;

                if (Program.isFC2)
                    fileName = BitConverter.ToUInt32(fileNameBytes, 0);
                else
                    fileName = BitConverter.ToUInt64(fileNameBytes, 0);

                xmlWriter.WriteStartElement(nodeName);
                xmlWriter.WriteAttributeString("ID", Program.listFilesDict.ContainsKey(fileName) ? Program.listFilesDict[fileName] : "__Unknown\\" + fileName.ToString("X16"));
                xmlWriter.WriteEndElement();
            }
        }

        private static void ReadListHashes(byte[] bytes, XmlWriter xmlWriter, string nodeName)
        {
            var resIds = Helpers.UnpackArray(bytes, 4);

            foreach (byte[] fileNameBytes in resIds)
            {
                uint fileName = BitConverter.ToUInt32(fileNameBytes, 0);
                
                xmlWriter.WriteStartElement(nodeName);
                xmlWriter.WriteAttributeString("ID", Program.listStringsDict.ContainsKey(fileName) ? Program.listStringsDict[fileName] : fileName.ToString("X16"));
                xmlWriter.WriteEndElement();
            }
        }
    }
}
