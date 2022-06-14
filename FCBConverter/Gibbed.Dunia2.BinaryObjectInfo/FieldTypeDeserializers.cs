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

using Gibbed.Dunia2.FileFormats;
using System;
using System.Text;

namespace Gibbed.Dunia2.BinaryObjectInfo
{
    public static class FieldTypeDeserializers
    {
        private static bool HasLeft(byte[] data, int offset, int count, int needCount)
        {
            return data != null &&
                   data.Length >= offset + count &&
                   offset + needCount <= offset + count;
        }

        public static object Deserialize(FieldType fieldType, byte[] data, int offset, int count, out int read)
        {
            switch (fieldType)
            {
                case FieldType.Boolean:
                {
                    if (HasLeft(data, offset, count, 1) == false)
                    {
                        throw new FormatException("field type Boolean requires 1 byte");
                    }

                    if (data[offset] != 0 &&
                        data[offset] != 1)
                    {
                        throw new FormatException("invalid value for field type Boolean");
                    }

                    read = 1;
                    return data[offset] != 0;
                }

                case FieldType.UInt8:
                {
                    if (HasLeft(data, offset, count, 1) == false)
                    {
                        throw new FormatException("field type UInt8 requires 1 byte");
                    }

                    read = 1;
                    return data[offset];
                }

                case FieldType.Int8:
                {
                    if (HasLeft(data, offset, count, 1) == false)
                    {
                        throw new FormatException("field type Int8 requires 1 byte");
                    }

                    read = 1;
                    return (sbyte)data[offset];
                }

                case FieldType.UInt16:
                {
                    if (HasLeft(data, offset, count, 2) == false)
                    {
                        throw new FormatException("field type UInt16 requires 2 bytes");
                    }

                    read = 2;
                    return BitConverter.ToUInt16(data, offset);
                }

                case FieldType.Int16:
                {
                    if (HasLeft(data, offset, count, 2) == false)
                    {
                        throw new FormatException("field type Int16 requires 2 bytes");
                    }

                    read = 2;
                    return BitConverter.ToInt16(data, offset);
                }

                case FieldType.UInt32:
                {
                    if (HasLeft(data, offset, count, 4) == false)
                    {
                        throw new FormatException("field type UInt32 requires 4 bytes");
                    }

                    read = 4;
                    return BitConverter.ToUInt32(data, offset);
                }

                case FieldType.Int32:
                {
                    if (HasLeft(data, offset, count, 4) == false)
                    {
                        throw new FormatException("field type Int32 requires 4 bytes");
                    }

                    read = 4;
                    return BitConverter.ToInt32(data, offset);
                }

                case FieldType.UInt64:
                {
                    if (HasLeft(data, offset, count, 8) == false)
                    {
                        throw new FormatException("field type UInt64 requires 8 bytes");
                    }

                    read = 8;
                    return BitConverter.ToUInt64(data, offset);
                }

                case FieldType.Int64:
                {
                    if (HasLeft(data, offset, count, 8) == false)
                    {
                        throw new FormatException("field type Int64 requires 8 bytes");
                    }

                    read = 8;
                    return BitConverter.ToInt64(data, offset);
                }

                case FieldType.Float32:
                {
                    if (HasLeft(data, offset, count, 4) == false)
                    {
                        throw new FormatException("field type Float32 requires 4 bytes");
                    }

                    read = 4;
                    return BitConverter.ToSingle(data, offset);
                }

                case FieldType.Float64:
                {
                    if (HasLeft(data, offset, count, 8) == false)
                    {
                        throw new FormatException("field type Float64 requires 8 bytes");
                    }

                    read = 8;
                    return BitConverter.ToDouble(data, offset);
                }

                case FieldType.Vector2:
                {
                    if (HasLeft(data, offset, count, 8) == false)
                    {
                        throw new FormatException("field type Vector2 requires 8 bytes");
                    }

                    read = 8;
                    return new Vector2
                    {
                        X = BitConverter.ToSingle(data, offset + 0),
                        Y = BitConverter.ToSingle(data, offset + 4),
                    };
                }

                case FieldType.Vector3:
                {
                    if (HasLeft(data, offset, count, 12) == false)
                    {
                        throw new FormatException("field type Vector3 requires 12 bytes");
                    }

                    read = 12;
                    return new Vector3
                    {
                        X = BitConverter.ToSingle(data, offset + 0),
                        Y = BitConverter.ToSingle(data, offset + 4),
                        Z = BitConverter.ToSingle(data, offset + 8),
                    };
                }

                case FieldType.Vector4:
                {
                    if (HasLeft(data, offset, count, 16) == false)
                    {
                        throw new FormatException("field type Vector4 requires 16 bytes");
                    }

                    read = 16;
                    return new Vector4
                    {
                        X = BitConverter.ToSingle(data, offset + 0),
                        Y = BitConverter.ToSingle(data, offset + 4),
                        Z = BitConverter.ToSingle(data, offset + 8),
                        W = BitConverter.ToSingle(data, offset + 12),
                    };
                }

                case FieldType.String:
                {
                    if (HasLeft(data, offset, count, 1) == false)
                    {
                        throw new FormatException("field type String requires at least 1 byte");
                    }

                    int length, o;
                    for (length = 0, o = offset; (data[o] != 0 && data[o] != 10) && o < data.Length; length++, o++)
                    {
                    }

                    if (o == data.Length)
                    {
                        throw new FormatException("invalid trailing byte value for field type String");
                    }

                    
                    if (data[data.Length - 1] != 0 && data[data.Length - 1] != 10)
                    {
                        throw new FormatException("invalid trailing byte value for field type String");
                    }
                    

                    read = length + 1;
                    return Encoding.UTF8.GetString(data, offset, length);
                }

                case FieldType.Enum:
                {
                    if (HasLeft(data, offset, count, 4) == false)
                    {
                        throw new FormatException("field type Enum requires 4 bytes");
                    }

                    read = 4;
                    return BitConverter.ToInt32(data, offset);
                }

                case FieldType.Hash32:
                {
                    if (HasLeft(data, offset, count, 4) == false)
                    {
                        throw new FormatException("field type Hash32 requires 4 bytes");
                    }

                    read = 4;
                    return BitConverter.ToUInt32(data, offset);
                }

                case FieldType.Hash64:
                {
                    if (HasLeft(data, offset, count, 8) == false)
                    {
                        throw new FormatException("field type Hash64 requires 8 bytes");
                    }

                    read = 8;
                    return BitConverter.ToUInt64(data, offset);
                }

                case FieldType.Id32:
                {
                    if (HasLeft(data, offset, count, 4) == false)
                    {
                        throw new FormatException("field type Id32 requires 4 bytes");
                    }

                    read = 4;
                    return BitConverter.ToUInt32(data, offset);
                }

                case FieldType.Id64:
                {
                    if (HasLeft(data, offset, count, 8) == false)
                    {
                        throw new FormatException("field type Id64 requires 8 bytes");
                    }

                    read = 8;
                    return BitConverter.ToUInt64(data, offset);
                }

                default:
                {
                    throw new NotSupportedException("unsupported field type");
                }
            }
        }
    }
}
