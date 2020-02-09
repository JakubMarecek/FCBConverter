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

using FCBConverter;
using Gibbed.Dunia2.BinaryObjectInfo;
using Gibbed.Dunia2.FileFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
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

            if (FCBConverter.Program.strings.ContainsKey(node.NameHash))
                writer.WriteAttributeString("name", FCBConverter.Program.strings[node.NameHash]);

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
                        writer.WriteStartElement("field");
                        writer.WriteAttributeString("hash", kv.Key.ToString("X8"));

                        string prefix = "value-";

                        string name = "";
                        if (FCBConverter.Program.strings.ContainsKey(kv.Key))
                        {
                            name = FCBConverter.Program.strings[kv.Key];
                            writer.WriteAttributeString("name", name);
                        }

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
                        else if (name == "ResIds")
                        {
                            var resIds = Helpers.UnpackArray(kv.Value, 8);

                            foreach (byte[] fileNameBytes in resIds)
                            {
                                ulong fileName = BitConverter.ToUInt64(fileNameBytes, 0);

                                writer.WriteStartElement("ResId");
                                writer.WriteAttributeString("ID", Program.m_HashList.ContainsKey(fileName) ? Program.m_HashList[fileName] : "__Unknown\\" + fileName.ToString("X16"));
                                writer.WriteEndElement();
                            }
                        }
                        else if (name == "hidDescriptor")
                        {
                            try
                            {
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
                            catch (Exception)
                            {
                                writer.WriteAttributeString("type", FieldType.BinHex.GetString());
                                writer.WriteAttributeString("legacy", "1");
                                writer.WriteBinHex(kv.Value, 0, kv.Value.Length);
                            }
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

                            if (FCBConverter.Program.strings.ContainsKey(v))
                                writer.WriteAttributeString("value-ComputeHash32", FCBConverter.Program.strings[v]);
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
                        else if (binaryHex.Length == 8 && (binaryHex.GetLast(2) == "00" && !name.StartsWithType("f") && !name.StartsWith("text_") && !name.ContainsCI("name") && name != "String") || name.Contains("locid") || name.StartsWith("loc") || name.StartsWithType("i") || name.StartsWithType("u") || name == "SoundId")
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
                        else if (binaryHex.Length == 16 && Regex.IsMatch(binaryHex, "^[0-9a-fA-F]{16}$", RegexOptions.Compiled) && binaryHex != "0000000000000000" && binaryHex != "ffffffffffffffff" && !name.StartsWith("vec") && !name.ContainsCI("value") && !name.StartsWith("text_"))
                        {
                            if (binaryHex.GetLast(2) != "00" && !name.ContainsCI("layerid") && !name.StartsWithType("ent") && name != "GameElementId" && !name.EndsWith("Id"))
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
                                if (FCBConverter.Program.strings.ContainsKey(result))
                                    writer.WriteAttributeString(prefix + "ComputeHash32", FCBConverter.Program.strings[result]);
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
                        else if (!name.Contains("offsetsArray") && !name.Contains("hashesArray") && !name.Contains("sizes") && !name.Contains("rootNodeIds") && !name.Contains("resourcePathIds"))
                        {
                            str = Regex.Replace(str, @"\p{C}+", string.Empty).Replace("?", "");
                            if (str != "")
                                writer.WriteAttributeString("strVal", str);

                        }

                        if (name != "data" && name != "ResIds" && name != "hidDescriptor")
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
    }
}
