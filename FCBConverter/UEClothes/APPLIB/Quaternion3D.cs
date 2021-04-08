// Author: id-daemon
// https://zenhax.com/viewtopic.php?f=5&t=12842

// Decompiled with JetBrains decompiler
// Type: APPLIB.Quaternion3D
// Assembly: UE4, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DF65A7A4-1036-462D-9045-BC0208CBF69F
// Assembly location: D:\test\fc5mod_clothes.exe

using System;

namespace APPLIB
{
  public class Quaternion3D
  {
    public float real;
    public float i;
    public float j;
    public float k;

    public Vector3D xyz
    {
      get
      {
        return new Vector3D(this.i, this.j, this.k);
      }
      set
      {
        this.i = value.X;
        this.j = value.Y;
        this.k = value.Z;
      }
    }

    public Quaternion3D()
    {
      this.real = 0.0f;
      this.i = 0.0f;
      this.j = 0.0f;
      this.k = 0.0f;
    }

    public Quaternion3D(float _real, float _i, float _j, float _k)
    {
      this.real = _real;
      this.i = _i;
      this.j = _j;
      this.k = _k;
    }

    public Quaternion3D(Vector3D vecXYZ, float _real)
    {
      this.real = _real;
      this.i = vecXYZ.X;
      this.j = vecXYZ.Y;
      this.k = vecXYZ.Z;
    }

    public Quaternion3D(Quaternion3D q)
    {
      this.real = q.real;
      this.i = q.i;
      this.j = q.j;
      this.k = q.k;
    }

    public Vector3D ToVec()
    {
      return new Vector3D(this.xyz);
    }

    public float Length
    {
      get
      {
        return Convert.ToSingle(Math.Sqrt((double) this.real * (double) this.real + (double) this.xyz.LengthSquared));
      }
    }

    public void Normalize()
    {
      float num = 1f / this.Length;
      this.xyz *= num;
      this.real *= num;
    }

    public static Quaternion3D Invert(Quaternion3D q)
    {
      float lengthSquared = q.LengthSquared;
      Quaternion3D quaternion3D;
      if ((double) lengthSquared != 0.0)
      {
        float num = 1f / lengthSquared;
        quaternion3D = new Quaternion3D(q.xyz * -num, q.real * num);
      }
      else
        quaternion3D = q;
      return quaternion3D;
    }

    public float LengthSquared
    {
      get
      {
        return this.real * this.real + this.xyz.LengthSquared;
      }
    }

    public static Quaternion3D Multiply(Quaternion3D left, Quaternion3D right)
    {
      return new Quaternion3D(right.real * left.xyz + left.real * right.xyz + Vector3D.Cross(left.xyz, right.xyz), left.real * right.real - Vector3D.Dot(left.xyz, right.xyz));
    }

    public static Quaternion3D operator *(Quaternion3D left, Quaternion3D right)
    {
      return Quaternion3D.Multiply(left, right);
    }
  }
}
