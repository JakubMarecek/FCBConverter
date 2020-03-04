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
using System.Xml.XPath;
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
                    List<byte[]> resIdsBytes = new List<byte[]>();

                    var resIds = fields.Current.Select("ResId");
                    while (resIds.MoveNext() == true)
                    {
                        resIdsBytes.Add(BitConverter.GetBytes(FCBConverter.Program.GetFileHash(resIds.Current.GetAttribute("ID", ""))));
                    }

                    resIdsBytes.Insert(0, BitConverter.GetBytes(resIdsBytes.Count));

                    node.Fields.Add(fieldNameHash, resIdsBytes.SelectMany(byteArr => byteArr).ToArray());
                }
                else if (fieldName == "hidDescriptor")
                {
                    string legacy = fields.Current.GetAttribute("legacy", "");
                    if (legacy == "1")
                    {
                        byte[] bytes = FCBConverter.Helpers.StringToByteArray(fields.Current.Value);
                        node.Fields.Add(fieldNameHash, bytes);
                    }
                    else
                    {
                        string hidDescriptor = fields.Current.SelectSingleNode("hidDescriptor").OuterXml;
                        byte[] bytes = FieldTypeSerializers.Serialize(FieldType.String, hidDescriptor);
                        node.Fields.Add(fieldNameHash, bytes);
                    }
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
    }
}
