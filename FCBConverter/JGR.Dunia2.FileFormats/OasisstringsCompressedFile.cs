// JGR.Dunia2.FileFormats.OasisstringsCompressedFile
using Gibbed.Dunia2.FileFormats;
using Gibbed.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

public class OasisstringsCompressedFile
{
    public class OasisSection
    {
        public class OasisLocalizedString
        {
            public string Enum;

            public uint EnumCRC;

            public uint Id;

            public uint SectionCRC;

            public string Value;

            public uint Extra;

            private OasisStringsDictionary hashDictionary;

            public OasisLocalizedString(OasisStringsDictionary dictionary)
            {
                hashDictionary = dictionary;
            }

            public OasisLocalizedString(OasisStringsDictionary dictionary, Node node, uint sectionCRC)
            {
                hashDictionary = dictionary;
                if (node.Name != "string")
                {
                    throw new Exception("invalide node type for constructing an OasisLocalizedString.");
                }
                FromNode(node, sectionCRC);
            }

            public void Deserialize(Stream input, Endian endian, uint nameCRC, bool newDawn)
            {
                Id = input.ReadValueU32(endian);
                SectionCRC = input.ReadValueU32(endian);
                if (SectionCRC != nameCRC)
                {
                    throw new FormatException("oasis string section CRC does not match the section's CRC value.");
                }
                EnumCRC = input.ReadValueU32(endian);
                if (hashDictionary != null)
                {
                    Enum = hashDictionary.GetEnumName(SectionCRC, EnumCRC);
                }
                if (newDawn)
                    Extra = input.ReadValueU32(endian);
            }

            public void Serialize(Stream output, Endian endian, bool newDawn)
            {
                output.WriteValueU32(Id, endian);
                output.WriteValueU32(SectionCRC, endian);
                output.WriteValueU32(EnumCRC, endian);
                if (newDawn)
                    output.WriteValueU32(Extra, endian);
            }

            public Node EmitNode(bool newDawn)
            {
                if (EnumCRC == 0 || Id == 0)
                {
                    throw new FormatException("attempting to emit XML node prior to proper OasisLocalizedString construction.");
                }
                Node obj = new Node
                {
                    Name = "string"
                };
                Attribute attribute = new Attribute
                {
                    Name = "enum"
                };
                if (Enum != null)
                {
                    attribute.Value = Enum;
                }
                else
                {
                    attribute.Value = string.Format("0x{0,8:X8}", EnumCRC);
                }
                obj.Attributes.Add(attribute);
                if (newDawn)
                {
                    Attribute itemExtra = new Attribute
                    {
                        Name = "extra",
                        Value = string.Format("0x{0,8:X8}", Extra)
                    };
                    obj.Attributes.Add(itemExtra);
                }
                Attribute item = new Attribute
                {
                    Name = "id",
                    Value = Id.ToString()
                };
                obj.Attributes.Add(item);
                Attribute item2 = new Attribute
                {
                    Name = "value",
                    Value = Value
                };
                obj.Attributes.Add(item2);
                return obj;
            }

            public void FromNode(Node node, uint sectionCRC)
            {
                if (node.Name != "string" || node.Attributes[0].Name != "enum" || node.Attributes[1].Name != "extra" || node.Attributes[1].Name != "id" || node.Attributes[2].Name != "value")
                {
                    throw new Exception("invalid node type for populating and OasisLocalizedString.");
                }
                if (node.Attributes[0].Value.StartsWith("0x"))
                {
                    EnumCRC = uint.Parse(node.Attributes[0].Value.Substring(2), NumberStyles.HexNumber);
                }
                else
                {
                    Enum = node.Attributes[0].Value;
                    EnumCRC = CRC32.Hash(Enum);
                }
                if (node.Attributes[1].Value.StartsWith("0x"))
                {
                    Id = uint.Parse(node.Attributes[1].Value.Substring(2), NumberStyles.HexNumber);
                }
                else
                {
                    Id = uint.Parse(node.Attributes[1].Value);
                }
                Value = node.Attributes[2].Value;
                SectionCRC = sectionCRC;
            }
        }

        private class CompressedValues
        {
            public uint LastSortedCRC;

            public int CompressedSize;

            public int DecompressedSize;

            public byte[] CompressedBytes;

            public void Deserialize(Stream input, Endian endian)
            {
                LastSortedCRC = input.ReadValueU32(endian);
                CompressedSize = input.ReadValueS32(endian);
                DecompressedSize = input.ReadValueS32(endian);
                CompressedBytes = input.ReadBytes(CompressedSize);
            }

            public void Serialize(Stream output, Endian endian)
            {
                output.WriteValueU32(LastSortedCRC, endian);
                output.WriteValueS32(CompressedSize, endian);
                output.WriteValueS32(DecompressedSize, endian);
                output.WriteBytes(CompressedBytes);
            }
        }

        private class DecompressedValues
        {
            public int StringCount;

            public List<uint> SortedEnums;

            public List<int> StringOffsets;

            public List<Tuple<uint, string>> IdValuePairs;

            public DecompressedValues()
            {
            }

            public DecompressedValues(List<OasisLocalizedString> oaStrings)
            {
                StringCount = oaStrings.Count;
                SortedEnums = new List<uint>();
                StringOffsets = new List<int>();
                IdValuePairs = new List<Tuple<uint, string>>();
                int num = 0;
                foreach (OasisLocalizedString item in oaStrings.OrderBy((OasisLocalizedString x) => x.EnumCRC).ToList())
                {
                    SortedEnums.Add(item.EnumCRC);
                    StringOffsets.Add(num);
                    uint id = item.Id;
                    string value = item.Value;
                    IdValuePairs.Add(new Tuple<uint, string>(id, value));
                    byte[] bytes = Encoding.Unicode.GetBytes(value);
                    num += bytes.Length + 6;
                }
            }

            public void Deserialize(Stream input, Endian endian)
            {
                StringCount = input.ReadValueS32(endian);
                SortedEnums = new List<uint>();
                for (int i = 0; i < StringCount; i++)
                {
                    SortedEnums.Add(input.ReadValueU32(endian));
                }
                StringOffsets = new List<int>();
                for (int j = 0; j < StringCount; j++)
                {
                    StringOffsets.Add(input.ReadValueS32(endian));
                }
                IdValuePairs = new List<Tuple<uint, string>>();
                for (int k = 0; k < StringCount; k++)
                {
                    uint item = input.ReadValueU32(endian);
                    string item2 = input.ReadStringZ(Encoding.Unicode);
                    Tuple<uint, string> item3 = new Tuple<uint, string>(item, item2);
                    IdValuePairs.Add(item3);
                }
            }

            public void Serialize(Stream output, Endian endian)
            {
                output.WriteValueS32(StringCount, endian);
                foreach (uint sortedEnum in SortedEnums)
                {
                    output.WriteValueU32(sortedEnum, endian);
                }
                foreach (int stringOffset in StringOffsets)
                {
                    output.WriteValueS32(stringOffset, endian);
                }
                foreach (Tuple<uint, string> idValuePair in IdValuePairs)
                {
                    output.WriteValueU32(idValuePair.Item1, endian);
                    output.WriteStringZ(idValuePair.Item2, Encoding.Unicode);
                }
            }
        }

        public string Name;

        public uint NameCRC;

        public uint StringCount;

        public List<OasisLocalizedString> LocalizedStrings = new List<OasisLocalizedString>();

        public uint CompressedValuesSectionsCount;

        private OasisStringsDictionary hashDictionary;

        private readonly uint MAX_LENGTH = 16384u;

        public OasisSection(OasisStringsDictionary dictionary)
        {
            hashDictionary = dictionary;
        }

        public OasisSection(OasisStringsDictionary hashDictionary, Node node)
        {
            if (node.Name != "section")
            {
                throw new Exception("invalid node type for constructing an OasisSection.");
            }
            FromNode(node);
        }

        public void Deserialize(Stream input, Endian endian, bool newDawn)
        {
            NameCRC = input.ReadValueU32(endian);
            if (hashDictionary != null)
            {
                Name = hashDictionary.GetSectionName(NameCRC);
            }
            StringCount = input.ReadValueU32(endian);
            Dictionary<uint, OasisLocalizedString> dictionary = new Dictionary<uint, OasisLocalizedString>();
            for (int i = 0; i < StringCount; i++)
            {
                OasisLocalizedString oasisLocalizedString = new OasisLocalizedString(hashDictionary);
                oasisLocalizedString.Deserialize(input, endian, NameCRC, newDawn);
                LocalizedStrings.Add(oasisLocalizedString);
                dictionary.Add(oasisLocalizedString.Id, oasisLocalizedString);
            }
            CompressedValuesSectionsCount = input.ReadValueU32(endian);
            for (int j = 0; j < CompressedValuesSectionsCount; j++)
            {
                CompressedValues compressedValues = new CompressedValues();
                compressedValues.Deserialize(input, endian);
                byte[] array = new LZ4Sharp.LZ4Decompressor64().Decompress(compressedValues.CompressedBytes);
                if (compressedValues.DecompressedSize != array.Length)
                {
                    throw new FormatException("compressed values section of oassisstring section decompressed to an unexpected size.");
                }
                DecompressedValues decompressedValues = new DecompressedValues();
                using (Stream input2 = new MemoryStream(array))
                {
                    decompressedValues.Deserialize(input2, endian);
                }
                for (int k = 0; k < decompressedValues.StringCount; k++)
                {
                    dictionary[decompressedValues.IdValuePairs[k].Item1].Value = decompressedValues.IdValuePairs[k].Item2;
                }
            }
            LocalizedStrings.Sort((OasisLocalizedString first, OasisLocalizedString second) => first.EnumCRC.CompareTo(second.EnumCRC));
        }

        public void Serialize(Stream output, Endian endian, bool newDawn)
        {
            output.WriteValueU32(NameCRC, endian);
            output.WriteValueU32(StringCount, endian);
            foreach (OasisLocalizedString localizedString in LocalizedStrings)
            {
                localizedString.Serialize(output, endian, newDawn);
            }
            LocalizedStrings.Sort((OasisLocalizedString first, OasisLocalizedString second) => first.EnumCRC.CompareTo(second.EnumCRC));
            List<DecompressedValues> list = new List<DecompressedValues>();
            List<OasisLocalizedString> list2 = new List<OasisLocalizedString>();
            uint num = 0u;
            uint num2 = 0u;
            foreach (OasisLocalizedString localizedString2 in LocalizedStrings)
            {
                num = (uint)((int)num + 2 * localizedString2.Value.Length);
                list2.Add(localizedString2);
                if (num >= MAX_LENGTH && localizedString2.EnumCRC != num2)
                {
                    list.Add(new DecompressedValues(list2));
                    list2 = new List<OasisLocalizedString>();
                    num = 0u;
                    num2 = 0u;
                }
                else
                {
                    num2 = localizedString2.EnumCRC;
                }
            }
            if (list2.Count != 0)
            {
                list.Add(new DecompressedValues(list2));
                list2 = new List<OasisLocalizedString>();
                num = 0u;
                num2 = 0u;
            }
            output.WriteValueU32((uint)list.Count);
            foreach (DecompressedValues item in list)
            {
                CompressedValues compressedValues = new CompressedValues();
                using (Stream stream = new MemoryStream())
                {
                    item.Serialize(stream, endian);
                    MemoryStream memoryStream = new MemoryStream();
                    stream.Position = 0L;
                    stream.CopyTo(memoryStream);
                    byte[] array = memoryStream.ToArray();
                    int num3 = array.Length;
                    compressedValues.CompressedBytes = new LZ4Sharp.LZ4Compressor64().Compress(array);
                    int num4 = compressedValues.CompressedSize = compressedValues.CompressedBytes.Length;
                    compressedValues.DecompressedSize = num3;
                    compressedValues.LastSortedCRC = item.SortedEnums[item.SortedEnums.Count - 1];
                }
                compressedValues.Serialize(output, endian);
            }
        }

        public Node EmitNode(bool newDawn)
        {
            if (NameCRC == 0 || StringCount == 0 || LocalizedStrings.Count <= 0 || CompressedValuesSectionsCount == 0)
            {
                throw new FormatException("attempting to emit XML node prior to proper OasisSection construction.");
            }
            Node node = new Node();
            node.Name = "section";
            Attribute attribute = new Attribute();
            attribute.Name = "name";
            if (Name != null)
            {
                attribute.Value = Name;
            }
            else
            {
                attribute.Value = string.Format("0x{0,8:X8}", NameCRC);
            }
            node.Attributes.Add(attribute);
            foreach (OasisLocalizedString localizedString in LocalizedStrings)
            {
                node.Children.Add(localizedString.EmitNode(newDawn));
            }
            return node;
        }

        public void FromNode(Node node)
        {
            if (node.Name != "section" || node.Attributes[0].Name != "name")
            {
                throw new Exception("invalid node type for populating an OasisSection.");
            }
            if (node.Attributes[0].Value.StartsWith("0x"))
            {
                NameCRC = uint.Parse(node.Attributes[0].Value.Substring(2), NumberStyles.HexNumber);
            }
            else
            {
                Name = node.Attributes[0].Value;
                NameCRC = CRC32.Hash(Name);
            }
            StringCount = (uint)node.Children.Count;
            foreach (Node child in node.Children)
            {
                LocalizedStrings.Add(new OasisLocalizedString(hashDictionary, child, NameCRC));
            }
        }
    }

    public class Node
    {
        public string Name;

        public string Value;

        public List<Attribute> Attributes = new List<Attribute>();

        public List<Node> Children = new List<Node>();
    }

    public class Attribute
    {
        public string Name;

        public string Value;
    }

    public class OasisStringsDictionary
    {
        private class Section
        {
            public Dictionary<uint, string> Enums = new Dictionary<uint, string>();

            public string Name
            {
                get;
                set;
            }

            public Section(XmlNode node)
            {
                Name = node.Attributes["name"].Value;
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    try
                    {
                        Enums.Add(CRC32.Hash(childNode.Attributes["enum"].Value), childNode.Attributes["enum"].Value);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private Dictionary<uint, Section> sections = new Dictionary<uint, Section>();

        public string DictionaryFile
        {
            get;
            set;
        }

        public OasisStringsDictionary(string dictionaryFile)
        {
            DictionaryFile = dictionaryFile;
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(dictionaryFile);
            foreach (XmlNode item in xmlDocument.SelectNodes("stringtable/section"))
            {
                sections.Add(CRC32.Hash(item.Attributes["name"].Value), new Section(item));
            }
        }

        public string GetSectionName(uint sectionhash)
        {
            return sections[sectionhash].Name;
        }

        public string GetEnumName(uint sectionhash, uint enumhash)
        {
            if (sections[sectionhash] != null)
            {
                return sections[sectionhash].Enums[enumhash];
            }
            return null;
        }
    }

    private ushort Unknown1 = 1;

    private OasisStringsDictionary hashDictionary;

    public Node Root
    {
        get;
        set;
    }

    public string Language
    {
        get;
        set;
    }

    public OasisstringsCompressedFile()
    {
        Language = "english";
    }

    public OasisstringsCompressedFile(string language)
    {
        Language = language;
    }

    public OasisstringsCompressedFile(string language, string dictionaryfile)
    {
        hashDictionary = new OasisStringsDictionary(dictionaryfile);
    }

    public void Deserialize(Stream input, bool newDawn)
    {
        if (input.ReadValueU32() != Unknown1)
        {
            throw new FormatException("not an oasisstrings_compressed.bin file");
        }
        Endian endian = Endian.Little;
        uint num = input.ReadValueU32(endian);
        Root = new Node
        {
            Name = "stringtable"
        };
        Attribute item = new Attribute
        {
            Name = "language",
            Value = Language
        };
        Root.Attributes.Add(item);
        for (int i = 0; i < num; i++)
        {
            OasisSection oasisSection = new OasisSection(hashDictionary);
            oasisSection.Deserialize(input, endian, newDawn);
            Root.Children.Add(oasisSection.EmitNode(newDawn));
        }
    }

    public void Serialize(Stream output, bool newDawn)
    {
        Endian endian = Endian.Little;
        output.WriteValueU32(Unknown1, endian);
        int count = Root.Children.Count;
        output.WriteValueS32(count);
        foreach (Node child in Root.Children)
        {
            new OasisSection(hashDictionary, child).Serialize(output, endian, newDawn);
        }
    }
}
