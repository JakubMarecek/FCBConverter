// Author: id-daemon
// https://zenhax.com/viewtopic.php?f=5&t=12842

using APPLIB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UE4;

internal class ue4
{
	private static int game = 0;

	private static int xbguvs = 2;

	private static string[] names = null;

	private static string readnamef(BinaryReader br)
	{
		string text = "";
		int num = br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			text += (char)br.ReadByte();
		}
		br.ReadByte();
		return text;
	}

	private static void readprops(List<prop> plist, MemoryStream fs, BinaryReader br)
	{
		while (true)
		{
			string text = names[br.ReadInt64()];
			if (text == "None")
			{
				break;
			}
			prop prop = new prop();
			prop.name = text;
			string text2 = prop.type = names[br.ReadInt64()];
			prop.size = br.ReadInt32();
			prop.id = br.ReadInt32();
			prop.fpos = fs.Position;
			switch (text2)
			{
				case "IntProperty":
					br.ReadByte();
					prop.ivalue = br.ReadInt32();
					break;
				case "StructProperty":
					prop.svalue = names[br.ReadInt64()];
					br.ReadByte();
					br.ReadInt64();
					br.ReadInt64();
					prop.fpos = fs.Position;
					fs.Seek(prop.size, SeekOrigin.Current);
					break;
				case "ObjectProperty":
					br.ReadByte();
					prop.ivalue = br.ReadInt32();
					break;
				case "ArrayProperty":
					prop.svalue = names[br.ReadInt64()];
					br.ReadByte();
					fs.Seek(prop.size, SeekOrigin.Current);
					break;
				case "MapProperty":
					fs.Seek(prop.size, SeekOrigin.Current);
					break;
				case "FloatProperty":
					br.ReadByte();
					prop.fvalue = br.ReadSingle();
					break;
				case "QWordProperty":
					br.ReadByte();
					br.ReadUInt64();
					break;
				case "EnumProperty":
					{
						br.ReadByte();
						prop.svalue = names[br.ReadInt64()];
						string text3 = names[br.ReadInt64()];
						break;
					}
				case "BoolProperty":
					prop.ivalue = br.ReadByte();
					br.ReadByte();
					break;
				case "StrProperty":
					{
						br.ReadByte();
						int num2 = br.ReadInt32();
						prop.svalue = "";
						if (num2 > 0)
						{
							for (int i = 0; i < num2 - 1; i++)
							{
								prop.svalue += (char)br.ReadByte();
							}
							br.ReadByte();
						}
						break;
					}
				case "ByteProperty":
					prop.svalue = names[br.ReadInt64()];
					br.ReadByte();
					fs.Seek(prop.size, SeekOrigin.Current);
					break;
				case "NameProperty":
					{
						br.ReadByte();
						prop.svalue = names[br.ReadInt32()];
						int num = br.ReadInt32();
						if (num > 0)
						{
							prop.svalue = prop.svalue + "_" + (num - 1);
						}
						break;
					}
				default:
					br.ReadByte();
					fs.Seek(prop.size, SeekOrigin.Current);
					break;
			}
			plist.Add(prop);
		}
	}

	public static void Convert(string ueasset, string xbg, int type)
	{
		if (type == 0)
		{
			xbguvs = 2;
		}
		if (type == 1)
		{
			xbguvs = 1;
		}

		string workDir = Path.GetDirectoryName(ueasset) + "\\";

		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		string[] array = null;
		FileStream fileStream = new FileStream(xbg, FileMode.Open, FileAccess.Read);
		BinaryReader binaryReader = new BinaryReader(fileStream);
		int num = binaryReader.ReadInt32();
		if (num != 1296388936) //HSEM
		{
			Console.WriteLine("2nd parameter must be XBG mesh file!");
			return;
		}
		fileStream.Seek(24L, SeekOrigin.Current);
		int num2 = binaryReader.ReadInt32();
		int[] array2 = new int[num2];
		long[] array3 = new long[num2];
		int[] array4 = new int[num2];
		int num3 = 0;
		long num4 = 0L;
		for (int i = 0; i < num2; i++)
		{
			array3[i] = fileStream.Position;
			array2[i] = binaryReader.ReadInt32();
			binaryReader.ReadInt32();
			array4[i] = binaryReader.ReadInt32();
			int num5 = binaryReader.ReadInt32();
			binaryReader.ReadInt32();
			if (array2[i] == 1313817669) //EDON
			{
				int num6 = binaryReader.ReadInt32();
				string[] array5 = new string[num6];
				for (int j = 0; j < num6; j++)
				{
					fileStream.Seek(68L, SeekOrigin.Current);
					string text = array5[j] = readnamef(binaryReader);
					int num7 = text.IndexOf(':');
					if (num7 >= 0)
					{
						text = text.Substring(num7 + 1);
					}
					if (!dictionary.ContainsKey(text))
					{
						dictionary.Add(text, j);
					}
				}
			}
			else if (array2[i] == 1397444164) //DNKS
			{
				num4 = array3[i] + array4[i] - num5;
			}
			if (array4[i] > num3)
			{
				num3 = array4[i];
			}
			fileStream.Seek(array3[i] + array4[i], SeekOrigin.Begin);
		}
		int num8 = 0;
		fileStream.Seek(-12L, SeekOrigin.End);
		int num9 = binaryReader.ReadInt32();
		if (num9 == 1296389185) //ATEM
		{
			num8 = binaryReader.ReadInt32();
		}
		NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
		numberFormatInfo.NumberDecimalSeparator = ".";
		FileStream fileStream2 = new FileStream(ueasset, FileMode.Open, FileAccess.Read);
		string str = Path.GetDirectoryName(fileStream2.Name) + "\\" + Path.GetFileNameWithoutExtension(fileStream2.Name);
		int num10 = 0;
		FileStream fileStream3 = null;
		if (File.Exists(str + ".uexp"))
		{
			fileStream3 = new FileStream(str + ".uexp", FileMode.Open, FileAccess.Read);
			num10 = (int)fileStream3.Length;
		}
		byte[] buffer = new byte[fileStream2.Length + num10];
		fileStream2.Read(buffer, 0, (int)fileStream2.Length);
		if (File.Exists(str + ".uexp"))
		{
			fileStream3.Read(buffer, (int)fileStream2.Length, num10);
		}
		MemoryStream memoryStream = new MemoryStream(buffer);
		BinaryReader binaryReader2 = new BinaryReader(memoryStream);
		StreamWriter streamWriter = null;
		StreamWriter streamWriter2 = new StreamWriter(workDir + "log.txt", true);
		StreamWriter streamWriter3 = new StreamWriter(workDir + Path.GetFileNameWithoutExtension(ueasset) + ".names.txt");
		StreamWriter streamWriter4 = new StreamWriter(workDir + Path.GetFileNameWithoutExtension(ueasset) + ".export.txt");
		StreamWriter streamWriter5 = new StreamWriter(workDir + Path.GetFileNameWithoutExtension(ueasset) + ".import.txt");
		memoryStream.Seek(41L, SeekOrigin.Begin);
		int num11 = binaryReader2.ReadInt32();
		long offset = binaryReader2.ReadInt32();
		binaryReader2.ReadInt32();
		binaryReader2.ReadInt32();
		int num12 = binaryReader2.ReadInt32();
		long offset2 = binaryReader2.ReadInt32();
		int num13 = binaryReader2.ReadInt32();
		long offset3 = binaryReader2.ReadInt32();
		memoryStream.Seek(offset, SeekOrigin.Begin);
		names = new string[num11];
		for (int i = 0; i < num11; i++)
		{
			int num14 = binaryReader2.ReadInt32();
			names[i] = "";
			if (num14 < 0)
			{
				num14 = -num14;
				for (int j = 0; j < num14 - 1; j++)
				{
					string[] array6;
					string[] array7 = array6 = names;
					int num15 = i;
					IntPtr intPtr = (IntPtr)num15;
					array7[num15] = array6[(long)intPtr] + (char)binaryReader2.ReadByte();
					binaryReader2.ReadByte();
				}
				memoryStream.Seek(2L, SeekOrigin.Current);
			}
			else
			{
				for (int j = 0; j < num14 - 1; j++)
				{
					string[] array6;
					string[] array8 = array6 = names;
					int num16 = i;
					IntPtr intPtr = (IntPtr)num16;
					array8[num16] = array6[(long)intPtr] + binaryReader2.ReadChar();
				}
				memoryStream.Seek(1L, SeekOrigin.Current);
			}
			binaryReader2.ReadInt32();
			streamWriter3.WriteLine(i.ToString("X") + "\t" + names[i]);
		}
		int num17 = 0;
		string[] array9 = new string[num12 + 1];
		string[] array10 = new string[num13 + 1];
		int[] array11 = new int[num12 + 1];
		int[] array12 = new int[num12 + 1];
		memoryStream.Seek(offset3, SeekOrigin.Begin);
		for (int i = 1; i <= num13; i++)
		{
			int num18 = binaryReader2.ReadInt32();
			binaryReader2.ReadInt32();
			int num19 = binaryReader2.ReadInt32();
			binaryReader2.ReadInt32();
			int num20 = -binaryReader2.ReadInt32();
			array10[i] = names[binaryReader2.ReadInt32()];
			int num21 = binaryReader2.ReadInt32();
			if (num21 > 0)
			{
				string[] array6;
				string[] array13 = array6 = array10;
				int num22 = i;
				IntPtr intPtr = (IntPtr)num22;
				array13[num22] = array6[(long)intPtr] + "_" + (num21 - 1);
			}
			streamWriter5.WriteLine(i.ToString("X") + "\t" + names[num18] + "\t" + names[num19] + "\t->" + num20.ToString("X") + "\t" + array10[i]);
		}
		List<string> list = new List<string>();
		list.Add("");
		List<actor> list2 = new List<actor>();
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		List<land> list3 = new List<land>();
		HashSet<int> hashSet = new HashSet<int>();
		memoryStream.Seek(offset2, SeekOrigin.Begin);
		for (int k = 1; k <= num12; k++)
		{
			num17 = binaryReader2.ReadInt32();
			if (num17 < 0)
			{
				list.Add(array10[-num17]);
			}
			else
			{
				list.Add(array9[num17]);
			}
			binaryReader2.ReadInt32();
			int num20 = binaryReader2.ReadInt32();
			binaryReader2.ReadInt32();
			array9[k] = names[binaryReader2.ReadInt32()];
			int num21 = binaryReader2.ReadInt32();
			if (num21 > 0)
			{
				string[] array6;
				string[] array14 = array6 = array9;
				int num23 = k;
				IntPtr intPtr = (IntPtr)num23;
				array14[num23] = array6[(long)intPtr] + "_" + (num21 - 1);
			}
			binaryReader2.ReadInt32();
			array12[k] = binaryReader2.ReadInt32();
			if (game == 0)
			{
				binaryReader2.ReadInt32();
			}
			array11[k] = binaryReader2.ReadInt32();
			if (game == 0)
			{
				binaryReader2.ReadInt32();
			}
			memoryStream.Seek(60L, SeekOrigin.Current);
			streamWriter4.WriteLine(k.ToString("X") + "\t" + array11[k].ToString("X") + "\t" + array12[k].ToString("X") + "\t->" + num20.ToString("X") + "\t" + array9[k] + "." + list[k]);
		}
		int num24 = 1;
		int[][] array15 = new int[num24][];
		Vector3D[][] array16 = new Vector3D[num24][];
		Vector3D[][] array17 = new Vector3D[num24][];
		Quaternion3D[][] array18 = new Quaternion3D[num24][];
		float[][,] array19 = new float[num24][,];
		float[][,] array20 = new float[num24][,];
		int[][,] array21 = new int[num24][,];
		int[][,] array22 = new int[num24][,];
		int num25 = 0;
		int num26 = 0;
		int num27 = 0;
		int num28 = 0;
		int[] array23 = null;
		int[] array24 = null;
		int[] array25 = null;
		int[][] array26 = null;
		int[] array27 = null;
		int[] array28 = null;
		float num29 = 0f;
		float num30 = 0f;
		for (int l = 1; l <= num12; l++)
		{
			if (list[l].ToLower() == "skeletalmesh")
			{
				memoryStream.Seek(array11[l], SeekOrigin.Begin);
				bool flag = false;
				List<int> list4 = new List<int>();
				List<prop> list5 = new List<prop>();
				readprops(list5, memoryStream, binaryReader2);
				foreach (prop item in list5)
				{
					if (item.name == "bHasVertexColors" && item.ivalue > 0)
					{
						flag = true;
					}
					else
					{
						if (item.name == "Skeleton" || !(item.type == "ArrayProperty") || !(item.name == "LODInfo"))
						{
							continue;
						}
						long position = memoryStream.Position;
						memoryStream.Seek(item.fpos + 9, SeekOrigin.Begin);
						int num31 = binaryReader2.ReadInt32();
						memoryStream.Seek(49L, SeekOrigin.Current);
						for (int i = 0; i < num31; i++)
						{
							List<prop> list6 = new List<prop>();
							readprops(list6, memoryStream, binaryReader2);
							foreach (prop item2 in list6)
							{
								if (item2.type == "ObjectProperty" && item2.name == "MorphTargetSet" && item2.ivalue != 0)
								{
									long position2 = memoryStream.Position;
									memoryStream.Seek(array11[item2.ivalue] + 24, SeekOrigin.Begin);
									int num32 = binaryReader2.ReadInt32();
									for (int j = 0; j < num32; j++)
									{
										list4.Add(binaryReader2.ReadInt32());
									}
									memoryStream.Seek(position2, SeekOrigin.Begin);
								}
							}
						}
						memoryStream.Seek(position, SeekOrigin.Begin);
					}
				}
				streamWriter2.WriteLine(array9[l]);
				string text2 = array9[l];
				MemoryStream memoryStream2 = new MemoryStream();
				MemoryStream memoryStream3 = null;
				StreamWriter streamWriter6 = new StreamWriter(memoryStream2);
				streamWriter = null;
				long position6 = memoryStream.Position;
				memoryStream.Seek(10L, SeekOrigin.Current);
				memoryStream.Seek(24L, SeekOrigin.Current);
				int num33 = binaryReader2.ReadInt32();
				memoryStream.Seek(num33 * 40, SeekOrigin.Current);
				int num34 = binaryReader2.ReadInt32();
				Quaternion3D quaternion3D = new Quaternion3D();
				Vector3D[] array29 = new Vector3D[num34];
				Vector3D[] array30 = new Vector3D[num34];
				Vector3D[] array31 = new Vector3D[num34];
				Quaternion3D[] array32 = new Quaternion3D[num34];
				Quaternion3D[] array33 = new Quaternion3D[num34];
				int[] array34 = new int[num34];
				array = new string[num34];
				streamWriter6.WriteLine(num34);
				for (int i = 0; i < num34; i++)
				{
					array[i] = names[binaryReader2.ReadInt32()];
					int num21 = binaryReader2.ReadInt32();
					if (num21 > 0)
					{
						string[] array6;
						string[] array35 = array6 = array;
						int num35 = i;
						IntPtr intPtr = (IntPtr)num35;
						array35[num35] = array6[(long)intPtr] + "_" + (num21 - 1);
					}
					array34[i] = binaryReader2.ReadInt32();
				}
				binaryReader2.ReadInt32();
				for (int i = 0; i < num34; i++)
				{
					quaternion3D.i = binaryReader2.ReadSingle();
					quaternion3D.j = binaryReader2.ReadSingle();
					quaternion3D.k = binaryReader2.ReadSingle();
					quaternion3D.real = binaryReader2.ReadSingle();
					array32[i] = new Quaternion3D(quaternion3D);
					array29[i] = C3D.ToEulerAngles(quaternion3D);
					array30[i] = new Vector3D(binaryReader2.ReadSingle(), binaryReader2.ReadSingle(), binaryReader2.ReadSingle());
					new Vector3D(binaryReader2.ReadSingle(), binaryReader2.ReadSingle(), binaryReader2.ReadSingle());
				}
				memoryStream.Seek(12 * binaryReader2.ReadInt32(), SeekOrigin.Current);
				for (int j = 0; j < num34; j++)
				{
					int num36 = array34[j];
					if (num36 < 0)
					{
						array31[j] = array30[j];
						array33[j] = array32[j];
						continue;
					}
					array33[j] = array33[num36] * array32[j];
					Quaternion3D right = new Quaternion3D(array30[j], 0f);
					Quaternion3D left = array33[num36] * right;
					Quaternion3D quaternion3D2 = left * new Quaternion3D(array33[num36].real, 0f - array33[num36].i, 0f - array33[num36].j, 0f - array33[num36].k);
					array31[j] = quaternion3D2.xyz;
					Vector3D[] array36;
					Vector3D[] array37 = array36 = array31;
					int num37 = j;
					IntPtr intPtr = (IntPtr)num37;
					array37[num37] = array36[(long)intPtr] + array31[num36];
				}
				for (int j = 0; j < num34; j++)
				{
					array31[j].Y = 0f - array31[j].Y;
					array33[j].i = 0f - array33[j].i;
					array33[j].k = 0f - array33[j].k;
				}
				for (int i = 0; i < num34; i++)
				{
					streamWriter6.WriteLine(array[i]);
					streamWriter6.WriteLine(array34[i]);
					streamWriter6.Write(array31[i].X.ToString("0.000000", numberFormatInfo));
					streamWriter6.Write(" " + array31[i].Y.ToString("0.000000", numberFormatInfo));
					streamWriter6.Write(" " + array31[i].Z.ToString("0.000000", numberFormatInfo));
					streamWriter6.Write(" " + array33[i].i.ToString("0.######", numberFormatInfo));
					streamWriter6.Write(" " + array33[i].j.ToString("0.######", numberFormatInfo));
					streamWriter6.Write(" " + array33[i].k.ToString("0.######", numberFormatInfo));
					streamWriter6.Write(" " + array33[i].real.ToString("0.######", numberFormatInfo));
					streamWriter6.WriteLine();
				}
				if (!Directory.Exists(workDir + "skeletalmesh.raw"))
				{
					Directory.CreateDirectory(workDir + "skeletalmesh.raw");
				}
				int num38 = (int)(array11[l] + array12[l] - memoryStream.Position);
				byte[] array38 = new byte[num38 + 1];
				memoryStream.Read(array38, 1, num38);
				if (flag)
				{
					array38[0] = 1;
				}
				File.WriteAllBytes(workDir + "skeletalmesh.raw\\" + text2 + ".raw", array38);
				memoryStream.Seek(-num38, SeekOrigin.Current);
				int num39 = binaryReader2.ReadInt32();
				for (int m = 0; m < num39; m++)
				{
					int num40 = 0;
					binaryReader2.ReadInt32();
					binaryReader2.ReadInt16();
					num28 = binaryReader2.ReadInt32();
					array23 = new int[num28];
					array24 = new int[num28];
					array25 = new int[num28];
					array26 = new int[num28][];
					array27 = new int[num28];
					array28 = new int[num28];
					for (int i = 0; i < num28; i++)
					{
						binaryReader2.ReadInt16();
						binaryReader2.ReadInt16();
						array23[i] = binaryReader2.ReadInt32();
						array24[i] = binaryReader2.ReadInt32();
						binaryReader2.ReadInt32();
						binaryReader2.ReadInt32();
						array27[i] = binaryReader2.ReadInt32();
						binaryReader2.ReadInt32();
						int num41 = binaryReader2.ReadInt32();
						array26[i] = new int[num41];
						for (int j = 0; j < num41; j++)
						{
							int num42 = binaryReader2.ReadInt16();
							array26[i][j] = num42;
						}
						array28[i] = binaryReader2.ReadInt32();
						binaryReader2.ReadInt32();
						memoryStream.Seek(22L, SeekOrigin.Current);
						memoryStream.Seek(binaryReader2.ReadInt32() * 4, SeekOrigin.Current);
						memoryStream.Seek(binaryReader2.ReadInt32() * 8 + 4, SeekOrigin.Current);
					}
					int num43 = binaryReader2.ReadByte();
					binaryReader2.ReadInt32();
					int num44 = 0;
					array15 = new int[num24][];
					array16 = new Vector3D[num24][];
					array17 = new Vector3D[num24][];
					array18 = new Quaternion3D[num24][];
					array19 = new float[num24][,];
					array20 = new float[num24][,];
					array21 = new int[num24][,];
					array22 = new int[num24][,];
					num25 = 0;
					num26 = 0;
					for (int n = 0; n < num24; n++)
					{
						num27 = binaryReader2.ReadInt32();
						array15[n] = new int[num27];
						for (int i = 0; i < num27; i++)
						{
							if (num43 == 4)
							{
								array15[n][i] = binaryReader2.ReadInt32();
							}
							else
							{
								array15[n][i] = binaryReader2.ReadUInt16();
							}
						}
						memoryStream.Seek(binaryReader2.ReadInt32() * 2, SeekOrigin.Current);
						memoryStream.Seek(binaryReader2.ReadInt32() * 2, SeekOrigin.Current);
						binaryReader2.ReadInt32();
						num25 = binaryReader2.ReadInt32();
						binaryReader2.ReadInt32();
						binaryReader2.ReadInt32();
						array16[n] = new Vector3D[num25];
						array17[n] = new Vector3D[num25];
						array18[n] = new Quaternion3D[num25];
						for (int j = 0; j < num25; j++)
						{
							float num45 = binaryReader2.ReadSingle();
							float num46 = binaryReader2.ReadSingle();
							float num47 = binaryReader2.ReadSingle();
							if (Math.Abs(num45) > num29)
							{
								num29 = Math.Abs(num45);
							}
							if (Math.Abs(num46) > num29)
							{
								num29 = Math.Abs(num46);
							}
							if (Math.Abs(num47) > num29)
							{
								num29 = Math.Abs(num47);
							}
							array16[n][j] = new Vector3D(num45, num46, num47);
						}
						binaryReader2.ReadInt16();
						num44 = binaryReader2.ReadInt32();
						num25 = binaryReader2.ReadInt32();
						binaryReader2.ReadInt32();
						binaryReader2.ReadInt32();
						binaryReader2.ReadInt32();
						binaryReader2.ReadInt32();
						for (int j = 0; j < num25; j++)
						{
							float num45 = (float)binaryReader2.ReadSByte() / 128f;
							float num46 = (float)binaryReader2.ReadSByte() / 128f;
							float num47 = (float)binaryReader2.ReadSByte() / 128f;
							float num48 = (float)binaryReader2.ReadSByte() / 128f;
							array17[n][j] = new Vector3D(num45, num46, num47);
							num45 = (float)binaryReader2.ReadSByte() / 128f;
							num46 = (float)binaryReader2.ReadSByte() / 128f;
							num47 = (float)binaryReader2.ReadSByte() / 128f;
							num48 = (float)binaryReader2.ReadSByte() / 128f;
							array18[n][j] = new Quaternion3D(num48, num45, num46, num47);
						}
						int num49 = binaryReader2.ReadInt32();
						binaryReader2.ReadInt32();
						array19[n] = new float[num25, 4];
						array20[n] = new float[num25, 4];
						array21[n] = new int[num25, 8];
						array22[n] = new int[num25, 8];
						for (int j = 0; j < num25; j++)
						{
							for (int num50 = 0; num50 < num44; num50++)
							{
								float num45;
								float num46;
								if (num49 != 4)
								{
									num45 = binaryReader2.ReadSingle();
									num46 = binaryReader2.ReadSingle();
								}
								else
								{
									num45 = UEClothes.System.Half.ToHalf(binaryReader2.ReadUInt16());
									num46 = UEClothes.System.Half.ToHalf(binaryReader2.ReadUInt16());
								}
								array19[n][j, num50] = num45;
								array20[n][j, num50] = num46;
								if (Math.Abs(num45) > num30)
								{
									num30 = Math.Abs(num45);
								}
								if (Math.Abs(num46) > num30)
								{
									num30 = Math.Abs(num46);
								}
							}
						}
						binaryReader2.ReadInt16();
						binaryReader2.ReadInt32();
						num25 = binaryReader2.ReadInt32();
						int num51 = binaryReader2.ReadInt32();
						num25 = binaryReader2.ReadInt32();
						if (num51 == 4)
						{
							for (int j = 0; j < num25; j++)
							{
								array21[n][j, 0] = binaryReader2.ReadInt32();
								array22[n][j, 0] = 255;
							}
						}
						else
						{
							num26 = 4;
							if (num51 == 16)
							{
								num26 = 8;
							}
							for (int j = 0; j < num25; j++)
							{
								for (int num50 = 0; num50 < num26; num50++)
								{
									array21[n][j, num50] = binaryReader2.ReadByte();
								}
								for (int num50 = 0; num50 < num26; num50++)
								{
									array22[n][j, num50] = binaryReader2.ReadByte();
								}
							}
						}
						if (flag)
						{
							binaryReader2.ReadInt16();
							int num52 = binaryReader2.ReadInt32();
							int num53 = binaryReader2.ReadInt32();
							num52 = binaryReader2.ReadInt32();
							num53 = binaryReader2.ReadInt32();
							memoryStream.Seek(num52 * num53, SeekOrigin.Current);
						}
					}
					num40 += num28;
					memoryStream3 = new MemoryStream();
					streamWriter = new StreamWriter(memoryStream3);
					streamWriter.WriteLine(num40);
					for (int i = 0; i < num28; i++)
					{
						int num54 = array23[i];
						streamWriter.WriteLine("Submesh_" + i);
						streamWriter.WriteLine(num44);
						streamWriter.WriteLine("0");
						streamWriter.WriteLine(array28[i]);
						for (int j = array27[i]; j < array27[i] + array28[i]; j++)
						{
							Vector3D vector3D = array16[array25[i]][j];
							streamWriter.Write(vector3D.X.ToString("0.######", numberFormatInfo));
							streamWriter.Write(" " + (0f - vector3D.Y).ToString("0.######", numberFormatInfo));
							streamWriter.WriteLine(" " + vector3D.Z.ToString("0.######", numberFormatInfo));
							streamWriter.Write(array17[array25[i]][j].X.ToString("0.######", numberFormatInfo));
							streamWriter.Write(" " + (0f - array17[array25[i]][j].Y).ToString("0.######", numberFormatInfo));
							streamWriter.WriteLine(" " + array17[array25[i]][j].Z.ToString("0.######", numberFormatInfo));
							streamWriter.WriteLine("0 0 0 0");
							for (int num50 = 0; num50 < num44; num50++)
							{
								streamWriter.WriteLine(array19[array25[i]][j, num50].ToString("0.######", numberFormatInfo) + " " + array20[array25[i]][j, num50].ToString("0.######", numberFormatInfo));
							}
							streamWriter.Write(array26[i][array21[array25[i]][j, 0]]);
							streamWriter.Write(" " + array26[i][array21[array25[i]][j, 1]]);
							streamWriter.Write(" " + array26[i][array21[array25[i]][j, 2]]);
							streamWriter.Write(" " + array26[i][array21[array25[i]][j, 3]]);
							if (num26 > 4)
							{
								streamWriter.Write(" " + array26[i][array21[array25[i]][j, 4]]);
								streamWriter.Write(" " + array26[i][array21[array25[i]][j, 5]]);
								streamWriter.Write(" " + array26[i][array21[array25[i]][j, 6]]);
								streamWriter.WriteLine(" " + array26[i][array21[array25[i]][j, 7]]);
							}
							else
							{
								streamWriter.WriteLine();
							}
							streamWriter.Write(((float)array22[array25[i]][j, 0] / 255f).ToString("0.######", numberFormatInfo));
							streamWriter.Write(" " + ((float)array22[array25[i]][j, 1] / 255f).ToString("0.######", numberFormatInfo));
							streamWriter.Write(" " + ((float)array22[array25[i]][j, 2] / 255f).ToString("0.######", numberFormatInfo));
							streamWriter.Write(" " + ((float)array22[array25[i]][j, 3] / 255f).ToString("0.######", numberFormatInfo));
							if (num26 > 4)
							{
								streamWriter.Write(" " + ((float)array22[array25[i]][j, 4] / 255f).ToString("0.######", numberFormatInfo));
								streamWriter.Write(" " + ((float)array22[array25[i]][j, 5] / 255f).ToString("0.######", numberFormatInfo));
								streamWriter.Write(" " + ((float)array22[array25[i]][j, 6] / 255f).ToString("0.######", numberFormatInfo));
								streamWriter.WriteLine(" " + ((float)array22[array25[i]][j, 7] / 255f).ToString("0.######", numberFormatInfo));
							}
							else
							{
								streamWriter.WriteLine();
							}
						}
						streamWriter.WriteLine(array24[i]);
						for (int j = 0; j < array24[i]; j++)
						{
							streamWriter.Write(array15[array25[i]][num54 + 2] - array27[i]);
							streamWriter.Write(" " + (array15[array25[i]][num54 + 1] - array27[i]));
							streamWriter.Write(" " + (array15[array25[i]][num54] - array27[i]));
							streamWriter.WriteLine();
							num54 += 3;
						}
					}
					FileStream fileStream4 = new FileStream(workDir + text2 + "_lod" + m + ".ascii", FileMode.Create);
					streamWriter6.Flush();
					array38 = memoryStream2.GetBuffer();
					fileStream4.Write(array38, 0, (int)memoryStream2.Length);
					streamWriter.Flush();
					array38 = memoryStream3.GetBuffer();
					fileStream4.Write(array38, 0, (int)memoryStream3.Length);
					fileStream4.Close();
					streamWriter.Close();
				}
			}
			else if (list[l].ToLower() == "LandscapeComponent".ToLower())
			{
				memoryStream.Seek(array11[l], SeekOrigin.Begin);
				List<prop> list7 = new List<prop>();
				readprops(list7, memoryStream, binaryReader2);
				land land = new land();
				foreach (prop item3 in list7)
				{
					if (item3.name == "SectionBaseX")
					{
						land.hmx = item3.ivalue;
					}
					else if (item3.name == "SectionBaseY")
					{
						land.hmy = item3.ivalue;
					}
					else if (item3.name == "HeightmapTexture")
					{
						land.hmtexture = item3.ivalue;
					}
					else if (item3.name == "HeightmapScaleBias")
					{
						memoryStream.Seek(item3.fpos, SeekOrigin.Begin);
						land.scale = new Vector3D(binaryReader2.ReadSingle(), binaryReader2.ReadSingle(), binaryReader2.ReadSingle());
					}
				}
				list3.Add(land);
			}
			else if (list[l].ToLower() == "staticmeshcomponent")
			{
				memoryStream.Seek(array11[l], SeekOrigin.Begin);
				List<prop> list8 = new List<prop>();
				readprops(list8, memoryStream, binaryReader2);
				actor actor = new actor();
				dictionary2.Add(l, list2.Count);
				foreach (prop item4 in list8)
				{
					if (item4.type == "ObjectProperty" && item4.name == "StaticMesh")
					{
						if (item4.ivalue < 0)
						{
							actor.meshname = array10[-item4.ivalue];
						}
						else
						{
							actor.meshname = array9[item4.ivalue];
						}
					}
					else if (item4.type == "ObjectProperty" && item4.name == "AttachParent")
					{
						actor.parent = item4.ivalue;
					}
					else if (item4.type == "StructProperty" && item4.name == "RelativeLocation")
					{
						memoryStream.Seek(item4.fpos, SeekOrigin.Begin);
						actor.pos = new Vector3D(binaryReader2.ReadSingle(), binaryReader2.ReadSingle(), binaryReader2.ReadSingle());
					}
					else if (item4.type == "StructProperty" && item4.name == "RelativeRotation")
					{
						memoryStream.Seek(item4.fpos, SeekOrigin.Begin);
						actor.rot = new Vector3D(binaryReader2.ReadSingle(), binaryReader2.ReadSingle(), binaryReader2.ReadSingle());
					}
					else if (item4.type == "StructProperty" && item4.name == "RelativeScale3D")
					{
						memoryStream.Seek(item4.fpos, SeekOrigin.Begin);
						actor.scale = new Vector3D(binaryReader2.ReadSingle(), binaryReader2.ReadSingle(), binaryReader2.ReadSingle());
					}
					else if (item4.type == "ArrayProperty" && item4.name == "OverrideMaterials")
					{
						long position3 = memoryStream.Position;
						memoryStream.Seek(item4.fpos + 9, SeekOrigin.Begin);
						int num55 = binaryReader2.ReadInt32();
						actor.overmat = new int[num55];
						for (int i = 0; i < num55; i++)
						{
							actor.overmat[i] = binaryReader2.ReadInt32();
						}
						memoryStream.Seek(position3, SeekOrigin.Begin);
					}
				}
				list2.Add(actor);
			}
			else
			{
				if (!(list[l].ToLower() == "staticmesh"))
				{
					continue;
				}
				memoryStream.Seek(array11[l], SeekOrigin.Begin);
				List<prop> list9 = new List<prop>();
				readprops(list9, memoryStream, binaryReader2);
				string[] array39 = null;
				foreach (prop item5 in list9)
				{
					if (item5.type == "ArrayProperty" && item5.name == "Materials")
					{
						long position4 = memoryStream.Position;
						memoryStream.Seek(item5.fpos + 9, SeekOrigin.Begin);
						int num56 = binaryReader2.ReadInt32();
						array39 = new string[num56];
						for (int i = 0; i < num56; i++)
						{
							array39[i] = array10[-binaryReader2.ReadInt32()];
						}
						memoryStream.Seek(position4, SeekOrigin.Begin);
					}
					else
					{
						if (!(item5.type == "ArrayProperty") || !(item5.name == "StaticMaterials"))
						{
							continue;
						}
						long position5 = memoryStream.Position;
						memoryStream.Seek(item5.fpos + 9, SeekOrigin.Begin);
						int num57 = binaryReader2.ReadInt32();
						array39 = new string[num57];
						memoryStream.Seek(49L, SeekOrigin.Current);
						for (int i = 0; i < num57; i++)
						{
							List<prop> list10 = new List<prop>();
							readprops(list10, memoryStream, binaryReader2);
							foreach (prop item6 in list10)
							{
								if (item6.name == "MaterialSlotName")
								{
									array39[i] = item6.svalue;
								}
							}
						}
						memoryStream.Seek(position5, SeekOrigin.Begin);
					}
				}
				string text3 = array9[l];
				if (!Directory.Exists(workDir + "staticmesh.raw"))
				{
					Directory.CreateDirectory(workDir + "staticmesh.raw");
				}
				int num58 = (int)(array11[l] + array12[l] - memoryStream.Position);
				byte[] array40 = new byte[num58];
				memoryStream.Read(array40, 0, num58);
				File.WriteAllBytes(workDir + "staticmesh.raw\\" + text3 + ".raw", array40);
				memoryStream.Seek(-num58, SeekOrigin.Current);
				StreamWriter streamWriter7 = new StreamWriter(workDir + "staticmesh.raw\\" + text3 + ".txt");
				string[] array41 = array39;
				foreach (string value in array41)
				{
					streamWriter7.WriteLine(value);
				}
				streamWriter7.Close();
				memoryStream.Seek(38L, SeekOrigin.Current);
				int num60 = binaryReader2.ReadInt32();
				for (int num61 = 0; num61 < num60; num61++)
				{
					binaryReader2.ReadByte();
					binaryReader2.ReadByte();
					num28 = binaryReader2.ReadByte();
					binaryReader2.ReadByte();
					binaryReader2.ReadByte();
					binaryReader2.ReadByte();
					int[] array42 = new int[num28];
					array23 = new int[num28];
					array24 = new int[num28];
					array25 = new int[num28];
					array27 = new int[num28];
					array28 = new int[num28];
					for (int i = 0; i < num28; i++)
					{
						array42[i] = binaryReader2.ReadInt32();
						array23[i] = binaryReader2.ReadInt32();
						array24[i] = binaryReader2.ReadInt32();
						array27[i] = binaryReader2.ReadInt32();
						array28[i] = binaryReader2.ReadInt32();
						binaryReader2.ReadInt32();
						binaryReader2.ReadInt32();
					}
					binaryReader2.ReadInt32();
					int num62 = 0;
					array15 = new int[num24][];
					array16 = new Vector3D[num24][];
					array17 = new Vector3D[num24][];
					array19 = new float[num24][,];
					array20 = new float[num24][,];
					array21 = new int[num24][,];
					array22 = new int[num24][,];
					num25 = 0;
					for (int num63 = 0; num63 < num24; num63++)
					{
						binaryReader2.ReadInt32();
						num25 = binaryReader2.ReadInt32();
						binaryReader2.ReadInt32();
						binaryReader2.ReadInt32();
						array16[num63] = new Vector3D[num25];
						for (int j = 0; j < num25; j++)
						{
							float xx = binaryReader2.ReadSingle();
							float yy = binaryReader2.ReadSingle();
							float zz = binaryReader2.ReadSingle();
							array16[num63][j] = new Vector3D(xx, yy, zz);
						}
						binaryReader2.ReadInt16();
						num62 = binaryReader2.ReadInt32();
						binaryReader2.ReadInt32();
						binaryReader2.ReadInt32();
						int num64 = binaryReader2.ReadInt32();
						int num65 = binaryReader2.ReadInt32();
						binaryReader2.ReadInt32();
						num25 = binaryReader2.ReadInt32();
						array17[num63] = new Vector3D[num25];
						array19[num63] = new float[num25, 8];
						array20[num63] = new float[num25, 8];
						for (int j = 0; j < num25; j++)
						{
							if (num65 == 1)
							{
								binaryReader2.ReadInt64();
								float xx = (float)(binaryReader2.ReadUInt16() - 32768) / 32768f;
								float yy = (float)(binaryReader2.ReadUInt16() - 32768) / 32768f;
								float zz = (float)(binaryReader2.ReadUInt16() - 32768) / 32768f;
								binaryReader2.ReadUInt16();
								array17[num63][j] = new Vector3D(xx, yy, zz);
							}
							else
							{
								binaryReader2.ReadInt32();
								float xx = (float)(binaryReader2.ReadByte() - 128) / 128f;
								float yy = (float)(binaryReader2.ReadByte() - 128) / 128f;
								float zz = (float)(binaryReader2.ReadByte() - 128) / 128f;
								binaryReader2.ReadByte();
								array17[num63][j] = new Vector3D(xx, yy, zz);
							}
							for (int num50 = 0; num50 < num62; num50++)
							{
								if (num64 == 1)
								{
									array19[num63][j, num50] = binaryReader2.ReadSingle();
									array20[num63][j, num50] = binaryReader2.ReadSingle();
								}
								else
								{
									array19[num63][j, num50] = UEClothes.System.Half.ToHalf(binaryReader2.ReadUInt16());
									array20[num63][j, num50] = UEClothes.System.Half.ToHalf(binaryReader2.ReadUInt16());
								}
							}
						}
						binaryReader2.ReadInt16();
						int num66 = binaryReader2.ReadInt32();
						int num67 = binaryReader2.ReadInt32();
						if (num66 > 0)
						{
							int num68 = binaryReader2.ReadInt32();
							num67 = binaryReader2.ReadInt32();
							memoryStream.Seek(num68 * num67, SeekOrigin.Current);
						}
						int num69 = binaryReader2.ReadInt32();
						binaryReader2.ReadInt32();
						num27 = binaryReader2.ReadInt32() / 2;
						if (num69 == 1)
						{
							num27 /= 2;
						}
						array15[num63] = new int[num27];
						for (int i = 0; i < num27; i++)
						{
							if (num69 == 1)
							{
								array15[num63][i] = binaryReader2.ReadInt32();
							}
							else
							{
								array15[num63][i] = binaryReader2.ReadUInt16();
							}
						}
						int num70 = 3;
						if (game == 1)
						{
							num70 = 4;
						}
						for (int i = 0; i < num70; i++)
						{
							num69 = binaryReader2.ReadInt32();
							binaryReader2.ReadInt32();
							num27 = binaryReader2.ReadInt32();
							memoryStream.Seek(num27, SeekOrigin.Current);
						}
					}
					if (game == 0)
					{
						memoryStream.Seek(12 * (num28 + 1), SeekOrigin.Current);
					}
					StreamWriter streamWriter8 = new StreamWriter(workDir + text3 + "_lod" + num61 + ".ascii");
					streamWriter8.WriteLine(0);
					streamWriter8.WriteLine(num28);
					for (int i = 0; i < num28; i++)
					{
						int num71 = array23[i];
						streamWriter8.WriteLine("Submesh_" + i);
						streamWriter8.WriteLine(num62);
						streamWriter8.WriteLine("1");
						streamWriter8.WriteLine(array39[array42[i]]);
						streamWriter8.WriteLine("0");
						streamWriter8.WriteLine(array28[i] - array27[i] + 1);
						for (int j = array27[i]; j <= array28[i]; j++)
						{
							Vector3D vector3D2 = array16[array25[i]][j];
							streamWriter8.Write(vector3D2.X.ToString("0.######", numberFormatInfo));
							streamWriter8.Write(" " + (0f - vector3D2.Y).ToString("0.######", numberFormatInfo));
							streamWriter8.WriteLine(" " + vector3D2.Z.ToString("0.######", numberFormatInfo));
							streamWriter8.Write(array17[array25[i]][j].X.ToString("0.######", numberFormatInfo));
							streamWriter8.Write(" " + (0f - array17[array25[i]][j].Y).ToString("0.######", numberFormatInfo));
							streamWriter8.WriteLine(" " + array17[array25[i]][j].Z.ToString("0.######", numberFormatInfo));
							streamWriter8.WriteLine("0 0 0 0");
							for (int num50 = 0; num50 < num62; num50++)
							{
								streamWriter8.WriteLine(array19[array25[i]][j, num50].ToString("0.######", numberFormatInfo) + " " + array20[array25[i]][j, num50].ToString("0.######", numberFormatInfo));
							}
						}
						streamWriter8.WriteLine(array24[i]);
						for (int j = 0; j < array24[i]; j++)
						{
							streamWriter8.Write(array15[array25[i]][num71 + 2] - array27[i]);
							streamWriter8.Write(" " + (array15[array25[i]][num71 + 1] - array27[i]));
							streamWriter8.Write(" " + (array15[array25[i]][num71] - array27[i]));
							streamWriter8.WriteLine();
							num71 += 3;
						}
					}
					streamWriter8.Close();
				}
			}
		}
		streamWriter2.Close();
		streamWriter3.Close();
		streamWriter4.Close();
		streamWriter5.Close();
		if (list3.Count > 0)
		{
			int num72 = 0;
			foreach (land item7 in list3)
			{
				if (!hashSet.Contains(item7.hmtexture))
				{
					hashSet.Add(item7.hmtexture);
					num72++;
				}
			}
			hashSet = new HashSet<int>();
			streamWriter = new StreamWriter(workDir + Path.GetFileNameWithoutExtension(ueasset) + "_landscape.ascii");
			streamWriter.WriteLine("0");
			streamWriter.WriteLine(num72);
			foreach (land item8 in list3)
			{
				if (hashSet.Contains(item8.hmtexture))
				{
					continue;
				}
				hashSet.Add(item8.hmtexture);
				memoryStream.Seek(array11[item8.hmtexture], SeekOrigin.Begin);
				List<prop> plist = new List<prop>();
				readprops(plist, memoryStream, binaryReader2);
				memoryStream.Seek(24L, SeekOrigin.Current);
				int num73 = binaryReader2.ReadInt32();
				int num74 = binaryReader2.ReadInt32();
				binaryReader2.ReadInt32();
				memoryStream.Seek(binaryReader2.ReadInt32(), SeekOrigin.Current);
				binaryReader2.ReadInt32();
				binaryReader2.ReadInt32();
				binaryReader2.ReadInt32();
				int num75 = binaryReader2.ReadInt32();
				memoryStream.Seek(16L, SeekOrigin.Current);
				if (num75 == 1281) //LM
				{
					num73 /= 2;
					num74 /= 2;
					memoryStream.Seek(32L, SeekOrigin.Current);
				}
				streamWriter.WriteLine("Section_" + item8.hmx + "_" + item8.hmy);
				streamWriter.WriteLine(1);
				streamWriter.WriteLine(0);
				streamWriter.WriteLine(num73 * num74);
				for (int i = 0; i < num74; i++)
				{
					for (int j = 0; j < num73; j++)
					{
						uint num76 = binaryReader2.ReadUInt32();
						int num77 = (int)((num76 & 0xFFFFFF) - 8388608);
						float num78 = (float)num77 / 16384f * 50f;
						streamWriter.Write(((float)(item8.hmx + j) * 50f).ToString("0.######", numberFormatInfo));
						streamWriter.Write(" " + ((float)(-item8.hmy - i) * 50f).ToString("0.######", numberFormatInfo));
						streamWriter.WriteLine(" " + num78.ToString("0.######", numberFormatInfo));
						streamWriter.WriteLine("0 0 0");
						streamWriter.WriteLine("0 0 0 0");
						streamWriter.WriteLine(((float)j / (float)num73).ToString("0.######", numberFormatInfo) + " " + ((float)i / (float)num74).ToString("0.######", numberFormatInfo));
					}
				}
				streamWriter.WriteLine((num73 - 1) * (num74 - 1) * 2);
				for (int i = 0; i < num74 - 1; i++)
				{
					for (int j = 0; j < num73 - 1; j++)
					{
						int num79 = i * num73 + j;
						streamWriter.WriteLine(num79 + " " + (num79 + 1) + " " + (num79 + num73));
						streamWriter.WriteLine(num79 + 1 + " " + (num79 + num73 + 1) + " " + (num79 + num73));
					}
				}
			}
			streamWriter.Close();
			streamWriter = null;
		}
		memoryStream.Close();
		FileStream fileStream5 = new FileStream(workDir + Path.GetFileNameWithoutExtension(ueasset) + ".xbg", FileMode.Create);
		BinaryWriter binaryWriter = new BinaryWriter(fileStream5);
		byte[] buffer2 = new byte[num3];
		binaryWriter.Write(num);
		binaryWriter.Write((short)71);
		binaryWriter.Write((short)13);
		binaryWriter.Write(0);
		binaryWriter.Write(0);
		binaryWriter.Write(0);
		binaryWriter.Write(0);
		binaryWriter.Write(0);
		binaryWriter.Write(num2);
		long num80 = 0L;
		int num81 = 0;
		int num82 = 36;
		int num83 = 1146;
		if (xbguvs == 2)
		{
			num82 = 40;
			num83 = 3194;
		}
		string path = workDir + Path.GetFileNameWithoutExtension(ueasset) + ".txt";
		//string path_name = Path.GetFileNameWithoutExtension(args[0]) + "_name.txt";
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(ueasset);
		string str2 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileNameWithoutExtension);
		//string text0 = str2 + "_LOD0";
		float num84 = 6.10351563E-05f;
		float num85 = 3.05175781E-05f;
		Console.WriteLine("Max pos = " + num29 / 100f);
		Console.WriteLine("Max UV = " + num30);
		if (num29 > 200f)
		{
			num84 = 0.000244140625f;
		}
		if (num30 > 4f)
		{
			num85 = 0.000244140625f;
		}
		if (num30 > 16f)
		{
			num85 = 0.0009765625f;
		}
		string[] array43 = null;
		for (int num50 = 0; num50 < num2; num50++)
		{
			if (array2[num50] == 1380799564) //LTMR
			{
				if (File.Exists(path))
				{
					string[] array44 = File.ReadAllLines(path);
					array43 = new string[num28];
					int num86 = 0;
					for (int i = 0; i < num28; i++)
					{
						if (i < array44.Length)
						{
							array43[i] = array44[i];
						}
						else
						{
							array43[i] = "material_" + (i + 1);
						}
						num86 += array43[i].Length;
					}
					num86 = num86 * 2 + num28 * 43 + 4;
					binaryWriter.Write(array2[num50]);
					binaryWriter.Write(1);
					binaryWriter.Write(num86 + 20);
					binaryWriter.Write(num86);
					binaryWriter.Write(0);
					binaryWriter.Write(num28);
					for (int i = 0; i < num28; i++)
					{
						string text4 = "graphics\\_materials\\" + array43[i] + ".material.bin";
						binaryWriter.Write(text4.Length);
						binaryWriter.Write(text4.ToCharArray());
						fileStream5.WriteByte(0);
						binaryWriter.Write(array43[i].Length);
						binaryWriter.Write(array43[i].ToCharArray());
						fileStream5.WriteByte(0);
					}
				}
				else
				{
					fileStream.Seek(array3[num50] + 20, SeekOrigin.Begin);
					int num87 = binaryReader.ReadInt32();
					array43 = new string[num87];
				}
			}
			else if (array2[num50] == 1346587984) //PMCP
			{
				binaryWriter.Write(array2[num50]);
				binaryWriter.Write(1);
				binaryWriter.Write(28);
				binaryWriter.Write(8);
				binaryWriter.Write(0);
				binaryWriter.Write(0);
				binaryWriter.Write(num84);
			}
			else if (array2[num50] == 1430474064) //PMCU
			{
				binaryWriter.Write(array2[num50]);
				binaryWriter.Write(1);
				binaryWriter.Write(28);
				binaryWriter.Write(8);
				binaryWriter.Write(0);
				binaryWriter.Write(0);
				binaryWriter.Write(num85);
			}
			else if (array2[num50] == 1397442884) //DIKS
			{
				binaryWriter.Write(array2[num50]);
				binaryWriter.Write(1);
				binaryWriter.Write(36);
				binaryWriter.Write(16);
				binaryWriter.Write(0);
				binaryWriter.Write(1);
				binaryWriter.Write(305419896); //xV4
				binaryWriter.Write((short)0);
				binaryWriter.Write((short)(-1));
				binaryWriter.Write(0);
			}
			else if (array2[num50] == 1397444164) //DNKS
			{
				binaryWriter.Write(array2[num50]);
				binaryWriter.Write(1);
				//string text5 = "moddedmesh_LOD0";
				string text5 = str2 + "_LOD0";

				if (type == 2)
					text5 = "FRAME_LOD0"; // weapons

				if (type == 3)
					text5 = "CHASSIS_LOD0"; // vehicles

				int num88 = text5.Length + 61;
				int num89 = 1048 * num28 + 4;
				binaryWriter.Write(num89 + num88 + 40);
				binaryWriter.Write(num88);
				binaryWriter.Write(1);
				binaryWriter.Write(1129076051); //SULC
				binaryWriter.Write(1);
				binaryWriter.Write(num89 + 20);
				binaryWriter.Write(num89);
				binaryWriter.Write(0);
				//----
				binaryWriter.Write(num28);
				if (num28 > array43.Length)
				{
					Console.WriteLine("Warning. Not enough materials (" + array43.Length + ") for " + num28 + " submeshes");
				}
				for (int i = 0; i < num28; i++)
				{
					binaryWriter.Write((short)i);
					binaryWriter.Write((short)array24[i]);
					binaryWriter.Write((short)(array24[i] * 3));
					binaryWriter.Write((short)num82);
					binaryWriter.Write((short)array28[i]);
					binaryWriter.Write((short)num83);
					if (array24[i] * 3 > 65535) //яяLC
					{
						Console.WriteLine("Warning. Face count in submesh " + i + " too big: " + array24[i]);
					}
					int num90 = array26[i].Length;
					for (int j = 0; j < num90; j++)
					{
						int num91 = array26[i][j];
						if (dictionary.ContainsKey(array[num91]))
						{
							binaryWriter.Write((short)dictionary[array[num91]]);
							continue;
						}
						Console.WriteLine("Warning. Bone not found in skeleton: " + array[num91]);
						binaryWriter.Write((short)0);
					}
					for (int j = 0; j < 256 - num90; j++)
					{
						binaryWriter.Write((short)(-1));
					}
					for (int j = 0; j < 128; j++)
					{
						binaryWriter.Write(0);
					}
					binaryWriter.Write(0);
					binaryWriter.Write(0);
					binaryWriter.Write(0);
				}
				binaryWriter.Write(1);
				binaryWriter.Write(288f);
				fileStream.Seek(num4 + 8, SeekOrigin.Begin);
				for (int i = 0; i < 10; i++)
				{
					binaryWriter.Write(binaryReader.ReadSingle());
				}
				binaryWriter.Write(0);
				binaryWriter.Write(0);
				binaryWriter.Write(text5.Length);
				binaryWriter.Write(text5.ToCharArray());
				fileStream5.WriteByte(0);
			}
			else if (array2[num50] == 1280263251) //SDOL
			{
				num80 = fileStream5.Position;
				binaryWriter.Write(array2[num50]);
				binaryWriter.Write(1);
				binaryWriter.Write(0);
				binaryWriter.Write(0);
				binaryWriter.Write(0);
				binaryWriter.Write(1);
				binaryWriter.Write(1);
				// lod start
				binaryWriter.Write(288f);
				binaryWriter.Write(1);
				binaryWriter.Write(num83);
				binaryWriter.Write(num82);
				binaryWriter.Write(num25);
				binaryWriter.Write(0);
				binaryWriter.Write(num28);
				for (int i = 0; i < num28; i++)
				{
					binaryWriter.Write(0);
					binaryWriter.Write(0);
					binaryWriter.Write(i);
					binaryWriter.Write(array23[i]);
					binaryWriter.Write(array27[i] + array28[i] - 1);
				}
				binaryWriter.Write(num25 * num82);
				int num92 = 0;
				long num93 = ((fileStream5.Position + 15) & 0xFFFFFFF0) - fileStream5.Position;
				for (int i = 0; i < num93; i++)
				{
					fileStream5.WriteByte((byte)(num93 - i));
				}
				for (int i = 0; i < num25; i++)
				{
					binaryWriter.Write((short)(array16[num92][i].X / num84 / 100f));
					binaryWriter.Write((short)(array16[num92][i].Y / num84 / 100f));
					binaryWriter.Write((short)(array16[num92][i].Z / num84 / 100f));
					binaryWriter.Write((short)1);
					binaryWriter.Write((short)(array19[num92][i, 0] / num85));
					binaryWriter.Write((short)(array20[num92][i, 0] / num85));
					if (xbguvs > 1)
					{
						binaryWriter.Write((short)(array19[num92][i, 1] / num85));
						binaryWriter.Write((short)(array20[num92][i, 1] / num85));
					}
					for (int j = 0; j < 4; j++)
					{
						binaryWriter.Write((byte)array22[num92][i, j]);
					}
					for (int j = 0; j < 4; j++)
					{
						binaryWriter.Write((byte)array21[num92][i, j]);
					}
					for (int j = 4; j < 8; j++)
					{
						binaryWriter.Write((byte)array22[num92][i, j]);
					}
					for (int j = 4; j < 8; j++)
					{
						binaryWriter.Write((byte)array21[num92][i, j]);
					}
					uint num94 = 0u;
					num94 += (uint)((array18[num92][i].k + 1f) * 512f);
					num94 <<= 10;
					num94 += (uint)((array18[num92][i].j + 1f) * 512f);
					num94 <<= 10;
					num94 += (uint)((array18[num92][i].i + 1f) * 512f);
					binaryWriter.Write(num94);
					num94 = 0u;
					num94 += (uint)((array17[num92][i].Z + 1f) * 512f);
					num94 <<= 10;
					num94 += (uint)((array17[num92][i].Y + 1f) * 512f);
					num94 <<= 10;
					num94 += (uint)((array17[num92][i].X + 1f) * 512f);
					if (array18[num92][i].real < 0f)
					{
						//num94 += 3221225472u; //
					}
					else
						num94 += 0xC0000000u;
					binaryWriter.Write(num94);
				}
				binaryWriter.Write(num27);
				num93 = ((fileStream5.Position + 15) & 0xFFFFFFF0) - fileStream5.Position; //(fileStream5.Position + 15L & (long)((ulong)-16)) - fileStream5.Position;
				for (int i = 0; i < num93; i++)
				{
					fileStream5.WriteByte((byte)(num93 - i));
				}
				for (int i = 0; i < num27; i++)
				{
					binaryWriter.Write((short)array15[num92][i]);
				}
				num81 = (int)(fileStream5.Position - num80);
			}
			else if (array2[num50] == 1297238084) //DHRM
			{
				int num95 = 1;
				binaryWriter.Write(array2[num50]);
				binaryWriter.Write(2);
				binaryWriter.Write(num95 * 8 + 25);
				binaryWriter.Write(num95 * 8 + 5);
				binaryWriter.Write(0);
				binaryWriter.Write((byte)0);
				binaryWriter.Write(num95);
				for (int i = 0; i < num95; i++)
				{
					binaryWriter.Write(0L);
				}
			}
			else if (array2[num50] == 5001028) //DOL
			{
				binaryWriter.Write(array2[num50]);
				binaryWriter.Write(1);
				binaryWriter.Write(28);
				binaryWriter.Write(8);
				binaryWriter.Write(0);
				binaryWriter.Write(1);
				binaryWriter.Write(98);
			}
			else
			{
				fileStream.Seek(array3[num50], SeekOrigin.Begin);
				fileStream.Read(buffer2, 0, array4[num50]);
				fileStream5.Write(buffer2, 0, array4[num50]);
			}
		}
		int value2 = (int)fileStream5.Position - 12;
		if (num8 != 0)
		{
			long num96 = ((fileStream5.Position + 3) & 0xFFFFFFFC) - fileStream5.Position; //(fileStream5.Position + 3L & (long)((ulong)-4)) - fileStream5.Position;
			for (int i = 0; i < num96; i++)
			{
				fileStream5.WriteByte(127);
			}
			int value3 = (int)fileStream5.Position;
			int num97 = (int)(fileStream.Length - num8);
			byte[] buffer3 = new byte[num97];
			fileStream.Seek(num8, SeekOrigin.Begin);
			fileStream.Read(buffer3, 0, num97);
			fileStream5.Write(buffer3, 0, num97);
			fileStream5.Seek(-8L, SeekOrigin.End);
			binaryWriter.Write(value3);
		}
		fileStream5.Seek(20L, SeekOrigin.Begin);
		binaryWriter.Write(value2);
		fileStream5.Seek(num80 + 8, SeekOrigin.Begin);
		binaryWriter.Write(num81);
		binaryWriter.Write(num81 - 20);
		binaryWriter.Close();
	}
}
