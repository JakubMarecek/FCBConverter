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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using FCBConverter;
using Gibbed.Dunia2.BinaryObjectInfo;
using Gibbed.Dunia2.BinaryObjectInfo.Definitions;
using Gibbed.Dunia2.FileFormats;

namespace Gibbed.Dunia2.ConvertBinaryObject
{
    public class Importing
    {
        public Importing()
        {
        }

        public BinaryObject Import(string basePath, XPathNavigator nav)
        {
            var root = new BinaryObject();
            ReadNode(root, new BinaryObject[0], basePath, nav);
            return root;
        }

        private void ReadNode(BinaryObject node,
                              IEnumerable<BinaryObject> parentChain,
                              string basePath,
                              XPathNavigator nav)
        {
            var chain = parentChain.Concat(new[] {node});

            string className;
            uint classNameHash;

            LoadNameAndHash(nav, out className, out classNameHash);

            node.NameHash = classNameHash;

            var fields = nav.Select("field");
            while (fields.MoveNext() == true)
            {
                if (fields.Current == null)
                {
                    throw new InvalidOperationException();
                }

                string fieldName;
                uint fieldNameHash;

                LoadNameAndHash(fields.Current, out fieldName, out fieldNameHash);

                if (fieldName == "data")
                {
                    var moveBinDataChunk = new FCBConverter.CombinedMoveFile.MoveBinDataChunk(FCBConverter.Program.isNewDawn);
                    byte[] data = moveBinDataChunk.Serialize(fields.Current);
                    node.Fields.Add(fieldNameHash, data);
                }
                else if (fieldName == "ResIds")
                {
                    WriteListFiles(fields, node, fieldNameHash, "ResId");
                }
                else if (fieldName == "TypeIds")
                {
                    WriteListHashes(fields, node, fieldNameHash, "TypeId");
                }
                else if (fieldName == "ArchetypeResDepList")
                {
                    WriteListFiles(fields, node, fieldNameHash, "Resource");
                }
                else if (fieldName == "hidShapePoints")
                {
                    List<byte[]> resIdsBytes = new List<byte[]>();

                    var resIds = fields.Current.Select("Point");
                    while (resIds.MoveNext() == true)
                    {
                        var data = FieldTypeSerializers.Serialize(FieldType.Vector3, FieldType.Invalid, resIds.Current);
                        resIdsBytes.Add(data);
                    }

                    resIdsBytes.Insert(0, BitConverter.GetBytes(resIdsBytes.Count));

                    node.Fields.Add(fieldNameHash, resIdsBytes.SelectMany(byteArr => byteArr).ToArray());
                }
                else if (fieldName == "hidDescriptor")
                {
                    string legacy = fields.Current.GetAttribute("legacy", "");
                    if (legacy == "1")
                    {
                        MemoryStream ms = new MemoryStream();

                        var rez = new Gibbed.Dunia2.FileFormats.XmlResourceFile();
                        rez.Root = Gibbed.Dunia2.ConvertXml.Program.ReadNode(fields.Current.SelectSingleNode("hidDescriptor"));
                        rez.Serialize(ms);

                        node.Fields.Add(fieldNameHash, ms.ToArray());
                    }
                    else
                    {
                        string hidDescriptor = fields.Current.SelectSingleNode("hidDescriptor").OuterXml;
                        byte[] bytes = FieldTypeSerializers.Serialize(FieldType.String, hidDescriptor);
                        node.Fields.Add(fieldNameHash, bytes);
                    }
                }
                else if (fieldName == "CNH_CompressedData")
                {
                    string CNH_CompressedData = fields.Current.SelectSingleNode("CNH_CompressedData").InnerXml;
                    File.WriteAllText(Program.m_Path + "\\tmp", CNH_CompressedData);
                    Program.ConvertXML(Program.m_Path + "\\tmp", Program.m_Path + "\\tmpc");

                    byte[] bytes = File.ReadAllBytes(Program.m_Path + "\\tmpc");

                    string compressionType = fields.Current.SelectSingleNode("CNH_CompressedData").GetAttribute("CompressionType", "");

                    byte[] compressedBytes = null;

                    if (compressionType == "LZ4")
                        compressedBytes = new LZ4Sharp.LZ4Compressor64().Compress(bytes);

                    if (compressionType == "LZO")
                    {
                        int compressedSize = bytes.Length + (bytes.Length / 16) + 64 + 3; // weird magic
                        compressedBytes = new byte[compressedSize];

                        var result = Gibbed.Dunia2.FileFormats.LZO.Compress(bytes,
                                                    0,
                                                    bytes.Length,
                                                    compressedBytes,
                                                    0,
                                                    ref compressedSize);

                        Array.Resize(ref compressedBytes, compressedSize);
                    }

                    List<byte[]> output = new List<byte[]>
                    {
                        compressedBytes
                    };

                    output.Insert(0, BitConverter.GetBytes(compressedBytes.Length));

                    node.Fields.Add(fieldNameHash, output.SelectMany(output => output).ToArray());

                    node.Fields.Add(CRC32.Hash("CNH_UncompressedSize"), BitConverter.GetBytes(bytes.Length));
                    node.Fields.Add(CRC32.Hash("CNH_CompressedSize"), BitConverter.GetBytes(compressedBytes.Length));

                    File.Delete(Program.m_Path + "\\tmp");
                    File.Delete(Program.m_Path + "\\tmpc");
                }
                else if (fieldName == "CNH_UncompressedSize" || fieldName == "CNH_CompressedSize")
                {
                    // do nothing
                }
                else if (fieldName == "ArchetypeNamesStores")
                {
                    List<byte[]> ArchetypeNameIds = new List<byte[]>();
                    List<byte[]> ArchetypeIds = new List<byte[]>();
                    List<byte[]> ArchetypeNameStrings = new List<byte[]>();

                    int stringsSize = 0;
                    var resIds = fields.Current.Select("Archetype");
                    while (resIds.MoveNext() == true)
                    {
                        ulong ArchetypeId = ulong.Parse(resIds.Current.GetAttribute("ArchetypeId", ""));
                        var ArchetypeName = resIds.Current.GetAttribute("ArchetypeName", "");

                        var ArchetypeNameB = Encoding.UTF8.GetBytes(ArchetypeName);
                        Array.Resize(ref ArchetypeNameB, ArchetypeNameB.Length + 1);

                        ArchetypeIds.Add(BitConverter.GetBytes(ArchetypeId));
                        ArchetypeNameStrings.Add(ArchetypeNameB);
                        ArchetypeNameIds.Add(BitConverter.GetBytes(CRC32.Hash(ArchetypeName)));

                        stringsSize += ArchetypeNameB.Length;
                    }

                    ArchetypeIds.Insert(0, BitConverter.GetBytes(ArchetypeIds.Count));
                    ArchetypeNameStrings.Insert(0, BitConverter.GetBytes(stringsSize));
                    ArchetypeNameIds.Insert(0, BitConverter.GetBytes(ArchetypeNameIds.Count));

                    node.Fields.Add(CRC32.Hash("ArchetypeNameIds"), ArchetypeNameIds.SelectMany(byteArr => byteArr).ToArray());
                    node.Fields.Add(CRC32.Hash("ArchetypeIds"), ArchetypeIds.SelectMany(byteArr => byteArr).ToArray());
                    node.Fields.Add(CRC32.Hash("ArchetypeNameStrings"), ArchetypeNameStrings.SelectMany(byteArr => byteArr).ToArray());
                }
                else if (fieldName == "buffer")
                {
                    string buffer = fields.Current.SelectSingleNode("buffer").InnerXml;
                    File.WriteAllText(Program.m_Path + "\\tmp", buffer);
                    Program.ConvertXML(Program.m_Path + "\\tmp", Program.m_Path + "\\tmpc");

                    byte[] bytes = File.ReadAllBytes(Program.m_Path + "\\tmpc");
                    node.Fields.Add(fieldNameHash, bytes);

                    File.Delete(Program.m_Path + "\\tmp");
                    File.Delete(Program.m_Path + "\\tmpc");
                }
                else
                {
                    FieldType fieldType;
                    var fieldTypeName = fields.Current.GetAttribute("type", "");
                    if (Enum.TryParse(fieldTypeName, true, out fieldType) == false)
                    {
                        throw new InvalidOperationException();
                    }

                    var arrayFieldType = FieldType.Invalid;
                    var arrayFieldTypeName = fields.Current.GetAttribute("array_type", "");
                    if (string.IsNullOrEmpty(arrayFieldTypeName) == false)
                    {
                        if (Enum.TryParse(arrayFieldTypeName, true, out arrayFieldType) == false)
                        {
                            throw new InvalidOperationException();
                        }
                    }

                    var data = FieldTypeSerializers.Serialize(fieldType, arrayFieldType, fields.Current);
                    node.Fields.Add(fieldNameHash, data);
                }
            }


            if (FCBConverter.Program.isCombinedMoveFile)
            {
                FCBConverter.CombinedMoveFile.OffsetsHashesArray.Serialize(node);
                FCBConverter.CombinedMoveFile.PerMoveResourceInfo.Serialize(node);
            }


            var children = nav.Select("object");
            while (children.MoveNext() == true)
            {
                var child = new BinaryObject();
                LoadChildNode(child, chain, basePath, children.Current);
                node.Children.Add(child);
            }
        }

        private void HandleChildNode(BinaryObject node,
                                     IEnumerable<BinaryObject> chain,
                                     string basePath,
                                     XPathNavigator nav)
        {
            string className;
            uint classNameHash;

            LoadNameAndHash(nav, out className, out classNameHash);

            ReadNode(node, chain, basePath, nav);
            return;

            throw new InvalidOperationException();
        }

        private void LoadChildNode(BinaryObject node,
                                   IEnumerable<BinaryObject> chain,
                                   string basePath,
                                   XPathNavigator nav)
        {
            var external = nav.GetAttribute("external", "");
            if (string.IsNullOrWhiteSpace(external) == true)
            {
                HandleChildNode(node, chain, basePath, nav);
                return;
            }

            var inputPath = Path.Combine(basePath, external);
            using (var input = File.OpenRead(inputPath))
            {
                var nestedDoc = new XPathDocument(input);
                var nestedNav = nestedDoc.CreateNavigator();

                var root = nestedNav.SelectSingleNode("/object");
                if (root == null)
                {
                    throw new InvalidOperationException();
                }

                HandleChildNode(node, chain, Path.GetDirectoryName(inputPath), root);
            }
        }

        private static uint? GetClassDefinitionByField(string classFieldName, uint? classFieldHash, XPathNavigator nav)
        {
            uint? hash = null;

            if (string.IsNullOrEmpty(classFieldName) == false)
            {
                var fieldByName = nav.SelectSingleNode("field[@name=\"" + classFieldName + "\"]");
                if (fieldByName != null)
                {
                    uint temp;
                    if (uint.TryParse(fieldByName.Value,
                                      NumberStyles.AllowHexSpecifier,
                                      CultureInfo.InvariantCulture,
                                      out temp) == false)
                    {
                        throw new InvalidOperationException();
                    }
                    hash = temp;
                }
            }

            if (hash.HasValue == false &&
                classFieldHash.HasValue == true)
            {
                var fieldByHash =
                    nav.SelectSingleNode("field[@hash=\"" +
                                         classFieldHash.Value.ToString("X8", CultureInfo.InvariantCulture) +
                                         "\"]");
                if (fieldByHash != null)
                {
                    uint temp;
                    if (uint.TryParse(fieldByHash.Value,
                                      NumberStyles.AllowHexSpecifier,
                                      CultureInfo.InvariantCulture,
                                      out temp) == false)
                    {
                        throw new InvalidOperationException();
                    }
                    hash = temp;
                }
            }

            return hash;
        }

        private static void LoadNameAndHash(XPathNavigator nav, out string name, out uint hash)
        {
            var nameAttribute = nav.GetAttribute("name", "");
            var hashAttribute = nav.GetAttribute("hash", "");

            if (string.IsNullOrWhiteSpace(nameAttribute) == true &&
                string.IsNullOrWhiteSpace(hashAttribute) == true)
            {
                throw new FormatException();
            }

            name = string.IsNullOrWhiteSpace(nameAttribute) == false ? nameAttribute : null;
            hash = name != null ? CRC32.Hash(name) : uint.Parse(hashAttribute, NumberStyles.AllowHexSpecifier);
        }

        private static void WriteListFiles(XPathNodeIterator fields, BinaryObject node, uint fieldNameHash, string nodeName)
        {
            List<byte[]> resIdsBytes = new List<byte[]>();

            var resIds = fields.Current.Select(nodeName);
            while (resIds.MoveNext() == true)
            {
                if (Program.isFC2)
                {
                    resIdsBytes.Add(BitConverter.GetBytes((uint)Program.GetFileHash(resIds.Current.GetAttribute("ID", ""), 5)));
                }
                else
                {
                    resIdsBytes.Add(BitConverter.GetBytes(Program.GetFileHash(resIds.Current.GetAttribute("ID", ""), 10)));
                }
            }

            resIdsBytes.Insert(0, BitConverter.GetBytes(resIdsBytes.Count));

            node.Fields.Add(fieldNameHash, resIdsBytes.SelectMany(byteArr => byteArr).ToArray());
        }

        private static void WriteListHashes(XPathNodeIterator fields, BinaryObject node, uint fieldNameHash, string nodeName)
        {
            List<byte[]> resIdsBytes = new List<byte[]>();

            var resIds = fields.Current.Select(nodeName);
            while (resIds.MoveNext() == true)
            {
                var value = Program.listStringsDict.FirstOrDefault(a => a.Value == resIds.Current.GetAttribute("ID", "")).Key;
                resIdsBytes.Add(BitConverter.GetBytes(value));
            }

            resIdsBytes.Insert(0, BitConverter.GetBytes(resIdsBytes.Count));

            node.Fields.Add(fieldNameHash, resIdsBytes.SelectMany(byteArr => byteArr).ToArray());
        }
    }
}
