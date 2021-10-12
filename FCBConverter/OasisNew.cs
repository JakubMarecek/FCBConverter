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

using Gibbed.Dunia2.FileFormats;
using Gibbed.IO;
using K4os.Compression.LZ4;
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

            int fileType = int.Parse(xmlRoot.Attribute("type").Value);

            IEnumerable<XElement> xmlSections = xmlRoot.Elements("section");

            var output = File.Create(outputFile);
            output.WriteValueU32(1);
            if (fileType == 1)
            {
                string hdr = xmlRoot.Attribute("hdr").Value;
                uint hdrCRC = 0;

                if (hdr.StartsWith("0x"))
                    hdrCRC = uint.Parse(hdr[2..], NumberStyles.HexNumber);
                else
                    hdrCRC = CRC32.Hash(hdr);

                output.WriteValueU32(hdrCRC);
            }
            output.WriteValueS32(xmlSections.Count());

            foreach (XElement xmlSection in xmlSections)
            {
                string sectionName = xmlSection.Attribute("name").Value;
                uint sectionNameCRC;

                if (sectionName.StartsWith("0x"))
                    sectionNameCRC = uint.Parse(sectionName[2..], NumberStyles.HexNumber);
                else
                    sectionNameCRC = CRC32.Hash(sectionName);

                IEnumerable<XElement> xmlStrings = xmlSection/*.Elements("subsection")*/.Elements("string");
                List<OasisString> oasisStringList = new();

                foreach (XElement xmlString in xmlStrings)
                {
                    string stringID = xmlString.Attribute("id").Value;
                    string stringEnum = xmlString.Attribute("enum").Value;
                    uint stringIDCRC;
                    uint stringEnumCRC;
                    uint stringMainCRC = 0;

                    if (stringID.StartsWith("0x"))
                        stringIDCRC = uint.Parse(stringID[2..], NumberStyles.HexNumber);
                    else
                        stringIDCRC = uint.Parse(stringID);

                    if (stringEnum.StartsWith("0x"))
                        stringEnumCRC = uint.Parse(stringEnum[2..], NumberStyles.HexNumber);
                    else
                        stringEnumCRC = CRC32.Hash(stringEnum);

                    if (fileType != 0)
                    {
                        string stringMain = "";

                        stringMain = xmlString.Attribute("extra").Value;

                        if (stringMain.StartsWith("0x"))
                            stringMainCRC = uint.Parse(stringMain[2..], NumberStyles.HexNumber);
                        else
                            stringMainCRC = CRC32.Hash(stringMain);
                    }

                    oasisStringList.Add(new OasisString
                    {
                        enumVal = stringEnumCRC,
                        id = stringIDCRC,
                        value = xmlString.Attribute("value").Value,
                        extra = stringMainCRC
                    });
                }
                oasisStringList = Enumerable.OrderBy(oasisStringList, (OasisString x) => x.id).ToList();

                output.WriteValueU32(sectionNameCRC);
                output.WriteValueS32(xmlStrings.Count());

                foreach (OasisString os in oasisStringList)
                {
                    output.WriteValueU32(os.id);
                    output.WriteValueU32(sectionNameCRC);
                    output.WriteValueU32(os.enumVal);

                    if (fileType != 0)
                        output.WriteValueU32(os.extra);
                }

                //output.WriteValueU32(1);

                /*IEnumerable<XElement> xmlSubSections = xmlSection.Elements("subsection");
                output.WriteValueU32((uint)xmlSubSections.Count());

                foreach (XElement xmlSubSection in xmlSubSections)*/
                /*{
                    //var xmlSubSectionStrings = xmlSubSection.Elements("string").Select(s => s.Attribute("id").Value);
                    var xmlSubSectionStrings = xmlSection.Elements("string").Select(s => s.Attribute("id").Value);


                    List<OasisString> oasisStringListTmp = new();

                    foreach (string sid in xmlSubSectionStrings)
                    {
                        oasisStringListTmp.Add(oasisStringList.Where(a => a.id == uint.Parse(sid)).Single());
                    }

                    oasisStringListTmp = Enumerable.OrderBy(oasisStringListTmp, (OasisString x) => x.enumVal).ToList();

                    MemoryStream memoryStream = new();
                    memoryStream.WriteValueS32(xmlStrings.Count());

                    foreach (OasisString oasisString in oasisStringListTmp)
                    {
                        memoryStream.WriteValueU32(oasisString.enumVal);
                    }

                    int offset = 0;
                    foreach (OasisString oasisString in oasisStringListTmp)
                    {
                        memoryStream.WriteValueS32(offset);
                        offset += Encoding.Unicode.GetBytes(oasisString.value).Length + 6;
                    }

                    foreach (OasisString oasisString in oasisStringListTmp)
                    {
                        memoryStream.WriteValueU32(oasisString.id);
                        memoryStream.WriteStringZ(oasisString.value, Encoding.Unicode);
                    }

                    memoryStream.Position = 0;
                    byte[] memStr = memoryStream.ToArray();
                    byte[] compressed = new byte[xmlStrings.Any() ? (int)Math.Ceiling(memStr.Length * 1.2) : 8];
                    int compressedSize = 1;

                    if (fileType == 1)
                        LZO.Compress(memStr, 0, memStr.Length, compressed, 0, ref compressedSize);
                    else
                    {
                        byte[] tmp = new byte[LZ4Codec.MaximumOutputSize(memStr.Length)];
                        compressedSize = LZ4Codec.Encode(memStr, tmp, LZ4Level.L00_FAST);
                        compressed = new byte[compressedSize];
                        Array.Copy(tmp, compressed, compressedSize);

                        //compressed = new LZ4Sharp.LZ4Compressor64().Compress(memStr);
                        //compressedSize = compressed.Length;
                    }

                    output.WriteValueU32(xmlStrings.Any() ? oasisStringListTmp[^1].enumVal : 0);
                    output.WriteValueS32(compressedSize);
                    output.WriteValueS32(memStr.Length);
                    output.Write(compressed, 0, compressedSize);
                }*/


                // edit by eprilx
                oasisStringList = Enumerable.OrderBy(oasisStringList, (OasisString x) => x.enumVal).ToList();

                List<List<OasisString>> oasisStringListDecompress = new();
                List<OasisString> tmpOasis = new();
                int decompressSize = 0;

                foreach (OasisString oasisString in oasisStringList)
                {
                    decompressSize += Encoding.Unicode.GetBytes(oasisString.value).Length + 6;
                    tmpOasis.Add(oasisString);

                    if (decompressSize > (1024 * 16) || oasisString.Equals(oasisStringList.Last()))
                    {
                        decompressSize = 0;
                        List<OasisString> tmpOasis2 = new List<OasisString>(tmpOasis);
                        oasisStringListDecompress.Add(tmpOasis2);
                        tmpOasis.Clear();
                    }

                }
                output.WriteValueU32((uint)(oasisStringListDecompress.Count()));

                foreach (List<OasisString> oasisStringList2 in oasisStringListDecompress)
                {
                    MemoryStream memoryStream = new();
                    memoryStream.WriteValueS32(oasisStringList2.Count());
                    foreach (OasisString oasisString in oasisStringList2)
                    {
                        memoryStream.WriteValueU32(oasisString.enumVal);
                    }

                    int offset = 0;
                    foreach (OasisString oasisString in oasisStringList2)
                    {
                        memoryStream.WriteValueS32(offset);
                        offset += Encoding.Unicode.GetBytes(oasisString.value).Length + 6;
                    }

                    foreach (OasisString oasisString in oasisStringList2)
                    {
                        memoryStream.WriteValueU32(oasisString.id);
                        memoryStream.WriteStringZ(oasisString.value, Encoding.Unicode);
                    }

                    memoryStream.Position = 0;
                    byte[] memStr = memoryStream.ToArray();
                    byte[] compressed = new byte[xmlStrings.Any() ? (int)Math.Ceiling(memStr.Length * 1.2) : 8];
                    int compressedSize = 1;

                    if (fileType == 1)
                        LZO.Compress(memStr, 0, memStr.Length, compressed, 0, ref compressedSize);
                    else
                    {
                        byte[] tmp = new byte[LZ4Codec.MaximumOutputSize(memStr.Length)];
                        compressedSize = LZ4Codec.Encode(memStr, tmp, LZ4Level.L00_FAST);
                        compressed = new byte[compressedSize];
                        Array.Copy(tmp, compressed, compressedSize);
                    }

                    output.WriteValueU32(xmlStrings.Any() ? oasisStringList2[^1].enumVal : 0);
                    output.WriteValueS32(compressedSize);
                    output.WriteValueS32(memStr.Length);
                    output.Write(compressed, 0, compressedSize);
                }
            }

            XElement xmlSpeeches = xmlRoot.Element("speeches");
            if (xmlSpeeches != null)
            {
                IEnumerable<XElement> speeches = xmlSpeeches.Elements("speech");

                output.WriteValueS32(speeches.Count());

                foreach (XElement speech in speeches)
                {
                    output.WriteValueU32(uint.Parse(speech.Attribute("id").Value));
                }
            }

            output.Flush();
            output.Close();
        }

        public static void OasisDeserialize(string inputFile, string outputFile)
        {
            bool subs = false;
            if (inputFile.Contains("_subtitles"))
            {
                subs = IsSubs2();
            }

            // 0 - FC5
            // 1 - FC3 FC4
            // 2 - ND FC6
            int fileType = 0;

            var input = File.OpenRead(inputFile);
            input.ReadValueU32();
            uint hash = input.ReadValueU32();

            if (hash == CRC32.Hash("oasisstrings") || hash == 0x9ba82025)
                fileType = 1;
            else
                input.Position -= sizeof(uint);

            uint sectionsCount = input.ReadValueU32();

            XDocument xmlDoc = new(new XDeclaration("1.0", "utf-8", "yes"));
            xmlDoc.Add(new XComment(Program.xmlheader));

            XElement xmlStringtable = new("stringtable");

            for (int i = 0; i < sectionsCount; i++)
            {
                uint sectionNameCRC = input.ReadValueU32();
                uint stringsCount = input.ReadValueU32();

                Dictionary<uint, OasisString> oasisStringList = new();

                for (int j = 0; j < stringsCount; j++)
                {
                    uint stringID = input.ReadValueU32();
                    input.ReadValueU32();
                    uint stringEnum = input.ReadValueU32();
                    uint stringMain = input.ReadValueU32();

                    /*if (stringMain == 0 || stringMain == 0xffffffff)
                        fileType = 2;
                    else if (stringMain == CRC32.Hash("Main"))
                        fileType = 1;
                    else if (stringMain == CRC32.Hash("main"))
                        fileType = 3;
                    else*/
                    if (stringMain == 0 || stringMain == 0xffffffff || subs)
                    {
                        fileType = 2;
                    }
                    else if (fileType == 1)
                    {
                    }
                    else
                    {
                        input.Position -= sizeof(uint);
                        stringMain = 1;
                    }

                    oasisStringList.Add(stringID, new OasisString
                    {
                        enumVal = stringEnum,
                        id = stringID,
                        main = stringMain,
                        value = ""
                    });
                }

                string sectionVal = Program.listStringsDict.ContainsKey(sectionNameCRC) ? Program.listStringsDict[sectionNameCRC] : string.Format("0x{0,8:X8}", sectionNameCRC);

                XElement xmlSection = new("section", new XAttribute("name", sectionVal));

                int compressedSections = input.ReadValueS32();
                for (int m = 0; m < compressedSections; m++)
                {
                    Dictionary<uint, string> oasisStringValList = new();

                    input.ReadValueU32();
                    int compressedSize = input.ReadValueS32();
                    int uncompressedSize = input.ReadValueS32();
                    byte[] compressed = input.ReadBytes(compressedSize);
                    byte[] decompressed = new byte[uncompressedSize];

                    if (fileType == 1)
                        LZO.Decompress(compressed, 0, compressedSize, decompressed, 0, ref uncompressedSize);
                    else
                        LZ4Codec.Decode(compressed, decompressed);
                        //decompressed = new LZ4Sharp.LZ4Decompressor64().Decompress(compressed);

                    {
                        MemoryStream memoryStream = new(decompressed);

                        int strCount = memoryStream.ReadValueS32();

                        for (int j = 0; j < strCount; j++)
                            memoryStream.ReadValueU32();

                        for (int j = 0; j < strCount; j++)
                            memoryStream.ReadValueU32();

                        for (int j = 0; j < strCount; j++)
                        {
                            uint id = memoryStream.ReadValueU32();
                            string str = memoryStream.ReadStringZ(Encoding.Unicode);
                            oasisStringValList.Add(id, str);
                        }
                    }

                    XElement xmlSubSection = new("subsection");

                    foreach (KeyValuePair<uint, string> oasisStringVal in oasisStringValList)
                    {
                        OasisString os = oasisStringList[oasisStringVal.Key];

                        string enumVal = Program.listStringsDict.ContainsKey(os.enumVal) ? Program.listStringsDict[os.enumVal] : string.Format("0x{0,8:X8}", os.enumVal);
                        string mainVal = Program.listStringsDict.ContainsKey(os.main) ? Program.listStringsDict[os.main] : string.Format("0x{0,8:X8}", os.main);

                        XElement xmlString = new("string");
                        xmlString.Add(new XAttribute("enum", enumVal));
                        //if (fileType == 1 || fileType == 3) xmlString.Add(new XAttribute("main", mainVal));
                        if (fileType != 0) xmlString.Add(new XAttribute("extra", mainVal));
                        xmlString.Add(new XAttribute("id", os.id));
                        xmlString.Add(new XAttribute("value", oasisStringValList.ContainsKey(os.id) ? oasisStringValList[os.id] : ""));
                        //xmlSubSection.Add(xmlString);
                        xmlSection.Add(xmlString);
                    }

                    //xmlSection.Add(xmlSubSection);
                }

                xmlStringtable.Add(xmlSection);
            }

            if (input.Position != input.Length)
            {
                int count = input.ReadValueS32();

                XElement xmlSpeeches = new("speeches");

                for (int i = 0; i < count; i++)
                {
                    int id = input.ReadValueS32();
                    xmlSpeeches.Add(new XElement("speech", new XAttribute("id", id)));
                }

                xmlStringtable.Add(xmlSpeeches);
            }

            xmlStringtable.Add(new XAttribute("type", fileType));

            if (fileType == 1)
            {
                string hdr = Program.listStringsDict.ContainsKey(hash) ? Program.listStringsDict[hash] : string.Format("0x{0,8:X8}", hash);
                xmlStringtable.Add(new XAttribute("hdr", hdr));
            }

            xmlDoc.Add(xmlStringtable);
            xmlDoc.Save(outputFile);
            input.Close();
        }

        private static bool IsSubs2()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("You're trying to convert subtitles oasis file. Unfortunately, ND and FC6 don't have any useful values which can help FCBConverter to know if the oasis file is really for ND or FC6.");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Is the oasis subtitles file from ND or FC6? Type Y for yes, N for no.");
            Console.ResetColor();
            ConsoleKeyInfo key = Console.ReadKey();
            return key.Key == ConsoleKey.Y;
        }
    }

    struct OasisString
    {
        public uint enumVal;
        public uint id;
        public uint main;
        public string value;
        public uint extra;
    }
}
