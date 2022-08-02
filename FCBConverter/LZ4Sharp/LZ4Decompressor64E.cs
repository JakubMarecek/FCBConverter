using System;
using LZ4Sharp;

public class LZ4Decompressor64E
{
	private const int STEPSIZE = 8;

	private readonly sbyte[] m_DecArray = new sbyte[8] { 0, 3, 2, 3, 0, 0, 0, 0 };

	private readonly sbyte[] m_Dec2table = new sbyte[8] { 0, 0, 0, -1, 0, 1, 2, 3 };

	public void DecompressKnownSize(byte[] compressed, byte[] decompressed)
	{
		DecompressKnownSize(compressed, decompressed, decompressed.Length);
	}

	public unsafe int DecompressKnownSize(byte[] compressed, byte[] decompressedBuffer, int decompressedSize)
	{
		fixed (byte* compressed2 = compressed)
		{
			fixed (byte* decompressedBuffer2 = decompressedBuffer)
			{
				return DecompressKnownSize(compressed2, decompressedBuffer2, decompressedSize);
			}
		}
	}

	public unsafe int DecompressKnownSize(byte* compressed, byte* decompressedBuffer, int decompressedSize)
	{
		fixed (sbyte* ptr7 = m_DecArray)
		{
			fixed (sbyte* ptr6 = m_Dec2table)
			{
				byte* ptr = compressed;
				byte* ptr2 = decompressedBuffer;
				byte* ptr3 = ptr2 + decompressedSize;
				while (true)
				{
					byte b = *(ptr++);
					int num;
					if ((long)(num = b >> 4) == 15)
					{
						int num2;
						while ((num2 = *(ptr++)) == 255)
						{
							num += 255;
						}
						num += num2;
					}
					byte* ptr4 = ptr2 + num;
					if (ptr4 > ptr3 - 8)
					{
						if (ptr4 > ptr3)
						{
							break;
						}
						LZ4Util.CopyMemory(ptr2, ptr, num);
						ptr += num;
					}
					else
					{
						do
						{
							*(long*)ptr2 = *(long*)ptr;
							ptr2 += 8;
							ptr += 8;
						}
						while (ptr2 < ptr4);
						ptr -= ptr2 - ptr4;
						ptr2 = ptr4;
						byte* ptr5 = ptr4 - (int)(*(ushort*)ptr);
						ptr += 2;
						if (ptr5 < decompressedBuffer)
						{
							break;
						}
						if ((long)(num = b & 0xF) == 15)
						{
							while (*ptr == byte.MaxValue)
							{
								ptr++;
								num += 255;
							}
							num += *(ptr++);
						}
						if (ptr2 - ptr5 < 8)
						{
							sbyte b2 = ptr6[(int)(ptr2 - ptr5)];
							*(ptr2++) = *(ptr5++);
							*(ptr2++) = *(ptr5++);
							*(ptr2++) = *(ptr5++);
							*(ptr2++) = *(ptr5++);
							ptr5 -= ptr7[ptr2 - ptr5];
							*(int*)ptr2 = *(int*)ptr5;
							ptr2 += 4;
							ptr5 -= b2;
						}
						else
						{
							*(long*)ptr2 = *(long*)ptr5;
							ptr2 += 8;
							ptr5 += 8;
						}
						ptr4 = ptr2 + num - 4;
						if (ptr4 <= ptr3 - 8)
						{
							if (ptr2 < ptr4)
							{
								do
								{
									*(long*)ptr2 = *(long*)ptr5;
									ptr2 += 8;
									ptr5 += 8;
								}
								while (ptr2 < ptr4);
							}
							ptr2 = ptr4;
							continue;
						}
						if (ptr4 > ptr3)
						{
							break;
						}
						if (ptr2 < ptr3 - 8)
						{
							do
							{
								*(long*)ptr2 = *(long*)ptr5;
								ptr2 += 8;
								ptr5 += 8;
							}
							while (ptr2 < ptr3 - 8);
						}
						while (ptr2 < ptr4)
						{
							*(ptr2++) = *(ptr5++);
						}
						ptr2 = ptr4;
						if (ptr2 != ptr3)
						{
							continue;
						}
					}
					return (int)(ptr - compressed);
				}
				return (int)(-(ptr - compressed));
			}
		}
	}

	public byte[] Decompress(byte[] compressed)
	{
		int num = compressed.Length;
		byte[] array;
		int num2;
		do
		{
			num *= 4;
			array = new byte[num];
			num2 = Decompress(compressed, array, compressed.Length);
		}
		while (num2 < 0 || array.Length < num2);
		byte[] array2 = new byte[num2];
		Buffer.BlockCopy(array, 0, array2, 0, array2.Length);
		return array2;
	}

	public int Decompress(byte[] compressed, byte[] decompressedBuffer)
	{
		return Decompress(compressed, decompressedBuffer, compressed.Length);
	}

	public unsafe int Decompress(byte[] compressedBuffer, byte[] decompressedBuffer, int compressedSize)
	{
		fixed (byte* compressedBuffer2 = compressedBuffer)
		{
			fixed (byte* decompressedBuffer2 = decompressedBuffer)
			{
				return Decompress(compressedBuffer2, decompressedBuffer2, compressedSize, decompressedBuffer.Length);
			}
		}
	}

	public unsafe int Decompress(byte[] compressedBuffer, int compressedPosition, byte[] decompressedBuffer, int decompressedPosition, int compressedSize)
	{
		fixed (byte* compressedBuffer2 = &compressedBuffer[compressedPosition])
		{
			fixed (byte* decompressedBuffer2 = &decompressedBuffer[decompressedPosition])
			{
				return Decompress(compressedBuffer2, decompressedBuffer2, compressedSize, decompressedBuffer.Length);
			}
		}
	}

	public unsafe int Decompress(byte* compressedBuffer, byte* decompressedBuffer, int compressedSize, int maxDecompressedSize)
	{
		fixed (sbyte* ptr8 = m_DecArray)
		{
			fixed (sbyte* ptr7 = m_Dec2table)
			{
				byte* ptr = compressedBuffer;
				byte* ptr2 = ptr + compressedSize;
				byte* ptr3 = decompressedBuffer;
				byte* ptr4 = ptr3 + maxDecompressedSize;
				while (true)
				{
					if (ptr < ptr2)
					{
						byte b = *(ptr++);
						int num;
						if ((long)(num = b >> 4) == 15)
						{
							int num2 = 255;
							while (ptr < ptr2 && num2 == 255)
							{
								num2 = *(ptr++);
								num += num2;
							}
						}
						byte* ptr5 = ptr3 + num;
						if (ptr5 > ptr4 - 8 || ptr + num > ptr2 - 8)
						{
							if (ptr5 > ptr4 || ptr + num > ptr2)
							{
								break;
							}
							LZ4Util.CopyMemory(ptr3, ptr, num);
							ptr3 += num;
							ptr += num;
							if (ptr < ptr2)
							{
								break;
							}
						}
						else
						{
							do
							{
								*(long*)ptr3 = *(long*)ptr;
								ptr3 += 8;
								ptr += 8;
							}
							while (ptr3 < ptr5);
							ptr -= ptr3 - ptr5;
							ptr3 = ptr5;
							int num3 = *(ushort*)ptr;
							ptr += 2;
							if (num3 >= 57344)
							{
								num3 += *(ptr++) << 13;
							}
							byte* ptr6 = ptr5 - num3;
							if (ptr6 < decompressedBuffer)
							{
								break;
							}
							if ((long)(num = b & 0xF) == 15)
							{
								while (ptr < ptr2)
								{
									int num4 = *(ptr++);
									num += num4;
									if (num4 != 255)
									{
										break;
									}
								}
							}
							if (ptr3 - ptr6 < 8)
							{
								sbyte b2 = ptr7[ptr3 - ptr6];
								*(ptr3++) = *(ptr6++);
								*(ptr3++) = *(ptr6++);
								*(ptr3++) = *(ptr6++);
								*(ptr3++) = *(ptr6++);
								ptr6 -= ptr8[ptr3 - ptr6];
								*(int*)ptr3 = *(int*)ptr6;
								ptr3 += 4;
								ptr6 -= b2;
							}
							else
							{
								*(long*)ptr3 = *(long*)ptr6;
								ptr3 += 8;
								ptr6 += 8;
							}
							ptr5 = ptr3 + num - 4;
							if (ptr5 > ptr4 - 8)
							{
								if (ptr5 > ptr4)
								{
									break;
								}
								if (ptr3 < ptr4 - 8)
								{
									do
									{
										*(long*)ptr3 = *(long*)ptr6;
										ptr3 += 8;
										ptr6 += 8;
									}
									while (ptr3 < ptr4 - 8);
								}
								while (ptr3 < ptr5)
								{
									*(ptr3++) = *(ptr6++);
								}
								ptr3 = ptr5;
								if (ptr3 == ptr4)
								{
									break;
								}
								continue;
							}
							if (ptr3 < ptr5)
							{
								do
								{
									*(long*)ptr3 = *(long*)ptr6;
									ptr3 += 8;
									ptr6 += 8;
								}
								while (ptr3 < ptr5);
							}
							ptr3 = ptr5;
							if (ptr2 - ptr < ptr4 - ptr3)
							{
								continue;
							}
							long num5 = ptr2 - ptr - (ptr4 - ptr3);
							LZ4Util.CopyMemory(ptr3 - num5, ptr, ptr2 - ptr);
						}
					}
					return (int)(ptr3 - decompressedBuffer);
				}
				return (int)(-(ptr - compressedBuffer));
			}
		}
	}
}
