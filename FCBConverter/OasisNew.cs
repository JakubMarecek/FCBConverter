using Gibbed.Dunia2.FileFormats;
using Gibbed.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FCBConverter
{
    class OasisNew
    {
        public static void OasisSerialize(string inputFile, string outputFile)
        {
            XDocument xmlDoc = XDocument.Load(inputFile);
            XElement xmlRoot = xmlDoc.Element("stringtable");

            IEnumerable<XElement> xmlSections = xmlRoot.Elements("section");

            var output = File.Create(outputFile);
            output.WriteValueU32(1);
            output.WriteValueU32(CRC32.Hash("oasisstrings"));
            output.WriteValueS32(xmlSections.Count());

            foreach (XElement xmlSection in xmlSections)
            {
                string sectionName = xmlSection.Attribute("name").Value;
                uint sectionNameCRC;

                if (sectionName.StartsWith("0x"))
                    sectionNameCRC = uint.Parse(sectionName[2..], NumberStyles.HexNumber);
                else
                    sectionNameCRC = CRC32.Hash(sectionName);

                IEnumerable<XElement> xmlStrings = xmlSection.Elements("string");
                List<OasisString> oasisStringList = new();

                output.WriteValueU32(sectionNameCRC);
                output.WriteValueS32(xmlStrings.Count());

                foreach (XElement xmlString in xmlStrings)
                {
                    string stringID = xmlString.Attribute("id").Value;
                    string stringEnum = xmlString.Attribute("enum").Value;
                    string stringMain = xmlString.Attribute("main").Value;
                    uint stringIDCRC;
                    uint stringEnumCRC;
                    uint stringMainCRC;

                    if (stringID.StartsWith("0x"))
                        stringIDCRC = uint.Parse(stringID[2..], NumberStyles.HexNumber);
                    else
                        stringIDCRC = uint.Parse(stringID);

                    if (stringEnum.StartsWith("0x"))
                        stringEnumCRC = uint.Parse(stringEnum[2..], NumberStyles.HexNumber);
                    else
                        stringEnumCRC = CRC32.Hash(stringEnum);

                    if (stringMain.StartsWith("0x"))
                        stringMainCRC = uint.Parse(stringMain[2..], NumberStyles.HexNumber);
                    else
                        stringMainCRC = CRC32.Hash(stringMain);

                    output.WriteValueU32(stringIDCRC);
                    output.WriteValueU32(sectionNameCRC);
                    output.WriteValueU32(stringEnumCRC);
                    output.WriteValueU32(stringMainCRC);

                    oasisStringList.Add(new OasisString
                    {
                        crc = stringEnumCRC,
                        id = stringIDCRC,
                        value = xmlString.Attribute("value").Value
                    });
                }

                output.WriteValueU32(1);

                {
                    MemoryStream memoryStream = new();
                    oasisStringList = Enumerable.OrderBy(oasisStringList, (OasisString x) => x.crc).ToList();

                    memoryStream.WriteValueS32(xmlStrings.Count());

                    foreach (OasisString oasisString in oasisStringList)
                    {
                        memoryStream.WriteValueU32(oasisString.crc);
                    }

                    int offset = 0;
                    foreach (OasisString oasisString in oasisStringList)
                    {
                        memoryStream.WriteValueS32(offset);
                        offset += Encoding.Unicode.GetBytes(oasisString.value).Length + 6;
                    }

                    foreach (OasisString oasisString in oasisStringList)
                    {
                        memoryStream.WriteValueU32(oasisString.id);
                        memoryStream.WriteStringZ(oasisString.value, Encoding.Unicode);
                    }

                    memoryStream.Position = 0;
                    byte[] memStr = memoryStream.ToArray();
                    byte[] compressed = new byte[xmlStrings.Any() ? (int)Math.Ceiling(memStr.Length * 1.2) : 8];
                    int compressedSize = 1;

                    LZO.Compress(memStr, 0, memStr.Length, compressed, 0, ref compressedSize);

                    output.WriteValueU32(xmlStrings.Any() ? oasisStringList[^1].crc : 0);
                    output.WriteValueS32(compressedSize);
                    output.WriteValueS32(memStr.Length);
                    output.Write(compressed, 0, compressedSize);
                }
            }

            output.Flush();
            output.Close();
        }

        public static void OasisDeserialize(string inputFile, string outputFile)
        {
            var input = File.OpenRead(inputFile);
            input.ReadValueU32();
            input.ReadValueU32();
            uint sectionsCount = input.ReadValueU32();

            XDocument xmlDoc = new(new XDeclaration("1.0", "utf-8", "yes"));
            XElement xmlStringtable = new("stringtable");

            for (int i = 0; i < sectionsCount; i++)
            {
                uint sectionNameCRC = input.ReadValueU32();
                uint stringsCount = input.ReadValueU32();

                XElement xmlSection = new("section", new XAttribute("name", string.Format("0x{0,8:X8}", sectionNameCRC)));

                for (int j = 0; j < stringsCount; j++)
                {
                    uint stringID = input.ReadValueU32();
                    input.ReadValueU32();
                    uint stringEnum = input.ReadValueU32();
                    uint stringMain = input.ReadValueU32();

                    XElement xmlString = new("string");
                    xmlString.Add(new XAttribute("enum", string.Format("0x{0,8:X8}", stringEnum)));
                    xmlString.Add(new XAttribute("main", string.Format("0x{0,8:X8}", stringMain)));
                    xmlString.Add(new XAttribute("id", stringID));





                }

                input.ReadValueU32();
                input.ReadValueU32();

                for (int j = 0; j < stringsCount; j++)
                {
                    input.ReadValueU32();
                }

                for (int j = 0; j < stringsCount; j++)
                {
                    input.ReadValueU32();
                }

                for (int j = 0; j < stringsCount; j++)
                {
                    input.ReadValueU32();

                }









                xmlStringtable.Add(xmlSection);
            }

            xmlDoc.Save(outputFile);
        }
    }

    struct OasisString
    {
        public uint crc;
        public uint id;
        public string value;
    }
}
