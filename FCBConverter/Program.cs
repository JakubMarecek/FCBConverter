﻿/* 
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

using Gibbed.IO;
using LZ4Sharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace FCBConverter
{
    class Program
    {
        public static string m_Path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        static readonly string m_File = @"\FCBConverterFileNames.list";
        public static Dictionary<ulong, string> m_HashList = new Dictionary<ulong, string>();

        static readonly string stringsFile = @"\FCBConverterStrings.list";
        public static Dictionary<uint, string> strings = new Dictionary<uint, string>();

        static readonly string nocompressFile = @"\FCBConverterNoCompress.txt";

        static readonly string excludeFile = @"\FCBConverterCompressExclude.txt";

        public static bool isCompressEnabled = true;
        public static bool isCombinedMoveFile = false;
        public static bool isNewDawn = false;

        static string excludeFromCompress = "";

        public static string version = "20210203-0000";

        public static string matWarn = " - DO NOT DELETE THIS! DO NOT CHANGE LINE NUMBER!";
        public static string xmlheader = "Converted by FCBConverter v" + version + ", author ArmanIII.";
        public static string xmlheaderfcb = "Based on Gibbed's Dunia Tools. Special thanks to: Fireboyd78 (FCBastard), Gibbed, xBaebsae";
        public static string xmlheaderfcb2 = Environment.NewLine +
            "Please remember that types are calculated and they may not be exactly the same as they are. Take care about this." + Environment.NewLine +
            "To change a value, set \"type\" attribute to a string which is after value-**here**. Example:" + Environment.NewLine +
            "Changing" + Environment.NewLine +
            "  <field hash=\"ABDC41FE\" name=\"fMaxHealth\" value-Float32=\"1000\" type=\"BinHex\">00007A44</field>" + Environment.NewLine +
            "to" + Environment.NewLine +
            "  <field hash=\"ABDC41FE\" name=\"fMaxHealth\" type=\"Float32\">1000</field>" + Environment.NewLine +
            "";
        public static string xmlheaderdepload = "Special thanks to: Fireboyd78 (FCBastard), Ekey (FC5 Unpacker), Gibbed";
        public static string xmlheadermarkup = "Special thanks to: Fireboyd78 (FCBastard), Ekey (FC5 Unpacker), Gibbed";
        public static string xmlheadermove = "Special thanks to: Fireboyd78 (FCBastard), Ekey (FC5 Unpacker), Gibbed";
        public static string xmlheadercombined1 = "Special thanks to: Fireboyd78 (FCBastard), Ekey (FC5 Unpacker), Gibbed";
        public static string xmlheaderoasis = "Special thanks to: AOY";

        //public static List<string> aaaa = new List<string>();

        static void Main(string[] args)
        {
            Console.Title = "FCBConverter";

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("*******************************************************************************************");
            Console.WriteLine("**** FCBConverter v" + version);
            Console.WriteLine("****   Author: ArmanIII");
            Console.WriteLine("****   Based on: Gibbed's Dunia Tools");
            Console.WriteLine("****   Special thanks to: Fireboyd78 (FCBastard), xBaebsae, Ekey (FC5 Unpacker), Gibbed");
            Console.WriteLine("*******************************************************************************************");
            Console.ResetColor();
            Console.WriteLine("");

            if (args.Length < 1)
            {
                Console.WriteLine("Converts many Far Cry formats to XML and vice versa.");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<Batch files converting>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter <folder> <search pattern>");
                Console.WriteLine("    folder - path for folder, use \\ to run it in the same directory where are you running the exe");
                Console.WriteLine("    search pattern - *.fcb for example convert all FCB files");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter D:\\fcb_files *.fcb");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<Unpacking DAT/FAT files>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter <fat file> <output dir>");
                Console.WriteLine("    fat file - path to fat file");
                Console.WriteLine("    output dir (optional) - output folder path, files will extracted to this newly created folder");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter D:\\common.fat D:\\common_unpacked");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<Packing to DAT/FAT>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter <input folder> <fat file> <FAT version>");
                Console.WriteLine("    input folder - input folder path with files");
                Console.WriteLine("    fat file - path to the new fat file");
                Console.WriteLine("    FAT version - can be -v9 (FC4, FC3, FC3BD) or -v5 (FC2), default version is 10 (FC5, FCND), note that older FAT versions can't be compressed");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter D:\\common_unpacked D:\\common.fat");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<Unpacking one file from DAT/FAT>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter <fat file> <output dir> <desired file>");
                Console.WriteLine("    fat file - path to fat file");
                Console.WriteLine("    output dir - output folder path, file will extracted to this folder");
                Console.WriteLine("    desired file - file path inside the FAT file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter D:\\common.fat D:\\ common.dbt.fcb");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For *.fcb, *.ndb files>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter <m_File>");
                Console.WriteLine("    m_File - fcb or xml file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter D:\\file.fcb");
                Console.WriteLine("    FCBConverter D:\\file.fcb.converted.xml");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For *.oasis.bin files>>>");
                Console.ResetColor();
                Console.WriteLine("Because format of oasis is different in FC5 and in New Dawn, if you want convert ND oasis, you must add _nd to filename, see examples.");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter <m_File>");
                Console.WriteLine("    m_File - oasis file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples FC5]");
                Console.WriteLine("    FCBConverter D:\\oasisstrings.oasis.bin");
                Console.WriteLine("    FCBConverter D:\\oasisstrings.oasis.bin.converted.xml");
                Console.WriteLine("");
                Console.WriteLine("[Examples New Dawn]");
                Console.WriteLine("    FCBConverter D:\\oasisstrings_nd.oasis.bin");
                Console.WriteLine("    FCBConverter D:\\oasisstrings_nd.oasis.bin.converted.xml");
                Console.WriteLine("");
                Console.WriteLine("[Examples FC4]");
                Console.WriteLine("    FCBConverter D:\\oasisstrings_compressed.bin");
                Console.WriteLine("    FCBConverter D:\\oasisstrings_compressed.bin.converted.xml");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For *_depload.dat files>>>");
                Console.ResetColor();
                Console.WriteLine("Converts *_depload.dat files to readable XML format.");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter <m_File>");
                Console.WriteLine("    m_File - *_depload.dat file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter D:\\patch_depload.dat");
                Console.WriteLine("    FCBConverter D:\\patch_depload.dat.converted.xml");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For soundinfo.bin files>>>");
                Console.ResetColor();
                Console.WriteLine("Converts soundinfo.bin files to readable XML format.");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter <m_File>");
                Console.WriteLine("    m_File - soundinfo.bin file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter D:\\soundinfo.bin");
                Console.WriteLine("    FCBConverter D:\\soundinfo.bin.converted.xml");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For *.lua to *.luac files>>>");
                Console.ResetColor();
                Console.WriteLine("Converts a lua script file to a lua file with LUAC header.");
                Console.WriteLine("This is only one way converter.");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter <m_File>");
                Console.WriteLine("    m_File - *.lua file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter D:\\script.lua");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For *.xbt files>>>");
                Console.ResetColor();
                Console.WriteLine("Converts *.xbt files to DDS format with *.tex header.");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter <m_File>");
                Console.WriteLine("    m_File - *.xbt file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter D:\\texture.xbt");
                Console.WriteLine("    FCBConverter D:\\texture.dds");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For *.material.bin files>>>");
                Console.ResetColor();
                Console.WriteLine("Converts *.material.bin files to xml format with *.material.mat header.");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter <m_File>");
                Console.WriteLine("    m_File - *.material.bin file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter D:\\abc.material.bin");
                Console.WriteLine("    FCBConverter D:\\abc.material.bin.converted.xml");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For *.move.bin files>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter <m_File>");
                Console.WriteLine("    m_File - *.move.bin file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter D:\\file.move.bin");
                Console.WriteLine("    FCBConverter D:\\file.move.bin.converted.xml");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For combinedmovefile.bin file>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter <m_File>");
                Console.WriteLine("    m_File - combinedmovefile.bin file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter D:\\combinedmovefile.bin");
                Console.WriteLine("    FCBConverter D:\\combinedmovefile.bin.converted.xml");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For *.cseq files>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter <m_File>");
                Console.WriteLine("    m_File - *.cseq file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter D:\\sequence.cseq");
                Console.WriteLine("    FCBConverter D:\\sequence.cseq.converted.xml");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For *.feu, *.swf files>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter <m_File>");
                Console.WriteLine("    m_File - *.feu or *.swf file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter D:\\ui.feu");
                Console.WriteLine("    FCBConverter D:\\ui.swf");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For *.bdl files>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter <m_File>");
                Console.WriteLine("    m_File - *.bdl file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter D:\\0003_0005_0000_0000.terrainnode.bdl");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.ResetColor();
                return;
            }

            if (File.Exists(m_Path + nocompressFile))
            {
                Console.WriteLine("Compression disabled.");
                Console.WriteLine("");
                isCompressEnabled = false;
            }

            string file = args[0];
            string outputFile = args.Length > 1 ? args[1] : "";
            excludeFromCompress = args.Length > 2 ? args[2] : "";

            Console.Title = "FCBConverter - " + file;

            if (outputFile.EndsWith(".fat"))
            {
                int ver = 10;

                if (excludeFromCompress == "-v9")
                    ver = 9;

                if (excludeFromCompress == "-v5")
                    ver = 5;

                LoadFile();
                PackBigFile(file, outputFile, ver);
                FIN();
            }
            else if (file.EndsWith(".fat") && Directory.Exists(outputFile) && excludeFromCompress != "") // excludeFromCompress is used as file name
            {
                UnpackBigFile(file, outputFile, excludeFromCompress);
                FIN();
            }
            else if (file.EndsWith(".fat"))
            {
                UnpackBigFile(file, outputFile);
                FIN();
            }
            else if (File.Exists(file))
            {
                Proccessing(file, outputFile);
            }
            else if (Directory.Exists(file) || file == @"\")
            {
                if (file == @"\")
                    file = Directory.GetCurrentDirectory();

                DirectoryInfo d = new DirectoryInfo(file);
                FileInfo[] files = d.GetFiles(outputFile);
                foreach (FileInfo fileInfo in files)
                {
                    Console.WriteLine("Processing: " + fileInfo.FullName + "...");
                    Proccessing(fileInfo.FullName, "");
                }
                Console.WriteLine("Job done!");
            }
            else
            {
                Console.WriteLine("Input file / directory doesn't exist!");
            }
            return;
        }

        static void Proccessing(string file, string outputFile)
        {
            Console.Title = "FCBConverter - " + file;

            if (file.Contains("worldsector"))
            {
                isCompressEnabled = false;
            }

            // ********************************************************************************************************************************************

            if (file.EndsWith(".feu"))
            {
                byte[] bytes = File.ReadAllBytes(file);

                bytes[0] = (byte)'F';
                bytes[1] = (byte)'W';
                bytes[2] = (byte)'S';

                string newPath = file.Replace(".feu", ".swf");

                File.WriteAllBytes(newPath, bytes);

                FIN();
                return;
            }

            if (file.EndsWith(".swf"))
            {
                byte[] bytes = File.ReadAllBytes(file);

                bytes[0] = (byte)'U';
                bytes[1] = (byte)'E';
                bytes[2] = (byte)'F';

                string newPath = file.Replace(".swf", ".feu");

                File.WriteAllBytes(newPath, bytes);

                FIN();
                return;
            }

            // ********************************************************************************************************************************************

            if (file.EndsWith(".terrainnode.bdl"))
            {
                TerrainNodeBdl(file);
                FIN();
                return;
            }

            if (file.EndsWith(".terrainnode.bdl.converted.xml"))
            {
                TerrainNodeXml(file);
                FIN();
                return;
            }

            // ********************************************************************************************************************************************

            var tmpformat = File.OpenRead(file);
            ushort fmt = tmpformat.ReadValueU8();
            tmpformat.Close();

            if (file.EndsWith(".cseq") || file.EndsWith(".gosm.xml") || file.EndsWith(".rml") || (file.EndsWith(".ndb") && fmt == 0))
            {
                string workingOriginalFile;

                if (outputFile != "")
                    workingOriginalFile = outputFile;
                else
                    workingOriginalFile = Path.GetDirectoryName(file) + "\\" + Path.GetFileName(file) + (file.EndsWith(".ndb") ? ".rml" : "") + ".converted.xml";

                var rez = new Gibbed.Dunia2.FileFormats.XmlResourceFile();
                using (var input = File.OpenRead(file))
                {
                    rez.Deserialize(input);
                }

                var settings = new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8,
                    Indent = true,
                    OmitXmlDeclaration = true
                };

                using (var writer = XmlWriter.Create(workingOriginalFile, settings))
                {
                    writer.WriteStartDocument();
                    Gibbed.Dunia2.ConvertXml.Program.WriteNode(writer, rez.Root);
                    writer.WriteEndDocument();
                }

                FIN();
                return;
            }

            if (file.EndsWith(".cseq.converted.xml") || file.EndsWith(".gosm.xml.converted.xml") || file.EndsWith(".rml.converted.xml"))
            {
                string workingOriginalFile;

                if (outputFile != "")
                    workingOriginalFile = outputFile;
                else
                {
                    workingOriginalFile = file.EndsWith(".ndb.rml.converted.xml") ? file.Replace(".rml.converted.xml", "") : file.Replace(".converted.xml", "");
                    string extension = Path.GetExtension(workingOriginalFile);
                    workingOriginalFile = Path.GetDirectoryName(workingOriginalFile) + "\\" + Path.GetFileNameWithoutExtension(workingOriginalFile) + "_new" + extension;
                }

                var rez = new Gibbed.Dunia2.FileFormats.XmlResourceFile();
                using (var input = File.OpenRead(file))
                {
                    var doc = new XPathDocument(input);
                    var nav = doc.CreateNavigator();

                    if (nav.MoveToFirstChild() == false)
                    {
                        throw new FormatException();
                    }

                    rez.Root = Gibbed.Dunia2.ConvertXml.Program.ReadNode(nav);
                }

                using (var output = File.Create(workingOriginalFile))
                {
                    rez.Serialize(output);
                }

                FIN();
                return;
            }

            // ********************************************************************************************************************************************

            if (file.EndsWith(".xbt") || file.EndsWith(".xbts"))
            {
                byte[] bytes = File.ReadAllBytes(file);

                int pos = IndexOf(bytes, new byte[] { 68, 68, 83 }); // DDS

                byte[] tex = bytes.Take(pos).ToArray();
                byte[] dds = bytes.Skip(pos).Take(bytes.Length).ToArray();

                string newPathDds = file.Replace(".xbts", ".dds").Replace(".xbt", ".dds");
                string newPathTex = file.Replace(".xbts", ".tex").Replace(".xbt", ".tex");

                if (file.EndsWith(".xbts"))
                {
                    byte[] xbts = bytes.Skip(bytes.Length - 20).Take(20).ToArray();
                    string newPathXbts = file.Replace(".xbts", ".texXbts");
                    File.WriteAllBytes(newPathXbts, xbts);
                    dds = dds.Take(dds.Length - 20).ToArray();
                }

                File.WriteAllBytes(newPathDds, dds);
                File.WriteAllBytes(newPathTex, tex);

                FIN();
                return;
            }

            if (file.EndsWith(".dds"))
            {
                string texPath = file.Replace(".dds", ".tex");
                string xbtsPath = file.Replace(".dds", ".texXbts");
                string xbtPath = file.Replace(".dds", ".xbt");
                bool isMips = file.Replace(".dds", "").EndsWith("_mips");
                string texNoMipsPath = file.Replace("_mips.dds", ".tex");
                int mipsCount = 0;

                if (!File.Exists(texPath))
                {
                    Console.WriteLine(texPath + " was not found! Cannot convert DDS to XBT.");
                    return;
                }

                if (isMips)
                {
                    if (!File.Exists(texNoMipsPath))
                    {
                        Console.WriteLine(texNoMipsPath + " was not found! Cannot convert DDS to XBT.");
                        return;
                    }

                    FileStream bin = new FileStream(texNoMipsPath, FileMode.Open);
                    bin.Seek(17, SeekOrigin.Begin);
                    mipsCount = bin.ReadByte();
                    bin.Close();
                }

                List<byte> bts = new List<byte>();

                byte[] bytesDDS = File.ReadAllBytes(file);
                byte[] bytesTEX = File.ReadAllBytes(texPath);

                bts.AddRange(bytesTEX);
                bts.AddRange(bytesDDS);

                if (isMips)
                {
                    bts[64] = (byte)mipsCount;
                }

                if (File.Exists(xbtsPath))
                {
                    byte[] bytesXBTS = File.ReadAllBytes(xbtsPath);
                    bts.AddRange(bytesXBTS);
                    xbtPath = file.Replace(".dds", ".xbts");
                }

                File.WriteAllBytes(xbtPath, bts.ToArray());

                FIN();
                return;
            }

            // ********************************************************************************************************************************************

            if (file.EndsWith(".lua"))
            {
                string newLuaFile = file.Replace(".lua", "_luac.lua");

                string luaScript = File.ReadAllText(file);

                string[] splitedScriptFile = luaScript.Split(new string[] { "<DominoMetadata" }, StringSplitOptions.None);

                string dominoMetadata = "<DominoMetadata" + splitedScriptFile[1];

                splitedScriptFile[0] = "-- Converted by FCBConverter by ArmanIII" + Environment.NewLine + splitedScriptFile[0];

                if (File.Exists(newLuaFile))
                    File.Delete(newLuaFile);

                FileStream bin = new FileStream(newLuaFile, FileMode.Create);
                bin.Write(BitConverter.GetBytes(0x4341554c), 0, 4);
                bin.Write(BitConverter.GetBytes(splitedScriptFile[0].Length), 0, sizeof(int));
                bin.Write(Encoding.UTF8.GetBytes(splitedScriptFile[0]), 0, splitedScriptFile[0].Length);
                bin.Write(Encoding.UTF8.GetBytes(dominoMetadata), 0, dominoMetadata.Length);
                bin.Close();

                FIN();
                return;
            }

            // ********************************************************************************************************************************************

            if (file.EndsWith(".oasis.bin"))
            {
                string workingOriginalFile;

                if (outputFile != "")
                    workingOriginalFile = outputFile;
                else
                    workingOriginalFile = Path.GetDirectoryName(file) + "\\" + Path.GetFileName(file) + ".converted.xml";


                OasisstringsCompressedFile rez = new OasisstringsCompressedFile();

                var input = File.OpenRead(file);
                rez.Deserialize(input, Path.GetFileName(file).EndsWith("_nd.oasis.bin"));

                var settings = new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8,
                    Indent = true,
                    OmitXmlDeclaration = true
                };

                using (var writer = XmlWriter.Create(workingOriginalFile, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteComment(xmlheader);
                    WriteOSNode(writer, rez.Root);
                    writer.WriteEndDocument();
                }

                FIN();
                return;
            }

            if (file.EndsWith(".oasis.bin.converted.xml"))
            {
                string workingOriginalFile;

                if (outputFile != "")
                    workingOriginalFile = outputFile;
                else
                {
                    workingOriginalFile = file.Replace(".converted.xml", "");
                    string extension = Path.GetExtension(workingOriginalFile);
                    workingOriginalFile = Path.GetDirectoryName(file) + "\\" + Path.GetFileNameWithoutExtension(workingOriginalFile) + ".new" + extension;
                }

                var rez = new OasisstringsCompressedFile();

                var input = File.OpenRead(file);
                var doc = new XPathDocument(input);
                var nav = doc.CreateNavigator();

                if (nav.MoveToFirstChild() == false)
                {
                    throw new FormatException();
                }

                rez.Root = ReadOSNode(nav);

                var output = File.Create(workingOriginalFile);
                rez.Serialize(output, Path.GetFileName(file).EndsWith("_nd.oasis.bin.converted.xml"));

                FIN();
                return;
            }

            if (file.EndsWith("oasisstrings_compressed.bin"))
            {
                string workingOriginalFile;

                if (outputFile != "")
                    workingOriginalFile = outputFile;
                else
                    workingOriginalFile = Path.GetDirectoryName(file) + "\\" + Path.GetFileName(file) + ".converted.xml";

                OasisstringsCompressedFileFC4 rez = new OasisstringsCompressedFileFC4();

                var input = File.OpenRead(file);
                rez.Deserialize(input);

                var settings = new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8,
                    Indent = true,
                    OmitXmlDeclaration = true
                };

                using (var writer = XmlWriter.Create(workingOriginalFile, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteComment(xmlheader);
                    writer.WriteComment(xmlheaderoasis);
                    WriteOSNode(writer, rez.Root);
                    writer.WriteEndDocument();
                }

                FIN();
                return;
            }

            if (file.EndsWith("oasisstrings_compressed.bin.converted.xml"))
            {
                string workingOriginalFile;

                if (outputFile != "")
                    workingOriginalFile = outputFile;
                else
                {
                    workingOriginalFile = file.Replace(".converted.xml", "");
                    string extension = Path.GetExtension(workingOriginalFile);
                    workingOriginalFile = Path.GetDirectoryName(file) + "\\" + Path.GetFileNameWithoutExtension(workingOriginalFile) + ".new" + extension;
                }

                var rez = new OasisstringsCompressedFileFC4();

                var input = File.OpenRead(file);
                var doc = new XPathDocument(input);
                var nav = doc.CreateNavigator();

                if (nav.MoveToFirstChild() == false)
                {
                    throw new FormatException();
                }

                rez.Root = ReadOSNode(nav);

                var output = File.Create(workingOriginalFile);
                rez.Serialize(output);

                FIN();
                return;
            }

            // ********************************************************************************************************************************************

            if (file.EndsWith("soundinfo.bin.converted.xml"))
            {
                SoundInfoConvertXml(file);
                FIN();
                return;
            }
            else if (file.EndsWith("soundinfo.bin"))
            {
                SoundInfoConvertBin(file);
                FIN();
                return;
            }

            // ********************************************************************************************************************************************

            LoadString();

            // ********************************************************************************************************************************************

            if (file.EndsWith(".markup.bin.converted.xml"))
            {
                MarkupConvertXml(file);
                FIN();
                return;
            }
            else if (file.EndsWith(".markup.bin"))
            {
                MarkupConvertBin(file);
                FIN();
                return;
            }

            // ********************************************************************************************************************************************

            if (file.EndsWith(".move.bin.converted.xml"))
            {
                MoveConvertXml(file);
                FIN();
                return;
            }
            else if (file.EndsWith(".move.bin"))
            {
                MoveConvertBin(file);
                FIN();
                return;
            }

            // ********************************************************************************************************************************************

            if (file.EndsWith("combinedmovefile.bin.converted.xml"))
            {
                isCombinedMoveFile = true;
                isCompressEnabled = false;
                CombinedMoveFileConvertXml(file);
                FIN();
                return;
            }
            else if (file.EndsWith("combinedmovefile.bin"))
            {
                isCombinedMoveFile = true;
                CombinedMoveFileConvertBin(file);
                FIN();
                return;
            }

            // ********************************************************************************************************************************************

            LoadFile();

            // ********************************************************************************************************************************************

            if (file.EndsWith("_depload.dat.converted.xml"))
            {
                DeploadConvertXml(file);
                FIN();
                return;
            }
            else if (file.EndsWith("_depload.dat"))
            {
                DeploadConvertDat(file);
                FIN();
                return;
            }

            // ********************************************************************************************************************************************

            if (file.EndsWith(".converted.xml"))
            {
                if (file.Replace(".converted.xml", "").EndsWith(".material.bin"))
                {
                    string newFileName = file.Replace(".converted.xml", "");
                    string matFile = newFileName.Replace(".bin", ".mat");
                    newFileName = newFileName.Replace(".material.bin", "_new" + ".material.bin");

                    ConvertXML(file, newFileName);

                    List<byte> bts = new List<byte>();
                    bts.AddRange(File.ReadAllBytes(matFile));
                    bts.AddRange(File.ReadAllBytes(newFileName));

                    File.WriteAllBytes(newFileName, bts.ToArray());
                }
                else if (file.EndsWith(".part.converted.xml") && File.Exists(file.Replace(".part.converted.xml", ".pt")))
                {
                    string newFileName = file.Replace(".converted.xml", "");
                    string matFile = newFileName.Replace(".part", ".pt");
                    newFileName = newFileName.Replace(".part", "_new.part");

                    ConvertXML(file, newFileName);

                    List<byte> bts = new List<byte>();
                    bts.AddRange(File.ReadAllBytes(matFile));
                    bts.AddRange(File.ReadAllBytes(newFileName));

                    File.WriteAllBytes(newFileName, bts.ToArray());
                }
                else if (file.EndsWith(".fcb.lzo.converted.xml"))
                {
                    string workingOriginalFile;

                    if (outputFile != "")
                        workingOriginalFile = outputFile;
                    else
                    {
                        workingOriginalFile = file.Replace(".lzo.converted.xml", "");
                        string extension = Path.GetExtension(workingOriginalFile);
                        workingOriginalFile = Path.GetDirectoryName(workingOriginalFile) + "\\" + Path.GetFileNameWithoutExtension(workingOriginalFile) + "_new" + extension;
                    }

                    var bof = new Gibbed.Dunia2.FileFormats.BinaryObjectFile();

                    var basePath = Path.ChangeExtension(file, null);

                    var doc = new XPathDocument(file);
                    var nav = doc.CreateNavigator();

                    var root = nav.SelectSingleNode("/object");

                    var importing = new Gibbed.Dunia2.ConvertBinaryObject.Importing();
                    bof.Root = importing.Import(basePath, root);

                    MemoryStream ms = new MemoryStream();
                    bof.Serialize(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    byte[] uncompressedBytes = ms.ToArray();

                    byte[] compressedBytes = new byte[uncompressedBytes.Length + (uncompressedBytes.Length / 16) + 64 + 3]; // weird magic
                    int outputSize = compressedBytes.Length;

                    var result = Gibbed.Dunia2.FileFormats.LZO.Compress(uncompressedBytes,
                                                0,
                                                uncompressedBytes.Length,
                                                compressedBytes,
                                                0,
                                                ref outputSize);

                    var output = File.Create(workingOriginalFile);
                    output.WriteValueS32(uncompressedBytes.Length);
                    output.WriteBytes(compressedBytes);
                    output.Flush();
                    output.Close();
                }
                else
                {
                    string workingOriginalFile;

                    if (outputFile != "")
                        workingOriginalFile = outputFile;
                    else
                    {
                        workingOriginalFile = file.Replace(".converted.xml", "");
                        string extension = Path.GetExtension(workingOriginalFile);
                        workingOriginalFile = Path.GetDirectoryName(workingOriginalFile) + "\\" + Path.GetFileNameWithoutExtension(workingOriginalFile) + "_new" + extension;
                    }

                    if (isCompressEnabled && new FileInfo(file).Length > 20000000)
                        Console.WriteLine("Compressing big files will take some time.");

                    ConvertXML(file, workingOriginalFile);
                }

                FIN();
                return;
            }
            else if (file.EndsWith(".fcb") || file.EndsWith(".ndb") || file.EndsWith(".bin") || file.EndsWith(".bwsk") || file.EndsWith(".part") || file.EndsWith(".dsc") || file.EndsWith(".skeleton"))
            {
                string workingOriginalFile;

                if (outputFile != "")
                    workingOriginalFile = outputFile;
                else
                    workingOriginalFile = Path.GetDirectoryName(file) + "\\" + Path.GetFileName(file) + ".converted.xml";

                tmpformat = File.OpenRead(file);
                uint nbCF = tmpformat.ReadValueU32();
                uint ver = tmpformat.ReadValueU32();
                tmpformat.Close();

                if (nbCF != 1178821230 && file.EndsWith(".fcb")) // nbCF
                {
                    var aaa = File.OpenRead(file);
                    int bbb = (int)aaa.ReadValueU32();

                    byte[] buffer = new byte[16 * 1024];
                    MemoryStream ms = new MemoryStream();
                    int read;
                    while ((read = aaa.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    byte[] ddd = ms.ToArray();

                    aaa.Close();

                    byte[] eee = new byte[bbb];

                    var result = Gibbed.Dunia2.FileFormats.LZO.Decompress(ddd,
                                                0,
                                                ddd.Length,
                                                eee,
                                                0,
                                                ref bbb);

                    var bof = new Gibbed.Dunia2.FileFormats.BinaryObjectFile();
                    bof.Deserialize(new MemoryStream(eee));
                    Gibbed.Dunia2.ConvertBinaryObject.Exporting.Export(workingOriginalFile.Replace(".fcb", ".fcb.lzo"), bof);
                }
                else
                {
                    if (file.EndsWith(".material.bin"))
                    {
                        byte[] bytes = File.ReadAllBytes(file);

                        int pos = IndexOf(bytes, new byte[] { 0x6E, 0x62, 0x43, 0x46 }); // nbCF

                        byte[] mat = bytes.Take(pos).ToArray();
                        byte[] fcb = bytes.Skip(pos).Take(bytes.Length).ToArray();

                        string newPathMat = file.Replace(".bin", ".mat");
                        File.WriteAllBytes(newPathMat, mat);
                        File.WriteAllBytes(file + "tmp", fcb);

                        ConvertFCB(file + "tmp", workingOriginalFile);

                        File.Delete(file + "tmp");
                    }
                    else if (ver != 2 && file.EndsWith(".part"))
                    {
                        byte[] bytes = File.ReadAllBytes(file);

                        int pos = IndexOf(bytes, new byte[] { 0x6E, 0x62, 0x43, 0x46, 0x02, 0x00 }); // nbCF

                        byte[] mat = bytes.Take(pos).ToArray();
                        byte[] fcb = bytes.Skip(pos).Take(bytes.Length).ToArray();

                        string newPathMat = file.Replace(".part", ".pt");
                        File.WriteAllBytes(newPathMat, mat);
                        File.WriteAllBytes(file + "tmp", fcb);

                        ConvertFCB(file + "tmp", workingOriginalFile);

                        File.Delete(file + "tmp");
                    }
                    else
                        ConvertFCB(file, workingOriginalFile);
                }

                FIN();
                return;
            }

            // ********************************************************************************************************************************************

            FIN();
        }

        static void FIN()
        {
            //File.WriteAllLines("a.txt", aaaa);
            Console.WriteLine("FIN");
            //Environment.Exit(0);
        }

        public static int IndexOf(byte[] arrayToSearchThrough, byte[] patternToFind)
        {
            if (patternToFind.Length > arrayToSearchThrough.Length)
                return -1;
            for (int i = 0; i < arrayToSearchThrough.Length - patternToFind.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < patternToFind.Length; j++)
                {
                    if (arrayToSearchThrough[i + j] != patternToFind[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }

        static void LoadFile(int dwVersion = 10)
        {
            if (m_HashList.Count() > 0)
                return;

            if (!File.Exists(m_Path + m_File))
            {
                Console.WriteLine(m_Path + m_File + " doesn't exist!");
                return;
            }

            string[] ss = File.ReadAllLines(m_Path + m_File);
            for (int i = 0; i < ss.Length; i++)
            {
                ulong a = dwVersion == 5 ? Gibbed.Dunia2.FileFormats.CRC32.Hash(ss[i]) : Gibbed.Dunia2.FileFormats.CRC64.Hash(ss[i]);
                if (!m_HashList.ContainsKey(a))
                    m_HashList.Add(a, ss[i]);
            }

            Console.WriteLine("Files loaded: " + m_HashList.Count);
        }

        static void LoadString()
        {
            if (strings.Count() > 0)
                return;

            if (!File.Exists(m_Path + stringsFile))
            {
                Console.WriteLine(m_Path + stringsFile + " doesn't exist!");
                return;
            }

            string[] ss = File.ReadAllLines(m_Path + stringsFile);
            for (int i = 0; i < ss.Length; i++)
            {
                uint a = Gibbed.Dunia2.FileFormats.CRC32.Hash(ss[i]);
                if (!strings.ContainsKey(a))
                    strings.Add(a, ss[i]);
            }

            Console.WriteLine("Strings loaded: " + strings.Count);
        }

        public static void ConvertFCB(string inputPath, string outputPath)
        {
            var bof = new Gibbed.Dunia2.FileFormats.BinaryObjectFile();
            var input = File.OpenRead(inputPath);
            bof.Deserialize(input);
            input.Close();

            Gibbed.Dunia2.ConvertBinaryObject.Exporting.Export(outputPath, bof);
        }

        public static void ConvertXML(string inputPath, string outputPath)
        {
            var bof = new Gibbed.Dunia2.FileFormats.BinaryObjectFile();

            var basePath = Path.ChangeExtension(inputPath, null);

            var doc = new XPathDocument(inputPath);
            var nav = doc.CreateNavigator();

            var root = nav.SelectSingleNode("/object");

            var importing = new Gibbed.Dunia2.ConvertBinaryObject.Importing();
            bof.Root = importing.Import(basePath, root);

            var output = File.Create(outputPath);
            bof.Serialize(output);
            output.Close();
        }
       
        /*
        static string[] ReadAllResourceLines(string resourceName)
        {
            using (Stream stream = Assembly.GetEntryAssembly()
                .GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return EnumerateLines(reader).ToArray();
            }
        }
        
        static IEnumerable<string> EnumerateLines(TextReader reader)
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }
        */
        
        public static void WriteOSNode(XmlWriter writer, OasisstringsCompressedFile.Node node)
        {
            writer.WriteStartElement(node.Name);

            foreach (var attribute in node.Attributes)
            {
                writer.WriteAttributeString(attribute.Name, attribute.Value);
            }

            foreach (var child in node.Children)
            {
                WriteOSNode(writer, child);
            }

            if (string.IsNullOrEmpty(node.Value) == false)
            {
                writer.WriteValue(node.Value);
            }

            writer.WriteEndElement();
        }

        public static OasisstringsCompressedFile.Node ReadOSNode(XPathNavigator nav)
        {
            var node = new OasisstringsCompressedFile.Node
            {
                Name = nav.Name
            };

            if (nav.MoveToFirstAttribute() == true)
            {
                node.Attributes = new List<OasisstringsCompressedFile.Attribute>();

                do
                {
                    node.Attributes.Add(new OasisstringsCompressedFile.Attribute()
                    {
                        Name = nav.Name,
                        Value = nav.Value,
                    });
                }
                while (nav.MoveToNextAttribute() == true);
                nav.MoveToParent();
            }

            var children = nav.SelectChildren(XPathNodeType.Element);
            if (children.Count > 0)
            {
                node.Value = "";
                node.Children = new List<OasisstringsCompressedFile.Node>();
                while (children.MoveNext() == true)
                {
                    if (children.Current == null)
                    {
                        throw new InvalidOperationException();
                    }

                    node.Children.Add(ReadOSNode(children.Current.CreateNavigator()));
                }
            }
            else
            {
                node.Value = nav.Value;
            }

            return node;
        }

        public static ulong GetFileHash(string fileName, int dwVersion = 10)
        {
            if (fileName.ToLowerInvariant().Contains("__unknown"))
            {
                var partName = Path.GetFileNameWithoutExtension(fileName);

                if (dwVersion >= 9)
                {
                    if (partName.Length > 16)
                    {
                        partName = partName.Substring(0, 16);
                    }
                }
                if (dwVersion == 5)
                {
                    if (partName.Length > 8)
                    {
                        partName = partName.Substring(0, 8);
                    }
                }

                return ulong.Parse(partName, NumberStyles.AllowHexSpecifier);
            }
            else
            {
                if (dwVersion >= 9)
                {
                    return Gibbed.Dunia2.FileFormats.CRC64.Hash(fileName);
                }
                if (dwVersion == 5)
                {
                    return Gibbed.Dunia2.FileFormats.CRC32.Hash(fileName);
                }
            }

            return 0;
        }

        static void DeploadConvertDat(string file)
        {
            FileStream DeploadStream = new FileStream(file, FileMode.Open);
            BinaryReader DeploadReader = new BinaryReader(DeploadStream);

            List<Depload.DependentFile> DependentFiles = new List<Depload.DependentFile>();
            List<ulong> DependencyFiles = new List<ulong>();
            List<byte> DependencyFilesTypes = new List<byte>();
            List<string> Types = new List<string>();

            int DependentFilesCount = DeploadReader.ReadInt32();
            for (int i = 0; i < DependentFilesCount; i++)
            {
                int dependencyFilesStartIndex = DeploadReader.ReadInt32();
                int countOfDependencyFiles = DeploadReader.ReadInt32();
                ulong fileHash = DeploadReader.ReadUInt64();
                DependentFiles.Add(new Depload.DependentFile { DependencyFilesStartIndex = dependencyFilesStartIndex, CountOfDependencyFiles = countOfDependencyFiles, FileHash = fileHash });
            }

            int DependencyFilesCount = DeploadReader.ReadInt32();
            for (int i = 0; i < DependencyFilesCount; i++)
            {
                ulong fileHash = DeploadReader.ReadUInt64();
                DependencyFiles.Add(fileHash);
            }

            int DependencyFilesTypesCount = DeploadReader.ReadInt32();
            for (int i = 0; i < DependencyFilesTypesCount; i++)
            {
                byte fileTypeIndex = DeploadReader.ReadByte();
                DependencyFilesTypes.Add(fileTypeIndex);
            }

            int TypesCount = DeploadReader.ReadInt32();
            for (int i = 0; i < TypesCount; i++)
            {
                uint typeHash = DeploadReader.ReadUInt32();
                Types.Add(strings.ContainsKey(typeHash) ? strings[typeHash] : typeHash.ToString("X8"));
            }

            DeploadReader.Dispose();
            DeploadStream.Dispose();

            // ****************************************************************************************************
            // ********** Proccess
            // ****************************************************************************************************

            List<Depload.DependencyLoaderItem> dependencyLoaderItems = new List<Depload.DependencyLoaderItem>();

            for (int i = 0; i < DependentFiles.Count; i++)
            {
                Depload.DependencyLoaderItem dependencyLoaderItem = new Depload.DependencyLoaderItem();
                dependencyLoaderItem.fileName = m_HashList.ContainsKey(DependentFiles[i].FileHash) ? m_HashList[DependentFiles[i].FileHash] : "__Unknown\\" + DependentFiles[i].FileHash.ToString("X16");

                dependencyLoaderItem.depFiles = new List<string>();
                dependencyLoaderItem.depTypes = new List<int>();

                for (int j = 0; j < DependentFiles[i].CountOfDependencyFiles; j++)
                {
                    ulong dependencyFile = DependencyFiles[DependentFiles[i].DependencyFilesStartIndex + j];
                    dependencyLoaderItem.depFiles.Add(m_HashList.ContainsKey(dependencyFile) ? m_HashList[dependencyFile] : "__Unknown\\" + dependencyFile.ToString("X16"));
                }

                for (int k = 0; k < DependentFiles[i].CountOfDependencyFiles; k++)
                {
                    byte depType = DependencyFilesTypes[DependentFiles[i].DependencyFilesStartIndex + k];
                    dependencyLoaderItem.depTypes.Add(depType);
                }

                dependencyLoaderItems.Add(dependencyLoaderItem);
            }

            // ****************************************************************************************************
            // ********** Write
            // ****************************************************************************************************

            XmlDocument xmlDoc = new XmlDocument();

            XmlDeclaration xmldecl;
            xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");

            XmlNode rootNode = xmlDoc.CreateElement("root");
            xmlDoc.AppendChild(rootNode);

            xmlDoc.InsertBefore(xmldecl, rootNode);

            XmlComment comment1 = xmlDoc.CreateComment(xmlheader);
            XmlComment comment2 = xmlDoc.CreateComment(xmlheaderdepload);

            xmlDoc.InsertBefore(comment1, rootNode);
            xmlDoc.InsertBefore(comment2, rootNode);

            for (int i = 0; i < dependencyLoaderItems.Count; i++)
            {
                XmlNode FileNode = xmlDoc.CreateElement("CBinaryResourceContainer");
                rootNode.AppendChild(FileNode);

                XmlAttribute FileNameAttribute = xmlDoc.CreateAttribute("ID");
                FileNameAttribute.Value = dependencyLoaderItems[i].fileName;
                FileNode.Attributes.Append(FileNameAttribute);

                for (int j = 0; j < dependencyLoaderItems[i].depFiles.Count; j++)
                {
                    XmlNode DependencyNode = xmlDoc.CreateElement(Types[dependencyLoaderItems[i].depTypes[j]]);
                    FileNode.AppendChild(DependencyNode);

                    XmlAttribute DependencyFileNameAttribute = xmlDoc.CreateAttribute("ID");
                    DependencyFileNameAttribute.Value = dependencyLoaderItems[i].depFiles[j];
                    DependencyNode.Attributes.Append(DependencyFileNameAttribute);
                }
            }

            xmlDoc.Save(file + ".converted.xml");
        }

        static void DeploadConvertXml(string file)
        {
            SortedDictionary<ulong, Depload.DependentFile> DependentFiles = new SortedDictionary<ulong, Depload.DependentFile>();
            List<ulong> DependencyFiles = new List<ulong>();
            List<byte> DependencyFilesTypes = new List<byte>();
            List<string> Types = new List<string>();


            XDocument doc = XDocument.Load(file);
            XElement root = doc.Element("root");

            IEnumerable<XElement> DependentFilesXML = root.Elements("CBinaryResourceContainer");
            foreach (XElement DependentFileXML in DependentFilesXML)
            {
                string fileName = DependentFileXML.Attribute("ID").Value.ToLowerInvariant();
                ulong fileHash = GetFileHash(fileName);

                Depload.DependentFile dependentFile = new Depload.DependentFile();
                dependentFile.DependencyFilesStartIndex = DependencyFiles.Count;
                dependentFile.FileHash = fileHash;

                int i = 0;
                IEnumerable<XElement> dependencies = DependentFileXML.Elements();
                foreach (XElement dependency in dependencies)
                {
                    string dependFileName = dependency.Attribute("ID").Value.ToString().ToLowerInvariant();
                    string dependType = dependency.Name.ToString();

                    if (!Types.Contains(dependType))
                        Types.Add(dependType);

                    DependencyFiles.Add(GetFileHash(dependFileName));
                    DependencyFilesTypes.Add((byte)Types.FindIndex(a => a == dependType));
                    i++;
                }

                dependentFile.CountOfDependencyFiles = i;

                //DependentFiles.Add(fileHash, dependentFile);
                DependentFiles[fileHash] = dependentFile;
            }

            string newName = file.Replace("_depload.dat.converted.xml", "_new_depload.dat");

            var output = File.Create(newName);
            output.WriteValueS32(DependentFiles.Count, 0);

            foreach (ulong dependentFileHash in DependentFiles.Keys)
            {
                var dependentFile = DependentFiles[dependentFileHash];

                output.WriteValueS32(dependentFile.DependencyFilesStartIndex, 0);
                output.WriteValueS32(dependentFile.CountOfDependencyFiles, 0);
                output.WriteValueU64(dependentFile.FileHash);
            }

            output.WriteValueS32(DependencyFiles.Count, 0);
            for (int i = 0; i < DependencyFiles.Count; i++)
            {
                output.WriteValueU64(DependencyFiles[i]);
            }

            output.WriteValueS32(DependencyFilesTypes.Count, 0);
            for (int i = 0; i < DependencyFilesTypes.Count; i++)
            {
                output.WriteByte(DependencyFilesTypes[i]);
            }

            output.WriteValueS32(Types.Count, 0);
            for (int i = 0; i < Types.Count; i++)
            {
                uint type = 0;

                if (strings.ContainsValue(Types[i]))
                    type = Gibbed.Dunia2.FileFormats.CRC32.Hash(Types[i]);
                else
                    type = uint.Parse(Types[i], NumberStyles.AllowHexSpecifier);

                output.WriteValueU32(type, 0);
            }

            output.Close();
        }

        static void SoundInfoConvertXml(string file)
        {
            string newName = file.Replace("soundinfo.bin.converted.xml", "soundinfo_new.bin");
            var output = File.Create(newName);

            XDocument doc = XDocument.Load(file);
            XElement root = doc.Element("SoundInfo");

            string version = root.Attribute("Version").Value;
            output.WriteValueU32(uint.Parse(version), 0);

            IEnumerable<XElement> Events = root.Element("Events").Elements("Event");
            IEnumerable<XElement> InitSoundBanks = root.Element("InitSoundBanks").Elements("InitBank");
            IEnumerable<XElement> SoundBanks = root.Element("SoundBanks").Elements("Bank");
            output.WriteValueU32((uint)Events.Count(), 0);
            output.WriteValueU32((uint)InitSoundBanks.Count(), 0);
            output.WriteValueU32((uint)SoundBanks.Count(), 0);

            foreach (XElement Event in Events)
            {
                string ShortID = Event.Attribute("ShortID").Value;
                string SoundBankID = Event.Attribute("SoundBankID").Value;
                string Priority = Event.Attribute("Priority").Value;
                string MemoryNodeId = Event.Attribute("MemoryNodeId").Value;
                string MaxRadius = Event.Attribute("MaxRadius").Value;
                string Unknown = Event.Attribute("Unknown").Value;
                string Duration = Event.Attribute("Duration").Value;

                output.WriteValueU32(uint.Parse(ShortID), 0);
                output.WriteValueU32(uint.Parse(SoundBankID), 0);
                output.WriteByte((byte)int.Parse(Priority));
                output.WriteByte((byte)int.Parse(MemoryNodeId));
                output.WriteByte((byte)int.Parse(MaxRadius));
                output.WriteByte((byte)int.Parse(Unknown));
                output.WriteValueF32(float.Parse(Duration, CultureInfo.InvariantCulture), 0);
            }

            foreach (XElement InitSoundBank in InitSoundBanks)
            {
                string ShortID = InitSoundBank.Attribute("ShortID").Value;
                output.WriteValueU32(uint.Parse(ShortID), 0);
            }

            foreach (XElement SoundBank in SoundBanks)
            {
                string ShortID = SoundBank.Attribute("ShortID").Value;
                string Unknown = SoundBank.Attribute("Unknown").Value;
                string bnkFileName = SoundBank.Attribute("bnkFileName").Value.ToString().ToLowerInvariant();

                output.WriteValueU32(uint.Parse(ShortID), 0);
                output.WriteValueU32(uint.Parse(Unknown), 0);
                output.WriteValueU64(GetFileHash(bnkFileName), 0);
            }

            output.Close();
        }

        static void SoundInfoConvertBin(string file)
        {
            FileStream SoundInfoStream = new FileStream(file, FileMode.Open);
            BinaryReader SoundInfoReader = new BinaryReader(SoundInfoStream);

            XmlDocument xmlDoc = new XmlDocument();

            XmlDeclaration xmldecl;
            xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");

            XmlNode rootNode = xmlDoc.CreateElement("SoundInfo");
            xmlDoc.AppendChild(rootNode);

            xmlDoc.InsertBefore(xmldecl, rootNode);

            XmlComment comment1 = xmlDoc.CreateComment(xmlheader);

            xmlDoc.InsertBefore(comment1, rootNode);

            uint Version = SoundInfoReader.ReadUInt32();
            uint EventsCount = SoundInfoReader.ReadUInt32();
            uint InitSoundBanksCount = SoundInfoReader.ReadUInt32();
            uint SoundBanksCount = SoundInfoReader.ReadUInt32();

            XmlAttribute rootNodeAttributeVersion = xmlDoc.CreateAttribute("Version");
            rootNodeAttributeVersion.Value = Version.ToString();
            rootNode.Attributes.Append(rootNodeAttributeVersion);

            XmlNode EventsNode = xmlDoc.CreateElement("Events");
            rootNode.AppendChild(EventsNode);

            for (int i = 0; i < EventsCount; i++)
            {
                uint ShortID = SoundInfoReader.ReadUInt32();
                uint SoundBankID = SoundInfoReader.ReadUInt32();
                byte Priority = SoundInfoReader.ReadByte();
                byte MemoryNodeId = SoundInfoReader.ReadByte();
                byte MaxRadius = SoundInfoReader.ReadByte();
                byte Unknown = SoundInfoReader.ReadByte();
                float Duration = SoundInfoReader.ReadSingle();

                XmlNode EventNode = xmlDoc.CreateElement("Event");
                EventsNode.AppendChild(EventNode);

                XmlAttribute EventNodeAttributeShortID = xmlDoc.CreateAttribute("ShortID");
                EventNodeAttributeShortID.Value = ShortID.ToString();
                EventNode.Attributes.Append(EventNodeAttributeShortID);

                XmlAttribute EventNodeAttributeSoundBankID = xmlDoc.CreateAttribute("SoundBankID");
                EventNodeAttributeSoundBankID.Value = SoundBankID.ToString();
                EventNode.Attributes.Append(EventNodeAttributeSoundBankID);

                XmlAttribute EventNodeAttributePriority = xmlDoc.CreateAttribute("Priority");
                EventNodeAttributePriority.Value = ((int)Priority).ToString();
                EventNode.Attributes.Append(EventNodeAttributePriority);

                XmlAttribute EventNodeAttributeMemoryNodeId = xmlDoc.CreateAttribute("MemoryNodeId");
                EventNodeAttributeMemoryNodeId.Value = ((int)MemoryNodeId).ToString();
                EventNode.Attributes.Append(EventNodeAttributeMemoryNodeId);

                XmlAttribute EventNodeAttributeMaxRadius = xmlDoc.CreateAttribute("MaxRadius");
                EventNodeAttributeMaxRadius.Value = ((int)MaxRadius).ToString();
                EventNode.Attributes.Append(EventNodeAttributeMaxRadius);

                XmlAttribute EventNodeAttributeUnknown = xmlDoc.CreateAttribute("Unknown");
                EventNodeAttributeUnknown.Value = ((int)Unknown).ToString();
                EventNode.Attributes.Append(EventNodeAttributeUnknown);

                XmlAttribute EventNodeAttributeDuration = xmlDoc.CreateAttribute("Duration");
                EventNodeAttributeDuration.Value = Duration.ToString(CultureInfo.InvariantCulture);
                EventNode.Attributes.Append(EventNodeAttributeDuration);
            }

            XmlNode InitSoundBanksNode = xmlDoc.CreateElement("InitSoundBanks");
            rootNode.AppendChild(InitSoundBanksNode);

            for (int i = 0; i < InitSoundBanksCount; i++)
            {
                uint ShortID = SoundInfoReader.ReadUInt32();

                XmlNode InitBankNode = xmlDoc.CreateElement("InitBank");
                InitSoundBanksNode.AppendChild(InitBankNode);

                XmlAttribute InitBankNodeAttributeShortID = xmlDoc.CreateAttribute("ShortID");
                InitBankNodeAttributeShortID.Value = ShortID.ToString();
                InitBankNode.Attributes.Append(InitBankNodeAttributeShortID);
            }

            XmlNode SoundBanksNode = xmlDoc.CreateElement("SoundBanks");
            rootNode.AppendChild(SoundBanksNode);

            for (int i = 0; i < SoundBanksCount; i++)
            {
                uint ShortID = SoundInfoReader.ReadUInt32();
                uint Unknown = SoundInfoReader.ReadUInt32();
                ulong bnkFileNameHash = SoundInfoReader.ReadUInt64();

                XmlNode BankNode = xmlDoc.CreateElement("Bank");
                SoundBanksNode.AppendChild(BankNode);

                XmlAttribute BankNodeAttributeShortID = xmlDoc.CreateAttribute("ShortID");
                BankNodeAttributeShortID.Value = ShortID.ToString();
                BankNode.Attributes.Append(BankNodeAttributeShortID);

                XmlAttribute BankNodeAttributeUnknown = xmlDoc.CreateAttribute("Unknown");
                BankNodeAttributeUnknown.Value = Unknown.ToString();
                BankNode.Attributes.Append(BankNodeAttributeUnknown);

                XmlAttribute BankNodeAttributebnkFileName = xmlDoc.CreateAttribute("bnkFileName");
                BankNodeAttributebnkFileName.Value = "soundbinary\\" + ShortID.ToString() + ".bnk";
                BankNode.Attributes.Append(BankNodeAttributebnkFileName);
            }

            SoundInfoReader.Dispose();
            SoundInfoStream.Dispose();

            xmlDoc.Save(file + ".converted.xml");
        }

        static void MarkupConvertBin(string file)
        {
            string onlyDir = Path.GetDirectoryName(file);

            XmlDocument xmlDoc = new XmlDocument();

            XmlDeclaration xmldecl;
            xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");

            XmlNode rootNode = xmlDoc.CreateElement("CMarkupResource");
            xmlDoc.AppendChild(rootNode);

            xmlDoc.InsertBefore(xmldecl, rootNode);

            XmlComment comment1 = xmlDoc.CreateComment(xmlheader);
            XmlComment comment2 = xmlDoc.CreateComment(xmlheadermarkup);

            xmlDoc.InsertBefore(comment1, rootNode);
            xmlDoc.InsertBefore(comment2, rootNode);


            FileStream MarkupStream = new FileStream(file, FileMode.Open);
            BinaryReader MarkupReader = new BinaryReader(MarkupStream);

            int ver = MarkupReader.ReadInt16();
            ushort groupCount0 = MarkupReader.ReadUInt16();
            ushort groupCount1 = MarkupReader.ReadUInt16();
            ushort groupCount2 = MarkupReader.ReadUInt16();
            ushort groupCount3 = MarkupReader.ReadUInt16();
            ushort groupCount4 = MarkupReader.ReadUInt16();

            XmlAttribute rootNodeAttributeVersion = xmlDoc.CreateAttribute("Version");
            rootNodeAttributeVersion.Value = ver.ToString();
            rootNode.Attributes.Append(rootNodeAttributeVersion);

            MarkupWriteGroup(MarkupReader, onlyDir, xmlDoc, rootNode, groupCount0, 0);
            MarkupWriteGroup(MarkupReader, onlyDir, xmlDoc, rootNode, groupCount1, 1);
            MarkupWriteGroup(MarkupReader, onlyDir, xmlDoc, rootNode, groupCount2, 2);
            MarkupWriteGroup(MarkupReader, onlyDir, xmlDoc, rootNode, groupCount3, 3);
            MarkupWriteGroup(MarkupReader, onlyDir, xmlDoc, rootNode, groupCount4, 4);

            MarkupReader.Dispose();
            MarkupStream.Dispose();

            xmlDoc.Save(file + ".converted.xml");
        }

        static void MarkupWriteGroup(BinaryReader MarkupReader, string onlyDir, XmlDocument xmlDoc, XmlNode rootNode, int count, int group)
        {
            XmlNode groupNode = xmlDoc.CreateElement("FrameGroup" + group.ToString());
            rootNode.AppendChild(groupNode);

            for (int i = 0; i < count; i++)
            {
                float unknown = MarkupReader.ReadSingle();
                uint fcbByteLength = MarkupReader.ReadUInt32();
                ulong probablyFileNameHash = MarkupReader.ReadUInt64();
                byte[] fcbData = MarkupReader.ReadBytes((int)fcbByteLength);

                string tmp = onlyDir + "\\" + probablyFileNameHash.ToString();
                File.WriteAllBytes(tmp, fcbData);
                ConvertFCB(tmp, tmp + "c");
                XmlDocument doc = new XmlDocument();
                doc.Load(tmp + "c");

                XmlNode FrameNode = xmlDoc.CreateElement("Frame");
                FrameNode.AppendChild(xmlDoc.ImportNode(doc.SelectSingleNode("object"), true));
                groupNode.AppendChild(FrameNode);

                XmlAttribute FrameNodeAttributeUnknown = xmlDoc.CreateAttribute("length");
                FrameNodeAttributeUnknown.Value = unknown.ToString(CultureInfo.InvariantCulture);
                FrameNode.Attributes.Append(FrameNodeAttributeUnknown);

                XmlAttribute FrameNodeAttributeFileNameHash = xmlDoc.CreateAttribute("AnimID");
                FrameNodeAttributeFileNameHash.Value = probablyFileNameHash.ToString();
                FrameNode.Attributes.Append(FrameNodeAttributeFileNameHash);

                File.Delete(tmp);
                File.Delete(tmp + "c");
            }
        }

        static void MarkupConvertXml(string file)
        {
            string onlyDir = Path.GetDirectoryName(file);

            string newName = file.Replace(".markup.bin.converted.xml", "_new.markup.bin");

            var output = File.Create(newName);

            XDocument doc = XDocument.Load(file);
            XElement root = doc.Element("CMarkupResource");

            output.WriteValueU16(ushort.Parse(root.Attribute("Version").Value));
            output.WriteValueU16((ushort)root.Element("FrameGroup0").Elements().Count());
            output.WriteValueU16((ushort)root.Element("FrameGroup1").Elements().Count());
            output.WriteValueU16((ushort)root.Element("FrameGroup2").Elements().Count());
            output.WriteValueU16((ushort)root.Element("FrameGroup3").Elements().Count());
            output.WriteValueU16((ushort)root.Element("FrameGroup4").Elements().Count());

            IEnumerable<XElement> allFrames = root.Descendants("Frame");
            foreach (XElement allFrame in allFrames)
            {
                float unknown = float.Parse(allFrame.Attribute("length").Value, CultureInfo.InvariantCulture);
                ulong FileNameHash = ulong.Parse(allFrame.Attribute("AnimID").Value);

                string tmp = onlyDir + "\\" + FileNameHash.ToString();
                XElement fcb = allFrame.Element("object");
                fcb.Save(tmp);

                ConvertXML(tmp, tmp + "c");

                byte[] fcbByte = File.ReadAllBytes(tmp + "c");

                output.WriteValueF32(unknown, 0);
                output.WriteValueU32((uint)fcbByte.Length);
                output.WriteValueU64(FileNameHash);
                output.WriteBytes(fcbByte);

                File.Delete(tmp);
                File.Delete(tmp + "c");
            }

            output.Close();
        }

        static void MoveConvertBin(string file)
        {
            string onlyDir = Path.GetDirectoryName(file);

            XmlDocument xmlDoc = new XmlDocument();

            XmlDeclaration xmldecl;
            xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");

            XmlNode rootNode = xmlDoc.CreateElement("CMoveResource");
            xmlDoc.AppendChild(rootNode);

            xmlDoc.InsertBefore(xmldecl, rootNode);

            XmlComment comment1 = xmlDoc.CreateComment(xmlheader);
            XmlComment comment2 = xmlDoc.CreateComment(xmlheadermove);

            xmlDoc.InsertBefore(comment1, rootNode);
            xmlDoc.InsertBefore(comment2, rootNode);


            FileStream MoveStream = new FileStream(file, FileMode.Open);
            BinaryReader MoveReader = new BinaryReader(MoveStream);

            ushort ver = MoveReader.ReadUInt16();
            ushort unk = MoveReader.ReadUInt16();

            isNewDawn = ver == 65;

            XmlAttribute rootNodeAttributeVersion = xmlDoc.CreateAttribute("Version");
            rootNodeAttributeVersion.Value = ver.ToString();
            rootNode.Attributes.Append(rootNodeAttributeVersion);

            XmlAttribute rootNodeAttributeUnknown = xmlDoc.CreateAttribute("Unknown");
            rootNodeAttributeUnknown.Value = unk.ToString();
            rootNode.Attributes.Append(rootNodeAttributeUnknown);


            byte[] fcbData = MoveReader.ReadBytes(100000000);

            string tmp = onlyDir + "\\tmp";
            File.WriteAllBytes(tmp, fcbData);
            ConvertFCB(tmp, tmp + "c");
            XmlDocument doc = new XmlDocument();
            doc.Load(tmp + "c");

            rootNode.AppendChild(xmlDoc.ImportNode(doc.SelectSingleNode("object"), true));

            File.Delete(tmp);
            File.Delete(tmp + "c");

            MoveReader.Dispose();
            MoveStream.Dispose();

            xmlDoc.Save(file + ".converted.xml");
        }

        static void MoveConvertXml(string file)
        {
            string onlyDir = Path.GetDirectoryName(file);

            string newName = file.Replace(".move.bin.converted.xml", "_new.move.bin");

            var output = File.Create(newName);

            XDocument doc = XDocument.Load(file);
            XElement root = doc.Element("CMoveResource");

            ushort ver = ushort.Parse(root.Attribute("Version").Value);
            isNewDawn = ver == 65;

            output.WriteValueU16(ver);
            output.WriteValueU16(ushort.Parse(root.Attribute("Unknown").Value));


            string tmp = onlyDir + "\\tmp";
            XElement fcb = root.Element("object");
            fcb.Save(tmp);

            ConvertXML(tmp, tmp + "c");

            byte[] fcbByte = File.ReadAllBytes(tmp + "c");

            output.WriteBytes(fcbByte);

            File.Delete(tmp);
            File.Delete(tmp + "c");

            output.Close();
        }

        static void CombinedMoveFileConvertBin(string file)
        {
            string onlyDir = Path.GetDirectoryName(file);
            
            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                CheckCharacters = false,
                OmitXmlDeclaration = false
            };
            var writer = XmlWriter.Create(file + ".converted.xml", settings);

            writer.WriteStartDocument();
            writer.WriteComment(xmlheader);
            writer.WriteComment(xmlheadercombined1);
            writer.WriteStartElement("CombinedMoveFile");


            FileStream CombinedMoveFileStream = new FileStream(file, FileMode.Open);
            BinaryReader CombinedMoveFileReader = new BinaryReader(CombinedMoveFileStream);

            uint moveCount = CombinedMoveFileReader.ReadUInt32();
            uint moveDataSize = CombinedMoveFileReader.ReadUInt32();
            uint fcbDataSize = CombinedMoveFileReader.ReadUInt32();

            if (moveCount != 64 && moveCount != 65)
            {
                Console.WriteLine("Unsupported version of CombinedMoveFile.bin!");
                return;
            }

            bool isNewDawn = moveCount == 65;

            writer.WriteAttributeString("Version", moveCount.ToString());

            byte[] moveData = CombinedMoveFileReader.ReadBytes((int)moveDataSize);
            byte[] fcbData = CombinedMoveFileReader.ReadBytes((int)fcbDataSize);

            //****

            string tmp = onlyDir + "\\tmp";
            File.WriteAllBytes(tmp, fcbData);
            ConvertFCB(tmp, tmp + "c");

            //****

            writer.WriteStartElement("PerMoveResourceInfos");

            uint currentOffset = 0;
            Stream moveDataStream = new MemoryStream(moveData);
            for (int i = 0; i < CombinedMoveFile.PerMoveResourceInfo.perMoveResourceInfos.Count(); i++)
            {
                long currentPos = moveDataStream.Position;
                byte[] resourcePathId = moveDataStream.ReadBytes(sizeof(ulong));
                moveDataStream.Seek(currentPos, SeekOrigin.Begin);

                var pmri = CombinedMoveFile.PerMoveResourceInfo.perMoveResourceInfos.Where(e => e.resourcePathId == BitConverter.ToUInt64(resourcePathId, 0)).SingleOrDefault();
                uint chunkLen = pmri.size;

                byte[] chunk = moveDataStream.ReadBytes((int)chunkLen);

                var moveBinDataChunk = new CombinedMoveFile.MoveBinDataChunk(currentOffset, true, isNewDawn, false);
                moveBinDataChunk.Deserialize(writer, chunk, pmri.rootNodeId);
                //writer.Flush();

                currentOffset += chunkLen;
            }
            /*
            Dictionary<uint, ulong> a = new Dictionary<uint, ulong>();
            foreach (KeyValuePair<uint, ulong> aa in OffsetsHashesArray.offsetsHashesDict)
                if (!OffsetsHashesArray.offsetsHashesDict2.ContainsKey(aa.Key))
                    a.Add(aa.Key, aa.Value);*/
                    
            writer.WriteEndElement();

            //****

            var doc = new XPathDocument(tmp + "c");
            var nav = doc.CreateNavigator();
            var root = nav.SelectSingleNode("/object");

            writer.WriteStartElement("FCBData");
            root.WriteSubtree(writer);
            writer.WriteEndElement();

            //****

            File.Delete(tmp);
            File.Delete(tmp + "c");

            CombinedMoveFileReader.Dispose();
            CombinedMoveFileStream.Dispose();

            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Flush();
            writer.Close();
        }

        static void CombinedMoveFileConvertXml(string file)
        {
            string onlyDir = Path.GetDirectoryName(file);

            string newName = file.Replace("combinedmovefile.bin.converted.xml", "combinedmovefile_new.bin");

            var output = File.Create(newName);

            var doc = new XPathDocument(file);
            var nav = doc.CreateNavigator();

            var root = nav.SelectSingleNode("/CombinedMoveFile");

            var CMove_BlendRoot_DTRoot = nav.Select("/CombinedMoveFile/PerMoveResourceInfos/CMove_BlendRoot_DTRoot");

            uint ver = uint.Parse(root.GetAttribute("Version", ""));

            List<byte[]> perMoveResourceInfos = new List<byte[]>();
            uint currentOffset = 0;
            while (CMove_BlendRoot_DTRoot.MoveNext() == true)
            {
                var moveBinDataChunk = new CombinedMoveFile.MoveBinDataChunk(currentOffset, true, ver == 65, false);
                byte[] chunk = moveBinDataChunk.Serialize(CMove_BlendRoot_DTRoot.Current);

                perMoveResourceInfos.Add(chunk);

                currentOffset += (uint)chunk.Length;
            }

            byte[] perMoveResourceInfosByte = perMoveResourceInfos.SelectMany(byteArr => byteArr).ToArray();

            string tmp = onlyDir + "\\tmp";
            var fcb = nav.SelectSingleNode("CombinedMoveFile/FCBData/object");
            XmlWriter writer = XmlWriter.Create(tmp);
            fcb.WriteSubtree(writer);
            writer.Close();

            ConvertXML(tmp, tmp + "c");

            byte[] fcbByte = File.ReadAllBytes(tmp + "c");

            output.WriteValueU32(ver);
            output.WriteValueU32((uint)perMoveResourceInfosByte.Length);
            output.WriteValueU32((uint)fcbByte.Length);
            output.WriteBytes(perMoveResourceInfosByte);
            output.WriteBytes(fcbByte);

            File.Delete(tmp);
            File.Delete(tmp + "c");

            output.Close();
        }

        static void TerrainNodeBdl(string file)
        {
            FileStream TerrainNodeStream = new FileStream(file, FileMode.Open);
            BinaryReader TerrainNodeReader = new BinaryReader(TerrainNodeStream);

            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                CheckCharacters = false,
                OmitXmlDeclaration = false
            };
            var writer = XmlWriter.Create(file + ".converted.xml", settings);

            writer.WriteStartDocument();
            writer.WriteComment(xmlheader);
            writer.WriteStartElement("TerrainNodeBundle");

            uint version = TerrainNodeReader.ReadUInt32();
            uint filesCount = TerrainNodeReader.ReadUInt32();

            writer.WriteAttributeString("Version", version.ToString());

            for (int i = 0; i < filesCount; i++)
            {
                ulong pathHash = TerrainNodeReader.ReadUInt64();
                uint pathLen = TerrainNodeReader.ReadUInt32();
                uint typeHash = TerrainNodeReader.ReadUInt32();
                uint typeLen = TerrainNodeReader.ReadUInt32();
                short unk1 = TerrainNodeReader.ReadInt16();
                short unk2 = TerrainNodeReader.ReadInt16();
                uint dataLen = TerrainNodeReader.ReadUInt32();

                int unk3 = TerrainNodeReader.ReadInt32();
                uint subFilesCount = TerrainNodeReader.ReadUInt32();
                uint subFilesLen = TerrainNodeReader.ReadUInt32();

                string path = new string(TerrainNodeReader.ReadChars((int)pathLen));
                string type = new string(TerrainNodeReader.ReadChars((int)typeLen));

                path = path.Remove(path.Length - 1);
                type = type.Remove(type.Length - 1);

                writer.WriteStartElement("File");
                //writer.WriteAttributeString("PathHash", m_HashList.ContainsKey(pathHash) ? m_HashList[pathHash] : "__Unknown\\" + pathHash.ToString("X16"));
                //writer.WriteAttributeString("TypeHash", strings[typeHash]);
                writer.WriteAttributeString("Unknown1", unk1.ToString());
                writer.WriteAttributeString("Unknown2", unk2.ToString());
                writer.WriteAttributeString("Unknown3", unk3.ToString());
                writer.WriteAttributeString("Path", path);
                writer.WriteAttributeString("Type", type);

                for (int j = 0; j < subFilesCount; j++)
                {
                    ulong subPathHash = TerrainNodeReader.ReadUInt64();
                    uint subPathLen = TerrainNodeReader.ReadUInt32();
                    uint subTypeHash = TerrainNodeReader.ReadUInt32();
                    uint subTypeLen = TerrainNodeReader.ReadUInt32();

                    int subUnk = TerrainNodeReader.ReadInt32();

                    string subPath = new string(TerrainNodeReader.ReadChars((int)subPathLen));
                    string subType = new string(TerrainNodeReader.ReadChars((int)subTypeLen));

                    subPath = subPath.Remove(subPath.Length - 1);
                    subType = subType.Remove(subType.Length - 1);

                    writer.WriteStartElement("SubFile");
                    writer.WriteAttributeString("Unknown", subUnk.ToString());
                    writer.WriteAttributeString("Path", subPath);
                    writer.WriteAttributeString("Type", subType);
                    writer.WriteEndElement();
                }

                byte[] data = TerrainNodeReader.ReadBytes((int)dataLen);

                // write to file
                string workingDir = Directory.GetParent(file).FullName;
                string folderName = Directory.GetParent(path).Name;
                string fileName = Path.GetFileName(path);

                Directory.CreateDirectory(workingDir + "\\" + folderName);
                File.WriteAllBytes(workingDir + "\\" + folderName + "\\" + fileName, data);

                writer.WriteEndElement();
            }

            TerrainNodeReader.Dispose();
            TerrainNodeStream.Dispose();

            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Flush();
            writer.Close();
        }

        static void TerrainNodeXml(string file)
        {
            string newName = file.Replace(".terrainnode.bdl.converted.xml", "_new.terrainnode.bdl");

            var output = File.Create(newName);

            var doc = new XPathDocument(file);
            var nav = doc.CreateNavigator();

            var root = nav.SelectSingleNode("/TerrainNodeBundle");
            XPathNodeIterator files = root.SelectChildren("File", "");

            uint version = uint.Parse(root.GetAttribute("Version", ""));
            uint filesCount = (uint)files.Count;

            output.WriteValueU32(version);
            output.WriteValueU32(filesCount);

            while (files.MoveNext() == true)
            {
                XPathNavigator fileXml = files.Current;

                XPathNodeIterator subFiles = fileXml.SelectChildren("SubFile", "");
                byte[] subFilesBytes = new byte[] { };

                if (subFiles.Count > 0)
                {
                    MemoryStream ms = new MemoryStream();

                    while (subFiles.MoveNext() == true)
                    {
                        XPathNavigator subFileXml = subFiles.Current;

                        string subPath = subFileXml.GetAttribute("Path", "");
                        string subType = subFileXml.GetAttribute("Type", "");
                        string subPathNull = subPath + char.MinValue;
                        string subTypeNull = subType + char.MinValue;

                        ulong subPathHash = Gibbed.Dunia2.FileFormats.CRC64.Hash(subPath);
                        uint subPathLen = (uint)subPathNull.Length;
                        uint subTypeHash = Gibbed.Dunia2.FileFormats.CRC32.Hash(subType);
                        uint subTypeLen = (uint)subTypeNull.Length;

                        int subUnk = int.Parse(subFileXml.GetAttribute("Unknown", ""));

                        ms.WriteValueU64(subPathHash);
                        ms.WriteValueU32(subPathLen);
                        ms.WriteValueU32(subTypeHash);
                        ms.WriteValueU32(subTypeLen);

                        ms.WriteValueS32(subUnk);

                        ms.WriteString(subPathNull, Encoding.Default);
                        ms.WriteString(subTypeNull, Encoding.Default);
                    }

                    subFilesBytes = ms.ToArray();
                }

                string path = fileXml.GetAttribute("Path", "");
                string type = fileXml.GetAttribute("Type", "");
                string pathNull = path + char.MinValue;
                string typeNull = type + char.MinValue;

                string workingDir = Directory.GetParent(file).FullName;
                string folderName = Directory.GetParent(path).Name;
                string fileName = Path.GetFileName(path);

                byte[] fileBytes = File.ReadAllBytes(workingDir + "\\" + folderName + "\\" + fileName);

                ulong pathHash = Gibbed.Dunia2.FileFormats.CRC64.Hash(path);
                uint pathLen = (uint)pathNull.Length;
                uint typeHash = Gibbed.Dunia2.FileFormats.CRC32.Hash(type);
                uint typeLen = (uint)typeNull.Length;
                short unk1 = short.Parse(fileXml.GetAttribute("Unknown1", ""));
                short unk2 = short.Parse(fileXml.GetAttribute("Unknown2", ""));
                uint dataLen = (uint)fileBytes.Length;

                int unk3 = int.Parse(fileXml.GetAttribute("Unknown3", ""));
                uint subFilesCount = (uint)subFiles.Count;
                uint subFilesLen = (uint)subFilesBytes.Length;

                output.WriteValueU64(pathHash);
                output.WriteValueU32(pathLen);
                output.WriteValueU32(typeHash);
                output.WriteValueU32(typeLen);
                output.WriteValueS16(unk1);
                output.WriteValueS16(unk2);
                output.WriteValueU32(dataLen);

                output.WriteValueS32(unk3);
                output.WriteValueU32(subFilesCount);
                output.WriteValueU32(subFilesLen);

                output.WriteString(pathNull, Encoding.Default);
                output.WriteString(typeNull, Encoding.Default);

                output.WriteBytes(subFilesBytes);

                output.WriteBytes(fileBytes);
            }

            output.Close();
        }

        static void UnpackBigFile(string m_FatFile, string m_DstFolder, string oneFile = "")
        {
            if (!File.Exists(m_FatFile))
            {
                Console.WriteLine("[ERROR]: Input file does not exist!");
                return;
            }

            if (m_DstFolder == "")
            {
                m_DstFolder = Path.GetDirectoryName(m_FatFile) + @"\" + Path.GetFileNameWithoutExtension(m_FatFile) + "_unpacked";
            }

            if (!Directory.Exists(m_DstFolder))
            {
                Directory.CreateDirectory(m_DstFolder);
            }

            string m_DatName = Path.GetDirectoryName(m_FatFile) + @"\" + Path.GetFileNameWithoutExtension(m_FatFile) + ".dat";

            SortedDictionary<ulong, FatEntry> Entries = GetFatEntries(m_FatFile, out int dwVersion);

            LoadFile(dwVersion);

            if (Entries == null)
            {
                Console.WriteLine("No files in the FAT were found!");
                return;
            }

            FileStream TDATStream = new FileStream(m_DatName, FileMode.Open);
            BinaryReader TDATReader = new BinaryReader(TDATStream);

            bool oneFileFound = false;
            ulong oneFileHash = GetFileHash(oneFile);

            foreach (KeyValuePair<ulong, FatEntry> pair in Entries)
            {
                FatEntry fatEntry = pair.Value;

                if (oneFile != "")
                {
                    if (fatEntry.NameHash != oneFileHash)
                        continue;

                    oneFileFound = true;
                }

                string m_Hash = fatEntry.NameHash.ToString(dwVersion >= 9 ? "X16" : "X8");
                string m_FileName;
                if (m_HashList.ContainsKey(fatEntry.NameHash))
                {
                    m_HashList.TryGetValue(fatEntry.NameHash, out m_FileName);
                }
                else
                {
                    m_FileName = @"__Unknown\" + m_Hash;
                }

                if (oneFileFound)
                {
                    m_FileName = Path.GetFileName(m_FileName);
                }

                string m_FullPath = m_DstFolder + @"\" + m_FileName;

                Console.WriteLine("[Unpacking]: {0}", m_FileName);

                byte[] pDstBuffer = new byte[] { };

                if (fatEntry.CompressionScheme == CompressionScheme.None)
                {
                    TDATStream.Seek(fatEntry.Offset, SeekOrigin.Begin);

                    if (dwVersion == 10)
                    {
                        pDstBuffer = new byte[fatEntry.UncompressedSize];
                        TDATStream.Read(pDstBuffer, 0, (int)fatEntry.UncompressedSize);
                    }
                    if (dwVersion <= 9) // because in FAT ver 9 and below there is this weird thing
                    {
                        pDstBuffer = new byte[fatEntry.CompressedSize];
                        TDATStream.Read(pDstBuffer, 0, (int)fatEntry.CompressedSize);
                    }
                }
                else if (fatEntry.CompressionScheme == CompressionScheme.LZO1x)
                {
                    TDATStream.Seek(fatEntry.Offset, SeekOrigin.Begin);

                    byte[] pSrcBuffer = new byte[fatEntry.CompressedSize];
                    pDstBuffer = new byte[fatEntry.UncompressedSize];

                    TDATStream.Read(pSrcBuffer, 0, (int)fatEntry.CompressedSize);

                    int actualUncompressedLength = (int)fatEntry.UncompressedSize;

                    var result = Gibbed.Dunia2.FileFormats.LZO.Decompress(pSrcBuffer,
                                                0,
                                                pSrcBuffer.Length,
                                                pDstBuffer,
                                                0,
                                                ref actualUncompressedLength);

                    if (result != Gibbed.Dunia2.FileFormats.LZO.ErrorCode.Success)
                    {
                        throw new FormatException(string.Format("LZO decompression failure ({0})", result));
                    }

                    if (actualUncompressedLength != fatEntry.UncompressedSize)
                    {
                        throw new FormatException("LZO decompression failure (uncompressed size mismatch)");
                    }
                }
                else if (fatEntry.CompressionScheme == CompressionScheme.LZ4)
                {
                    TDATStream.Seek(fatEntry.Offset, SeekOrigin.Begin);

                    byte[] pSrcBuffer = new byte[fatEntry.CompressedSize];
                    pDstBuffer = new byte[fatEntry.UncompressedSize];

                    TDATStream.Read(pSrcBuffer, 0, (int)fatEntry.CompressedSize);

                    LZ4Decompressor64 TLZ4Decompressor64 = new LZ4Decompressor64();
                    TLZ4Decompressor64.Decompress(pSrcBuffer, pDstBuffer);
                }
                else
                {
                    //https://www.youtube.com/watch?v=AXzEcwYs8Eo
                    throw new Exception("WHAT THE FUCK???");
                }

                if (m_FullPath.Contains(@"__Unknown"))
                {
                    uint dwID = 0;

                    if (pDstBuffer.Length > 4)
                        dwID = BitConverter.ToUInt32(pDstBuffer, 0);

                    m_FullPath = UnpackBigFileFileType(m_FullPath, dwID);
                }
                else
                {
                    if (!Directory.Exists(Path.GetDirectoryName(m_FullPath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(m_FullPath));
                    }
                }

                FileStream TSaveStream = new FileStream(m_FullPath, FileMode.Create);
                TSaveStream.Write(pDstBuffer, 0, pDstBuffer.Length);
                TSaveStream.Close();
            }

            if (oneFile != "" && !oneFileFound)
            {
                Console.WriteLine("File " + oneFile + " was not found in " + m_FatFile);
            }

            TDATReader.Dispose();
            TDATStream.Dispose();
        }

        static string UnpackBigFileFileType(string m_UnknownFileName, uint dwID)
        {
            string m_Directory = Path.GetDirectoryName(m_UnknownFileName);
            string m_FileName = Path.GetFileName(m_UnknownFileName);

            if (dwID == 0x004D4154) //TAM
            {
                m_UnknownFileName = m_Directory + @"\MAT\" + m_FileName + ".material.bin";
            }
            else
            if (dwID == 0x474E5089) //PNG
            {
                m_UnknownFileName = m_Directory + @"\PNG\" + m_FileName + ".png";
            }
            else
            if (dwID == 0x42444947) //GIDB
            {
                m_UnknownFileName = m_Directory + @"\GIDB\" + m_FileName + ".bin";
            }
            else
            if (dwID == 0x4D4F4D41) //MOMA
            {
                m_UnknownFileName = m_Directory + @"\ANIM\" + m_FileName + ".bin";
            }
            else
            if (dwID == 0x4D760040) //MOVE
            {
                m_UnknownFileName = m_Directory + @"\MOVE\" + m_FileName + ".move.bin";
            }
            else
            if (dwID == 0x00534B4C) //SKL
            {
                m_UnknownFileName = m_Directory + @"\SKEL\" + m_FileName + ".skeleton";
            }
            else
            if (dwID == 0x01194170 || dwID == 0x00194170) //pA
            {
                m_UnknownFileName = m_Directory + @"\DPAX\" + m_FileName + ".dpax";
            }
            else
            if (dwID == 0x44484B42) //BKHD
            {
                m_UnknownFileName = m_Directory + @"\BNK\" + m_FileName + ".bnk";
            }
            else
            if (dwID == 0x8464555) //UEF
            {
                m_UnknownFileName = m_Directory + @"\FEU\" + m_FileName + ".feu";
            }
            else
            if (dwID == 0x46464952) //RIFF
            {
                m_UnknownFileName = m_Directory + @"\WEM\" + m_FileName + ".wem";
            }
            else
            if (dwID == 0x4D455348) //HSEM
            {
                m_UnknownFileName = m_Directory + @"\XBG\" + m_FileName + ".xbg";
            }
            else
            if (dwID == 0x00584254) //XBT
            {
                m_UnknownFileName = m_Directory + @"\XBT\" + m_FileName + ".xbt";
            }
            else
            if (dwID == 0x4643626E || dwID == 0x00000004 || dwID == 0x00000023) //nbCF
            {
                m_UnknownFileName = m_Directory + @"\FCB\" + m_FileName + ".fcb";
            }
            else
            if (dwID == 0x78647064) //dpdx
            {
                m_UnknownFileName = m_Directory + @"\DPDX\" + m_FileName + ".dpdx";
            }
            else
            if (dwID == 0x4341554C) //LUAC
            {
                m_UnknownFileName = m_Directory + @"\LUA\" + m_FileName + ".lua";
            }
            else
            if (dwID == 0x5161754C) //LuaQ
            {
                m_UnknownFileName = m_Directory + @"\LUA\" + m_FileName + ".lua";
            }
            else
            if (dwID == 0x3CBFBBEF || dwID == 0x6D783F3C || dwID == 0x003CFEFF || dwID == 0x6172673C) //XML, //<graphics
            {
                m_UnknownFileName = m_Directory + @"\XML\" + m_FileName + ".xml";
            }
            else
            if (dwID == 0x6E69423C) //<binary
            {
                m_UnknownFileName = m_Directory + @"\BINXML\" + m_FileName + ".xml";
            }
            else
            if (dwID == 0x54425043) //CPBT
            {
                m_UnknownFileName = m_Directory + @"\CPBT\" + m_FileName + ".cpubt";
            }
            else
            if (dwID == 0xE9001052) //SDAT
            {
                m_UnknownFileName = m_Directory + @"\SDAT\" + m_FileName + ".sdat";
            }
            else
            if (dwID == 0x000000B0 || dwID == 0x000000B6) //MAB
            {
                m_UnknownFileName = m_Directory + @"\MAB\" + m_FileName + ".mab";
            }
            else
            if (dwID == 0x01) //WSECBDL
            {
                m_UnknownFileName = m_Directory + @"\WSECBDL\" + m_FileName + ".wsecbdl";
            }
            else
            if (dwID == 0x694B4942 || dwID == 0x6732424B) //BIKi //KB2g
            {
                m_UnknownFileName = m_Directory + @"\BIK\" + m_FileName + ".bik";
            }
            else
            if (dwID == 0x00000032 || dwID == 0x00000036) //hkx
            {
                m_UnknownFileName = m_Directory + @"\HKX\" + m_FileName + ".hkx";
            }

            if (!Directory.Exists(Path.GetDirectoryName(m_UnknownFileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(m_UnknownFileName));
            }

            return m_UnknownFileName;
        }

        static void PackBigFile(string sourceDir, string outputFile, int dwVersion = 10)
        {
            if (sourceDir.EndsWith("\\"))
            {
                Console.WriteLine("Bad source dir name!");
                Environment.Exit(0);
            }
            if (!outputFile.EndsWith(".fat"))
            {
                Console.WriteLine("Output filename is wrong!");
                Environment.Exit(0);
            }

            if (dwVersion < 10)
            {
                isCompressEnabled = false;
                Console.WriteLine("Compression is not available for older FATs.");
            }

            List<string> notCompress = new List<string>();

            string m_Path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (File.Exists(m_Path + excludeFile))
            {
                notCompress.AddRange(File.ReadAllLines(m_Path + excludeFile));
                notCompress.RemoveAt(0);
            }

            if (excludeFromCompress != "")
            {
                string[] exts = excludeFromCompress.Split(',');
                notCompress.AddRange(exts);
            }

            notCompress = notCompress.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            if (isCompressEnabled)
                Console.WriteLine("Excluded extensions from compressing: " + String.Join(", ", notCompress.ToArray()));

            string fatFile = outputFile;
            string datFile = fatFile.Replace(".fat", ".dat");

            string[] allFiles = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);

            SortedDictionary<ulong, FatEntry> Entries = new SortedDictionary<ulong, FatEntry>();

            var outputDat = File.Open(datFile, FileMode.OpenOrCreate);
            outputDat.SetLength(0);

            foreach (string file in allFiles)
            {
                string fatFileName = file.Replace(sourceDir + "\\", "");
                string extension = Path.GetExtension(fatFileName);

                byte[] bytes = File.ReadAllBytes(file);

                FatEntry entry = new FatEntry();

                byte[] outputBytes;

                if (isCompressEnabled && !notCompress.Contains(extension))
                {
                    outputBytes = new LZ4Sharp.LZ4Compressor64().Compress(bytes);

                    entry.CompressionScheme = CompressionScheme.LZ4;
                    entry.UncompressedSize = (uint)bytes.Length;
                }
                else
                {
                    outputBytes = bytes;

                    entry.CompressionScheme = CompressionScheme.None;

                    if (dwVersion == 10)
                        entry.UncompressedSize = (uint)bytes.Length;
                    else if (dwVersion <= 9)
                        entry.UncompressedSize = 0;
                }

                entry.NameHash = GetFileHash(fatFileName, dwVersion);
                entry.CompressedSize = (uint)outputBytes.Length;
                entry.Offset = outputDat.Position;
                Entries[entry.NameHash] = entry;

                outputDat.Write(outputBytes, 0, outputBytes.Length);
                outputDat.Seek(outputDat.Position.Align(16), SeekOrigin.Begin);

                Console.WriteLine("[Packing]: " + fatFileName);
            }

            outputDat.Flush();
            outputDat.Close();

            var output = File.Create(fatFile);
            output.WriteValueU32(0x46415432, 0);
            output.WriteValueS32(dwVersion, 0);

            output.WriteByte(1);
            output.WriteByte(0);
            //output.WriteByte(3);
            output.WriteValueU16(0);

            output.WriteValueS32(0, 0); // sub FATs are hard to edit, so they aren't supported by packing process
            output.WriteValueS32(0, 0);
            output.WriteValueS32(Entries.Count, 0);

            foreach (ulong entryE in Entries.Keys)
            {
                var fatEntry = Entries[entryE];

                if (dwVersion == 10)
                {
                    uint value = (uint)((ulong)((long)fatEntry.NameHash & -4294967296L) >> 32);
                    uint value2 = (uint)(fatEntry.NameHash & uint.MaxValue);
                    uint num = 0u;
                    num = (uint)((int)num | ((int)(fatEntry.UncompressedSize << 2) & -4));
                    num = (uint)((int)num | (int)((long)(int)fatEntry.CompressionScheme & 3L));
                    uint value3 = (uint)((fatEntry.Offset & 0x7FFFFFFF8) >> 3);
                    uint num2 = 0u;
                    num2 = (uint)((int)num2 | (int)((fatEntry.Offset & 7) << 29));
                    num2 |= (fatEntry.CompressedSize & 0x1FFFFFFF);

                    output.WriteValueU32(value, 0);
                    output.WriteValueU32(value2, 0);
                    output.WriteValueU32(num, 0);
                    output.WriteValueU32(value3, 0);
                    output.WriteValueU32(num2, 0);
                }
                if (dwVersion == 9)
                {
                    var a = (uint)((fatEntry.NameHash & 0xFFFFFFFF00000000ul) >> 32);
                    var b = (uint)((fatEntry.NameHash & 0x00000000FFFFFFFFul) >> 0);

                    uint c = 0;
                    c |= ((fatEntry.UncompressedSize << 2) & 0xFFFFFFFCu);
                    c |= (uint)(((byte)fatEntry.CompressionScheme << 0) & 0x00000003u);

                    var d = (uint)((fatEntry.Offset & 0X00000003FFFFFFFCL) >> 2);

                    uint e = 0;
                    e |= (uint)((fatEntry.Offset & 0X0000000000000003L) << 30);
                    e |= (fatEntry.CompressedSize & 0x3FFFFFFFu) << 0;

                    output.WriteValueU32(a, 0);
                    output.WriteValueU32(b, 0);
                    output.WriteValueU32(c, 0);
                    output.WriteValueU32(d, 0);
                    output.WriteValueU32(e, 0);
                }
                if (dwVersion == 5)
                {
                    uint a = (uint)fatEntry.NameHash;
                    uint b = 0;
                    b |= ((fatEntry.UncompressedSize << 2) & 0xFFFFFFFCu);
                    b |= (uint)(((byte)fatEntry.CompressionScheme << 0) & 0x00000003u);
                    ulong c = 0;
                    c |= ((ulong)(fatEntry.Offset << 30) & 0xFFFFFFFFC0000000ul);
                    c |= (ulong)((fatEntry.CompressedSize << 0) & 0x000000003FFFFFFFul);

                    output.WriteValueU32(a, 0);
                    output.WriteValueU32(b, 0);
                    output.WriteValueU64(c, 0);
                }
            }

            output.WriteValueU32(0, 0);

            if (dwVersion >= 9)
                output.WriteValueU32(0, 0);

            output.Flush();
            output.Close();
        }

        static SortedDictionary<ulong, FatEntry> GetFatEntries(string fatFile, out int dwVersion)
        {
            SortedDictionary<ulong, FatEntry> Entries = new SortedDictionary<ulong, FatEntry>();

            FileStream TFATStream = new FileStream(fatFile, FileMode.Open);
            BinaryReader TFATReader = new BinaryReader(TFATStream);

            int dwMagic = TFATReader.ReadInt32();
            dwVersion = TFATReader.ReadInt32();
            int dwUnknown = TFATReader.ReadInt32();

            if (dwMagic != 0x46415432)
            {
                Console.WriteLine("Invalid FAT Index file!");
                TFATReader.Dispose();
                TFATStream.Dispose();
                TFATReader.Close();
                TFATStream.Close();
                return null;
            }

            // versions
            // 10 - FC5, FCND
            // 9 - FC3, FC3BD, FC4
            // 5 - FC2
            if (dwVersion != 10 && dwVersion != 9 && dwVersion != 5)
            {
                Console.WriteLine("Invalid version of FAT Index file!");
                TFATReader.Dispose();
                TFATStream.Dispose();
                TFATReader.Close();
                TFATStream.Close();
                return null;
            }

            int dwSubfatTotalEntryCount = 0;
            int dwSubfatCount = 0;

            if (dwVersion >= 9)
            {
                dwSubfatTotalEntryCount = TFATReader.ReadInt32();
                dwSubfatCount = TFATReader.ReadInt32();
            }

            int dwTotalFiles = TFATReader.ReadInt32();

            for (int i = 0; i < dwTotalFiles; i++)
            {
                FatEntry entry = GetFatEntriesDeserialize(TFATReader, dwVersion);
                Entries[entry.NameHash] = entry;
            }

            uint unknown1Count = TFATReader.ReadUInt32();
            for (uint i = 0; i < unknown1Count; i++)
            {
                throw new NotSupportedException();
                TFATReader.ReadBytes(16);
            }

            if (dwVersion >= 7)
            {
                uint unknown2Count = TFATReader.ReadUInt32();
                for (uint i = 0; i < unknown2Count; i++)
                {
                    TFATReader.ReadBytes(16);
                }
            }

            // we support sub fats, but for packing it's better and easier to remove them
            for (int i = 0; i < dwSubfatCount; i++)
            {
                uint subfatEntryCount = TFATReader.ReadUInt32();
                for (uint j = 0; j < subfatEntryCount; j++)
                {
                    FatEntry entry = GetFatEntriesDeserialize(TFATReader, dwVersion);
                    Entries[entry.NameHash] = entry;
                }
            }

            TFATReader.Dispose();
            TFATStream.Dispose();
            TFATReader.Close();
            TFATStream.Close();

            return Entries;
        }

        static FatEntry GetFatEntriesDeserialize(BinaryReader TFATReader, int dwVersion)
        {
            ulong dwHash = 0;

            if (dwVersion == 10 || dwVersion == 9)
            {
                dwHash = TFATReader.ReadUInt64();
                dwHash = (dwHash << 32) + (dwHash >> 32);
            }
            if (dwVersion == 5)
            {
                dwHash = TFATReader.ReadUInt32();
            }

            uint dwUncompressedSize = TFATReader.ReadUInt32();
            uint dwUnresolvedOffset = TFATReader.ReadUInt32();
            uint dwCompressedSize = TFATReader.ReadUInt32();

            uint dwFlag = 0;
            ulong dwOffset = 0;

            if (dwVersion == 10)
            {
                dwFlag = dwUncompressedSize & 3;
                dwOffset = dwCompressedSize >> 29 | 8ul * dwUnresolvedOffset;
                dwCompressedSize = (dwCompressedSize & 0x1FFFFFFF);
                dwUncompressedSize = (dwUncompressedSize >> 2);
            }
            if (dwVersion == 9)
            {
                dwFlag = (dwUncompressedSize & 0x00000003u) >> 0;
                dwOffset = (ulong)dwUnresolvedOffset << 2;
                dwOffset |= (dwCompressedSize & 0xC0000000u) >> 30;
                dwCompressedSize = (uint)((dwCompressedSize & 0x3FFFFFFFul) >> 0);
                dwUncompressedSize = (dwUncompressedSize & 0xFFFFFFFCu) >> 2;
            }
            if (dwVersion == 5)
            {
                dwFlag = (dwUncompressedSize & 0x00000003u) >> 0;
                dwOffset = (ulong)dwCompressedSize << 2;
                dwOffset |= (dwUnresolvedOffset & 0xC0000000u) >> 30;
                dwCompressedSize = (uint)((dwUnresolvedOffset & 0x3FFFFFFFul) >> 0);
                dwUncompressedSize = (dwUncompressedSize & 0xFFFFFFFCu) >> 2;
            }

            var entry = new FatEntry();
            entry.NameHash = dwHash;
            entry.UncompressedSize = dwUncompressedSize;
            entry.Offset = (long)dwOffset;
            entry.CompressedSize = dwCompressedSize;
            entry.CompressionScheme = (CompressionScheme)dwFlag;

            return entry;
        }
    }
}
