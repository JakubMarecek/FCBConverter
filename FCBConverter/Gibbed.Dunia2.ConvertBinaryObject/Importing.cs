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
                    FCBConverter.MoveBinDataChunk moveBinDataChunk = new FCBConverter.MoveBinDataChunk();
                    byte[] data = moveBinDataChunk.Serialize(fields.Current, true);
                    node.Fields.Add(fieldNameHash, data);
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
                FCBConverter.OffsetsHashesArray.Serialize(node);



                var PerMoveResourceInfos = nav.Select("PerMoveResourceInfo");
                if (PerMoveResourceInfos.Count > 0)
                {
                    List<byte[]> sizes = new List<byte[]>();
                    List<byte[]> rootNodeIds = new List<byte[]>();
                    List<byte[]> resourcePathIds = new List<byte[]>();
                    while (PerMoveResourceInfos.MoveNext() == true)
                    {
                        sizes.Add(BitConverter.GetBytes(uint.Parse(PerMoveResourceInfos.Current.GetAttribute("size", ""))));
                        rootNodeIds.Add(FCBConverter.Helpers.StringToByteArray(PerMoveResourceInfos.Current.GetAttribute("rootNodeId", "")));
                        resourcePathIds.Add(FCBConverter.Helpers.StringToByteArray(PerMoveResourceInfos.Current.GetAttribute("resourcePathId", "")));
                    }
                    if (sizes.Count > 0)
                    {
                        sizes.Insert(0, BitConverter.GetBytes(sizes.Count));
                        rootNodeIds.Insert(0, BitConverter.GetBytes(rootNodeIds.Count));
                        resourcePathIds.Insert(0, BitConverter.GetBytes(resourcePathIds.Count));

                        node.Fields.Add(CRC32.Hash("sizes"), sizes.SelectMany(byteArr => byteArr).ToArray());
                        node.Fields.Add(CRC32.Hash("rootNodeIds"), rootNodeIds.SelectMany(byteArr => byteArr).ToArray());
                        node.Fields.Add(CRC32.Hash("resourcePathIds"), resourcePathIds.SelectMany(byteArr => byteArr).ToArray());
                    }
                }
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
