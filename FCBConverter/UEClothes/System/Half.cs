// Author: id-daemon
// https://zenhax.com/viewtopic.php?f=5&t=12842

// Decompiled with JetBrains decompiler
// Type: System.Half
// Assembly: UE4, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DF65A7A4-1036-462D-9045-BC0208CBF69F
// Assembly location: D:\test\fc5mod_clothes.exe

using System;
using System.Diagnostics;
using System.Globalization;

namespace UEClothes.System
{
  [Serializable]
  public struct Half : IComparable, IFormattable, IConvertible, IComparable<Half>, IEquatable<Half>
  {
    public static readonly Half Epsilon = Half.ToHalf((ushort) 1);
    public static readonly Half MaxValue = Half.ToHalf((ushort) 31743);
    public static readonly Half MinValue = Half.ToHalf((ushort) 64511);
    public static readonly Half NaN = Half.ToHalf((ushort) 65024);
    public static readonly Half NegativeInfinity = Half.ToHalf((ushort) 64512);
    public static readonly Half PositiveInfinity = Half.ToHalf((ushort) 31744);
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal ushort value;
        internal static object baseTable = null;

        public Half(float value)
    {
      this = HalfHelper.SingleToHalf(value);
    }

    public Half(int value)
      : this((float) value)
    {
    }

    public Half(long value)
      : this((float) value)
    {
    }

    public Half(double value)
      : this((float) value)
    {
    }

    public Half(Decimal value)
      : this((float) value)
    {
    }

    public Half(uint value)
      : this((float) value)
    {
    }

    public Half(ulong value)
      : this((float) value)
    {
    }

    public static Half Negate(Half half)
    {
      return -half;
    }

    public static Half Add(Half half1, Half half2)
    {
      return half1 + half2;
    }

    public static Half Subtract(Half half1, Half half2)
    {
      return half1 - half2;
    }

    public static Half Multiply(Half half1, Half half2)
    {
      return half1 * half2;
    }

    public static Half Divide(Half half1, Half half2)
    {
      return half1 / half2;
    }

    public static Half operator +(Half half)
    {
      return half;
    }

    public static Half operator -(Half half)
    {
      return HalfHelper.Negate(half);
    }

    public static Half operator ++(Half half)
    {
      return (Half) ((float) half + 1f);
    }

    public static Half operator --(Half half)
    {
      return (Half) ((float) half - 1f);
    }

    public static Half operator +(Half half1, Half half2)
    {
      return (Half) ((float) half1 + (float) half2);
    }

    public static Half operator -(Half half1, Half half2)
    {
      return (Half) ((float) half1 - (float) half2);
    }

    public static Half operator *(Half half1, Half half2)
    {
      return (Half) ((float) half1 * (float) half2);
    }

    public static Half operator /(Half half1, Half half2)
    {
      return (Half) ((float) half1 / (float) half2);
    }

    public static bool operator ==(Half half1, Half half2)
    {
      return !Half.IsNaN(half1) && (int) half1.value == (int) half2.value;
    }

    public static bool operator !=(Half half1, Half half2)
    {
      return (int) half1.value != (int) half2.value;
    }

    public static bool operator <(Half half1, Half half2)
    {
      return (double) (float) half1 < (double) (float) half2;
    }

    public static bool operator >(Half half1, Half half2)
    {
      return (double) (float) half1 > (double) (float) half2;
    }

    public static bool operator <=(Half half1, Half half2)
    {
      return half1 == half2 || half1 < half2;
    }

    public static bool operator >=(Half half1, Half half2)
    {
      return half1 == half2 || half1 > half2;
    }

    public static implicit operator Half(byte value)
    {
      return new Half((float) value);
    }

    public static implicit operator Half(short value)
    {
      return new Half((float) value);
    }

    public static implicit operator Half(char value)
    {
      return new Half((float) value);
    }

    public static implicit operator Half(int value)
    {
      return new Half((float) value);
    }

    public static implicit operator Half(long value)
    {
      return new Half((float) value);
    }

    public static explicit operator Half(float value)
    {
      return new Half(value);
    }

    public static explicit operator Half(double value)
    {
      return new Half((float) value);
    }

    public static explicit operator Half(Decimal value)
    {
      return new Half((float) value);
    }

    public static explicit operator byte(Half value)
    {
      return (byte) (float) value;
    }

    public static explicit operator char(Half value)
    {
      return (char) (float) value;
    }

    public static explicit operator short(Half value)
    {
      return (short) (float) value;
    }

    public static explicit operator int(Half value)
    {
      return (int) (float) value;
    }

    public static explicit operator long(Half value)
    {
      return (long) (float) value;
    }

    public static implicit operator float(Half value)
    {
      return HalfHelper.HalfToSingle(value);
    }

    public static implicit operator double(Half value)
    {
      return (double) (float) value;
    }

    public static explicit operator Decimal(Half value)
    {
      return (Decimal) (float) value;
    }

    public static implicit operator Half(sbyte value)
    {
      return new Half((float) value);
    }

    public static implicit operator Half(ushort value)
    {
      return new Half((float) value);
    }

    public static implicit operator Half(uint value)
    {
      return new Half((float) value);
    }

    public static implicit operator Half(ulong value)
    {
      return new Half((float) value);
    }

    public static explicit operator sbyte(Half value)
    {
      return (sbyte) (float) value;
    }

    public static explicit operator ushort(Half value)
    {
      return (ushort) (float) value;
    }

    public static explicit operator uint(Half value)
    {
      return (uint) (float) value;
    }

    public static explicit operator ulong(Half value)
    {
      return (ulong) (float) value;
    }

    public int CompareTo(Half other)
    {
      int num = 0;
      if (this < other)
        num = -1;
      else if (this > other)
        num = 1;
      else if (this != other)
      {
        if (!Half.IsNaN(this))
          num = 1;
        else if (!Half.IsNaN(other))
          num = -1;
      }
      return num;
    }

    public int CompareTo(object obj)
    {
      if (obj == null)
        return 1;
      if (obj is Half other)
        return this.CompareTo(other);
      throw new ArgumentException("Object must be of type Half.");
    }

    public bool Equals(Half other)
    {
      if (other == this)
        return true;
      return Half.IsNaN(other) && Half.IsNaN(this);
    }

    public override bool Equals(object obj)
    {
      bool flag = false;
      if (obj is Half half && (half == this || Half.IsNaN(half) && Half.IsNaN(this)))
        flag = true;
      return flag;
    }

    public override int GetHashCode()
    {
      return this.value.GetHashCode();
    }

    public TypeCode GetTypeCode()
    {
      return (TypeCode) 255;
    }

    public static byte[] GetBytes(Half value)
    {
      return BitConverter.GetBytes(value.value);
    }

    public static ushort GetBits(Half value)
    {
      return value.value;
    }

    public static Half ToHalf(byte[] value, int startIndex)
    {
      return Half.ToHalf((ushort) BitConverter.ToInt16(value, startIndex));
    }

    public static Half ToHalf(ushort bits)
    {
      return new Half() { value = bits };
    }

    public static int Sign(Half value)
    {
      if (value < (Half) 0)
        return -1;
      if (value > (Half) 0)
        return 1;
      if (value != (Half) 0)
        throw new ArithmeticException("Function does not accept floating point Not-a-Number values.");
      return 0;
    }

    public static Half Abs(Half value)
    {
      return HalfHelper.Abs(value);
    }

    public static Half Max(Half value1, Half value2)
    {
      return !(value1 < value2) ? value1 : value2;
    }

    public static Half Min(Half value1, Half value2)
    {
      return !(value1 < value2) ? value2 : value1;
    }

    public static bool IsNaN(Half half)
    {
      return HalfHelper.IsNaN(half);
    }

    public static bool IsInfinity(Half half)
    {
      return HalfHelper.IsInfinity(half);
    }

    public static bool IsNegativeInfinity(Half half)
    {
      return HalfHelper.IsNegativeInfinity(half);
    }

    public static bool IsPositiveInfinity(Half half)
    {
      return HalfHelper.IsPositiveInfinity(half);
    }

    public static Half Parse(string value)
    {
      return (Half) float.Parse(value, (IFormatProvider) CultureInfo.InvariantCulture);
    }

    public static Half Parse(string value, IFormatProvider provider)
    {
      return (Half) float.Parse(value, provider);
    }

    public static Half Parse(string value, NumberStyles style)
    {
      return (Half) float.Parse(value, style, (IFormatProvider) CultureInfo.InvariantCulture);
    }

    public static Half Parse(string value, NumberStyles style, IFormatProvider provider)
    {
      return (Half) float.Parse(value, style, provider);
    }

    public static bool TryParse(string value, out Half result)
    {
      float result1;
      if (float.TryParse(value, out result1))
      {
        result = (Half) result1;
        return true;
      }
      result = new Half();
      return false;
    }

    public static bool TryParse(
      string value,
      NumberStyles style,
      IFormatProvider provider,
      out Half result)
    {
      bool flag = false;
      float result1;
      if (float.TryParse(value, style, provider, out result1))
      {
        result = (Half) result1;
        flag = true;
      }
      else
        result = new Half();
      return flag;
    }

    public override string ToString()
    {
      return ((float) this).ToString((IFormatProvider) CultureInfo.InvariantCulture);
    }

    public string ToString(IFormatProvider formatProvider)
    {
      return ((float) this).ToString(formatProvider);
    }

    public string ToString(string format)
    {
      return ((float) this).ToString(format, (IFormatProvider) CultureInfo.InvariantCulture);
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
      return ((float) this).ToString(format, formatProvider);
    }

    float IConvertible.ToSingle(IFormatProvider provider)
    {
      return (float) this;
    }

    TypeCode IConvertible.GetTypeCode()
    {
      return this.GetTypeCode();
    }

    bool IConvertible.ToBoolean(IFormatProvider provider)
    {
      return Convert.ToBoolean((float) this);
    }

    byte IConvertible.ToByte(IFormatProvider provider)
    {
      return Convert.ToByte((float) this);
    }

    char IConvertible.ToChar(IFormatProvider provider)
    {
      throw new InvalidCastException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Invalid cast from '{0}' to '{1}'.", (object) nameof (Half), (object) "Char"));
    }

    DateTime IConvertible.ToDateTime(IFormatProvider provider)
    {
      throw new InvalidCastException(string.Format((IFormatProvider) CultureInfo.CurrentCulture, "Invalid cast from '{0}' to '{1}'.", (object) nameof (Half), (object) "DateTime"));
    }

    Decimal IConvertible.ToDecimal(IFormatProvider provider)
    {
      return Convert.ToDecimal((float) this);
    }

    double IConvertible.ToDouble(IFormatProvider provider)
    {
      return Convert.ToDouble((float) this);
    }

    short IConvertible.ToInt16(IFormatProvider provider)
    {
      return Convert.ToInt16((float) this);
    }

    int IConvertible.ToInt32(IFormatProvider provider)
    {
      return Convert.ToInt32((float) this);
    }

    long IConvertible.ToInt64(IFormatProvider provider)
    {
      return Convert.ToInt64((float) this);
    }

    sbyte IConvertible.ToSByte(IFormatProvider provider)
    {
      return Convert.ToSByte((float) this);
    }

    string IConvertible.ToString(IFormatProvider provider)
    {
      return Convert.ToString((float) this, (IFormatProvider) CultureInfo.InvariantCulture);
    }

    object IConvertible.ToType(Type conversionType, IFormatProvider provider)
    {
      return ((IConvertible) (float) this).ToType(conversionType, provider);
    }

    ushort IConvertible.ToUInt16(IFormatProvider provider)
    {
      return Convert.ToUInt16((float) this);
    }

    uint IConvertible.ToUInt32(IFormatProvider provider)
    {
      return Convert.ToUInt32((float) this);
    }

    ulong IConvertible.ToUInt64(IFormatProvider provider)
    {
      return Convert.ToUInt64((float) this);
    }
  }
}
