// Author: id-daemon
// https://zenhax.com/viewtopic.php?f=5&t=12842

// System.HalfHelper
using System.Runtime.InteropServices;

namespace UEClothes.System
{
    [ComVisible(false)]
	internal static class HalfHelper
	{
		private static uint[] mantissaTable = GenerateMantissaTable();

		private static uint[] exponentTable = GenerateExponentTable();

		private static ushort[] offsetTable = GenerateOffsetTable();

		private static ushort[] baseTable = GenerateBaseTable();

		private static sbyte[] shiftTable = GenerateShiftTable();

		private static uint ConvertMantissa(int i)
		{
			uint num = (uint)(i << 13);
			uint num2 = 0u;
			while ((num & 0x800000) == 0)
			{
				num2 -= 8388608;
				num <<= 1;
			}
			num &= 4286578687u;
			num2 += 947912704;
			return num | num2;
		}

		private static uint[] GenerateMantissaTable()
		{
			uint[] array = new uint[2048];
			array[0] = 0u;
			for (int i = 1; i < 1024; i++)
			{
				array[i] = ConvertMantissa(i);
			}
			for (int j = 1024; j < 2048; j++)
			{
				array[j] = (uint)(939524096 + (j - 1024 << 13));
			}
			return array;
		}

		private static uint[] GenerateExponentTable()
		{
			uint[] array = new uint[64];
			array[0] = 0u;
			for (int i = 1; i < 31; i++)
			{
				array[i] = (uint)(i << 23);
			}
			array[31] = 1199570944u;
			array[32] = 2147483648u;
			for (int j = 33; j < 63; j++)
			{
				array[j] = (uint)(2147483648u + (j - 32 << 23));
			}
			array[63] = 3347054592u;
			return array;
		}

		private static ushort[] GenerateOffsetTable()
		{
			ushort[] array = new ushort[64];
			array[0] = 0;
			for (int i = 1; i < 32; i++)
			{
				array[i] = 1024;
			}
			array[32] = 0;
			for (int j = 33; j < 64; j++)
			{
				array[j] = 1024;
			}
			return array;
		}

		private static ushort[] GenerateBaseTable()
		{
			ushort[] array = new ushort[512];
			for (int i = 0; i < 256; i++)
			{
				sbyte b = (sbyte)(127 - i);
				if (b > 24)
				{
					array[i] = 0;
					array[i | 0x100] = 32768;
				}
				else if (b > 14)
				{
					array[i] = (ushort)(1024 >> 18 + b);
					array[i | 0x100] = (ushort)((1024 >> 18 + b) | 0x8000);
				}
				else if (b >= -15)
				{
					array[i] = (ushort)(15 - b << 10);
					array[i | 0x100] = (ushort)((15 - b << 10) | 0x8000);
				}
				else if (b > sbyte.MinValue)
				{
					array[i] = 31744;
					array[i | 0x100] = 64512;
				}
				else
				{
					array[i] = 31744;
					array[i | 0x100] = 64512;
				}
			}
			return array;
		}

		private static sbyte[] GenerateShiftTable()
		{
			sbyte[] array = new sbyte[512];
			for (int i = 0; i < 256; i++)
			{
				sbyte b = (sbyte)(127 - i);
				if (b > 24)
				{
					array[i] = 24;
					array[i | 0x100] = 24;
				}
				else if (b > 14)
				{
					array[i] = (sbyte)(b - 1);
					array[i | 0x100] = (sbyte)(b - 1);
				}
				else if (b >= -15)
				{
					array[i] = 13;
					array[i | 0x100] = 13;
				}
				else if (b > sbyte.MinValue)
				{
					array[i] = 24;
					array[i | 0x100] = 24;
				}
				else
				{
					array[i] = 13;
					array[i | 0x100] = 13;
				}
			}
			return array;
		}

		public unsafe static float HalfToSingle(Half half)
		{
			uint num = mantissaTable[offsetTable[half.value >> 10] + (half.value & 0x3FF)] + exponentTable[half.value >> 10];
			return *(float*)(&num);
		}

		public unsafe static Half SingleToHalf(float single)
		{
			uint num = *(uint*)(&single);
			ushort bits = (ushort)(baseTable[(num >> 23) & 0x1FF] + ((num & 0x7FFFFF) >> (int)shiftTable[num >> 23]));
			return Half.ToHalf(bits);
		}

		public static Half Negate(Half half)
		{
			return Half.ToHalf((ushort)(half.value ^ 0x8000));
		}

		public static Half Abs(Half half)
		{
			return Half.ToHalf((ushort)(half.value & 0x7FFF));
		}

		public static bool IsNaN(Half half)
		{
			return (half.value & 0x7FFF) > 31744;
		}

		public static bool IsInfinity(Half half)
		{
			return (half.value & 0x7FFF) == 31744;
		}

		public static bool IsPositiveInfinity(Half half)
		{
			return half.value == 31744;
		}

		public static bool IsNegativeInfinity(Half half)
		{
			return half.value == 64512;
		}
	}
}