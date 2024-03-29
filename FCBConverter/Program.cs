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
using K4os.Compression.LZ4;
using LZ4Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using UnluacNET;

namespace FCBConverter
{
    class Program
    {
        public static string m_Path = "";

        static readonly string listFiles = @"\FCBConverterFileNames.list";
        static readonly string listFiles_5 = @"\FCBConverterFileNames_5.list";
        public static Dictionary<ulong, string> listFilesDict = new Dictionary<ulong, string>();

        static readonly string listStrings = @"\FCBConverterStrings.list";
        public static Dictionary<uint, string> listStringsDict = new Dictionary<uint, string>();

        static readonly string settingsFile = @"\FCBConverterSettings.xml";
        static readonly string defsFile = @"\FCBConverterDefinitions.xml";

        static DefinitionsLoader definitionLoader;

        public static bool isCompressEnabled = true;
        public static bool isCombinedMoveFile = false;
        public static bool isNewDawn = false;
        public static bool isEntLibNamesStores = false;
        public static bool isFC2 = false;
        public static string excludeFilesFromCompress = "";
        public static string excludeFilesFromPack = "";

        public static string version = "20230711-1830";

        public static string matWarn = " - DO NOT DELETE THIS! DO NOT CHANGE LINE NUMBER!";
        public static string xmlheader = "Converted by FCBConverter v" + version + ", author ArmanIII.";
        public static string xmlheaderlua = "Converted using UnluacNET by Fireboyd78";
        public static string xmlheaderfcb = "Please remember that types are calculated and they may not be exactly the same as they are. Take care about this.";
        public static string xmlheaderthanks = "Based on Gibbed's Dunia Tools. Special thanks to: Fireboyd78 (FCBastard), Ekey (FC5 Unpacker), Gibbed, xBaebsae, id-daemon, Ganic, legendhavoc175, miru, eprilx";
        public static string xmlheaderbnk = $"Adding new WEM files is possible. DIDX will be calculated automatically, only required is WEMFile entry in DATA.{Environment.NewLine}Since not all binary data are converted into readable format, you can use Wwise to create your own SoundBank and then use FCBConverter to edit IDs inside the SoundBank.";

        public const string convGuessedPrefix = "value-"; //GuessedTypeAndVal-

        [DllImport("luac51", EntryPoint = "Process", CallingConvention = CallingConvention.Cdecl)]
        static extern int LuacLibProcess(string inPath, string outPath, string bytecodePath, out IntPtr error);

        [DllImport("luac51", EntryPoint = "ProcessBytes", CallingConvention = CallingConvention.Cdecl)]
        static extern int LuacLibProcessBytes(byte[] inBuffer, int inSize, out IntPtr outBuffer, out int outSize, string bytecodePath, out IntPtr error);

        [DllImport("luac51", EntryPoint = "FreeMem", CallingConvention = CallingConvention.Cdecl)]
        static extern void LuacLibFreeMem(IntPtr obj);

        static void Main(string[] args)
        {
            /*HashSet<uint> gameProjIDs = new();
            string[] ff = new string[] { "FC5.xml", "FC6.xml", "ND.xml" };
            HashSet<string> tw = new();

            foreach (var f in ff)
            {
                XDocument docDuniaSoundData = XDocument.Load(f);

                IEnumerable<XElement> Events = docDuniaSoundData.Root.Descendants("Event");
                foreach (var el in Events)
                    gameProjIDs.Add(uint.Parse(el.Attribute("ShortID").Value));

                IEnumerable<XElement> SoundBanks = docDuniaSoundData.Root.Descendants("Bank");
                foreach (var el in SoundBanks)
                    gameProjIDs.Add(uint.Parse(el.Attribute("ShortID").Value));

                IEnumerable<XElement> RTPCs = docDuniaSoundData.Root.Descendants("RTPC");
                foreach (var el in RTPCs)
                    gameProjIDs.Add(uint.Parse(el.Attribute("ShortID").Value));

                IEnumerable<XElement> Effects = docDuniaSoundData.Root.Descendants("Effect");
                foreach (var el in Effects)
                    gameProjIDs.Add(uint.Parse(el.Attribute("ShortID").Value));

                IEnumerable<XElement> AuxiliaryBuses = docDuniaSoundData.Root.Descendants("AuxiliaryBus");
                foreach (var el in AuxiliaryBuses)
                    gameProjIDs.Add(uint.Parse(el.Attribute("ShortID").Value));

                IEnumerable<XElement> Triggers = docDuniaSoundData.Root.Descendants("Trigger");
                foreach (var el in Triggers)
                    gameProjIDs.Add(uint.Parse(el.Attribute("ShortID").Value));

                IEnumerable<XElement> MemoryNodeAssociations = docDuniaSoundData.Root.Descendants("MemoryNodeAssociation");
                foreach (var el in MemoryNodeAssociations)
                    gameProjIDs.Add(uint.Parse(el.Attribute("ItemId").Value));

                IEnumerable<XElement> Streams = docDuniaSoundData.Root.Descendants("Stream");
                foreach (var el in Streams)
                    gameProjIDs.Add(uint.Parse(el.Attribute("Name").Value.Replace(".wem", "")));

                IEnumerable<XElement> StateGroup = docDuniaSoundData.Root.Descendants("StateGroup");
                foreach (var el in StateGroup)
                    gameProjIDs.Add(uint.Parse(el.Attribute("ShortID").Value));

                IEnumerable<XElement> State = docDuniaSoundData.Root.Descendants("State");
                foreach (var el in State)
                    gameProjIDs.Add(uint.Parse(el.Attribute("ShortID").Value));

                IEnumerable<XElement> SwitchGroup = docDuniaSoundData.Root.Descendants("SwitchGroup");
                foreach (var el in SwitchGroup)
                    gameProjIDs.Add(uint.Parse(el.Attribute("ShortID").Value));

                IEnumerable<XElement> Switch = docDuniaSoundData.Root.Descendants("Switch");
                foreach (var el in Switch)
                    gameProjIDs.Add(uint.Parse(el.Attribute("ShortID").Value));

                IEnumerable<XElement> InitSoundBanks = docDuniaSoundData.Root.Descendants("InitSoundBank");
                foreach (var el in InitSoundBanks)
                    gameProjIDs.Add(uint.Parse(el.Attribute("ShortID").Value));
            }

            foreach (var item in gameProjIDs)
            {
                string a = item.ToString();
                if (!tw.Contains(a))
                    tw.Add(a);
            }

            File.WriteAllLines("so.txt", tw.ToArray());*/

            using var processModule = Process.GetCurrentProcess().MainModule;
            m_Path = Path.GetDirectoryName(processModule?.FileName);

            Console.Title = "FCBConverter";

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("*******************************************************************************************");
            Console.WriteLine("**** FCBConverter v" + version);
            Console.WriteLine("****   Author: ArmanIII");
            Console.WriteLine("****   " + xmlheaderthanks);
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
                Console.WriteLine("    FCBConverter -source=<folder> -filter=<search pattern> <allow subfolders>");
                Console.WriteLine("    folder - path for folder, use \\ to run it in the same directory where are you running the exe");
                Console.WriteLine("    search pattern - *.fcb for example convert all FCB files");
                Console.WriteLine("    allow subfolders - if you set \"-subfolders\", batch convert will process all found subfolders");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter -source=D:\\fcb_files -filter=*.fcb -subfolders");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<Unpacking DAT/FAT files>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter -source=<fat file> -out=<output dir>");
                Console.WriteLine("    fat file - path to fat file");
                Console.WriteLine("    output dir (optional) - output folder path, files will extracted to this newly created folder");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter -source=D:\\common.fat -out=D:\\common_unpacked");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<Packing to DAT/FAT>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter -source=<input folder> -fat=<fat file> <FAT version>");
                Console.WriteLine("    input folder - input folder path with files");
                Console.WriteLine("    fat file - path to the new fat file");
                Console.WriteLine("    FAT version - can be -v9 (FCP, FC4, FC3, FC3BD) or -v5 (FC2) or -v11 (FC6), default version is 10 (FC5, FCND), note that older FAT versions can't be compressed");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter -source=D:\\common_unpacked -fat=D:\\common.fat");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<Unpacking one file from DAT/FAT>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter -source=<fat file> -out=<output dir> -single=<desired file>");
                Console.WriteLine("    fat file - path to fat file");
                Console.WriteLine("    output dir - output folder path, file will extracted to this folder");
                Console.WriteLine("    desired file - file path inside the FAT file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter -source=D:\\common.fat -out=D:\\ -single=common.dbt.fcb");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For *.fcb, *.ndb files>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter -source=<source file>");
                Console.WriteLine("    source file - fcb or xml file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter -source=D:\\file.fcb");
                Console.WriteLine("    FCBConverter -source=D:\\file.fcb.converted.xml");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For *.oasis.bin files>>>");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("For New Dawn and FC6 FCBConverter will ask to specify the game due to differences in files.");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter -source=<source file>");
                Console.WriteLine("    source file - oasis file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples FC5]");
                Console.WriteLine("    FCBConverter -source=D:\\oasisstrings.oasis.bin");
                Console.WriteLine("    FCBConverter -source=D:\\oasisstrings.oasis.bin.converted.xml");
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
                Console.WriteLine("    FCBConverter -source=<source file>");
                Console.WriteLine("    source file - *_depload.dat file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter -source=D:\\patch_depload.dat");
                Console.WriteLine("    FCBConverter -source=D:\\patch_depload.dat.converted.xml");
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
                Console.WriteLine("    FCBConverter -source=<source file>");
                Console.WriteLine("    source file - soundinfo.bin file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter -source=D:\\soundinfo.bin");
                Console.WriteLine("    FCBConverter -source=D:\\soundinfo.bin.converted.xml");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For compiled LUAQ *.lua to decompiled *.lua files>>>");
                Console.ResetColor();
                Console.WriteLine("Decompiles a LUAQ lua file. If a lua contains LUAC data, it will be converted into two separate files - lua and xml.");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter -source=<source file>");
                Console.WriteLine("    source file - *.lua file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter -source=D:\\script.lua");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For *.lua/*.xml to *.luac files>>>");
                Console.ResetColor();
                Console.WriteLine("Converts a lua script file to a lua file with LUAC header.");
                Console.WriteLine("Requires a xml file with LUAC definitions.");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter -source=<source file>");
                Console.WriteLine("    source file - *.xml file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter -source=D:\\script.xml");
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
                Console.WriteLine("    FCBConverter -source=<source file>");
                Console.WriteLine("    source file - *.xbt file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter -source=D:\\texture.xbt");
                Console.WriteLine("    FCBConverter -source=D:\\texture.dds");
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
                Console.WriteLine("    FCBConverter -source=<source file>");
                Console.WriteLine("    source file - *.material.bin file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter -source=D:\\abc.material.bin");
                Console.WriteLine("    FCBConverter -source=D:\\abc.material.bin.converted.xml");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For *.move.bin files>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter -source=<source file>");
                Console.WriteLine("    source file - *.move.bin file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter -source=D:\\file.move.bin");
                Console.WriteLine("    FCBConverter -source=D:\\file.move.bin.converted.xml");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For combinedmovefile.bin file>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter -source=<source file>");
                Console.WriteLine("    source file - combinedmovefile.bin file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter -source=D:\\combinedmovefile.bin");
                Console.WriteLine("    FCBConverter -source=D:\\combinedmovefile.bin.converted.xml");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For *.cseq files>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter -source=<source file>");
                Console.WriteLine("    source file - *.cseq file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter -source=D:\\sequence.cseq");
                Console.WriteLine("    FCBConverter -source=D:\\sequence.cseq.converted.xml");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For *.feu, *.swf files>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter -source=<source file>");
                Console.WriteLine("    source file - *.feu or *.swf file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter -source=D:\\ui.feu");
                Console.WriteLine("    FCBConverter -source=D:\\ui.swf");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For *.bdl files>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter -source=<source file>");
                Console.WriteLine("    source file - *.bdl file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter -source=D:\\0003_0005_0000_0000.terrainnode.bdl");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For *.bnk, *.wem files>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter -source=<source file>");
                Console.WriteLine("    source file - *.bnk or *.wem file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter -source=D:\\3253855986.bnk");
                Console.WriteLine("    FCBConverter -source=D:\\3253855986.bnk.converted.xml");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("<<<For Unreal Engine to XBG files>>>");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FCBConverter -UE2XBG=<type> -ue=<uasset> -xbg=<ref_xbg>");
                Console.WriteLine("    type - type of model - 0 clothes, 1 hairs, 2 weapons, 3 vehicles");
                Console.WriteLine("    uasset - source file from Unreal Engine");
                Console.WriteLine("    ref_xbg - reference XBG file");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FCBConverter -UE2XBG=0 -ue=handw_avatar_mygloves_aver_mf.uasset -xbg=handw_avatar_samfisher01_aver_mf.xbg");
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========================================================================");
                Console.ResetColor();
                return;
            }

            Arguments arguments = new(args);

            string file = args[0];

            Console.Title = "FCBConverter - " + file;

            bool bKeep = false;

            if (LoadSetting("CompressFile") == "false" && arguments["enablecompress"] != "true")
            {
                Console.WriteLine("Compression disabled.");
                Console.WriteLine("");
                isCompressEnabled = false;
            }

            excludeFilesFromCompress = arguments["excludeFilesFromCompress"] ?? excludeFilesFromCompress;
            excludeFilesFromPack = arguments["excludeFilesFromPack"] ?? excludeFilesFromPack;

            if (arguments["disablecompress"] == "true")
            {
                Console.WriteLine("Compression disabled via param.");
                Console.WriteLine("");
                isCompressEnabled = false;
            }

            bKeep = arguments["keep"] == "true";
            isFC2 = arguments["fc2"] == "true";

            if (file.EndsWith("entitylibrarynamestoresid.fcb"))
                isEntLibNamesStores = true;

            definitionLoader = new DefinitionsLoader(m_Path + defsFile, file);

            try
            {
                string source = arguments["source"] ?? "";

                if (file.EndsWith("_replace.txt"))
                {
                    BinaryReplaceValues(file);
                    FIN();
                }

                else if (file.EndsWith(".png") && source == "") // specific - doesn't use args, for Win Open With
                {
                    ConvertPNG2XBT(file);
                    FIN();
                }

                else if (source.EndsWith(".png"))
                {
                    ConvertPNG2XBT(source);
                    FIN();
                }

                else if (Directory.Exists(source) && arguments["fat"] != null && arguments["fat"].EndsWith(".fat"))
                {
                    int ver = 10;

                    if (arguments["v11"] == "true")
                        ver = 11;

                    if (arguments["v9"] == "true")
                        ver = 9;

                    if (arguments["v5"] == "true")
                        ver = 5;

                    LoadFile();
                    PackBigFile(source, arguments["fat"], ver);
                    FIN();
                }

                else if (source.EndsWith(".fat") && arguments["out"] != null && arguments["single"] != null) // excludeFromCompress is used as file name
                {
                    UnpackBigFile(source, arguments["out"], arguments["single"]);
                    FIN();
                }

                else if (file.EndsWith(".fat") && source == "") // specific - doesn't use args, for Win Open With
                {
                    UnpackBigFile(file, "");
                    FIN();
                }

                else if (source.EndsWith(".fat"))
                {
                    UnpackBigFile(source, arguments["out"] ?? "");
                    FIN();
                }

                else if (File.Exists(file) && source == "") // specific - doesn't use args, for Win Open With
                {
                    Proccessing(file, "");
                }

                else if (File.Exists(source))
                {
                    Proccessing(source, arguments["out"] ?? "");
                }

                else if ((Directory.Exists(source) || source == @"\") && arguments["filter"] != null)
                {
                    if (source == @"\")
                        source = Directory.GetCurrentDirectory();

                    ProcessSubFolders(source, arguments["filter"], arguments["subfolders"] == "true");
                    Console.WriteLine("Job done!");
                }

                else if (arguments["xbgFP"] == "true")
                {
                    FixXBGForFP(arguments["xbg"], arguments["data"]);
                }

                else if (arguments["xbgData"] == "true")
                {
                    GetDataFromXBG(arguments["xbg"]);
                }

                else if(arguments["UE2XBG"] != null)
                {
                    ConvertUE2XBG(arguments["ue"], arguments["xbg"], int.Parse(arguments["UE2XBG"]));
                }

                else
                {
                    Console.WriteLine("Input file / directory doesn't exist!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(1);
            }

            if (bKeep)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }

            return;

            /*
            string file = args[0];
            string param2 = args.Length > 1 ? args[1] : "";
            string param3 = args.Length > 2 ? args[2] : "";
            string param4 = args.Length > 3 ? args[3] : "";
            string param5 = args.Length > 4 ? args[4] : "";
            string param6 = args.Length > 5 ? args[5] : "";

            string enC = "-enablecompress";
            string disC = "-disablecompress";
            string keep = "-keep";
            bool bKeep = false;

            if (LoadSetting("CompressFile") == "false" && param2 != enC && param3 != enC && param4 != enC)
            {
                Console.WriteLine("Compression disabled.");
                Console.WriteLine("");
                isCompressEnabled = false;
            }

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i].StartsWith("-excludeFilesFromCompress="))
                {
                    excludeFilesFromCompress = args[i].Replace("-excludeFilesFromCompress=", "");
                }

                if (args[i].StartsWith("-excludeFilesFromPack="))
                {
                    excludeFilesFromPack = args[i].Replace("-excludeFilesFromPack=", "");
                }

                if (args[i] == disC)
                {
                    Console.WriteLine("Compression disabled via param.");
                    Console.WriteLine("");
                    isCompressEnabled = false;
                }
            }

            if (file == keep)
            {
                file = "";
                bKeep = true;
            }
            if (param2 == keep)
            {
                param2 = "";
                bKeep = true;
            }
            if (param3 == keep)
            {
                param3 = "";
                bKeep = true;
            }
            if (param4 == keep)
            {
                param4 = "";
                bKeep = true;
            }
            if (param5 == keep)
            {
                param5 = "";
                bKeep = true;
            }
            if (param6 == keep)
            {
                param6 = "";
                bKeep = true;
            }

            if (param2 == "-fc2")
            {
                param2 = "";
                isFC2 = true;
            }
            if (param3 == "-fc2")
            {
                param3 = "";
                isFC2 = true;
            }
            if (param4 == "-fc2")
            {
                param4 = "";
                isFC2 = true;
            }

            Console.Title = "FCBConverter - " + file;

            if (file.EndsWith("entitylibrarynamestoresid.fcb"))
                isEntLibNamesStores = true;

            definitionLoader = new DefinitionsLoader(m_Path + defsFile, file);

            try
            {
                if (file.EndsWith("_replace.txt"))
                {
                    BinaryReplaceValues(file);
                    FIN();
                }
                else if (param2.EndsWith(".fat"))
                {
                    int ver = 10;

                    if (param3 == "-v11")
                        ver = 11;

                    if (param3 == "-v9")
                        ver = 9;

                    if (param3 == "-v5")
                        ver = 5;

                    LoadFile();
                    PackBigFile(file, param2, ver);
                    FIN();
                }
                else if (file.EndsWith(".fat") && Directory.Exists(param2) && param3 != "") // excludeFromCompress is used as file name
                {
                    UnpackBigFile(file, param2, param3);
                    FIN();
                }
                else if (file.EndsWith(".fat"))
                {
                    UnpackBigFile(file, param2);
                    FIN();
                }
                else if (File.Exists(file))
                {
                    Proccessing(file, param2);
                }
                else if (Directory.Exists(file) || file == @"\")
                {
                    if (file == @"\")
                        file = Directory.GetCurrentDirectory();

                    ProcessSubFolders(file, param2, param3 == "-subfolders");
                    Console.WriteLine("Job done!");
                }
                else if (file == "-xbgFP")
                {
                    FixXBGForFP(param2, param3);
                }
                else if (file == "-xbgData")
                {
                    GetDataFromXBG(param2);
                }
                else if (file.StartsWith("-ue="))
                {
                    ConvertUE2XBG(param2, param3, int.Parse(file.Replace("-ue=", "")));
                }
                else
                {
                    Console.WriteLine("Input file / directory doesn't exist!");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(1);
            }

            if (bKeep)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }

            return;*/
        }

        static void ProcessSubFolders(string folder, string filter, bool subFolders)
        {
            DirectoryInfo d = new DirectoryInfo(folder);

            string[] searchPatterns = filter.Split(',');
            foreach (string sep in searchPatterns)
            {
                FileInfo[] files = d.GetFiles(sep);

                foreach (FileInfo fileInfo in files)
                {
                    Console.WriteLine("Processing: " + fileInfo.FullName + "...");
                    Proccessing(fileInfo.FullName, "");
                }
            }

            if (subFolders)
            {
                DirectoryInfo[] dirs = d.GetDirectories();

                foreach (DirectoryInfo dirInfo in dirs)
                {
                    ProcessSubFolders(dirInfo.FullName, filter, true);
                }
            }
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

            if (file.EndsWith(".bnk"))
            {
                BNKExtract(file);

                FIN();
                return;
            }

            if (file.EndsWith(".bnk.converted.xml"))
            {
                BNKPack(file);

                FIN();
                return;
            }

            if (file.EndsWith(".wem"))
            {
                WEMToOGG(file);

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

            if ((file.EndsWith(".cseq") && fmt == 0) || file.EndsWith(".gosm.xml") || file.EndsWith(".rml") || (file.EndsWith(".ndb") && fmt == 0))
            {
                string workingOriginalFile;

                if (outputFile != "")
                    workingOriginalFile = outputFile;
                else
                    workingOriginalFile = Path.GetDirectoryName(file) + "\\" + Path.GetFileName(file) + (file.EndsWith(".ndb") || file.EndsWith(".cseq") ? ".rml" : "") + ".converted.xml";

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

            if (file.EndsWith(".gosm.xml.converted.xml") || file.EndsWith(".rml.converted.xml"))
            {
                string workingOriginalFile;

                if (outputFile != "")
                    workingOriginalFile = outputFile;
                else
                {
                    workingOriginalFile = file.EndsWith(".ndb.rml.converted.xml") || file.EndsWith(".cseq.rml.converted.xml") ? file.Replace(".rml.converted.xml", "") : file.Replace(".converted.xml", "");
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
                /*byte[] bytes = File.ReadAllBytes(file);

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
                File.WriteAllBytes(newPathTex, tex);*/

                ConvertXBT(file);
                FIN();
                return;
            }

            if (file.EndsWith(".dds") || file.EndsWith(".xbt.converted.xml") || file.EndsWith(".xbts.converted.xml"))
            {
                /*string texPath = file.Replace(".dds", ".tex");
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

                File.WriteAllBytes(xbtPath, bts.ToArray());*/

                ConvertDDS(file);
                FIN();
                return;
            }

            // ********************************************************************************************************************************************

            if (file.EndsWith(".decompiled.lua"))
            {
                string luac = file.Replace(".decompiled.lua", ".lua");
                int ret = LuacLibProcess(file, luac, luac, out IntPtr error);
                if (ret != 0)
                {
                    string str = Marshal.PtrToStringAnsi(error);
                    Console.WriteLine("Error during parsing Lua: " + str);
                }

                FIN();
                return;
            }

            if (file.EndsWith(".lua"))
            {
                byte[] luaBytesLuaq = Array.Empty<byte>();

                FileStream luaFile = File.OpenRead(file);
                uint luaType = luaFile.ReadValueU32();

                if (luaType == 0x4341554C)
                {
                    int luaLen = luaFile.ReadValueS32();
                    luaBytesLuaq = luaFile.ReadBytes(luaLen);
                    byte[] luaBytesLuac = luaFile.ReadBytes((int)(luaFile.Length - luaLen - (sizeof(int) * 2)));

                    File.WriteAllBytes(file + ".converted.xml", luaBytesLuac);
                }

                if (luaType == 0x61754C1B)
                {
                    luaFile.Seek(0, SeekOrigin.Begin);
                    luaBytesLuaq = luaFile.ReadBytes((int)luaFile.Length);
                }

                luaFile.Close();

                MemoryStream luaMS = new(luaBytesLuaq);
                var header = new BHeader(luaMS);
                LFunction lMain = header.Function.Parse(luaMS, header);

                var d = new Decompiler(lMain);
                d.Decompile();
                var writer = new StreamWriter(file + ".converted.lua", false, new UTF8Encoding(false));
                writer.WriteLine("--" + xmlheader);
                writer.WriteLine("--" + xmlheaderlua);
                writer.WriteLine("");
                d.Print(new Output(writer));
                writer.Flush();

                FIN();
                return;
            }

            if (file.EndsWith(".lua.converted.xml"))
            {
                string newLuaFile = file.Replace(".lua.converted.xml", "_new.lua");

                string luaFile = file.Replace(".lua.converted.xml", ".lua.converted.lua");

                string dominoMetadata = File.ReadAllText(file);

                if (File.Exists(newLuaFile))
                    File.Delete(newLuaFile);

                if (LoadSetting("UseLuaBytecode") == "true")
                {
                    byte[] bts = File.ReadAllBytes(luaFile);

                    byte[] dominoLuaBytecode;

                    var a = LoadSetting("LuaBytecodePath");
                    int ret = LuacLibProcessBytes(bts, bts.Length, out nint buffer, out int bufferLength, a != "" ? a : luaFile, out IntPtr error);
                    if (ret != 0)
                    {
                        string str = Marshal.PtrToStringAnsi(error);
                        Console.WriteLine("Error during parsing Lua: " + str);
                        return;
                    }

                    dominoLuaBytecode = new byte[bufferLength];
                    Marshal.Copy(buffer, dominoLuaBytecode, 0, bufferLength);

                    LuacLibFreeMem(buffer);

                    FileStream bin = new FileStream(newLuaFile, FileMode.Create);
                    bin.Write(BitConverter.GetBytes(0x4341554c), 0, 4);
                    bin.Write(BitConverter.GetBytes(dominoLuaBytecode.Length), 0, sizeof(int));
                    bin.Write(dominoLuaBytecode, 0, dominoLuaBytecode.Length);
                    bin.Write(Encoding.UTF8.GetBytes(dominoMetadata), 0, dominoMetadata.Length);
                    bin.Close();
                }
                else
                {
                    string dominoLuaData = File.ReadAllText(luaFile);

                    dominoLuaData = "--" + xmlheader + Environment.NewLine + dominoLuaData;

                    FileStream bin = new FileStream(newLuaFile, FileMode.Create);
                    bin.Write(BitConverter.GetBytes(0x4341554c), 0, 4);
                    bin.Write(BitConverter.GetBytes(dominoLuaData.Length), 0, sizeof(int));
                    bin.Write(Encoding.UTF8.GetBytes(dominoLuaData), 0, dominoLuaData.Length);
                    bin.Write(Encoding.UTF8.GetBytes(dominoMetadata), 0, dominoMetadata.Length);
                    bin.Close();
                }

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
                LoadFile(isFC2 ? 5 : 10);
                SoundInfoConvertBin(file);
                FIN();
                return;
            }

            // ********************************************************************************************************************************************

            LoadString();

            // ********************************************************************************************************************************************

            if (file.EndsWith(".oasis.bin"))
            {
                string workingOriginalFile;

                if (outputFile != "")
                    workingOriginalFile = outputFile;
                else
                    workingOriginalFile = Path.GetDirectoryName(file) + "\\" + Path.GetFileName(file) + ".converted.xml";

                OasisNew.OasisDeserialize(file, workingOriginalFile);

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
                    workingOriginalFile = file.Replace(".oasis.bin.converted.xml", "");
                    workingOriginalFile = Path.GetDirectoryName(file) + "\\" + Path.GetFileNameWithoutExtension(workingOriginalFile) + "_new.oasis.bin";
                }

                OasisNew.OasisSerialize(file, workingOriginalFile);

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

                OasisNew.OasisDeserialize(file, workingOriginalFile);

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
                    workingOriginalFile = file.Replace(".bin.converted.xml", "");
                    workingOriginalFile = Path.GetDirectoryName(file) + "\\new_" + Path.GetFileNameWithoutExtension(workingOriginalFile) + ".bin";
                }

                OasisNew.OasisSerialize(file, workingOriginalFile);

                FIN();
                return;
            }

            // ********************************************************************************************************************************************

            if (file.EndsWith(".hkx"))
            {
                FileStream hkxFile = File.OpenRead(file);
                uint ver = hkxFile.ReadValueU32();
                uint unk1 = hkxFile.ReadValueU32();
                uint hkxSize = hkxFile.ReadValueU32();
                uint fcbSize = hkxFile.ReadValueU32();
                uint unk2 = hkxFile.ReadValueU32();
                uint unk3 = hkxFile.ReadValueU32();
                uint unk4 = hkxFile.ReadValueU32();
                uint unk5 = hkxFile.ReadValueU32();
                byte[] hkxPure = hkxFile.ReadBytes((int)hkxSize);
                byte[] fcbData = hkxFile.ReadBytes((int)fcbSize);
                hkxFile.Close();

                string baseFileName = file.Replace(".hkx", "");

                XElement root = new XElement("HKXHeader");
                root.Add(new XElement("Version", ver.ToString()));
                root.Add(new XElement("Unknown1", unk1.ToString()));
                root.Add(new XElement("Unknown2", unk2.ToString()));
                root.Add(new XElement("Unknown3", unk3.ToString()));
                root.Add(new XElement("Unknown4", unk4.ToString()));
                root.Add(new XElement("Unknown5", unk5.ToString()));

                XDocument xDoc = new XDocument();
                xDoc.Add(root);
                xDoc.Save(baseFileName + ".hkx.converted.xml");

                File.WriteAllBytes(baseFileName + ".pure.hkx", hkxPure);

                File.WriteAllBytes(baseFileName + "tmp", fcbData);
                ConvertFCB(baseFileName + "tmp", baseFileName + ".config.xml");
                File.Delete(baseFileName + "tmp");

                FIN();
                return;
            }

            if (file.EndsWith(".hkx.converted.xml"))
            {
                string baseFileName = file.Replace(".hkx.converted.xml", "");

                byte[] hkxPure = File.ReadAllBytes(baseFileName + ".pure.hkx");

                ConvertXML(baseFileName + ".config.xml", baseFileName + "tmp");
                byte[] fcbData = File.ReadAllBytes(baseFileName + "tmp");
                File.Delete(baseFileName + "tmp");

                XDocument xDoc = XDocument.Load(file);
                XElement root = xDoc.Element("HKXHeader");

                var output = File.Create(baseFileName + "_new.hkx");
                output.WriteValueU32(uint.Parse(root.Element("Version").Value));
                output.WriteValueU32(uint.Parse(root.Element("Unknown1").Value));
                output.WriteValueU32((uint)hkxPure.Length);
                output.WriteValueU32((uint)fcbData.Length);
                output.WriteValueU32(uint.Parse(root.Element("Unknown2").Value));
                output.WriteValueU32(uint.Parse(root.Element("Unknown3").Value));
                output.WriteValueU32(uint.Parse(root.Element("Unknown4").Value));
                output.WriteValueU32(uint.Parse(root.Element("Unknown5").Value));
                output.WriteBytes(hkxPure);
                output.WriteBytes(fcbData);
                output.Close();

                FIN();
                return;
            }

            // ********************************************************************************************************************************************

            if (file.EndsWith(".spx"))
            {
                byte[] bytes = File.ReadAllBytes(file);

                int[] poses = Helpers.SearchBytesMultiple(bytes, new byte[] { 0x50, 0x48, 0x58 });

                if (poses.Length > 1)
                {
                    throw new Exception("Multiple PHX not supported.");
                }

                FileStream spxFile = File.OpenRead(file);
                byte[] fcbData = spxFile.ReadBytes(poses[0]);
                byte[] phxData = spxFile.ReadBytes((int)(spxFile.Length - poses[0]));
                spxFile.Close();

                string baseFileName = file.Replace(".spx", "");

                File.WriteAllBytes(baseFileName + ".phx", phxData);

                File.WriteAllBytes(baseFileName + "tmp", fcbData);
                ConvertFCB(baseFileName + "tmp", baseFileName + ".spx.converted.xml");
                File.Delete(baseFileName + "tmp");

                FIN();
                return;
            }

            if (file.EndsWith(".spx.converted.xml"))
            {
                string baseFileName = file.Replace(".spx.converted.xml", "");

                byte[] phxData = File.ReadAllBytes(baseFileName + ".phx");

                ConvertXML(file, baseFileName + "tmp");
                byte[] fcbData = File.ReadAllBytes(baseFileName + "tmp");
                File.Delete(baseFileName + "tmp");

                var output = File.Create(baseFileName + "_new.spx");
                output.WriteBytes(fcbData);
                output.WriteBytes(phxData);
                output.Close();
                FIN();
                return;
            }

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
            else if (file.EndsWith(".mab"))
            {
                FC4MarkupExtr(file);
                FIN();
                return;
            }
            else if (file.EndsWith(".mab.converted.xml"))
            {
                FC4MarkupPack(file);
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

            LoadFile(isFC2 ? 5 : 10);

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

                    bof.Root = Gibbed.Dunia2.ConvertBinaryObject.Importing.Import(basePath, root, definitionLoader);

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

                    Array.Resize(ref compressedBytes, outputSize);

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
            else if (file.EndsWith(".obj") || file.EndsWith(".lib") || file.EndsWith(".cseq") || file.EndsWith(".fcb") || file.EndsWith(".ndb") || file.EndsWith(".bin") || file.EndsWith(".bwsk") || file.EndsWith(".part") || file.EndsWith(".dsc") || file.EndsWith(".skeleton") || file.EndsWith(".animtrackcol"))
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
                    Gibbed.Dunia2.ConvertBinaryObject.Exporting.Export(workingOriginalFile.Replace(".fcb", ".fcb.lzo"), bof, definitionLoader);
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

        static string LoadSetting(string settingName)
        {
            XDocument settingsXML = XDocument.Load(m_Path + settingsFile);
            XElement root = settingsXML.Element("FCBConverter");
            string selSettVal = root.Element(settingName).Value;
            return selSettVal;
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
            Console.WriteLine("Loading list of files...");

            if (listFilesDict.Count() > 0)
                return;

            if (!File.Exists(m_Path + listFiles))
            {
                Console.WriteLine(m_Path + listFiles + " doesn't exist!");
                return;
            }

            string[] ss;

            if (dwVersion == 5) 
                ss = File.ReadAllLines(m_Path + listFiles_5);
            else
                ss = File.ReadAllLines(m_Path + listFiles);

            for (int i = 0; i < ss.Length; i++)
            {
                ulong a = dwVersion == 5 ? Gibbed.Dunia2.FileFormats.CRC32.Hash(ss[i]) : Gibbed.Dunia2.FileFormats.CRC64.Hash(ss[i]);
                if (!listFilesDict.ContainsKey(a))
                    listFilesDict.Add(a, ss[i]);
            }

            Console.WriteLine("Files loaded: " + listFilesDict.Count);
        }

        static void LoadString()
        {
            Console.WriteLine("Loading list of strings...");

            if (listStringsDict.Count() > 0)
                return;

            if (!File.Exists(m_Path + listStrings))
            {
                Console.WriteLine(m_Path + listStrings + " doesn't exist!");
                return;
            }

            string[] ss = File.ReadAllLines(m_Path + listStrings);
            for (int i = 0; i < ss.Length; i++)
            {
                uint a = Gibbed.Dunia2.FileFormats.CRC32.Hash(ss[i]);
                if (!listStringsDict.ContainsKey(a))
                    listStringsDict.Add(a, ss[i]);
            }

            Console.WriteLine("Strings loaded: " + listStringsDict.Count);
        }

        public static void ConvertFCB(string inputPath, string outputPath)
        {
            var bof = new Gibbed.Dunia2.FileFormats.BinaryObjectFile();
            var input = File.OpenRead(inputPath);
            bof.Deserialize(input);
            input.Close();

            Gibbed.Dunia2.ConvertBinaryObject.Exporting.Export(outputPath, bof, definitionLoader);
        }

        public static void ConvertXML(string inputPath, string outputPath)
        {
            var bof = new Gibbed.Dunia2.FileFormats.BinaryObjectFile();

            var basePath = Path.ChangeExtension(inputPath, null);

            var doc = new XPathDocument(inputPath);
            var nav = doc.CreateNavigator();

            var root = nav.SelectSingleNode("/object");

            bof.Root = Gibbed.Dunia2.ConvertBinaryObject.Importing.Import(basePath, root, definitionLoader);

            var output = File.Create(outputPath);
            bof.Serialize(output);
            output.Close();
        }

        /*
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
        */

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
                Types.Add(listStringsDict.ContainsKey(typeHash) ? listStringsDict[typeHash] : typeHash.ToString("X8"));
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
                dependencyLoaderItem.fileName = listFilesDict.ContainsKey(DependentFiles[i].FileHash) ? listFilesDict[DependentFiles[i].FileHash] : "__Unknown\\" + DependentFiles[i].FileHash.ToString("X16");

                dependencyLoaderItem.depFiles = new List<string>();
                dependencyLoaderItem.depTypes = new List<int>();

                for (int j = 0; j < DependentFiles[i].CountOfDependencyFiles; j++)
                {
                    ulong dependencyFile = DependencyFiles[DependentFiles[i].DependencyFilesStartIndex + j];
                    dependencyLoaderItem.depFiles.Add(listFilesDict.ContainsKey(dependencyFile) ? listFilesDict[dependencyFile] : "__Unknown\\" + dependencyFile.ToString("X16"));
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
            XmlComment comment2 = xmlDoc.CreateComment(xmlheaderthanks);

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

                if (listStringsDict.ContainsValue(Types[i]))
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

            IEnumerable<XElement> MemoryNodeAssociations = null;
            IEnumerable<XElement> MemoryNodeAssociations2 = null;
            if (version == "14")
            {
                MemoryNodeAssociations = root.Element("MemoryNodeAssociations").Elements("MemoryNodeAssociation");
                MemoryNodeAssociations2 = root.Element("MemoryNodeAssociations2").Elements("MemoryNodeAssociation");
                string SoundChangeList = root.Attribute("SoundChangeList").Value;

                output.WriteValueU32((uint)MemoryNodeAssociations.Count(), 0);
                output.WriteValueU32(uint.Parse(SoundChangeList), 0);
                output.WriteValueU32((uint)MemoryNodeAssociations2.Count(), 0);
            }

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

                if (version == "14")
                {
                    string Unknown2 = Event.Attribute("Unknown2").Value;
                    output.WriteValueU32(uint.Parse(Unknown2), 0);

                    IEnumerable<XElement> Streams = Event.Element("Streams")?.Elements("Stream");
                    output.WriteValueU32((uint)(Streams != null ? Streams.Count() : 0), 0);
                    if (Streams != null && Streams.Count() > 0)
                    {
                        foreach (XElement Str in Streams)
                        {
                            output.WriteValueU64(GetFileHash(Str.Value.ToLowerInvariant()), 0);
                        }
                    }
                }
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

            if (version == "14")
            {
                foreach (XElement MemoryNodeAssociation in MemoryNodeAssociations)
                {
                    string SoundBankID = MemoryNodeAssociation.Attribute("SoundBankID").Value;
                    string MemoryNodeID = MemoryNodeAssociation.Attribute("MemoryNodeID").Value;
                    output.WriteValueU32(uint.Parse(SoundBankID), 0);
                    output.WriteValueU32(uint.Parse(MemoryNodeID), 0);
                }

                output.WriteBytes(new byte[MemoryNodeAssociations.Count() * 8]);

                foreach (XElement MemoryNodeAssociation in MemoryNodeAssociations2)
                {
                    string SoundBankID = MemoryNodeAssociation.Attribute("SoundBankID").Value;
                    string MemoryNodeID = MemoryNodeAssociation.Attribute("MemoryNodeID").Value;
                    output.WriteValueU32(uint.Parse(SoundBankID), 0);
                    output.WriteValueU32(uint.Parse(MemoryNodeID), 0);
                }

                output.WriteBytes(new byte[MemoryNodeAssociations2.Count() * 8]);
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

            bool ver14 = Version == 14;
            uint MemoryNodeAssociationsCount = 0;
            uint SoundChangeList = 0;
            uint MemoryNodeAssociations2Count = 0;
            if (ver14)
            {
                MemoryNodeAssociationsCount = SoundInfoReader.ReadUInt32();
                SoundChangeList = SoundInfoReader.ReadUInt32();
                MemoryNodeAssociations2Count = SoundInfoReader.ReadUInt32();

                XmlAttribute rootNodeAttributeSoundChangeList = xmlDoc.CreateAttribute("SoundChangeList");
                rootNodeAttributeSoundChangeList.Value = SoundChangeList.ToString();
                rootNode.Attributes.Append(rootNodeAttributeSoundChangeList);
            }

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

                if (ver14)
                {
                    uint unknown2 = SoundInfoReader.ReadUInt32();
                    uint StreamsCount = SoundInfoReader.ReadUInt32();

                    XmlAttribute EventNodeAttributeUnknown2 = xmlDoc.CreateAttribute("Unknown2");
                    EventNodeAttributeUnknown2.Value = unknown2.ToString();
                    EventNode.Attributes.Append(EventNodeAttributeUnknown2);

                    if (StreamsCount > 0)
                    {
                        XmlNode StreamsNode = xmlDoc.CreateElement("Streams");
                        EventNode.AppendChild(StreamsNode);

                        for (int j = 0; j < StreamsCount; j++)
                        {
                            ulong StreamFileNameHash = SoundInfoReader.ReadUInt64();

                            XmlNode StreamNode = xmlDoc.CreateElement("Stream");
                            StreamNode.InnerText = listFilesDict.ContainsKey(StreamFileNameHash) ? listFilesDict[StreamFileNameHash] : "__Unknown\\" + StreamFileNameHash.ToString("X16");
                            StreamsNode.AppendChild(StreamNode);
                        }
                    }
                }
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

            if (ver14)
            {
                XmlNode MemoryNodeAssociationsNode = xmlDoc.CreateElement("MemoryNodeAssociations");
                rootNode.AppendChild(MemoryNodeAssociationsNode);

                for (int i = 0; i < MemoryNodeAssociationsCount; i++)
                {
                    uint SoundBankID = SoundInfoReader.ReadUInt32();
                    uint MemoryNodeID = SoundInfoReader.ReadUInt32();

                    XmlNode MemoryNodeAssociationNode = xmlDoc.CreateElement("MemoryNodeAssociation");
                    MemoryNodeAssociationsNode.AppendChild(MemoryNodeAssociationNode);

                    XmlAttribute BankNodeAttributeSoundBankID = xmlDoc.CreateAttribute("SoundBankID");
                    BankNodeAttributeSoundBankID.Value = SoundBankID.ToString();
                    MemoryNodeAssociationNode.Attributes.Append(BankNodeAttributeSoundBankID);

                    XmlAttribute BankNodeAttributeMemoryNodeID = xmlDoc.CreateAttribute("MemoryNodeID");
                    BankNodeAttributeMemoryNodeID.Value = MemoryNodeID.ToString();
                    MemoryNodeAssociationNode.Attributes.Append(BankNodeAttributeMemoryNodeID);
                }

                SoundInfoReader.ReadBytes((int)MemoryNodeAssociationsCount * 8);

                XmlNode MemoryNodeAssociations2Node = xmlDoc.CreateElement("MemoryNodeAssociations2");
                rootNode.AppendChild(MemoryNodeAssociations2Node);

                for (int i = 0; i < MemoryNodeAssociations2Count; i++)
                {
                    uint SoundBankID = SoundInfoReader.ReadUInt32();
                    uint MemoryNodeID = SoundInfoReader.ReadUInt32();

                    XmlNode MemoryNodeAssociationNode = xmlDoc.CreateElement("MemoryNodeAssociation");
                    MemoryNodeAssociations2Node.AppendChild(MemoryNodeAssociationNode);

                    XmlAttribute BankNodeAttributeSoundBankID = xmlDoc.CreateAttribute("SoundBankID");
                    BankNodeAttributeSoundBankID.Value = SoundBankID.ToString();
                    MemoryNodeAssociationNode.Attributes.Append(BankNodeAttributeSoundBankID);

                    XmlAttribute BankNodeAttributeMemoryNodeID = xmlDoc.CreateAttribute("MemoryNodeID");
                    BankNodeAttributeMemoryNodeID.Value = MemoryNodeID.ToString();
                    MemoryNodeAssociationNode.Attributes.Append(BankNodeAttributeMemoryNodeID);
                }

                SoundInfoReader.ReadBytes((int)MemoryNodeAssociations2Count * 8);
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
            XmlComment comment2 = xmlDoc.CreateComment(xmlheaderthanks);

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

                XmlAttribute FrameNodeAttributeUnknown = xmlDoc.CreateAttribute("Time");
                FrameNodeAttributeUnknown.Value = unknown.ToString(CultureInfo.InvariantCulture);
                FrameNode.Attributes.Append(FrameNodeAttributeUnknown);

                XmlAttribute FrameNodeAttributeFileNameHash = xmlDoc.CreateAttribute("FrameCRC64");
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

            byte cnt = 0;
            IEnumerable<XElement> allFrames = root.Descendants("Frame");
            foreach (XElement allFrame in allFrames)
            {
                float unknown = float.Parse(allFrame.Attribute("Time").Value, CultureInfo.InvariantCulture);

                string tmp = file + "_" + cnt.ToString();
                XElement fcb = allFrame.Element("object");
                fcb.Save(tmp);

                ConvertXML(tmp, tmp + "c");

                byte[] fcbByte = File.ReadAllBytes(tmp + "c");

                ulong crc = Gibbed.Dunia2.FileFormats.CRC64.Hash(fcbByte, 0, fcbByte.Length);

                output.WriteValueF32(unknown, 0);
                output.WriteValueU32((uint)fcbByte.Length);
                output.WriteValueU64(crc);
                output.WriteBytes(fcbByte);

                File.Delete(tmp);
                File.Delete(tmp + "c");
                cnt++;
            }

            output.Close();
        }

        static void FC4MarkupExtr(string file)
        {
            string onlyDir = Path.GetDirectoryName(file);

            XDocument xmlDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

            xmlDoc.Add(new XComment(xmlheader));
            xmlDoc.Add(new XComment(xmlheaderthanks));

            XElement root = new XElement("CMarkupResource");

            FileStream MabStream = new FileStream(file, FileMode.Open);

            MemoryStream ms = new MemoryStream();
            MabStream.CopyTo(ms);

            byte[] byteSequence = new byte[] { 0x6E, 0x62, 0x43, 0x46 }; // nbCF
            int[] poses = Helpers.SearchBytesMultiple(ms.ToArray(), byteSequence);

            foreach (int pos in poses)
            {
                MabStream.Seek(pos - 16, SeekOrigin.Begin);

                uint hash = MabStream.ReadValueU32();
                float time = MabStream.ReadValueF32();
                uint unknown = MabStream.ReadValueU32();
                ushort fcbLen = MabStream.ReadValueU16();
                ushort secLen = MabStream.ReadValueU16();

                byte[] fcbBytes = MabStream.ReadBytes(fcbLen);

                string tmp = onlyDir + "\\" + hash.ToString();
                File.WriteAllBytes(tmp, fcbBytes);
                ConvertFCB(tmp, tmp + "c");
                XDocument doc = XDocument.Load(tmp + "c");

                XElement xFrame = new XElement("Frame");
                xFrame.Add(new XAttribute("FrameCRC32", hash.ToString()));
                xFrame.Add(new XAttribute("Time", time.ToString(CultureInfo.InvariantCulture)));
                xFrame.Add(new XAttribute("Unknown", unknown.ToString()));
                xFrame.Add(doc.Root);
                root.Add(xFrame);

                File.Delete(tmp);
                File.Delete(tmp + "c");
            }

            byte[] lastBytes = MabStream.ReadBytes((int)(MabStream.Length - MabStream.Position));
            File.WriteAllBytes(file + ".converted.bin", lastBytes);

            MabStream.Dispose();

            xmlDoc.Add(root);
            xmlDoc.Save(file + ".converted.xml");
        }
        
        static void FC4MarkupPack(string file)
        {
            string originalMab = file.Replace(".converted.xml", "");
            string lastBinaryFile = file.Replace(".converted.xml", ".converted.bin");
            string newMab = originalMab.Replace(".mab", "_new.mab");

            XDocument doc = XDocument.Load(file);
            IEnumerable<XElement> frames = doc.Element("CMarkupResource").Elements("Frame");

            var output = new MemoryStream();

            int cnt = 0;
            foreach (XElement frame in frames)
            {
                float time = float.Parse(frame.Attribute("Time").Value, CultureInfo.InvariantCulture);
                int unknown = int.Parse(frame.Attribute("Unknown").Value);

                string tmp = file + "_" + cnt.ToString();
                XElement fcb = frame.Element("object");
                fcb.Save(tmp);

                ConvertXML(tmp, tmp + "c");

                byte[] fcbByte = File.ReadAllBytes(tmp + "c");

                uint crc = Gibbed.Dunia2.FileFormats.CRC32.Hash(fcbByte, 0, fcbByte.Length);

                output.WriteValueU32(crc);
                output.WriteValueF32(time, 0);
                output.WriteValueS32(unknown);
                output.WriteValueU16((ushort)fcbByte.Length);
                output.WriteValueU16(0);
                output.WriteBytes(fcbByte);

                File.Delete(tmp);
                File.Delete(tmp + "c");
                cnt++;
            }

            output.Flush();
            output.Seek(0, SeekOrigin.Begin);

            File.Copy(originalMab, newMab, true);

            FileStream MabStream = new(newMab, FileMode.Open);

            MemoryStream ms = new();
            MabStream.CopyTo(ms);

            byte[] byteSequence = new byte[] { 0x6E, 0x62, 0x43, 0x46 }; // nbCF
            int[] poses = Helpers.SearchBytesMultiple(ms.ToArray(), byteSequence);

            int startMarkupPos = poses[0] - 16;

            MabStream.SetLength(startMarkupPos);

            MabStream.WriteBytes(output.ToArray());

            byte[] lastBytes = File.ReadAllBytes(lastBinaryFile);

            MabStream.WriteBytes(lastBytes);

            MabStream.Flush();
            MabStream.Close();
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
            XmlComment comment2 = xmlDoc.CreateComment(xmlheaderthanks);

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
            writer.WriteComment(xmlheaderthanks);
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
                //writer.WriteAttributeString("PathHash", listFilesDict.ContainsKey(pathHash) ? listFilesDict[pathHash] : "__Unknown\\" + pathHash.ToString("X16"));
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

            int cnt = 0;
            foreach (KeyValuePair<ulong, FatEntry> pair in Entries)
            {
                cnt++;

                FatEntry fatEntry = pair.Value;

                if (oneFile != "")
                {
                    if (fatEntry.NameHash != oneFileHash)
                        continue;

                    oneFileFound = true;
                }

                string m_Hash = fatEntry.NameHash.ToString(dwVersion >= 9 ? "X16" : "X8");
                string fileName;
                if (listFilesDict.ContainsKey(fatEntry.NameHash) && fatEntry.NameHash > 0)
                {
                    listFilesDict.TryGetValue(fatEntry.NameHash, out fileName);
                }
                else
                {
                    fileName = @"__Unknown\" + m_Hash;
                }

                if (oneFileFound)
                {
                    fileName = Path.GetFileName(fileName);
                }

                string m_FullPath = m_DstFolder + @"\" + fileName;

                Console.WriteLine($"[Unpacking {cnt} / {Entries.Count}]: {fileName}");

                byte[] pDstBuffer = new byte[] { };

                if (fatEntry.CompressionScheme == CompressionScheme.None)
                {
                    TDATStream.Seek(fatEntry.Offset, SeekOrigin.Begin);

                    if (dwVersion == 11 || dwVersion == 10)
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

                    if (dwVersion == 9)
                    {
                        LZ4Decompressor64 TLZ4Decompressor64 = new LZ4Decompressor64();
                        TLZ4Decompressor64.Different = true;
                        TLZ4Decompressor64.Decompress(pSrcBuffer, pDstBuffer);
                    }
                    if (dwVersion > 9)
                    {
                        LZ4Codec.Decode(pSrcBuffer, pDstBuffer);
                    }
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
            string fileName = Path.GetFileName(m_UnknownFileName);

            if (dwID == 0x004D4154) //TAM
            {
                m_UnknownFileName = m_Directory + @"\MAT\" + fileName + ".material.bin";
            }
            else
            if (dwID == 0x474E5089) //PNG
            {
                m_UnknownFileName = m_Directory + @"\PNG\" + fileName + ".png";
            }
            else
            if (dwID == 0x42444947) //GIDB
            {
                m_UnknownFileName = m_Directory + @"\GIDB\" + fileName + ".bin";
            }
            else
            if (dwID == 0x4D4F4D41) //MOMA
            {
                m_UnknownFileName = m_Directory + @"\ANIM\" + fileName + ".bin";
            }
            else
            if (dwID == 0x4D760040) //MOVE
            {
                m_UnknownFileName = m_Directory + @"\MOVE\" + fileName + ".move.bin";
            }
            else
            if (dwID == 0x00534B4C) //SKL
            {
                m_UnknownFileName = m_Directory + @"\SKEL\" + fileName + ".skeleton";
            }
            else
            if (dwID == 0x01194170 || dwID == 0x00194170) //pA
            {
                m_UnknownFileName = m_Directory + @"\DPAX\" + fileName + ".dpax";
            }
            else
            if (dwID == 0x44484B42) //BKHD
            {
                m_UnknownFileName = m_Directory + @"\BNK\" + fileName + ".bnk";
            }
            else
            if (dwID == 0x8464555) //UEF
            {
                m_UnknownFileName = m_Directory + @"\FEU\" + fileName + ".feu";
            }
            else
            if (dwID == 0x46464952) //RIFF
            {
                m_UnknownFileName = m_Directory + @"\WEM\" + fileName + ".wem";
            }
            else
            if (dwID == 0x4D455348) //HSEM
            {
                m_UnknownFileName = m_Directory + @"\XBG\" + fileName + ".xbg";
            }
            else
            if (dwID == 0x00584254) //XBT
            {
                m_UnknownFileName = m_Directory + @"\XBT\" + fileName + ".xbt";
            }
            else
            if (dwID == 0x4643626E || dwID == 0x00000004 || dwID == 0x00000023) //nbCF
            {
                m_UnknownFileName = m_Directory + @"\FCB\" + fileName + ".fcb";
            }
            else
            if (dwID == 0x78647064) //dpdx
            {
                m_UnknownFileName = m_Directory + @"\DPDX\" + fileName + ".dpdx";
            }
            else
            if (dwID == 0x4341554C) //LUAC
            {
                m_UnknownFileName = m_Directory + @"\LUA\" + fileName + ".lua";
            }
            else
            if (dwID == 0x5161754C) //LuaQ
            {
                m_UnknownFileName = m_Directory + @"\LUA\" + fileName + ".lua";
            }
            else
            if (dwID == 0x3CBFBBEF || dwID == 0x6D783F3C || dwID == 0x003CFEFF || dwID == 0x6172673C) //XML, //<graphics
            {
                m_UnknownFileName = m_Directory + @"\XML\" + fileName + ".xml";
            }
            else
            if (dwID == 0x6E69423C) //<binary
            {
                m_UnknownFileName = m_Directory + @"\BINXML\" + fileName + ".xml";
            }
            else
            if (dwID == 0x54425043) //CPBT
            {
                m_UnknownFileName = m_Directory + @"\CPBT\" + fileName + ".cpubt";
            }
            else
            if (dwID == 0xE9001052) //SDAT
            {
                m_UnknownFileName = m_Directory + @"\SDAT\" + fileName + ".sdat";
            }
            else
            if (dwID == 0x000000B0 || dwID == 0x000000B6) //MAB
            {
                m_UnknownFileName = m_Directory + @"\MAB\" + fileName + ".mab";
            }
            else
            if (dwID == 0x01) //WSECBDL
            {
                m_UnknownFileName = m_Directory + @"\WSECBDL\" + fileName + ".wsecbdl";
            }
            else
            if (dwID == 0x694B4942 || dwID == 0x6732424B || dwID == 0x6A32424B) //BIKi //KB2g // KB2j
            {
                m_UnknownFileName = m_Directory + @"\BIK\" + fileName + ".bik";
            }
            else
            if (dwID == 0x00000032 || dwID == 0x00000036) //hkx
            {
                m_UnknownFileName = m_Directory + @"\HKX\" + fileName + ".hkx";
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

            List<string> notCompress = new();

            string excludeFiles = LoadSetting("CompressExcludeFiles");
            if (excludeFiles != "" && excludeFilesFromCompress == "")
                notCompress.AddRange(excludeFiles.Split(','));

            if (excludeFilesFromCompress != "")
                notCompress.AddRange(excludeFilesFromCompress.Split(','));

            notCompress = notCompress.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            if (isCompressEnabled)
                Console.WriteLine("Excluded extensions from compressing: " + String.Join(", ", notCompress.ToArray()));

            string fatFile = outputFile;
            string datFile = fatFile.Replace(".fat", ".dat");

            string[] allFiles = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);

            SortedDictionary<ulong, FatEntry> Entries = new SortedDictionary<ulong, FatEntry>();

            var outputDat = File.Open(datFile, FileMode.OpenOrCreate);
            outputDat.SetLength(0);

            int cnt = 0;
            foreach (string file in allFiles)
            {
                cnt++;

                string fatFileName = file.Replace(sourceDir + "\\", "");
                string extension = Path.GetExtension(fatFileName);

                if (excludeFilesFromPack != "" && excludeFilesFromPack.Contains(extension)) continue;

                byte[] bytes = File.ReadAllBytes(file);

                FatEntry entry = new FatEntry();

                byte[] outputBytes;

                if (isCompressEnabled && !notCompress.Contains(extension))
                {
                    byte[] tmp = new byte[LZ4Codec.MaximumOutputSize(bytes.Length)];
                    int compressedSize = LZ4Codec.Encode(bytes, tmp, LZ4Level.L00_FAST);
                    outputBytes = new byte[compressedSize];
                    Array.Copy(tmp, outputBytes, compressedSize);

                    entry.CompressionScheme = CompressionScheme.LZ4;
                    entry.UncompressedSize = (uint)bytes.Length;
                }
                else
                {
                    outputBytes = bytes;

                    entry.CompressionScheme = CompressionScheme.None;

                    if (dwVersion == 10 || dwVersion == 11)
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

                Console.WriteLine($"[Packing {cnt} / {allFiles.Length}]: {fatFileName}");
            }

            outputDat.Flush();
            outputDat.Close();

            // versions
            // 11 - FC6
            // 10 - FC5, FCND
            // 9 - FC3, FC3BD, FC4
            // 5 - FC2

            var output = File.Create(fatFile);
            output.WriteValueU32(0x46415432, 0);
            output.WriteValueS32(dwVersion, 0);

            output.WriteByte(1);

            if (dwVersion >= 10)
                output.WriteByte(0);

            if (dwVersion <= 9)
                output.WriteByte(3);

            output.WriteValueU16(0);

            if (dwVersion >= 9)
            {
                output.WriteValueS32(0, 0); // dwSubfatTotalEntryCount
                output.WriteValueS32(0, 0); // dwSubfatCount
            }

            output.WriteValueS32(Entries.Count, 0);

            foreach (ulong entryE in Entries.Keys)
            {
                var fatEntry = Entries[entryE];

                if (dwVersion == 11)
                {
                    uint dwHash = (uint)((fatEntry.NameHash & 0xFFFFFFFF00000000ul) >> 32);
                    uint dwHash2 = (uint)((fatEntry.NameHash & 0x00000000FFFFFFFFul) >> 0);

                    uint dwUncompressedSize = 0u;
                    dwUncompressedSize = (uint)((int)dwUncompressedSize | ((int)(fatEntry.UncompressedSize << 2) & -4));
                    dwUncompressedSize = (uint)((int)dwUncompressedSize | (int)((int)fatEntry.CompressionScheme & 3L));

                    uint dwUnresolvedOffset = (uint)(((fatEntry.Offset >> 4) & 0x7FFFFFFF8) >> 3);

                    uint dwCompressedSize = 0u;
                    dwCompressedSize = (uint)((int)dwCompressedSize | (int)((fatEntry.Offset >> 4) << 29));
                    dwCompressedSize |= (fatEntry.CompressedSize & 0x1FFFFFFF);

                    output.WriteValueU32(dwHash, 0);
                    output.WriteValueU32(dwHash2, 0);
                    output.WriteValueU32(dwUncompressedSize, 0);
                    output.WriteValueU32(dwUnresolvedOffset, 0);
                    output.WriteValueU32(dwCompressedSize, 0);
                }
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
            // 11 - FC6
            // 10 - FC5, FCND
            // 9 - FC3, FC3BD, FC4
            // 5 - FC2
            if (dwVersion != 11 && dwVersion != 10 && dwVersion != 9 && dwVersion != 5)
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
            if (unknown1Count > 0)
                throw new NotSupportedException();
            /*for (uint i = 0; i < unknown1Count; i++)
            {
                throw new NotSupportedException();
                TFATReader.ReadBytes(16);
            }*/

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

            if (dwVersion == 11 || dwVersion == 10 || dwVersion == 9)
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

            if (dwVersion == 11)
            {
                dwFlag = dwUncompressedSize & 3;
                dwOffset = ((ulong)dwCompressedSize >> 29 | (ulong)dwUnresolvedOffset << 3) << 4; // thx to ミルクティー (miru)
                dwCompressedSize = (dwCompressedSize & 0x1FFFFFFF);
                dwUncompressedSize = (dwUncompressedSize >> 2);
            }
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

                /*dwUncompressedSize = ((dwUncompressedSize >> 2) & 0x3FFFFFFFu);
                dwFlag = (byte)((dwUncompressedSize >> 0) & 0x3u);
                dwOffset = ((ulong)(dwUnresolvedOffset << 2) | ((dwCompressedSize >> 30) & 0x3u));
                dwCompressedSize = (uint)((dwCompressedSize >> 0) & 0x3FFFFFFFul);*/

                /*dwFlag = dwUncompressedSize & 3;
                dwUncompressedSize = dwUncompressedSize >> 2;
                dwOffset = (ulong)(dwUnresolvedOffset * 4L) + (dwCompressedSize >> 30);
                dwCompressedSize = dwCompressedSize & 0x3FFFFFFF;*/
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

        static void BNKExtract(string file)
        {
            HashSet<uint> gameProjIDs = new();

            string[] soundsIDs = File.ReadAllLines(m_Path + "\\SoundDataIDs.bin");

            foreach (var id in soundsIDs)
                gameProjIDs.Add(uint.Parse(id));

            // https://wiki.xentax.com/index.php/Wwise_SoundBank_(*.bnk)

            List<uint> wemIDs = new List<uint>();
            List<uint> wemOffsets = new List<uint>();
            List<uint> wemLengths = new List<uint>();

            string onlyDir = Path.GetDirectoryName(file);
            string fileName = Path.GetFileNameWithoutExtension(file);
            string fileNameXml = file + ".converted.xml";

            XDocument xmlDoc = new(new XDeclaration("1.0", "utf-8", "yes"));
            xmlDoc.Add(new XComment(xmlheader));
            xmlDoc.Add(new XComment(xmlheaderbnk));
            XElement rootXml = new("SoundBank");

            FileStream BNKStream = new(file, FileMode.Open);

            uint bnkVersion = 0;

            do
            {
                uint sectionName = BNKStream.ReadValueU32();
                uint sectionLength = BNKStream.ReadValueU32();
                long pos = BNKStream.Position;

                XElement xSec = new(BNKGetKnownName(sectionName, BNKNames.sectionsNames, "Section"));

                if (sectionName == 0x44484B42) // BKHD
                {
                    bnkVersion = BNKStream.ReadValueU32();
                    uint bnkID = BNKStream.ReadValueU32();

                    uint btsRe = sectionLength - (sizeof(uint) * 2);
                    byte[] unknown = BNKStream.ReadBytes((int)btsRe);

                    xSec.Add(new XAttribute("Version", bnkVersion));
                    xSec.Add(new XAttribute("SoundBankID", bnkID));
                    xSec.Add(new XElement("Binary", Helpers.ByteArrayToString(unknown)));
                }
                else if (sectionName == 0x58444944) // DIDX
                {
                    uint filesCount = sectionLength / 12;

                    for (int i = 0; i < filesCount; i++)
                    {
                        uint wemID = BNKStream.ReadValueU32();
                        uint wemOffset = BNKStream.ReadValueU32();
                        uint wemLength = BNKStream.ReadValueU32();

                        wemIDs.Add(wemID);
                        wemOffsets.Add(wemOffset);
                        wemLengths.Add(wemLength);

                        XElement objFile = new("WEMFile");
                        objFile.Add(new XAttribute("ID", wemID.ToString()));
                        objFile.Add(new XAttribute("Offset", wemOffset.ToString()));
                        objFile.Add(new XAttribute("Length", wemLength.ToString()));
                        xSec.Add(objFile);

                        gameProjIDs.Add(wemID);
                    }
                }
                else if (sectionName == 0x41544144) // DATA
                {
                    long dataPosStart = BNKStream.Position;

                    for (int i = 0; i < wemIDs.Count(); i++)
                    {
                        long offset = dataPosStart + wemOffsets[i];
                        BNKStream.Seek(offset, SeekOrigin.Begin);

                        byte[] wem = BNKStream.ReadBytes((int)wemLengths[i]);
                        string wemFN = fileName + "_" + i.ToString() + "_" + wemIDs[i] + ".wem";
                        string wemFileName = onlyDir + "\\" + wemFN;
                        string wavFileName = onlyDir + "\\" + fileName + "_" + i.ToString() + "_" + wemIDs[i] + ".ogg";

                        File.WriteAllBytes(wemFileName, wem);

                        WEMToOGG(wemFileName, wavFileName);

                        XElement objFile = new("WEMFile");
                        objFile.Add(new XAttribute("ID", wemIDs[i]));
                        objFile.Add(new XAttribute("FileName", wemFN));
                        xSec.Add(objFile);
                    }
                }
                else if (sectionName == 0x43524948) // HIRC
                {
                    uint objectsCount = BNKStream.ReadValueU32();

                    List<Tuple<string, uint, byte[]>> data = new();

                    for (int i = 0; i < objectsCount; i++)
                    {
                        byte type = (byte)BNKStream.ReadByte();
                        uint objectLength = BNKStream.ReadValueU32() - 4;
                        uint objectID = BNKStream.ReadValueU32();

                        data.Add(new(BNKGetKnownName(type, BNKNames.hircObjects, "Object"), objectID, BNKStream.ReadBytes((int)objectLength)));

                        gameProjIDs.Add(objectID);

                        /*
                        XElement xObj = new(BNKGetKnownName(type, BNKNames.hircObjects, "Object"));
                        xObj.Add(new XAttribute("ObjectID", objectID.ToString()));

                        if (type == 0x01) // 1 Settings
                        {
                            ushort countSettings = BNKStream.ReadValueU16();

                            List<ushort> settings = new List<ushort>();
                            for (int j = 0; j < countSettings; j++)
                            {
                                ushort typeSetting = BNKStream.ReadValueU16();
                                settings.Add(typeSetting);
                            }

                            for (int j = 0; j < countSettings; j++)
                            {
                                float settingVal = BNKStream.ReadValueF32();

                                XElement xSett = new(BNKGetKnownName(settings[j], BNKNames.eventActionSettings, "Setting"), settingVal.ToString(CultureInfo.InvariantCulture));
                                xObj.Add(xSett);
                            }
                        }
                        else if (type == 0x02) // 2 Sound SFX/Sound Voice
                        {
                            long posT = BNKStream.Position;

                            byte unknown1 = (byte)BNKStream.ReadByte();
                            byte unknown2 = (byte)BNKStream.ReadByte();
                            byte unknown3 = (byte)BNKStream.ReadByte();
                            byte unknown4 = (byte)BNKStream.ReadByte();
                            byte included = (byte)BNKStream.ReadByte();

                            xObj.Add(new XAttribute("Unknown1", unknown1));
                            xObj.Add(new XAttribute("Unknown2", unknown2));
                            xObj.Add(new XAttribute("Unknown3", unknown3));
                            xObj.Add(new XAttribute("Unknown4", unknown4));
                            xObj.Add(new XAttribute("Stream", BNKGetKnownName(included, BNKNames.eventActionIncluded, "Included")));

                            if (unknown1 == 2)
                            {
                                uint unknownID = BNKStream.ReadValueU32();
                                xObj.Add(new XAttribute("UnknownID", unknownID));
                            }

                            uint wemID = BNKStream.ReadValueU32();
                            uint wemSize = BNKStream.ReadValueU32();
                            byte soundType = (byte)BNKStream.ReadByte();
                            byte overrideParentEffects = (byte)BNKStream.ReadByte();
                            byte effectsCount = (byte)BNKStream.ReadByte();

                            xObj.Add(new XAttribute("WemID", wemID));
                            xObj.Add(new XAttribute("WemSize", wemSize));
                            xObj.Add(new XAttribute("SoundType", BNKGetKnownName(soundType, BNKNames.eventActionSoundType, "SoundType")));

                            XElement xEffects = new("Effects");

                            xEffects.Add(new XAttribute("OverrideParentEffects", overrideParentEffects.ToString()));
                            if (effectsCount > 0)
                            {
                                byte bitMaskEffectBypass = (byte)BNKStream.ReadByte();
                                xEffects.Add(new XAttribute("bitMaskEffectBypass", bitMaskEffectBypass.ToString()));
                            }

                            for (int j = 0; j < effectsCount; j++)
                            {
                                byte effectIdx = (byte)BNKStream.ReadByte();
                                uint effectObjID = BNKStream.ReadValueU32();
                                byte unkEff1 = (byte)BNKStream.ReadByte();
                                byte unkEff2 = (byte)BNKStream.ReadByte();

                                XElement xEffect = new("Effect");
                                xEffect.Add(new XAttribute("EffectIndex", effectIdx.ToString()));
                                xEffect.Add(new XAttribute("EffectObjectID", effectObjID.ToString()));
                                xEffect.Add(new XAttribute("Unknown1", unkEff1.ToString()));
                                xEffect.Add(new XAttribute("Unknown2", unkEff2.ToString()));
                                xEffects.Add(xEffect);
                            }
                            xObj.Add(xEffects);

                            BNKStream.ReadByte(); // always zero

                            uint outputBusID = BNKStream.ReadValueU32();
                            uint parentObjID = BNKStream.ReadValueU32();

                            byte overrideParentPlaybackPriority = (byte)BNKStream.ReadByte();
                            byte additionalParams = (byte)BNKStream.ReadByte();

                            xObj.Add(new XAttribute("OutputBusID", outputBusID.ToString()));
                            xObj.Add(new XAttribute("ParentObjID", parentObjID.ToString()));
                            xObj.Add(new XAttribute("OverrideParentPlaybackPriority", overrideParentPlaybackPriority.ToString()));

                            XElement xAddParams = new("AdditionalParameters");
                            List<byte> addParams = new();
                            for (int j = 0; j < additionalParams; j++)
                            {
                                byte addParamType = (byte)BNKStream.ReadByte();
                                addParams.Add(addParamType);
                            }
                            for (int j = 0; j < additionalParams; j++)
                            {
                                string pname = BNKGetKnownName(addParams[j], BNKNames.eventActionAddsParams, "Param");

                                if (addParams[j] == 0x3A)
                                {
                                    uint addParamVal = BNKStream.ReadValueU32();
                                    xAddParams.Add(new XElement(pname, addParamVal.ToString()));
                                }
                                else
                                {
                                    float addParamVal = BNKStream.ReadValueF32();
                                    xAddParams.Add(new XElement(pname, addParamVal.ToString(CultureInfo.InvariantCulture)));
                                }
                            }
                            xObj.Add(xAddParams);

                            // -------------------------------------------------------------------------------------------------------------------------------------

                            //BNKStream.ReadByte(); // always zero
                            byte unknownParams = (byte)BNKStream.ReadByte();

                            XElement xUnkParams = new("UnknownRangeParameters");
                            List<byte> aunkParams = new();
                            for (int j = 0; j < unknownParams; j++)
                            {
                                byte unkParamType = (byte)BNKStream.ReadByte();
                                aunkParams.Add(unkParamType);
                            }
                            for (int j = 0; j < unknownParams; j++)
                            {
                                string pname = BNKGetKnownName(aunkParams[j], BNKNames.eventActionAddsParams, "Param");

                                XElement unElm = new XElement(pname);
                                float unkParamVal1 = BNKStream.ReadValueF32();
                                float unkParamVal2 = BNKStream.ReadValueF32();
                                unElm.Add(new XAttribute("Min", unkParamVal1.ToString(CultureInfo.InvariantCulture)));
                                unElm.Add(new XAttribute("Max", unkParamVal2.ToString(CultureInfo.InvariantCulture)));
                                xUnkParams.Add(unElm);
                            }
                            xObj.Add(xUnkParams);

                            XElement xPositioning = new("Positioning");

                            byte positioningType = (byte)BNKStream.ReadByte();
                            xPositioning.Add(new XAttribute("Type", positioningType.ToString()));

                            // maybe ((positioningType % 7) == 3)      && positioningType != 199 && positioningType != 192
                            if (positioningType > 15 && (positioningType % 7) != 3 && positioningType != 195)
                            {
                                byte positioningSettings = (byte)BNKStream.ReadByte();
                                xPositioning.Add(new XAttribute("Settings", positioningSettings.ToString()));

                                uint attenuationID = BNKStream.ReadValueU32();
                                xPositioning.Add(new XAttribute("AttenuationID", attenuationID.ToString()));
                            }

                            xObj.Add(xPositioning);

                            XElement xAuxiliary = new("Auxiliary");

                            byte auxSettings = (byte)BNKStream.ReadByte();
                            xAuxiliary.Add(new XAttribute("Settings", auxSettings.ToString()));

                            if (auxSettings > 7)
                            {
                                uint auxBus1 = BNKStream.ReadValueU32();
                                uint auxBus2 = BNKStream.ReadValueU32();
                                uint auxBus3 = BNKStream.ReadValueU32();
                                uint auxBus4 = BNKStream.ReadValueU32();

                                XElement usrDfnAuxSnds = new("UserDefinedAuxiliarySends");
                                usrDfnAuxSnds.Add(new XElement("AuxiliaryBus1", auxBus1.ToString()));
                                usrDfnAuxSnds.Add(new XElement("AuxiliaryBus2", auxBus2.ToString()));
                                usrDfnAuxSnds.Add(new XElement("AuxiliaryBus3", auxBus3.ToString()));
                                usrDfnAuxSnds.Add(new XElement("AuxiliaryBus4", auxBus4.ToString()));
                                xAuxiliary.Add(usrDfnAuxSnds);
                            }

                            xObj.Add(xAuxiliary);

                            /*byte positioningType = (byte)BNKStream.ReadByte();
                            byte positioningSettings = (byte)BNKStream.ReadByte();

                            xObj.Add(new XAttribute("PositioningType", positioningType.ToString()));
                            xObj.Add(new XAttribute("PositioningSettings", positioningSettings.ToString()));

                            if (positioningSettings > 3 || (positioningSettings == 0x01 && bnkVersion == 120))
                            {
                                byte auxSettings = 0;

                                if ((positioningSettings == 0x0F && bnkVersion == 128))
                                {
                                }
                                else
                                {
                                    uint attenuationID = BNKStream.ReadValueU32();
                                    xObj.Add(new XAttribute("AttenuationID", attenuationID.ToString()));

                                    auxSettings = (byte)BNKStream.ReadByte();
                                    xObj.Add(new XAttribute("AuxiliarySettings", auxSettings.ToString()));
                                }

                                if (auxSettings > 7 || (positioningSettings == 0x0F && bnkVersion == 128))
                                {
                                    uint auxBus1 = BNKStream.ReadValueU32();
                                    uint auxBus2 = BNKStream.ReadValueU32();
                                    uint auxBus3 = BNKStream.ReadValueU32();
                                    uint auxBus4 = BNKStream.ReadValueU32();

                                    XElement usrDfnAuxSnds = new XElement("UserDefinedAuxiliarySends");
                                    usrDfnAuxSnds.Add(new XElement("AuxiliaryBus1", auxBus1.ToString()));
                                    usrDfnAuxSnds.Add(new XElement("AuxiliaryBus2", auxBus2.ToString()));
                                    usrDfnAuxSnds.Add(new XElement("AuxiliaryBus3", auxBus3.ToString()));
                                    usrDfnAuxSnds.Add(new XElement("AuxiliaryBus4", auxBus4.ToString()));
                                    xObj.Add(usrDfnAuxSnds);
                                }
                            }*

                            byte playbackSettings = (byte)BNKStream.ReadByte();
                            byte onRetPhysVoice = (byte)BNKStream.ReadByte();
                            byte limitSndInst = (byte)BNKStream.ReadByte();
                            byte posUnknown2 = (byte)BNKStream.ReadByte();
                            byte virtualVoiceBehav = (byte)BNKStream.ReadByte();

                            xObj.Add(new XAttribute("PlaybackSettings", playbackSettings.ToString()));
                            xObj.Add(new XAttribute("OnReturnToPhysVoice", onRetPhysVoice.ToString()));
                            xObj.Add(new XAttribute("LimitSoundInstances", limitSndInst.ToString()));
                            xObj.Add(new XAttribute("PosUnknown", posUnknown2.ToString()));
                            xObj.Add(new XAttribute("VirtualVoiceBehavior", virtualVoiceBehav.ToString()));

                            BNKStream.ReadByte(); // always zero
                            if (bnkVersion == 120)
                            {
                                BNKStream.ReadByte();
                                BNKStream.ReadByte();
                            }

                            byte statesPropsCnt = (byte)BNKStream.ReadByte();

                            XElement statesProps = new("StatesProperties");
                            for (int j = 0; j < statesPropsCnt; j++)
                            {
                                byte propSettName = (byte)BNKStream.ReadByte();
                                byte propUnk1 = (byte)BNKStream.ReadByte();
                                byte propUnk2 = (byte)BNKStream.ReadByte();

                                XElement stPrp = new("Property");
                                stPrp.Add(new XAttribute("SettingName", BNKGetKnownName(propSettName, BNKNames.eventActionSettings, "Setting")));
                                stPrp.Add(new XAttribute("Unknown1", propUnk1.ToString()));
                                stPrp.Add(new XAttribute("Unknown2", propUnk2.ToString()));
                                statesProps.Add(stPrp);
                            }
                            xObj.Add(statesProps);

                            byte stateGroupsCnt = (byte)BNKStream.ReadByte();

                            XElement statesGrps = new("StateGroups");
                            for (int j = 0; j < stateGroupsCnt; j++)
                            {
                                uint stateGroupID = BNKStream.ReadValueU32();
                                byte changeOccursAt = (byte)BNKStream.ReadByte();

                                XElement stGrp = new("StateGroup");
                                stGrp.Add(new XAttribute("ID", stateGroupID.ToString()));
                                stGrp.Add(new XAttribute("ChangeOccursAt", changeOccursAt.ToString()));

                                byte settsDiffCnt = (byte)BNKStream.ReadByte();

                                XElement settDiffs = new("SettingsDiffs");
                                for (int k = 0; k < settsDiffCnt; k++)
                                {
                                    uint stateObjID = BNKStream.ReadValueU32();
                                    uint settingsID = BNKStream.ReadValueU32();

                                    XElement stGrpS = new("Diff");
                                    stGrpS.Add(new XAttribute("StateObjID", stateObjID.ToString()));
                                    stGrpS.Add(new XAttribute("SettingsID", settingsID.ToString()));
                                    settDiffs.Add(stGrpS);
                                }
                                stGrp.Add(settDiffs);

                                statesGrps.Add(stGrp);
                            }
                            xObj.Add(statesGrps);

                            ushort rtcpCnt = BNKStream.ReadValueU16();

                            XElement rtpcs = new("RTPCs");
                            for (int j = 0; j < rtcpCnt; j++)
                            {
                                uint gameParamID = BNKStream.ReadValueU32();
                                byte rtcpUnk1 = (byte)BNKStream.ReadByte();
                                byte rtcpUnk2 = (byte)BNKStream.ReadByte();
                                byte axisType = (byte)BNKStream.ReadByte();
                                uint rtcpUnkID = BNKStream.ReadValueU32();
                                byte rtcpUnk3 = (byte)BNKStream.ReadByte();
                                byte pointsCnt = (byte)BNKStream.ReadByte();
                                byte rtcpUnk4 = (byte)BNKStream.ReadByte();

                                XElement rtpc = new("RTPC");
                                rtpc.Add(new XAttribute("GameParamID", gameParamID.ToString()));
                                rtpc.Add(new XAttribute("AxisType", BNKGetKnownName(axisType, BNKNames.eventActionSettings, "Type")));
                                rtpc.Add(new XAttribute("Unknown1", rtcpUnk1.ToString()));
                                rtpc.Add(new XAttribute("Unknown2", rtcpUnk2.ToString()));
                                rtpc.Add(new XAttribute("UnknownID", rtcpUnkID.ToString()));
                                rtpc.Add(new XAttribute("Unknown3", rtcpUnk3.ToString()));
                                rtpc.Add(new XAttribute("Unknown4", rtcpUnk4.ToString()));

                                XElement rtpcPoints = new("Points");
                                for (int k = 0; k < pointsCnt; k++)
                                {
                                    float posX = BNKStream.ReadValueF32();
                                    float posY = BNKStream.ReadValueF32();
                                    uint shapeCurve = BNKStream.ReadValueU32();

                                    XElement rtpcPoint = new("Point");
                                    rtpcPoint.Add(new XAttribute("PosX", posX.ToString(CultureInfo.InvariantCulture)));
                                    rtpcPoint.Add(new XAttribute("PosY", posY.ToString(CultureInfo.InvariantCulture)));
                                    rtpcPoint.Add(new XAttribute("Shape", BNKGetKnownName(shapeCurve, BNKNames.rtpcShape, "Shape")));
                                    rtpcPoints.Add(rtpcPoint);
                                }
                                rtpc.Add(rtpcPoints);

                                rtpcs.Add(rtpc);
                            }
                            xObj.Add(rtpcs);

                            posT = posT + objectLength - BNKStream.Position;
                            byte[] soundStruct = BNKStream.ReadBytes((int)posT);

                            if (soundStruct.Length > 0)
                                xObj.Add(new XElement("Binary", Helpers.ByteArrayToString(soundStruct).ToUpper()));
                        }
                        else if (type == 0x03) // 3 Event Action
                        {
                            byte scope = (byte)BNKStream.ReadByte();
                            byte actionType = (byte)BNKStream.ReadByte();
                            uint refGameObjID = BNKStream.ReadValueU32();
                            BNKStream.ReadByte(); // always zero
                            byte additionalParamsLen = (byte)BNKStream.ReadByte();

                            xObj.Add(new XAttribute("Scope", BNKGetKnownName(scope, BNKNames.eventActionScopes, "Scope")));
                            xObj.Add(new XAttribute("ActionType", BNKGetKnownName(actionType, BNKNames.eventActionTypes, "ActionType")));
                            xObj.Add(new XAttribute("ReferenceID", refGameObjID.ToString()));

                            XElement xParams = new("Params");

                            List<byte> paramTypes = new();
                            for (int j = 0; j < additionalParamsLen; j++)
                            {
                                byte paramType = (byte)BNKStream.ReadByte();
                                paramTypes.Add(paramType);
                            }
                            for (int j = 0; j < additionalParamsLen; j++)
                            {
                                //if (paramTypes[j] == 0x0E || paramTypes[j] == 0x0F || paramTypes[j] == 0x10)
                                //{
                                //}
                                uint paramVal = BNKStream.ReadValueU32();
                                xParams.Add(new XElement(BNKGetKnownName(paramTypes[j], BNKNames.eventActionParams, "Param"), paramVal.ToString()));
                            }

                            xObj.Add(xParams);


                            byte nextParams = (byte)BNKStream.ReadByte(); // should be always zero

                            XElement xNextParams = new("NextParams");
                            paramTypes = new();
                            for (int j = 0; j < nextParams; j++)
                            {
                                byte paramType = (byte)BNKStream.ReadByte();
                                paramTypes.Add(paramType);
                            }
                            for (int j = 0; j < nextParams; j++)
                            {
                                int paramVal1 = BNKStream.ReadValueS32();
                                uint paramVal2 = BNKStream.ReadValueU32();
                                XElement xNPVal = new(BNKGetKnownName(paramTypes[j], BNKNames.eventActionParams, "Param"));
                                xNPVal.Add(new XAttribute("Value1", paramVal1.ToString()));
                                xNPVal.Add(new XAttribute("Value2", paramVal2.ToString()));
                                xNextParams.Add(xNPVal);
                            }
                            xObj.Add(xNextParams);


                            if (actionType != 0x1C && actionType != 0x21)
                            {
                                byte unknown = (byte)BNKStream.ReadByte();
                                xObj.Add(new XElement("Unknown", unknown.ToString()));

                                if (actionType == 0x01 || actionType == 0x04) // Stop Play
                                {
                                    uint soundBankID = BNKStream.ReadValueU32();
                                    xObj.Add(new XElement("SoundBankID", soundBankID.ToString()));
                                }
                                if (actionType == 0x02) // Pause
                                {
                                    byte[] objData = BNKStream.ReadBytes(15);
                                    xObj.Add(new XElement("Binary", Helpers.ByteArrayToString(objData).ToUpper()));
                                }
                                if (actionType == 0x06 || actionType == 0x07) // Mute UnMute
                                {
                                    byte[] objData = BNKStream.ReadBytes(1);
                                    xObj.Add(new XElement("Binary", Helpers.ByteArrayToString(objData).ToUpper()));
                                }
                                if (actionType == 0x12) // SetState
                                {
                                    byte[] objData = BNKStream.ReadBytes(7);
                                    xObj.Add(new XElement("Binary", Helpers.ByteArrayToString(objData).ToUpper()));
                                }
                                if (actionType == 0x1E) // Seek
                                {
                                    byte[] objData = BNKStream.ReadBytes(17);
                                    xObj.Add(new XElement("Binary", Helpers.ByteArrayToString(objData).ToUpper()));
                                }
                                if (actionType == 0x13 || actionType == 0x14) // SetGameParameter ResetGameParameter
                                {
                                    byte[] objData = BNKStream.ReadBytes(18);
                                    xObj.Add(new XElement("Binary", Helpers.ByteArrayToString(objData).ToUpper()));
                                }

                                if (bnkVersion == 120)
                                {
                                    if (actionType == 0x0B || actionType == 0x0A) // ResetVoiceVolume SetVoiceVolume
                                    {
                                        byte[] objData = BNKStream.ReadBytes(17);
                                        xObj.Add(new XElement("Binary", Helpers.ByteArrayToString(objData).ToUpper()));
                                    }
                                }
                                if (bnkVersion == 128)
                                {
                                    if (actionType == 0x0B || actionType == 0x0A) // ResetVoiceVolume SetVoiceVolume
                                    {
                                        byte[] objData = BNKStream.ReadBytes(14);
                                        xObj.Add(new XElement("Binary", Helpers.ByteArrayToString(objData).ToUpper()));
                                    }
                                }
                            }
                        }
                        else if (type == 0x04) // 4 Event
                        {
                            byte countEvents = (byte)BNKStream.ReadByte();

                            // ver 120 3 bytes space
                            // ver 128 no space
                            if (bnkVersion == 120)
                                BNKStream.ReadBytes(3);

                            for (int j = 0; j < countEvents; j++)
                            {
                                uint evID = BNKStream.ReadValueU32();
                                xObj.Add(new XElement("EventActionID", evID));
                            }
                        }
                        else
                        {
                            byte[] objData = BNKStream.ReadBytes((int)objectLength);

                            xObj.Add(new XElement("Binary", Helpers.ByteArrayToString(objData).ToUpper()));
                        }

                        xSec.Add(xObj);*/
                    }
                    
                    foreach (var d in data)
                    {
                        XElement xObj = new(d.Item1);
                        xObj.Add(new XAttribute("ObjectID", d.Item2.ToString()));

                        byte[] objData = d.Item3;

                        xObj.Add(new XElement("Binary", Helpers.ByteArrayToString(objData).ToUpper()));

                        List<uint> added = new();

                        XElement pv = new("PossibleIDs");
                        for (int j = 0; j < objData.Length - 3; j++)
                        {
                            byte[] arr = new byte[4];
                            Buffer.BlockCopy(objData, j, arr, 0, 4);
                            uint possibleVal = BitConverter.ToUInt32(arr, 0);
                            if (gameProjIDs.Contains(possibleVal) && !added.Contains(possibleVal))
                            {
                                pv.Add(new XElement("ID", new XAttribute("OrigHex", Helpers.ByteArrayToString(arr)), new XAttribute("UInt32", possibleVal.ToString())));
                                added.Add(possibleVal);
                            }
                        }
                        xObj.Add(pv);

                        xSec.Add(xObj);
                    }
                }
                else // every other sections - skip them
                {
                    byte[] section = BNKStream.ReadBytes((int)sectionLength);

                    xSec.Add(new XElement("Binary", Helpers.ByteArrayToString(section).ToUpper()));
                }

                rootXml.Add(xSec);

                BNKStream.Seek(pos + sectionLength, SeekOrigin.Begin);
            }
            while (BNKStream.Position < BNKStream.Length);

            BNKStream.Close();

            xmlDoc.Add(rootXml);
            xmlDoc.Save(fileNameXml);
        }

        static void BNKPack(string file)
        {
            string newName = file.Replace(".bnk.converted.xml", "_new.bnk");
            string onlyDir = Path.GetDirectoryName(file);

            Dictionary<uint, uint> wemLens = new();
            List<long> wemPos = new();

            FileStream BNKStream = File.Create(newName);

            XDocument xDoc = XDocument.Load(file);
            XElement xRoot = xDoc.Element("SoundBank");

            int? dataLen = xRoot.Element("DATA_Data")?.Elements().Count();

            uint bnkVersion = 0;

            foreach (XElement xSection in xRoot.Elements())
            {
                uint sectionName = BNKGetValFromKnownName(xSection.Name.ToString(), BNKNames.sectionsNames, "Section");

                BNKStream.WriteValueU32(sectionName);
                BNKStream.WriteValueU32(0); // will be changed later

                long sectionStartPos = BNKStream.Position;

                if (sectionName == 0x44484B42) // BKHD
                {
                    bnkVersion = uint.Parse(xSection.Attribute("Version").Value);
                    uint bnkID = uint.Parse(xSection.Attribute("SoundBankID").Value);
                    byte[] unknown = Helpers.StringToByteArray(xSection.Element("Binary").Value);

                    BNKStream.WriteValueU32(bnkVersion);
                    BNKStream.WriteValueU32(bnkID);
                    BNKStream.WriteBytes(unknown);
                }
                else if (sectionName == 0x58444944) // DIDX
                {
                    // this section can be empty in XML because it can be / must be calculated again

                    for (int i = 0; i < dataLen; i++)
                    {
                        wemPos.Add(BNKStream.Position);
                        BNKStream.WriteValueU32(0); // will be changed later
                        BNKStream.WriteValueU32(0); // will be changed later
                        BNKStream.WriteValueU32(0); // will be changed later
                    }
                }
                else if (sectionName == 0x41544144) // DATA
                {
                    IEnumerable<XElement> WEMFiles = xSection.Elements("WEMFile");
                    uint startPos = (uint)BNKStream.Position;
                    int i = 0;

                    foreach (XElement WEMFile in WEMFiles)
                    {
                        string fileName = WEMFile.Attribute("FileName").Value;
                        byte[] wemFile = File.ReadAllBytes(onlyDir + "\\" + fileName);
                        uint offset = (uint)BNKStream.Position - startPos;

                        uint wemID = uint.Parse(WEMFile.Attribute("ID").Value);
                        uint wemLength = (uint)wemFile.Length;

                        wemLens.Add(wemID, wemLength);

                        BNKStream.WriteBytes(wemFile);

                        // padding 16 bytes
                        if (i < dataLen - 1)
                            BNKStream.Seek(BNKStream.Position + BNKStream.Position.Padding(16), SeekOrigin.Begin);

                        long cPos = BNKStream.Position;
                        BNKStream.Seek(wemPos[i], SeekOrigin.Begin);
                        BNKStream.WriteValueU32(wemID);
                        BNKStream.WriteValueU32(offset);
                        BNKStream.WriteValueU32(wemLength);
                        BNKStream.Seek(cPos, SeekOrigin.Begin);

                        i++;
                    }
                }
                else if (sectionName == 0x43524948) // HIRC
                {
                    uint objectsCount = (uint)xSection.Elements().Count();
                    BNKStream.WriteValueU32(objectsCount);

                    foreach (XElement obj in xSection.Elements())
                    {
                        byte type = (byte)BNKGetValFromKnownName(obj.Name.ToString(), BNKNames.hircObjects, "Object");
                        BNKStream.WriteByte(type);

                        long objStartPos = BNKStream.Position;

                        BNKStream.WriteValueU32(0); // will be changed later
                        BNKStream.WriteValueU32(uint.Parse(obj.Attribute("ObjectID").Value));

                        /*if (type == 0x01) // 1 Settings
                        {
                            IEnumerable<XElement> settings = obj.Elements();

                            BNKStream.WriteValueU16((ushort)settings.Count());

                            foreach (XElement sett in settings)
                            {
                                string settName = sett.Name.ToString();
                                byte settingType = 0;

                                if (settName.StartsWith("UnknownSetting"))
                                {
                                    settName = settName.Replace("UnknownSetting", "");
                                    settingType = byte.Parse(settName);
                                }
                                else
                                    settingType = (byte)BNKGetValFromKnownName(settName, BNKNames.eventActionSettings, "Setting");

                                BNKStream.WriteValueU16(settingType);
                            }

                            foreach (XElement sett in settings)
                            {
                                BNKStream.WriteValueF32(float.Parse(sett.Value));
                            }
                        }
                        else if (type == 0x02) // 2 Sound SFX/Sound Voice
                        {
                            byte unknown1 = byte.Parse(obj.Attribute("Unknown1").Value);
                            BNKStream.WriteByte(unknown1);
                            BNKStream.WriteByte(byte.Parse(obj.Attribute("Unknown2").Value));
                            BNKStream.WriteByte(byte.Parse(obj.Attribute("Unknown3").Value));
                            BNKStream.WriteByte(byte.Parse(obj.Attribute("Unknown4").Value));

                            byte included = (byte)BNKGetValFromKnownName(obj.Attribute("Stream").Value, BNKNames.eventActionIncluded, "Included");
                            BNKStream.WriteByte(included);

                            if (unknown1 == 2)
                            {
                                uint unknownID = uint.Parse(obj.Attribute("UnknownID").Value);
                                BNKStream.WriteValueU32(unknownID);
                            }

                            uint wemID = uint.Parse(obj.Attribute("WemID").Value);
                            uint wemLen = uint.Parse(obj.Attribute("WemSize").Value);

                            BNKStream.WriteValueU32(wemID);
                            BNKStream.WriteValueU32(wemLens.ContainsKey(wemID) ? wemLens[wemID] : wemLen);

                            byte soundType = (byte)BNKGetValFromKnownName(obj.Attribute("SoundType").Value, BNKNames.eventActionSoundType, "SoundType");
                            BNKStream.WriteByte(soundType);

                            XElement effsPrnt = obj.Element("Effects");

                            BNKStream.WriteByte(byte.Parse(effsPrnt.Attribute("OverrideParentEffects").Value));

                            IEnumerable<XElement> effects = effsPrnt.Elements("Effect");

                            BNKStream.WriteByte((byte)effects.Count());

                            if (effects.Count() > 0)
                            {
                                BNKStream.WriteByte(byte.Parse(effsPrnt.Attribute("bitMaskEffectBypass").Value));
                            }

                            foreach (XElement efft in effects)
                            {
                                BNKStream.WriteByte(byte.Parse(efft.Attribute("EffectIndex").Value));
                                BNKStream.WriteValueU32(uint.Parse(efft.Attribute("EffectObjectID").Value));
                                BNKStream.WriteByte(byte.Parse(efft.Attribute("Unknown1").Value));
                                BNKStream.WriteByte(byte.Parse(efft.Attribute("Unknown2").Value));
                            }

                            BNKStream.WriteByte(0);

                            BNKStream.WriteValueU32(uint.Parse(obj.Attribute("OutputBusID").Value));
                            BNKStream.WriteValueU32(uint.Parse(obj.Attribute("ParentObjID").Value));
                            BNKStream.WriteByte(byte.Parse(obj.Attribute("OverrideParentPlaybackPriority").Value));

                            IEnumerable<XElement> addsParams = obj.Element("AdditionalParameters").Elements();

                            BNKStream.WriteByte((byte)addsParams.Count());

                            foreach (XElement addsParam in addsParams)
                            {
                                BNKStream.WriteByte((byte)BNKGetValFromKnownName(addsParam.Name.ToString(), BNKNames.eventActionAddsParams, "Param"));
                            }

                            foreach (XElement addsParam in addsParams)
                            {
                                byte pname = (byte)BNKGetValFromKnownName(addsParam.Name.ToString(), BNKNames.eventActionAddsParams, "Param");
                                if (pname == 0x3A)
                                    BNKStream.WriteValueU32(uint.Parse(addsParam.Value));
                                else
                                    BNKStream.WriteValueF32(float.Parse(addsParam.Value, CultureInfo.InvariantCulture));
                            }

                            // -------------------------------------------------------------------------------------------------------------------------------------

                            IEnumerable<XElement> unkParams = obj.Element("UnknownRangeParameters").Elements();

                            BNKStream.WriteByte((byte)unkParams.Count());

                            foreach (XElement unkParam in unkParams)
                            {
                                BNKStream.WriteByte((byte)BNKGetValFromKnownName(unkParam.Name.ToString(), BNKNames.eventActionAddsParams, "Param"));
                            }

                            foreach (XElement unkParam in unkParams)
                            {
                                byte pname = (byte)BNKGetValFromKnownName(unkParam.Name.ToString(), BNKNames.eventActionAddsParams, "Param");
                                BNKStream.WriteValueF32(float.Parse(unkParam.Attribute("Min").Value, CultureInfo.InvariantCulture));
                                BNKStream.WriteValueF32(float.Parse(unkParam.Attribute("Max").Value, CultureInfo.InvariantCulture));
                            }

                            XElement xPositioning = obj.Element("Positioning");
                            byte positioningType = byte.Parse(xPositioning.Attribute("Type").Value);

                            BNKStream.WriteByte(positioningType);

                            if (positioningType > 15 && (positioningType % 7) != 3)
                            {
                                BNKStream.WriteByte(byte.Parse(xPositioning.Attribute("Settings").Value));
                                BNKStream.WriteValueU32(uint.Parse(xPositioning.Attribute("AttenuationID").Value));
                            }

                            XElement xAuxiliary = obj.Element("Auxiliary");
                            byte auxSettings = byte.Parse(xAuxiliary.Attribute("Settings").Value);

                            BNKStream.WriteByte(auxSettings);

                            if (auxSettings > 7)
                            {
                                XElement usrDfnAuxSnds = xAuxiliary.Element("UserDefinedAuxiliarySends");

                                BNKStream.WriteValueU32(uint.Parse(usrDfnAuxSnds.Element("AuxiliaryBus1").Value));
                                BNKStream.WriteValueU32(uint.Parse(usrDfnAuxSnds.Element("AuxiliaryBus2").Value));
                                BNKStream.WriteValueU32(uint.Parse(usrDfnAuxSnds.Element("AuxiliaryBus3").Value));
                                BNKStream.WriteValueU32(uint.Parse(usrDfnAuxSnds.Element("AuxiliaryBus4").Value));
                            }

                            BNKStream.WriteByte(byte.Parse(obj.Attribute("PlaybackSettings").Value));
                            BNKStream.WriteByte(byte.Parse(obj.Attribute("OnReturnToPhysVoice").Value));
                            BNKStream.WriteByte(byte.Parse(obj.Attribute("LimitSoundInstances").Value));
                            BNKStream.WriteByte(byte.Parse(obj.Attribute("PosUnknown").Value));
                            BNKStream.WriteByte(byte.Parse(obj.Attribute("VirtualVoiceBehavior").Value));

                            BNKStream.WriteByte(0);
                            if (bnkVersion == 120)
                            {
                                BNKStream.WriteByte(0);
                                BNKStream.WriteByte(0);
                            }

                            IEnumerable<XElement> statesProps = obj.Element("StatesProperties").Elements("Property");

                            BNKStream.WriteByte((byte)statesProps.Count());

                            foreach (XElement stateProp in statesProps)
                            {
                                byte settName = (byte)BNKGetValFromKnownName(stateProp.Attribute("SettingName").Value, BNKNames.eventActionSettings, "Setting");

                                BNKStream.WriteByte(settName);
                                BNKStream.WriteByte(byte.Parse(stateProp.Attribute("Unknown1").Value));
                                BNKStream.WriteByte(byte.Parse(stateProp.Attribute("Unknown2").Value));
                            }

                            IEnumerable<XElement> statesGrps = obj.Element("StateGroups").Elements("StateGroup");

                            BNKStream.WriteByte((byte)statesGrps.Count());

                            foreach (XElement statesGrp in statesGrps)
                            {
                                BNKStream.WriteValueU32(uint.Parse(statesGrp.Attribute("ID").Value));
                                BNKStream.WriteByte(byte.Parse(statesGrp.Attribute("ChangeOccursAt").Value));

                                IEnumerable<XElement> settDiffs = statesGrp.Element("SettingsDiffs").Elements("Diff");

                                BNKStream.WriteByte((byte)settDiffs.Count());

                                foreach (XElement settDiff in settDiffs)
                                {
                                    BNKStream.WriteValueU32(uint.Parse(settDiff.Attribute("StateObjID").Value));
                                    BNKStream.WriteValueU32(uint.Parse(settDiff.Attribute("SettingsID").Value));
                                }
                            }

                            IEnumerable<XElement> rtpcs = obj.Element("RTPCs").Elements("RTPC");

                            BNKStream.WriteValueU16((ushort)rtpcs.Count());

                            foreach (XElement rtpc in rtpcs)
                            {
                                byte axisType = (byte)BNKGetValFromKnownName(rtpc.Attribute("AxisType").Value, BNKNames.eventActionSettings, "Type");

                                IEnumerable<XElement> rtpcPoints = rtpc.Element("Points").Elements("Point");

                                BNKStream.WriteValueU32(uint.Parse(rtpc.Attribute("GameParamID").Value));
                                BNKStream.WriteByte(byte.Parse(rtpc.Attribute("Unknown1").Value));
                                BNKStream.WriteByte(byte.Parse(rtpc.Attribute("Unknown2").Value));
                                BNKStream.WriteByte(axisType);
                                BNKStream.WriteValueU32(uint.Parse(rtpc.Attribute("UnknownID").Value));
                                BNKStream.WriteByte(byte.Parse(rtpc.Attribute("Unknown3").Value));
                                BNKStream.WriteByte((byte)rtpcPoints.Count());
                                BNKStream.WriteByte(byte.Parse(rtpc.Attribute("Unknown4").Value));

                                foreach (XElement rtcpPoint in rtpcPoints)
                                {
                                    byte shapeCurve = (byte)BNKGetValFromKnownName(rtcpPoint.Attribute("Shape").Value, BNKNames.rtpcShape, "Shape");

                                    BNKStream.WriteValueF32(float.Parse(rtcpPoint.Attribute("PosX").Value, CultureInfo.InvariantCulture));
                                    BNKStream.WriteValueF32(float.Parse(rtcpPoint.Attribute("PosY").Value, CultureInfo.InvariantCulture));
                                    BNKStream.WriteValueU32(shapeCurve);
                                }
                            }

                            XElement binary = obj.Element("Binary");
                            if (binary != null)
                            {
                                byte[] soundStruct = Helpers.StringToByteArray(binary.Value);
                                BNKStream.WriteBytes(soundStruct);
                            }
                        }
                        else if (type == 0x03) // 3 Event Action
                        {
                            byte scope = (byte)BNKGetValFromKnownName(obj.Attribute("Scope").Value, BNKNames.eventActionScopes, "Scope");
                            byte actionType = (byte)BNKGetValFromKnownName(obj.Attribute("ActionType").Value, BNKNames.eventActionTypes, "ActionType");

                            BNKStream.WriteByte(scope);
                            BNKStream.WriteByte(actionType);
                            BNKStream.WriteValueU32(uint.Parse(obj.Attribute("ReferenceID").Value));
                            BNKStream.WriteByte(0);

                            IEnumerable<XElement> addParams = obj.Element("Params").Elements();

                            BNKStream.WriteByte((byte)addParams.Count());

                            foreach (XElement addParam in addParams)
                            {
                                byte paramType = (byte)BNKGetValFromKnownName(addParam.Name.ToString(), BNKNames.eventActionParams, "Param");
                                BNKStream.WriteByte(paramType);
                            }

                            foreach (XElement addParam in addParams)
                            {
                                BNKStream.WriteValueU32(uint.Parse(addParam.Value));
                            }


                            IEnumerable<XElement> nextParams = obj.Element("NextParams").Elements();

                            BNKStream.WriteByte((byte)nextParams.Count());

                            foreach (XElement nextParam in nextParams)
                            {
                                byte paramType = (byte)BNKGetValFromKnownName(nextParam.Name.ToString(), BNKNames.eventActionParams, "Param");
                                BNKStream.WriteByte(paramType);
                            }

                            foreach (XElement nextParam in nextParams)
                            {
                                BNKStream.WriteValueS32(int.Parse(nextParam.Attribute("Value1").Value));
                                BNKStream.WriteValueU32(uint.Parse(nextParam.Attribute("Value2").Value));
                            }

                            if (actionType != 0x1C && actionType != 0x21)
                            {
                                BNKStream.WriteByte(byte.Parse(obj.Element("Unknown").Value));

                                if (actionType == 0x01 || actionType == 0x04) // Stop Play
                                {
                                    BNKStream.WriteValueU32(uint.Parse(obj.Element("SoundBankID").Value));
                                }
                                if (actionType == 0x06 || actionType == 0x07 || actionType == 0x0B || actionType == 0x12 || actionType == 0x1E || actionType == 0x02 || actionType == 0x0A || actionType == 0x13 || actionType == 0x14) // SetGameParameter ResetGameParameter ResetVoiceVolume SetState Seek Pause SetVoiceVolume
                                {
                                    byte[] objData = Helpers.StringToByteArray(obj.Element("Binary").Value);
                                    BNKStream.WriteBytes(objData);
                                }
                            }
                        }
                        else if (type == 0x04) // 4 Event
                        {
                            IEnumerable<XElement> eventsIDs = obj.Elements("EventActionID");

                            BNKStream.WriteByte((byte)eventsIDs.Count());

                            if (bnkVersion == 120)
                                BNKStream.WriteBytes(new byte[] { 0, 0, 0 });

                            foreach (XElement eventsID in eventsIDs)
                            {
                                BNKStream.WriteValueU32(uint.Parse(eventsID.Value));
                            }
                        }
                        else*/
                        {
                            byte[] binaryData = Helpers.StringToByteArray(obj.Element("Binary").Value);
                            MemoryStream bdStr = new(binaryData);

                            foreach (var id in obj.Element("PossibleIDs").Elements("ID"))
                            {
                                byte[] search = Helpers.StringToByteArray(id.Attribute("OrigHex").Value);
                                uint replace = uint.Parse(id.Attribute("UInt32").Value);
                                
                                int[] poses = Helpers.SearchBytesMultiple(binaryData, search);
        
                                foreach (int pos in poses)
                                {
                                    bdStr.Position = pos;
                                    bdStr.WriteValueU32(replace);
                                }
                            }

                            binaryData = bdStr.ToArray();
                            BNKStream.WriteBytes(binaryData);
                        }

                        long currObjPos = BNKStream.Position;
                        long objLength = BNKStream.Position - objStartPos - sizeof(uint);
                        BNKStream.Seek(objStartPos, SeekOrigin.Begin);
                        BNKStream.WriteValueU32((uint)objLength);
                        BNKStream.Seek(currObjPos, SeekOrigin.Begin);
                    }
                }
                else
                {
                    byte[] data = Helpers.StringToByteArray(xSection.Element("Binary").Value);
                    BNKStream.WriteBytes(data);
                }

                long currPos = BNKStream.Position;
                long sectionLength = BNKStream.Position - sectionStartPos;
                BNKStream.Seek(sectionStartPos - sizeof(uint), SeekOrigin.Begin);
                BNKStream.WriteValueU32((uint)sectionLength);
                BNKStream.Seek(currPos, SeekOrigin.Begin);
            }

            BNKStream.Flush();
            BNKStream.Close();
        }

        static string BNKGetKnownName(uint numVal, Dictionary<uint, string> arrNames, string nameType)
        {
            string name = "Unknown" + nameType + numVal.ToString();
            if (arrNames.ContainsKey(numVal)) name = arrNames[numVal];
            return name;
        }

        static uint BNKGetValFromKnownName(string name, Dictionary<uint, string> arrNames, string nameType)
        {
            uint val = 0;
            if (name.StartsWith("Unknown" + nameType))
            {
                name = name.Replace("Unknown" + nameType, "");
                val = uint.Parse(name);
            }
            else
                val = arrNames.FirstOrDefault(x => x.Value == name).Key;
            return val;
        }

        static void WEMToOGG(string file, string output = "")
        {
            if (output == "")
            {
                output = file.Replace(".wem", ".ogg");
            }

            try
            {
                //WEMSharp.WEMFile wemFile = new WEMSharp.WEMFile(file, WEMSharp.WEMForcePacketFormat.ForceModPackets);
                //wemFile.GenerateOGG(output, m_Path + "\\packed_codebooks_aoTuV_603.bin", false, false);

                Process process1 = new();
                process1.StartInfo.FileName = m_Path + "\\ww2ogg.exe";
                process1.StartInfo.Arguments = "\"" + file + "\" --pcb \"" + m_Path + "\\packed_codebooks_aoTuV_603.bin\"  -o \"" + output + "\"";
                process1.StartInfo.UseShellExecute = false;
                process1.Start();
                process1.WaitForExit();

                Process process = new();
                process.StartInfo.FileName = m_Path + "\\revorb.exe";
                process.StartInfo.Arguments = "\"" + output + "\"";
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                Console.ResetColor();
            }
        }

        static void BinaryReplaceValues(string file)
        {
            if (!File.Exists(file))
                return;

            string onlyDir = Path.GetDirectoryName(file);
            byte[] bytes = null;
            FileStream fileStream = null;
            Gibbed.Dunia2.BinaryObjectInfo.FieldType fieldTypeSearch = Gibbed.Dunia2.BinaryObjectInfo.FieldType.Invalid;
            Gibbed.Dunia2.BinaryObjectInfo.FieldType fieldTypeReplace = Gibbed.Dunia2.BinaryObjectInfo.FieldType.Invalid;
            int lineStart = 0;

            string[] lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("="))
                {
                    string[] lineSplit = lines[i].Split('=');

                    string ff = onlyDir + "\\" + lineSplit[0];

                    string newFileName = onlyDir + "\\" + lineSplit[1];

                    File.Copy(ff, newFileName, true);
                    bytes = File.ReadAllBytes(newFileName);
                    fileStream = new FileStream(newFileName, FileMode.Open, FileAccess.ReadWrite);

                    lineStart = i;
                }
                else if (lineStart + 1 == i)
                {
                    string[] lineSplit = lines[i].Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

                    Enum.TryParse(lineSplit[0], true, out fieldTypeSearch);
                    Enum.TryParse(lineSplit[1], true, out fieldTypeReplace);
                }
                else if (lines[i] == "END")
                {
                    fileStream.Dispose();
                    fileStream.Close();
                }
                else if (lines[i] != "")
                {
                    string[] lineSplit = lines[i].Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

                    byte[] search = Gibbed.Dunia2.BinaryObjectInfo.FieldTypeSerializers.Serialize(fieldTypeSearch, lineSplit[0]);
                    byte[] replace = Gibbed.Dunia2.BinaryObjectInfo.FieldTypeSerializers.Serialize(fieldTypeReplace, lineSplit[1]);

                    int[] poses = Helpers.SearchBytesMultiple(bytes, search);

                    foreach (int pos in poses)
                    {
                        fileStream.Position = pos;
                        fileStream.Write(replace, 0, replace.Length);
                    }
                }
            }
        }

        static void ConvertXBT(string file)
        {
            string newPathDds = file.Replace(".xbts", ".dds").Replace(".xbt", ".dds");

            XDocument xmlDoc = new(new XDeclaration("1.0", "utf-8", "yes"));
            xmlDoc.Add(new XComment(xmlheader));
            XElement root = new("XBTInfo");

            FileStream XBTStream = new(file, FileMode.Open);

            uint type = XBTStream.ReadValueU32();
            if (type != 0x00584254)
                return;

            uint version = XBTStream.ReadValueU32();
            uint hdrLen = XBTStream.ReadValueU32();
            byte param1 = (byte)XBTStream.ReadByte();
            byte param2 = (byte)XBTStream.ReadByte();
            byte param3 = (byte)XBTStream.ReadByte();
            byte param4 = (byte)XBTStream.ReadByte();
            byte param5 = (byte)XBTStream.ReadByte();
            byte mipsFileMipsCount = (byte)XBTStream.ReadByte();
            byte param7 = (byte)XBTStream.ReadByte();
            byte param8 = (byte)XBTStream.ReadByte();
            uint unknownID1 = XBTStream.ReadValueU32();
            uint unknownID2 = XBTStream.ReadValueU32();
            uint unknownID3 = XBTStream.ReadValueU32();

            string mipsFileName = "";
            uint mipsFileCheck = XBTStream.ReadValueU32();
            if (mipsFileCheck != 0)
            {
                XBTStream.Seek(XBTStream.Position - sizeof(uint), SeekOrigin.Begin);
                byte[] mF = XBTStream.ReadBytes((int)(hdrLen - 32));
                mipsFileName = Encoding.Default.GetString(mF);
                mipsFileName = Regex.Replace(mipsFileName, @"\p{C}+", string.Empty);
            }

            root.Add(new XElement("Version", version));
            root.Add(new XElement("Param1", param1));
            root.Add(new XElement("Param2", param2));
            root.Add(new XElement("Param3", param3));
            root.Add(new XElement("Param4", param4));
            root.Add(new XElement("Param5", param5));
            root.Add(new XElement(version <= 112 ? "Param6" : "MipsFileMipsCount", mipsFileMipsCount));
            root.Add(new XElement("Param7", param7));
            root.Add(new XElement("Param8", param8));
            root.Add(new XElement("UnknownID1", unknownID1));
            root.Add(new XElement("UnknownID2", unknownID2));
            root.Add(new XElement("UnknownID3", unknownID3));
            root.Add(new XElement("MipsFileName", mipsFileName));

            int toRead = (int)XBTStream.Length - (int)hdrLen;

            if (file.EndsWith(".xbts"))
            {
                toRead -= 20;
            }

            byte[] dds = XBTStream.ReadBytes(toRead);

            File.WriteAllBytes(newPathDds, dds);

            if (file.EndsWith(".xbts"))
            {
                byte[] xbts = XBTStream.ReadBytes(20);
                root.Add(new XElement("XBTSData", Helpers.ByteArrayToString(xbts)));
            }

            XBTStream.Close();

            xmlDoc.Add(root);
            xmlDoc.Save(file + ".converted.xml");
        }

        static (byte[], XElement, uint) ReadXBTXMLHeader(string xmlName, uint version = 0, string customMips = "", int customMipsCount = -1)
        {
            XDocument xDoc = XDocument.Load(xmlName);
            XElement xRoot = xDoc.Element("XBTInfo");

            uint ver = uint.Parse(xRoot.Element("Version").Value);
            if (version > 0) ver = version;

            string mipsName = ver <= 112 ? "Param6" : "MipsFileMipsCount";

            MemoryStream memoryStream = new();
            memoryStream.WriteValueU32(0x00584254);
            memoryStream.WriteValueU32(ver);
            memoryStream.WriteValueU32(0);
            memoryStream.WriteByte(byte.Parse(xRoot.Element("Param1").Value));
            memoryStream.WriteByte(byte.Parse(xRoot.Element("Param2").Value));
            memoryStream.WriteByte(byte.Parse(xRoot.Element("Param3").Value));
            memoryStream.WriteByte(byte.Parse(xRoot.Element("Param4").Value));
            memoryStream.WriteByte(byte.Parse(xRoot.Element("Param5").Value));
            memoryStream.WriteByte(customMipsCount != -1 ? (byte)customMipsCount : byte.Parse(xRoot.Element(mipsName).Value));
            memoryStream.WriteByte(byte.Parse(xRoot.Element("Param7").Value));
            memoryStream.WriteByte(byte.Parse(xRoot.Element("Param8").Value));
            memoryStream.WriteValueU32(uint.Parse(xRoot.Element("UnknownID1").Value));
            memoryStream.WriteValueU32(uint.Parse(xRoot.Element("UnknownID2").Value));
            memoryStream.WriteValueU32(uint.Parse(xRoot.Element("UnknownID3").Value));

            if (customMips != "")
            {
                memoryStream.WriteBytes(Encoding.Default.GetBytes(customMips));
            }
            else
            {
                string mipsFile = xRoot.Element("MipsFileName").Value;
                if (mipsFile != "")
                {
                    memoryStream.WriteBytes(Encoding.Default.GetBytes(mipsFile));
                }
            }

            memoryStream.WriteByte(0);

            long pad = (memoryStream.Position + (4 - (memoryStream.Position % 4)) % 4) - 1;
            memoryStream.Seek(pad, SeekOrigin.Begin);
            memoryStream.WriteByte(0);

            return (memoryStream.ToArray(), xRoot, ver);
        }

        static void ConvertDDS(string file)
        {
            string newFileName = "";
            string xmlName = "";
            string ddsName = "";
            string nonMipsXml = "";

            if (file.EndsWith(".dds"))
            {
                newFileName = file.Replace(".dds", "_new.xbt");
                xmlName = file.Replace(".dds", ".xbt.converted.xml");
                ddsName = file;
            }
            if (file.EndsWith(".xbt.converted.xml") || file.EndsWith(".xbts.converted.xml"))
            {
                newFileName = file.Replace(".xbt.converted.xml", "_new.xbt").Replace(".xbts.converted.xml", "_new.xbt");
                xmlName = file;
                ddsName = file.Replace(".xbt.converted.xml", ".dds").Replace(".xbts.converted.xml", ".dds");
            }
            if (file.EndsWith("_mips.xbt.converted.xml") || file.EndsWith("_mips.dds"))
            {
                nonMipsXml = file.Replace("_mips.xbt.converted.xml", "").Replace("_mips.dds", "");
                nonMipsXml += ".xbt.converted.xml";
            }

            var xbtHdr = ReadXBTXMLHeader(xmlName);

            byte[] dds = File.ReadAllBytes(ddsName);

            if (nonMipsXml != "" && xbtHdr.Item3 >= 116)
            {
                if (!File.Exists(nonMipsXml))
                {
                    Console.WriteLine("Can't convert the file, missing non mips XBTInfo file.");
                    return;
                }

                XDocument oMiXD = XDocument.Load(nonMipsXml);
                uint mipsCount = uint.Parse(oMiXD.Element("XBTInfo").Element("MipsFileMipsCount").Value);

                MemoryStream memoryStream1 = new(dds);
                memoryStream1.Seek(28, SeekOrigin.Begin);
                memoryStream1.WriteValueU32(mipsCount);
                dds = memoryStream1.ToArray();
            }

            FileStream XBTStream = File.Create(newFileName);
            XBTStream.WriteBytes(xbtHdr.Item1);
            XBTStream.Seek(sizeof(uint) * 2, SeekOrigin.Begin);
            XBTStream.WriteValueU32((uint)xbtHdr.Item1.Length);
            XBTStream.Seek(xbtHdr.Item1.Length, SeekOrigin.Begin);
            XBTStream.WriteBytes(dds);

            XElement xbts = xbtHdr.Item2.Element("XBTSData");
            if (xbts != null)
            {
                XBTStream.WriteBytes(Helpers.StringToByteArray(xbts.Value));
            }

            XBTStream.Dispose();
            XBTStream.Close();
        }

        static void FixXBGForFP(string editXbg, string data)
        {
            if (!File.Exists(editXbg))
            {
                Console.WriteLine("File " + editXbg + " doesn't exist!");
                return;
            }

            string newF = Path.GetDirectoryName(editXbg) + "\\" + Path.GetFileNameWithoutExtension(editXbg) + "_new.xbg";
            File.Copy(editXbg, newF, true);

            FileStream XBGEditStream = new(newF, FileMode.Open, FileAccess.ReadWrite);

            uint editHdr = XBGEditStream.ReadValueU32();
            ushort majorVer = XBGEditStream.ReadValueU16();
            ushort minorVer = XBGEditStream.ReadValueU16();

            int ver = 0;

            if (editHdr != 0x4D455348) // HSEM
            {
                Console.WriteLine(editXbg + " is not a valid XBG file.");
                return;
            }

            if (majorVer == 0x47 && minorVer == 0x0D) ver = 1;
            if (majorVer == 0x49 && minorVer == 0x13) ver = 2;

            if (ver == 0)
            {
                Console.WriteLine(editXbg + " is wrong version.");
                return;
            }

            Dictionary<string, string[]> dataP = new();

            string[] dataParse = data.Split('|');
            foreach (string dP in dataParse)
            {
                string[] vals = dP.Split(';');
                string[] valsData = vals[1].Split(',');
                dataP.Add(vals[0], valsData);
            }

            Dictionary<int, Dictionary<int, Dictionary<int, Helpers.HideFacesStruct>>> sourceXbgSkelHideFacesFP = new();
            if (dataP.ContainsKey("SKELHIDEFACESFP"))
            {
                string[] dataToWrite = dataP["SKELHIDEFACESFP"];
                for (int i = 0; i < dataToWrite.Length; i++)
                {
                    string[] matChilds = dataToWrite[i].Split('+');
                    Dictionary<int, Dictionary<int, Helpers.HideFacesStruct>> matsArr = new();

                    for (int j = 0; j < matChilds.Length; j++)
                    {
                        Dictionary<int, Helpers.HideFacesStruct> hfSA = new();

                        if (matChilds[j] != "" && matChilds[j] != "0")
                        {
                            string[] matParts = matChilds[j].Split('-');

                            for (int k = 0; k < matParts.Length; k++)
                            {
                                string[] partData = matParts[k].Split('*');

                                hfSA.Add(k, new()
                                {
                                    id = ulong.Parse(partData[0]),
                                    start = ushort.Parse(partData[1]),
                                    count = ushort.Parse(partData[2])
                                });
                            }
                        }

                        matsArr.Add(j, hfSA);
                    }

                    sourceXbgSkelHideFacesFP.Add(i, matsArr);
                }
            }


            uint DHRMResize = 0;

            XBGEditStream.Seek(28, SeekOrigin.Begin);
            uint editSectCount = XBGEditStream.ReadValueU32();
            for (int i = 0; i < editSectCount; i++)
            {
                uint sectHdrK = XBGEditStream.ReadValueU32();
                XBGEditStream.ReadValueU32(); // always one
                uint sectionLen = XBGEditStream.ReadValueU32();
                uint sectionLenShorted = XBGEditStream.ReadValueU32();
                uint subCount = XBGEditStream.ReadValueU32();

                if (subCount > 0)
                {
                    for (int j = 0; j < subCount; j++)
                    {
                        uint subSectHdrK = XBGEditStream.ReadValueU32();
                        XBGEditStream.ReadValueU32(); // always one
                        uint subSectionLen = XBGEditStream.ReadValueU32();
                        uint subSectionLenShorted = XBGEditStream.ReadValueU32();
                        XBGEditStream.ReadValueU32(); // hope zero

                        long subPos = XBGEditStream.Position;

                        if (subSectHdrK == 0x434C5553) // SULC
                        {
                            if (sourceXbgSkelHideFacesFP.Any())
                            {
                                for (int k = 0; k < sourceXbgSkelHideFacesFP.Count; k++)
                                {
                                    uint subMeshCount = XBGEditStream.ReadValueU32();

                                    for (int l = 0; l < sourceXbgSkelHideFacesFP[k].Count; l++)
                                    {
                                        if (ver == 1) XBGEditStream.Seek(XBGEditStream.Position + 524, SeekOrigin.Begin);
                                        if (ver == 2) XBGEditStream.Seek(XBGEditStream.Position + 676, SeekOrigin.Begin);

                                        int amnt = 0;
                                        for (int m = 0; m < sourceXbgSkelHideFacesFP[k][l].Count; m++)
                                        {
                                            var pars = sourceXbgSkelHideFacesFP[k][l][m];
                                            XBGEditStream.WriteValueU32(0);
                                            XBGEditStream.WriteValueU64(pars.id);
                                            XBGEditStream.WriteValueU16(pars.start);
                                            XBGEditStream.WriteValueU16(pars.count);
                                            amnt++;
                                        }

                                        int wspace = 512 - (amnt * (sizeof(uint) + sizeof(ulong) + sizeof(ushort) + sizeof(ushort)));
                                        for (int n = 0; n < wspace; n++)
                                            XBGEditStream.WriteByte(0);

                                        XBGEditStream.WriteValueU32(0);
                                        XBGEditStream.WriteValueS32(amnt);
                                        XBGEditStream.WriteValueU32(0);
                                    }
                                }
                            }
                        }

                        XBGEditStream.Seek(subPos + subSectionLenShorted, SeekOrigin.Begin);
                    }
                }

                long pos = XBGEditStream.Position;

                if (sectHdrK == 0x4D524844) // DHRM
                {
                    string[] dataToWrite = dataP["MESHPARTHIDE"];

                    if (dataToWrite.Length > 0 && dataToWrite[0] != "")
                    {
                        XBGEditStream.Seek(pos + sectionLenShorted, SeekOrigin.Begin);
                        int leftToEnd = (int)(XBGEditStream.Length - pos - sectionLenShorted);
                        byte[] leftToEndBts = new byte[leftToEnd];
                        XBGEditStream.Read(leftToEndBts, 0, leftToEndBts.Length);

                        XBGEditStream.Seek(pos, SeekOrigin.Begin);
                        XBGEditStream.SetLength(pos);

                        DHRMResize = (uint)(dataToWrite.Length * sizeof(ulong) + sizeof(uint) + sizeof(byte) - sectionLenShorted);

                        XBGEditStream.WriteByte(0);
                        XBGEditStream.WriteValueU32((uint)dataToWrite.Length);
                        foreach (string valToWrite in dataToWrite)
                        {
                            XBGEditStream.WriteValueU64(ulong.Parse(valToWrite));
                        }
                        XBGEditStream.Write(leftToEndBts, 0, leftToEndBts.Length);

                        XBGEditStream.Seek(pos - sizeof(uint) * 3, SeekOrigin.Begin);
                        XBGEditStream.WriteValueU32(sectionLen + DHRMResize);
                        XBGEditStream.WriteValueU32(sectionLenShorted + DHRMResize);
                        XBGEditStream.Flush();

                        sectionLenShorted += DHRMResize;
                    }
                }

                XBGEditStream.Seek(pos + sectionLenShorted, SeekOrigin.Begin);
            }

            XBGEditStream.Seek(-sizeof(ulong), SeekOrigin.End);
            uint val = XBGEditStream.ReadValueU32();
            XBGEditStream.Seek(-sizeof(ulong), SeekOrigin.End);
            val += DHRMResize;
            XBGEditStream.WriteValueU32(val);

            XBGEditStream.Seek(20, SeekOrigin.Begin);
            val = XBGEditStream.ReadValueU32();
            XBGEditStream.Seek(20, SeekOrigin.Begin);
            val += DHRMResize;
            XBGEditStream.WriteValueU32(val);

            XBGEditStream.Close();
        }

        static void GetDataFromXBG(string sourceXbg)
        {
            if (!File.Exists(sourceXbg))
            {
                Console.WriteLine("File " + sourceXbg + " doesn't exist!");
                return;
            }

            FileStream XBGSourceStream = new(sourceXbg, FileMode.Open, FileAccess.ReadWrite);

            uint editHdr = XBGSourceStream.ReadValueU32();
            ushort majorVer = XBGSourceStream.ReadValueU16();
            ushort minorVer = XBGSourceStream.ReadValueU16();

            int ver = 0;

            if (editHdr != 0x4D455348) // HSEM
            {
                Console.WriteLine(sourceXbg + " is not a valid XBG file.");
                return;
            }

            if (majorVer == 0x47 && minorVer == 0x0D) ver = 1;
            if (majorVer == 0x49 && minorVer == 0x13) ver = 2;

            if (ver == 0)
            {
                Console.WriteLine(sourceXbg + " is wrong version.");
                return;
            }

            string dataP = "";
            uint skeletonCount = 0;

            XBGSourceStream.Seek(28, SeekOrigin.Begin);
            uint editSectCount = XBGSourceStream.ReadValueU32();
            for (int i = 0; i < editSectCount; i++)
            {
                uint sectHdrK = XBGSourceStream.ReadValueU32();
                XBGSourceStream.ReadValueU32(); // always one
                uint sectionLen = XBGSourceStream.ReadValueU32();
                uint sectionLenShorted = XBGSourceStream.ReadValueU32();
                uint subCount = XBGSourceStream.ReadValueU32();

                if (subCount > 0)
                {
                    for (int j = 0; j < subCount; j++)
                    {
                        uint subSectHdrK = XBGSourceStream.ReadValueU32();
                        XBGSourceStream.ReadValueU32(); // always one
                        uint subSectionLen = XBGSourceStream.ReadValueU32();
                        uint subSectionLenShorted = XBGSourceStream.ReadValueU32();
                        XBGSourceStream.ReadValueU32(); // hope zero

                        long subPos = XBGSourceStream.Position;

                        if (subSectHdrK == 0x434C5553) // SULC
                        {
                            string vals_faces = "";
                            string vals_verts = "";
                            string vals_meshes = "";
                            string vals_fphide = "";
                            for (int k = 0; k < skeletonCount; k++)
                            {
                                string vals_faces_child = "";
                                string vals_verts_child = "";
                                string vals_fphide_child = "";

                                uint meshCount = XBGSourceStream.ReadValueU32();

                                for (int l = 0; l < meshCount; l++)
                                {
                                    XBGSourceStream.ReadValueU16();
                                    ushort facesCount = XBGSourceStream.ReadValueU16();
                                    XBGSourceStream.ReadValueU16();
                                    if (ver == 1) XBGSourceStream.ReadValueU16();
                                    ushort vertsCount = XBGSourceStream.ReadValueU16();

                                    if (ver == 1) XBGSourceStream.Seek(514, SeekOrigin.Current);
                                    if (ver == 2) XBGSourceStream.Seek(668, SeekOrigin.Current);
                                    long smidd = XBGSourceStream.Position;
                                    XBGSourceStream.Seek(516, SeekOrigin.Current);
                                    uint cnt = XBGSourceStream.ReadValueU32();
                                    long send = XBGSourceStream.Position + 4;

                                    XBGSourceStream.Seek(smidd, SeekOrigin.Begin);

                                    string tmp = "";
                                    for (int m = 0; m < cnt; m++)
                                    {
                                        XBGSourceStream.ReadValueU32();
                                        ulong id = XBGSourceStream.ReadValueU64();
                                        ushort idx = XBGSourceStream.ReadValueU16();
                                        ushort len = XBGSourceStream.ReadValueU16();
                                        tmp += (m > 0 ? "-" : "") + id + "*" + idx + "*" + len;
                                    }
                                    vals_fphide_child += (l > 0 ? "+" : "") + tmp;

                                    XBGSourceStream.Seek(send, SeekOrigin.Begin);

                                    vals_faces_child += (l > 0 ? "+" : "") + facesCount.ToString();
                                    vals_verts_child += (l > 0 ? "+" : "") + vertsCount.ToString();
                                }

                                vals_faces += (k > 0 ? "," : "") + vals_faces_child.ToString();
                                vals_verts += (k > 0 ? "," : "") + vals_verts_child.ToString();
                                vals_meshes += (k > 0 ? "," : "") + meshCount.ToString();
                                vals_fphide += (k > 0 ? "," : "") + vals_fphide_child;
                            }
                            dataP += (dataP == "" ? "" : "|") + "SKELFACES;" + vals_faces.ToString();
                            dataP += (dataP == "" ? "" : "|") + "SKELVERTS;" + vals_verts.ToString();
                            dataP += (dataP == "" ? "" : "|") + "SKELMATS;" + vals_meshes.ToString();
                            dataP += (dataP == "" ? "" : "|") + "SKELHIDEFACESFP;" + vals_fphide;
                        }

                        XBGSourceStream.Seek(subPos + subSectionLenShorted, SeekOrigin.Begin);
                    }
                }

                long pos = XBGSourceStream.Position;

                if (sectHdrK == 0x4D524844) // DHRM
                {
                    //read
                    XBGSourceStream.ReadByte();
                    XBGSourceStream.ReadValueU32();

                    string vals = "";
                    int cnt = (int)((sectionLenShorted - sizeof(uint) - sizeof(byte)) / sizeof(ulong));
                    for (int j = 0; j < cnt; j++)
                    {
                        vals += (vals == "" ? "" : ",") + XBGSourceStream.ReadValueU64().ToString();
                    }
                    dataP += (dataP == "" ? "" : "|") + "MESHPARTHIDE;" + vals;
                }

                /*if (sectHdrK == 0x4C4F44) // DOL
                {
                    uint lods = XBGSourceStream.ReadValueU32();
                    dataP += (dataP == "" ? "" : "|") + "LODS;" + lods;

                    XBGSourceStream.Seek(sulcStartPos, SeekOrigin.Begin);
                    string vals = "";
                    string vals_faces = "";
                    for (int k = 0; k < lods; k++)
                    {
                        uint subMeshCount = XBGSourceStream.ReadValueU32();

                        for (int l = 0; l < subMeshCount; l++)
                        {
                            XBGSourceStream.ReadValueU16();
                            ushort facesCount = XBGSourceStream.ReadValueU16();
                            XBGSourceStream.Seek(1044, SeekOrigin.Current);
                            vals_faces += (vals_faces == "" ? "" : ",") + facesCount.ToString();
                        }

                        vals += (vals == "" ? "" : ",") + subMeshCount.ToString();
                    }
                    dataP += (dataP == "" ? "" : "|") + "SUBMESHES;" + vals;
                    dataP += (dataP == "" ? "" : "|") + "FACES;" + vals_faces.ToString();
                }*/

                if (sectHdrK == 0x534B4944) // DIKS
                {
                    skeletonCount = XBGSourceStream.ReadValueU32();
                    dataP += (dataP == "" ? "" : "|") + "SKEL;" + skeletonCount;
                }

                if (sectHdrK == 0x524D544C) // LTMR
                {
                    string matsFiles = "";
                    uint matCount = XBGSourceStream.ReadValueU32();

                    for (int j = 0; j < matCount; j++)
                    {
                        uint pathStrLen = XBGSourceStream.ReadValueU32();
                        byte[] path = new byte[pathStrLen];
                        XBGSourceStream.Read(path, 0, (int)pathStrLen);

                        XBGSourceStream.ReadByte();

                        uint nameStrLen = XBGSourceStream.ReadValueU32();
                        byte[] name = new byte[nameStrLen];
                        XBGSourceStream.Read(name, 0, (int)nameStrLen);

                        XBGSourceStream.ReadByte();

                        matsFiles += (matsFiles == "" ? "" : ",") + Encoding.Default.GetString(name) + "*" + Encoding.Default.GetString(path);
                    }

                    dataP += (dataP == "" ? "" : "|") + "MATS;" + matsFiles;
                }

                XBGSourceStream.Seek(pos + sectionLenShorted, SeekOrigin.Begin);
            }

            XBGSourceStream.Close();

            //Console.Clear();
            Console.WriteLine("data" + dataP);
        }

        static void ConvertUE2XBG(string uePath, string sourceXbg, int type)
        {
            if (!File.Exists(uePath))
            {
                Console.WriteLine("File " + uePath + " doesn't exist!");
                return;
            }

            if (!File.Exists(sourceXbg))
            {
                Console.WriteLine("File " + sourceXbg + " doesn't exist!");
                return;
            }

            string workDir = Path.GetDirectoryName(uePath) + "\\";

            string uexp = workDir + Path.GetFileNameWithoutExtension(uePath) + ".uexp";
            if (!File.Exists(uexp))
            {
                Console.WriteLine("File " + uexp + " doesn't exist!");
                return;
            }

            string txt = workDir + Path.GetFileNameWithoutExtension(uePath) + ".txt";
            if (!File.Exists(txt))
            {
                Console.WriteLine("File " + txt + " doesn't exist!");
                return;
            }

            ue4.Convert(uePath, sourceXbg, type);
        }

        static void ConvertPNG2XBT(string file)
        {
            string gameStr = null;
            string baseResStr = null;
            string mipsResStr = null;
            string mipsHDResStr = null;
            string mipsPathStr = null;
            string xmlTemplate = m_Path + "\\XBTTemplate.xml";
            bool hasXML = false;

            string baseFileNameXML = file.Replace(".png", ".xml");

            if (File.Exists(baseFileNameXML))
            {
                xmlTemplate = baseFileNameXML;
                hasXML = true;

                XDocument xDoc = XDocument.Load(baseFileNameXML);
                XElement xRoot = xDoc.Element("XBTInfo");

                baseResStr = xRoot.Element("FCBConvParamBaseRes")?.Value;
                mipsResStr = xRoot.Element("FCBConvParamMipsRes")?.Value;
                mipsHDResStr = xRoot.Element("FCBConvParamMipsHDRes")?.Value;
                mipsPathStr = xRoot.Element("FCBConvParamMipsPath")?.Value;
            }

            if (gameStr == null && !hasXML)
            {
                Console.WriteLine("For which game you want to make XBT?:");
                Console.WriteLine("  5 = FC5 / ND");
                Console.WriteLine("  6 = FC6");
                gameStr = Console.ReadLine();
                Console.WriteLine("");
            }

            if (baseResStr == null)
            {
                Console.WriteLine("Write resolution of base (NON-MIPS) texture (for example resolution 512x256 or 512x512):");
                baseResStr = Console.ReadLine();
                Console.WriteLine("");
            }

            if (!baseResStr.Contains('x'))
            {
                Console.WriteLine("Wrong value! Exiting...");
                Console.ReadKey();
                return;
            }

            string[] resBaseSplit = baseResStr.Split('x');

            if (mipsResStr == null)
            {
                Console.WriteLine("Write resolution of MIPS texture (for example resolution 2048x1024 or 2048x2048):");
                Console.WriteLine("Note: if you don't want MIPS, then type 0");
                mipsResStr = Console.ReadLine();
                Console.WriteLine("");
            }

            if (!mipsResStr.Contains('x') && mipsResStr != "0")
            {
                Console.WriteLine("Wrong value! Exiting...");
                Console.ReadKey();
                return;
            }
            if (mipsResStr == "0") mipsResStr = "0x0";

            string[] resMipsSplit = mipsResStr.Split('x');

            if (mipsHDResStr == null)
            {
                Console.WriteLine("Write resolution of HD MIPS texture (for example resolution 4096x2048 or 4096x4096):");
                Console.WriteLine("Note: if you don't have HD, then type 0");
                mipsHDResStr = Console.ReadLine();
                Console.WriteLine("");
            }

            if (!mipsHDResStr.Contains('x') && mipsHDResStr != "0")
            {
                Console.WriteLine("Wrong value! Exiting...");
                Console.ReadKey();
                return;
            }
            if (mipsHDResStr == "0") mipsHDResStr = "0x0";

            string[] resMipsHDSplit = mipsHDResStr.Split('x');

            if (mipsPathStr == null && !hasXML)
            {
                Console.WriteLine("Write path to folder where will be textures placed (it means path inside DAT FAT, ending with \\, for example ):");
                mipsPathStr = Console.ReadLine();
                Console.WriteLine("");
            }

            BinaryReader br = new BinaryReader(File.OpenRead(file));
            br.BaseStream.Position = 16;
            byte[] widthbytes = new byte[sizeof(int)];
            for (int i = 0; i < sizeof(int); i++) widthbytes[sizeof(int) - 1 - i] = br.ReadByte();
            int pngWidth = BitConverter.ToInt32(widthbytes, 0);
            byte[] heightbytes = new byte[sizeof(int)];
            for (int i = 0; i < sizeof(int); i++) heightbytes[sizeof(int) - 1 - i] = br.ReadByte();
            int pngHeight = BitConverter.ToInt32(heightbytes, 0);

            int pngMaxResVal = Math.Max(pngWidth, pngHeight);
            int maxMipsRes = Math.Max(int.Parse(resMipsSplit[0]), int.Parse(resMipsSplit[1]));
            int maxMipsHDRes = Math.Max(int.Parse(resMipsHDSplit[0]), int.Parse(resMipsHDSplit[1]));

            int tmp = Math.Max(int.Parse(resBaseSplit[0]), int.Parse(resBaseSplit[1]));

            if (pngMaxResVal != maxMipsHDRes && maxMipsHDRes > 0 && maxMipsRes > 0)
            {
                Console.WriteLine("You set HD MIPS resolution different than source texture! Exiting...");
                Console.ReadKey();
                return;
            }

            if (pngMaxResVal != maxMipsRes && maxMipsHDRes == 0 && maxMipsRes > 0)
            {
                Console.WriteLine("You set MIPS resolution different than source texture! Exiting...");
                Console.ReadKey();
                return;
            }

            if (pngMaxResVal != tmp && maxMipsHDRes == 0 && maxMipsRes == 0)
            {
                Console.WriteLine("You set base resolution different than source texture! Exiting...");
                Console.ReadKey();
                return;
            }

            int mipsCount = 0;
            int mipsHDCount = 0;

            for (int i = 1; i < 10; i++)
            {
                tmp *= 2;
                if (tmp == maxMipsRes) mipsCount = i;
                if (tmp == maxMipsHDRes) mipsHDCount = i;
            }

            string baseFileName = Path.GetFileNameWithoutExtension(file);
            string parentDir = Directory.GetParent(file).FullName;

            string newPNGBase = parentDir + "\\" + baseFileName + ".png";
            string newPNGMips = parentDir + "\\" + baseFileName + "_mips.png";
            string newPNGBaseHD = parentDir + "\\" + baseFileName + "_hd.png";
            string newPNGMipsHD = parentDir + "\\" + baseFileName + "_hd_mips.png";

            ProcessStartInfo info;
            Process proc;

            if (mipsCount > 0)
            {
                File.Copy(file, newPNGMips, true);

                info = new ProcessStartInfo(m_Path + "\\texconv.exe", $"-m 0 -dx10 -f DXT1 -w {resBaseSplit[0]} -h {resBaseSplit[1]} -if FANT_DITHER \"{newPNGBase}\" -y");
                info.WorkingDirectory = parentDir;
                info.UseShellExecute = false;
                proc = Process.Start(info);
                proc.WaitForExit();

                if (mipsHDCount > 0)
                {
                    info = new ProcessStartInfo(m_Path + "\\texconv.exe", $"-m {mipsCount} -dx10 -f DXT1 -w {resMipsSplit[0]} -h {resMipsSplit[1]} -if FANT_DITHER \"{newPNGMips}\" -y");
                    info.WorkingDirectory = parentDir;
                    info.UseShellExecute = false;
                    proc = Process.Start(info);
                    proc.WaitForExit();
                }
                else
                {
                    info = new ProcessStartInfo(m_Path + "\\texconv.exe", $"-m {mipsCount} -dx10 -f DXT1 \"{newPNGMips}\" -y");
                    info.WorkingDirectory = parentDir;
                    info.UseShellExecute = false;
                    proc = Process.Start(info);
                    proc.WaitForExit();
                }

                File.Delete(newPNGMips);
            }
            else
            {
                info = new ProcessStartInfo(m_Path + "\\texconv.exe", $"-m 0 -dx10 -f DXT1 \"{newPNGBase}\" -y");
                info.WorkingDirectory = parentDir;
                info.UseShellExecute = false;
                proc = Process.Start(info);
                proc.WaitForExit();
            }

            if (mipsHDCount > 0)
            {
                File.Copy(file, newPNGBaseHD, true);
                File.Copy(file, newPNGMipsHD, true);

                info = new ProcessStartInfo(m_Path + "\\texconv.exe", $"-m 0 -dx10 -f DXT1 -w {resBaseSplit[0]} -h {resBaseSplit[1]} -if FANT_DITHER \"{newPNGBaseHD}\" -y");
                info.WorkingDirectory = parentDir;
                info.UseShellExecute = false;
                proc = Process.Start(info);
                proc.WaitForExit();

                info = new ProcessStartInfo(m_Path + "\\texconv.exe", $"-m {mipsHDCount} -dx10 -f DXT1 \"{newPNGMipsHD}\" -y");
                info.WorkingDirectory = parentDir;
                info.UseShellExecute = false;
                proc = Process.Start(info);
                proc.WaitForExit();

                File.Delete(newPNGBaseHD);
                File.Delete(newPNGMipsHD);
            }

            string newXBTBase = newPNGBase.Replace(".png", ".xbt");
            string newXBTMips = newPNGMips.Replace(".png", ".xbt");
            string newXBTBaseHD = newPNGBaseHD.Replace(".png", ".xbt");
            string newXBTMipsHD = newPNGMipsHD.Replace(".png", ".xbt");

            string newDDSBase = newPNGBase.Replace(".png", ".dds");
            string newDDSMips = newPNGMips.Replace(".png", ".dds");
            string newDDSBaseHD = newPNGBaseHD.Replace(".png", ".dds");
            string newDDSMipsHD = newPNGMipsHD.Replace(".png", ".dds");

            void convert(string dds, string xbt, string mips, int mipscnt)
            {
                uint ver = 0;
                if (gameStr == "5") ver = 116;
                if (gameStr == "6") ver = 119;

                var xbtHdr = ReadXBTXMLHeader(xmlTemplate, ver, mips, mipscnt);

                FileStream XBTStream = File.Create(xbt);
                XBTStream.WriteBytes(xbtHdr.Item1);
                XBTStream.Seek(sizeof(uint) * 2, SeekOrigin.Begin);
                XBTStream.WriteValueU32((uint)xbtHdr.Item1.Length);
                XBTStream.Seek(xbtHdr.Item1.Length, SeekOrigin.Begin);
                XBTStream.WriteBytes(File.ReadAllBytes(dds));
                XBTStream.Dispose();
                XBTStream.Close();
                File.Delete(dds);
            }

            if (mipsCount > 0)
            {
                convert(newDDSBase, newXBTBase, mipsPathStr + baseFileName + "_mips.xbt", mipsCount);
                convert(newDDSMips, newXBTMips, "", 0);
            }
            else
                convert(newDDSBase, newXBTBase, "", 0);

            if (mipsHDCount > 0)
            {
                convert(newDDSBaseHD, newXBTBaseHD, mipsPathStr + baseFileName + "_mips.xbt", mipsHDCount);
                convert(newDDSMipsHD, newXBTMipsHD, "", 0);
            }

            Console.WriteLine("XBT files were successfully created.");
        }
    }
}
