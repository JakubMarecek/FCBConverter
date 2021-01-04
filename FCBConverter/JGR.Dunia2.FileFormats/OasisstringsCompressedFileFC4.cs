using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Gibbed.IO;

// Token: 0x02000002 RID: 2
public class OasisstringsCompressedFileFC4
{
	// Token: 0x17000001 RID: 1
	// (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
	// (set) Token: 0x06000002 RID: 2 RVA: 0x00002058 File Offset: 0x00000258
	public OasisstringsCompressedFile.Node Root { get; set; }

	// Token: 0x17000002 RID: 2
	// (get) Token: 0x06000003 RID: 3 RVA: 0x00002061 File Offset: 0x00000261
	// (set) Token: 0x06000004 RID: 4 RVA: 0x00002069 File Offset: 0x00000269
	public string Language { get; set; }

	// Token: 0x06000005 RID: 5 RVA: 0x00002072 File Offset: 0x00000272
	public OasisstringsCompressedFileFC4()
	{
		this.Language = "english";
	}

	// Token: 0x06000006 RID: 6 RVA: 0x00002097 File Offset: 0x00000297
	public OasisstringsCompressedFileFC4(string language)
	{
		this.Language = language;
	}

	// Token: 0x06000007 RID: 7 RVA: 0x000020B8 File Offset: 0x000002B8
	public OasisstringsCompressedFileFC4(string language, string dictionaryfile)
	{
		this.hashDictionary = new OasisStringsDictionary(dictionaryfile);
	}

	// Token: 0x06000008 RID: 8 RVA: 0x000020E0 File Offset: 0x000002E0
	public void Deserialize(Stream input)
	{
		if (StreamHelpers.ReadValueU32(input) != (uint)this.Unknown1)
		{
			throw new FormatException("not an oasisstrings_compressed.bin file");
		}
		Endian endian = 0;
		this.Unknown2 = StreamHelpers.ReadValueU32(input, endian);
		uint num = StreamHelpers.ReadValueU32(input, endian);
		this.Root = new OasisstringsCompressedFile.Node();
		this.Root.Name = "stringtable";
		OasisstringsCompressedFile.Attribute attribute = new OasisstringsCompressedFile.Attribute();
		attribute.Name = "language";
		attribute.Value = this.Language;
		this.Root.Attributes.Add(attribute);
		int num2 = 0;
		while ((long)num2 < (long)((ulong)num))
		{
			OasisSection oasisSection = new OasisSection(this.hashDictionary);
			oasisSection.Deserialize(input, endian);
			this.Root.Children.Add(oasisSection.EmitNode());
			num2++;
		}
	}

	// Token: 0x06000009 RID: 9 RVA: 0x000021A4 File Offset: 0x000003A4
	public void Serialize(Stream output)
	{
		Endian endian = 0;
		StreamHelpers.WriteValueU32(output, (uint)this.Unknown1, endian);
		StreamHelpers.WriteValueU32(output, this.Unknown2, endian);
		int count = this.Root.Children.Count;
		StreamHelpers.WriteValueS32(output, count);
		foreach (OasisstringsCompressedFile.Node node in this.Root.Children)
		{
			OasisSection oasisSection = new OasisSection(this.hashDictionary, node);
			oasisSection.Serialize(output, endian);
		}
	}

	// Token: 0x04000001 RID: 1
	private ushort Unknown1 = 1;

	// Token: 0x04000002 RID: 2
	private uint Unknown2 = 1457411698U;

	// Token: 0x04000003 RID: 3
	private OasisStringsDictionary hashDictionary;

	// Token: 0x02000003 RID: 3
	public class OasisSection
	{
		// Token: 0x0600000A RID: 10 RVA: 0x00002240 File Offset: 0x00000440
		public OasisSection(OasisStringsDictionary dictionary)
		{
			this.hashDictionary = dictionary;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x0000225A File Offset: 0x0000045A
		public OasisSection(OasisStringsDictionary hashDictionary, OasisstringsCompressedFile.Node node)
		{
			if (node.Name != "section")
			{
				throw new Exception("invalid node type for constructing an OasisSection.");
			}
			this.FromNode(node);
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002294 File Offset: 0x00000494
		public void Deserialize(Stream input, Endian endian)
		{
			this.NameCRC = StreamHelpers.ReadValueU32(input, endian);
			if (this.hashDictionary != null)
			{
				this.Name = this.hashDictionary.GetSectionName(this.NameCRC);
			}
			this.StringCount = StreamHelpers.ReadValueU32(input, endian);
			Dictionary<uint, OasisLocalizedString> dictionary = new Dictionary<uint, OasisLocalizedString>();
			int num = 0;
			while ((long)num < (long)((ulong)this.StringCount))
			{
				OasisLocalizedString oasisLocalizedString = new OasisLocalizedString(this.hashDictionary);
				oasisLocalizedString.Deserialize(input, endian, this.NameCRC);
				this.LocalizedStrings.Add(oasisLocalizedString);
				dictionary.Add(oasisLocalizedString.Id, oasisLocalizedString);
				num++;
			}
			this.CompressedValuesSectionsCount = StreamHelpers.ReadValueU32(input, endian);
			int num2 = 0;
			while ((long)num2 < (long)((ulong)this.CompressedValuesSectionsCount))
			{
				CompressedValues compressedValues = new CompressedValues();
				compressedValues.Deserialize(input, endian);
				byte[] array = new byte[compressedValues.DecompressedSize];
				int decompressedSize = compressedValues.DecompressedSize;
                Gibbed.Dunia2.FileFormats.LZO.Decompress(compressedValues.CompressedBytes, 0, compressedValues.CompressedSize, array, 0, ref decompressedSize);
				if (decompressedSize != compressedValues.DecompressedSize)
				{
					throw new FormatException("compressed values section of oassisstring section decompressed to an unexpected size.");
				}
				DecompressedValues decompressedValues = new DecompressedValues();
				using (Stream stream = new MemoryStream(array))
				{
					decompressedValues.Deserialize(stream, endian);
				}
				foreach (KeyValuePair<uint, string> keyValuePair in decompressedValues.IdValuePairs)
				{
					dictionary[keyValuePair.Key].Value = keyValuePair.Value;
				}
				num2++;
			}
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002434 File Offset: 0x00000634
		public void Serialize(Stream output, Endian endian)
		{
			StreamHelpers.WriteValueU32(output, this.NameCRC, endian);
			StreamHelpers.WriteValueU32(output, this.StringCount, endian);
			foreach (OasisLocalizedString oasisLocalizedString in this.LocalizedStrings)
			{
				oasisLocalizedString.Serialize(output, endian);
			}
			StreamHelpers.WriteValueU32(output, 1U);
			DecompressedValues decompressedValues = new DecompressedValues(this.LocalizedStrings);
			CompressedValues compressedValues = new CompressedValues();
			using (Stream stream = new MemoryStream())
			{
				decompressedValues.Serialize(stream, endian);
				MemoryStream memoryStream = new MemoryStream();
				stream.Position = 0L;
				byte[] array = new byte[4096];
				int count;
				while ((count = stream.Read(array, 0, array.Length)) != 0)
				{
					memoryStream.Write(array, 0, count);
				}
				byte[] array2 = memoryStream.ToArray();
				int num = array2.Length;
				byte[] array3 = new byte[LocalizedStrings.Count == 0 ? 8 : (int)Math.Ceiling((double)num * 1.2)];
				int num2 = 1;
				Gibbed.Dunia2.FileFormats.LZO.Compress(array2, 0, num, array3, 0, ref num2);
				compressedValues.CompressedBytes = new byte[num2];
				Array.Copy(array3, compressedValues.CompressedBytes, num2);
				compressedValues.CompressedSize = num2;
				compressedValues.DecompressedSize = num;

				if (LocalizedStrings.Count == 0)
					compressedValues.LastSortedCRC = 0;
                else
					compressedValues.LastSortedCRC = decompressedValues.SortedEnums[decompressedValues.SortedEnums.Count - 1];
			}
			compressedValues.Serialize(output, endian);
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000025A8 File Offset: 0x000007A8
		public OasisstringsCompressedFile.Node EmitNode()
		{
			if (this.NameCRC == 0U || this.StringCount <= 0U || this.LocalizedStrings.Count <= 0 || this.CompressedValuesSectionsCount <= 0U || this.LocalizedStrings[0].Value == "")
			{
				this.NameCRC = this.NameCRC;
			}
			OasisstringsCompressedFile.Node node = new OasisstringsCompressedFile.Node();
			node.Name = "section";
			OasisstringsCompressedFile.Attribute attribute = new OasisstringsCompressedFile.Attribute();
			attribute.Name = "name";
			if (this.Name != null)
			{
				attribute.Value = this.Name;
			}
			else
			{
				attribute.Value = string.Format("0x{0,8:X8}", this.NameCRC);
			}
			node.Attributes.Add(attribute);
			foreach (OasisLocalizedString oasisLocalizedString in this.LocalizedStrings)
			{
				node.Children.Add(oasisLocalizedString.EmitNode());
			}
			return node;
		}

		// Token: 0x0600000F RID: 15 RVA: 0x000026B8 File Offset: 0x000008B8
		public void FromNode(OasisstringsCompressedFile.Node node)
		{
			if (node.Name != "section" || node.Attributes[0].Name != "name")
			{
				throw new Exception("invalid node type for populating an OasisSection.");
			}
			if (node.Attributes[0].Value.StartsWith("0x"))
			{
				this.NameCRC = uint.Parse(node.Attributes[0].Value.Substring(2), NumberStyles.HexNumber);
			}
			else
			{
				this.Name = node.Attributes[0].Value;
				this.NameCRC = Gibbed.Dunia2.FileFormats.CRC32.Hash(this.Name);
			}
			this.StringCount = (uint)node.Children.Count;
			foreach (OasisstringsCompressedFile.Node node2 in node.Children)
			{
				this.LocalizedStrings.Add(new OasisLocalizedString(this.hashDictionary, node2, this.NameCRC));
			}
		}

		// Token: 0x04000006 RID: 6
		public string Name;

		// Token: 0x04000007 RID: 7
		public uint NameCRC;

		// Token: 0x04000008 RID: 8
		public uint StringCount;

		// Token: 0x04000009 RID: 9
		public List<OasisLocalizedString> LocalizedStrings = new List<OasisLocalizedString>();

		// Token: 0x0400000A RID: 10
		public uint CompressedValuesSectionsCount;

		// Token: 0x0400000B RID: 11
		private OasisStringsDictionary hashDictionary;

		// Token: 0x02000004 RID: 4
		public class OasisLocalizedString
		{
			// Token: 0x06000010 RID: 16 RVA: 0x000027DC File Offset: 0x000009DC
			public OasisLocalizedString(OasisStringsDictionary dictionary)
			{
				this.hashDictionary = dictionary;
			}

			// Token: 0x06000011 RID: 17 RVA: 0x000027EB File Offset: 0x000009EB
			public OasisLocalizedString(OasisStringsDictionary dictionary, OasisstringsCompressedFile.Node node, uint sectionCRC)
			{
				this.hashDictionary = dictionary;
				if (node.Name != "string")
				{
					throw new Exception("invalide node type for constructing an OasisLocalizedString.");
				}
				this.FromNode(node, sectionCRC);
			}

			// Token: 0x06000012 RID: 18 RVA: 0x00002820 File Offset: 0x00000A20
			public void Deserialize(Stream input, Endian endian, uint nameCRC)
			{
				this.Id = StreamHelpers.ReadValueU32(input, endian);
				this.SectionCRC = StreamHelpers.ReadValueU32(input, endian);
				if (this.SectionCRC != nameCRC)
				{
					throw new FormatException("oasis string section CRC does not match the section's CRC value.");
				}
				this.EnumCRC = StreamHelpers.ReadValueU32(input, endian);
				if (this.hashDictionary != null)
				{
					this.Enum = this.hashDictionary.GetEnumName(this.SectionCRC, this.EnumCRC);
				}
				this.MainCRC = StreamHelpers.ReadValueU32(input, endian);
				/*if (this.MainCRC != 3207122276U && MainCRC != 521822810U)
				{
					throw new FormatException("encountered oasisstring with MainCRC different from CRC(\"Main\").");
				}*/
			}

			// Token: 0x06000013 RID: 19 RVA: 0x000028B2 File Offset: 0x00000AB2
			public void Serialize(Stream output, Endian endian)
			{
				StreamHelpers.WriteValueU32(output, this.Id, endian);
				StreamHelpers.WriteValueU32(output, this.SectionCRC, endian);
				StreamHelpers.WriteValueU32(output, this.EnumCRC, endian);
				StreamHelpers.WriteValueU32(output, this.MainCRC, endian);
			}

			// Token: 0x06000014 RID: 20 RVA: 0x000028E8 File Offset: 0x00000AE8
			public OasisstringsCompressedFile.Node EmitNode()
			{
				if (this.EnumCRC == 0U || this.Id == 0U)
				{
					throw new FormatException("attempting to emit XML node prior to proper OasisLocalizedString construction.");
				}
				OasisstringsCompressedFile.Node node = new OasisstringsCompressedFile.Node();
				node.Name = "string";
				OasisstringsCompressedFile.Attribute attribute = new OasisstringsCompressedFile.Attribute();
				attribute.Name = "enum";
				if (this.Enum != null)
				{
					attribute.Value = this.Enum;
				}
				else
				{
					attribute.Value = string.Format("0x{0,8:X8}", this.EnumCRC);
				}
				node.Attributes.Add(attribute);
				OasisstringsCompressedFile.Attribute attribute4 = new OasisstringsCompressedFile.Attribute();
				attribute4.Name = "main";
				attribute4.Value = string.Format("0x{0,8:X8}", this.MainCRC);
				node.Attributes.Add(attribute4);
				OasisstringsCompressedFile.Attribute attribute2 = new OasisstringsCompressedFile.Attribute();
				attribute2.Name = "id";
				attribute2.Value = this.Id.ToString();
				node.Attributes.Add(attribute2);
				OasisstringsCompressedFile.Attribute attribute3 = new OasisstringsCompressedFile.Attribute();
				attribute3.Name = "value";
				attribute3.Value = this.Value;
				node.Attributes.Add(attribute3);
				return node;
			}

			// Token: 0x06000015 RID: 21 RVA: 0x000029C8 File Offset: 0x00000BC8
			public void FromNode(OasisstringsCompressedFile.Node node, uint sectionCRC)
			{
				if (node.Name != "string" || node.Attributes[0].Name != "enum" || node.Attributes[1].Name != "main" || node.Attributes[2].Name != "id" || node.Attributes[3].Name != "value")
				{
					throw new Exception("invalid node type for populating and OasisLocalizedString.");
				}
				if (node.Attributes[0].Value.StartsWith("0x"))
				{
					this.EnumCRC = uint.Parse(node.Attributes[0].Value.Substring(2), NumberStyles.HexNumber);
				}
				else
				{
					this.Enum = node.Attributes[0].Value;
					this.EnumCRC = Gibbed.Dunia2.FileFormats.CRC32.Hash(this.Enum);
				}

				if (node.Attributes[1].Value.StartsWith("0x"))
				{
					this.MainCRC = uint.Parse(node.Attributes[1].Value.Substring(2), NumberStyles.HexNumber);
				}
				else
				{
					this.MainCRC = Gibbed.Dunia2.FileFormats.CRC32.Hash(node.Attributes[1].Value);
				}

				if (node.Attributes[2].Value.StartsWith("0x"))
				{
					this.Id = uint.Parse(node.Attributes[2].Value.Substring(2), NumberStyles.HexNumber);
				}
				else
				{
					this.Id = uint.Parse(node.Attributes[2].Value);
				}
				this.Value = node.Attributes[3].Value;
				this.SectionCRC = sectionCRC;
				//this.MainCRC = 3207122276U;
			}

			// Token: 0x0400000D RID: 13
			public string Enum;

			// Token: 0x0400000E RID: 14
			public uint EnumCRC;

			// Token: 0x0400000F RID: 15
			public uint Id;

			// Token: 0x04000010 RID: 16
			public uint SectionCRC;

			// Token: 0x04000011 RID: 17
			public uint MainCRC;

			// Token: 0x04000012 RID: 18
			public string Value;

			// Token: 0x04000013 RID: 19
			private OasisStringsDictionary hashDictionary;
		}

		// Token: 0x02000005 RID: 5
		private class CompressedValues
		{
			// Token: 0x06000016 RID: 22 RVA: 0x00002B42 File Offset: 0x00000D42
			public void Deserialize(Stream input, Endian endian)
			{
				this.LastSortedCRC = StreamHelpers.ReadValueU32(input, endian);
				this.CompressedSize = StreamHelpers.ReadValueS32(input, endian);
				this.DecompressedSize = StreamHelpers.ReadValueS32(input, endian);
				this.CompressedBytes = StreamHelpers.ReadBytes(input, this.CompressedSize);
			}

			// Token: 0x06000017 RID: 23 RVA: 0x00002B7D File Offset: 0x00000D7D
			public void Serialize(Stream output, Endian endian)
			{
				StreamHelpers.WriteValueU32(output, this.LastSortedCRC, endian);
				StreamHelpers.WriteValueS32(output, this.CompressedSize, endian);
				StreamHelpers.WriteValueS32(output, this.DecompressedSize, endian);
				StreamHelpers.WriteBytes(output, this.CompressedBytes);
			}

			// Token: 0x04000014 RID: 20
			public uint LastSortedCRC;

			// Token: 0x04000015 RID: 21
			public int CompressedSize;

			// Token: 0x04000016 RID: 22
			public int DecompressedSize;

			// Token: 0x04000017 RID: 23
			public byte[] CompressedBytes;
		}

		// Token: 0x02000006 RID: 6
		private class DecompressedValues
		{
			// Token: 0x06000019 RID: 25 RVA: 0x00002BBA File Offset: 0x00000DBA
			public DecompressedValues()
			{
			}

			// Token: 0x0600001A RID: 26 RVA: 0x00002BCC File Offset: 0x00000DCC
			public DecompressedValues(List<OasisLocalizedString> oaStrings)
			{
				this.StringCount = oaStrings.Count;
				this.SortedEnums = new List<uint>();
				this.StringOffsets = new List<int>();
				this.IdValuePairs = new Dictionary<uint, string>();
				int num = 0;
				List<OasisLocalizedString> list = Enumerable.OrderBy(oaStrings, (OasisLocalizedString x) => x.EnumCRC).ToList();
				foreach (OasisLocalizedString oasisLocalizedString in list)
				{
					this.SortedEnums.Add(oasisLocalizedString.EnumCRC);
					this.StringOffsets.Add(num);
					uint id = oasisLocalizedString.Id;
					string value = oasisLocalizedString.Value;
					this.IdValuePairs.Add(id, value);
					byte[] bytes = Encoding.Unicode.GetBytes(value);
					num += bytes.Length + 6;
				}
			}

			// Token: 0x0600001B RID: 27 RVA: 0x00002CC4 File Offset: 0x00000EC4
			public void Deserialize(Stream input, Endian endian)
			{
				this.StringCount = StreamHelpers.ReadValueS32(input, endian);
				this.SortedEnums = new List<uint>();
				for (int i = 0; i < this.StringCount; i++)
				{
					this.SortedEnums.Add(StreamHelpers.ReadValueU32(input, endian));
				}
				this.StringOffsets = new List<int>();
				for (int j = 0; j < this.StringCount; j++)
				{
					this.StringOffsets.Add(StreamHelpers.ReadValueS32(input, endian));
				}
				this.IdValuePairs = new Dictionary<uint, string>();
				for (int k = 0; k < this.StringCount; k++)
				{
					uint key = StreamHelpers.ReadValueU32(input, endian);
					string value = StreamHelpers.ReadStringZ(input, Encoding.Unicode);
					this.IdValuePairs.Add(key, value);
				}
			}

			// Token: 0x0600001C RID: 28 RVA: 0x00002D7C File Offset: 0x00000F7C
			public void Serialize(Stream output, Endian endian)
			{
				StreamHelpers.WriteValueS32(output, this.StringCount, endian);
				foreach (uint num in this.SortedEnums)
				{
					StreamHelpers.WriteValueU32(output, num, endian);
				}
				foreach (int num2 in this.StringOffsets)
				{
					StreamHelpers.WriteValueS32(output, num2, endian);
				}
				foreach (KeyValuePair<uint, string> keyValuePair in this.IdValuePairs)
				{
					StreamHelpers.WriteValueU32(output, keyValuePair.Key, endian);
					StreamHelpers.WriteStringZ(output, keyValuePair.Value, Encoding.Unicode);
				}
			}

			// Token: 0x04000018 RID: 24
			public int StringCount;

			// Token: 0x04000019 RID: 25
			public List<uint> SortedEnums;

			// Token: 0x0400001A RID: 26
			public List<int> StringOffsets;

			// Token: 0x0400001B RID: 27
			public Dictionary<uint, string> IdValuePairs;
		}
	}

	// Token: 0x02000009 RID: 9
	public class OasisStringsDictionary
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000020 RID: 32 RVA: 0x00002EA6 File Offset: 0x000010A6
		// (set) Token: 0x06000021 RID: 33 RVA: 0x00002EAE File Offset: 0x000010AE
		public string DictionaryFile { get; set; }

		// Token: 0x06000022 RID: 34 RVA: 0x00002EB8 File Offset: 0x000010B8
		public OasisStringsDictionary(string dictionaryFile)
		{
			this.DictionaryFile = dictionaryFile;
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(dictionaryFile);
			foreach (object obj in xmlDocument.SelectNodes("stringtable/section"))
			{
				XmlNode xmlNode = (XmlNode)obj;
				this.sections.Add(Gibbed.Dunia2.FileFormats.CRC32.Hash(xmlNode.Attributes["name"].Value), new Section(xmlNode));
			}
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00002F60 File Offset: 0x00001160
		public string GetSectionName(uint sectionhash)
		{
			return this.sections[sectionhash].Name;
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00002F73 File Offset: 0x00001173
		public string GetEnumName(uint sectionhash, uint enumhash)
		{
			if (this.sections[sectionhash] != null)
			{
				return this.sections[sectionhash].Enums[enumhash];
			}
			return null;
		}

		// Token: 0x04000023 RID: 35
		private Dictionary<uint, Section> sections = new Dictionary<uint, Section>();

		// Token: 0x0200000A RID: 10
		private class Section
		{
			// Token: 0x17000004 RID: 4
			// (get) Token: 0x06000025 RID: 37 RVA: 0x00002F9C File Offset: 0x0000119C
			// (set) Token: 0x06000026 RID: 38 RVA: 0x00002FA4 File Offset: 0x000011A4
			public string Name { get; set; }

			// Token: 0x06000027 RID: 39 RVA: 0x00002FB0 File Offset: 0x000011B0
			public Section(XmlNode node)
			{
				this.Name = node.Attributes["name"].Value;
				foreach (object obj in node.ChildNodes)
				{
					XmlNode xmlNode = (XmlNode)obj;
					try
					{
						this.Enums.Add(Gibbed.Dunia2.FileFormats.CRC32.Hash(xmlNode.Attributes["enum"].Value), xmlNode.Attributes["enum"].Value);
					}
					catch
					{
					}
				}
			}

			// Token: 0x04000025 RID: 37
			public Dictionary<uint, string> Enums = new Dictionary<uint, string>();
		}
	}
}
