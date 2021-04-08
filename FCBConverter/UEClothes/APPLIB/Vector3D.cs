// Author: id-daemon
// https://zenhax.com/viewtopic.php?f=5&t=12842

// Decompiled with JetBrains decompiler
// Type: APPLIB.Vector3D
// Assembly: UE4, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DF65A7A4-1036-462D-9045-BC0208CBF69F
// Assembly location: D:\test\fc5mod_clothes.exe

using System;

namespace APPLIB
{
  public class Vector3D
  {
    public static readonly Vector3D UnitX = new Vector3D(1f, 0.0f, 0.0f);
    public static readonly Vector3D UnitY = new Vector3D(0.0f, 1f, 0.0f);
    public static readonly Vector3D UnitZ = new Vector3D(0.0f, 0.0f, 1f);
    public static readonly Vector3D Zero = new Vector3D(0.0f, 0.0f, 0.0f);
    public static readonly Vector3D One = new Vector3D(1f, 1f, 1f);
    public float X;
    public float Y;
    public float Z;

    public override bool Equals(object obj)
    {
      if (obj == null)
        return false;
      Vector3D Vec = obj as Vector3D;
      return (object) Vec != null && this.IsEqualTo(Vec);
    }

    public override int GetHashCode()
    {
      return (17 * 23 + this.X.GetHashCode()) * 23 + this.Y.GetHashCode();
    }

    public Vector3D()
    {
      this.X = 0.0f;
      this.Y = 0.0f;
      this.Z = 0.0f;
    }

    public Vector3D(float xx, float yy, float zz)
    {
      this.X = xx;
      this.Y = yy;
      this.Z = zz;
    }

    public Vector3D(Vector3D Vec)
    {
      this.X = Vec.X;
      this.Y = Vec.Y;
      this.Z = Vec.Z;
    }

    public void SetVector(float xx, float yy, float zz)
    {
      this.X = xx;
      this.Y = yy;
      this.Z = zz;
    }

    public float DotProduct(Vector3D Vec)
    {
      return (float) ((double) this.X * (double) Vec.X + (double) this.Y * (double) Vec.Y + (double) this.Z * (double) Vec.Z);
    }

    public float Length()
    {
      return (float) Math.Sqrt((double) this.X * (double) this.X + (double) this.Y * (double) this.Y + (double) this.Z * (double) this.Z);
    }

    public float AngleTo(Vector3D Vec)
    {
      float num1 = this.DotProduct(Vec);
      float num2 = this.Length() * Vec.Length();
      return (double) num2 == 0.0 ? 0.0f : (float) Math.Acos((double) num1 / (double) num2);
    }

    public Vector3D UnitVector()
    {
      Vector3D vector3D = new Vector3D();
      float num = this.Length();
      if ((double) num == 0.0)
      {
        vector3D.SetVector(0.0f, 0.0f, 0.0f);
        return vector3D;
      }
      vector3D.X = this.X / num;
      vector3D.Y = this.Y / num;
      vector3D.Z = this.Z / num;
      return vector3D;
    }

    public bool IsCodirectionalTo(Vector3D Vec)
    {
      Vector3D vector3D1 = this.UnitVector();
      Vector3D vector3D2 = Vec.UnitVector();
      return (double) vector3D1.X == (double) vector3D2.X && (double) vector3D1.Y == (double) vector3D2.Y && (double) vector3D1.Z == (double) vector3D2.Z;
    }

    public bool IsEqualTo(Vector3D Vec)
    {
      return (double) this.X == (double) Vec.X && (double) this.Y == (double) Vec.Y && (double) this.Z == (double) Vec.Z;
    }

    public bool IsParallelTo(Vector3D Vec)
    {
      Vector3D vector3D1 = this.UnitVector();
      Vector3D vector3D2 = Vec.UnitVector();
      return (((double) vector3D1.X != (double) vector3D2.X || (double) vector3D1.Y != (double) vector3D2.Y ? 0 : ((double) vector3D1.Z == (double) vector3D2.Z ? 1 : 0)) | ((double) vector3D1.X != -(double) vector3D2.X || (double) vector3D1.Y != -(double) vector3D2.Y ? 0 : ((double) vector3D1.Z == (double) vector3D2.Z ? 1 : 0))) != 0;
    }

    public bool IsPerpendicularTo(Vector3D Vec)
    {
      return (double) this.AngleTo(Vec) == Math.PI / 2.0;
    }

    public object IsXAxis()
    {
      return (double) this.X != 0.0 && (double) this.Y == 0.0 && (double) this.Z == 0.0 ? (object) true : (object) false;
    }

    public object IsYAxis()
    {
      return (double) this.X == 0.0 && (double) this.Y != 0.0 && (double) this.Z == 0.0 ? (object) true : (object) false;
    }

    public object IsZAxis()
    {
      return (double) this.X == 0.0 && (double) this.Y == 0.0 && (double) this.Z != 0.0 ? (object) true : (object) false;
    }

    public void Negate()
    {
      this.X *= -1f;
      this.Y *= -1f;
      this.Z *= -1f;
    }

    public Vector3D Add(Vector3D Vec)
    {
      return new Vector3D()
      {
        X = this.X + Vec.X,
        Y = this.Y + Vec.Y,
        Z = this.Z + Vec.Z
      };
    }

    public Vector3D Subtract(Vector3D Vec)
    {
      return new Vector3D()
      {
        X = this.X - Vec.X,
        Y = this.Y - Vec.Y,
        Z = this.Z - Vec.Z
      };
    }

    public static bool operator ==(Vector3D a, Vector3D b)
    {
      return (double) a.X == (double) b.X && (double) a.Y == (double) b.Y && (double) a.Z == (double) b.Z;
    }

    public static bool operator !=(Vector3D a, Vector3D b)
    {
      return (double) a.X != (double) b.X || (double) a.Y != (double) b.Y || (double) a.Z != (double) b.Z;
    }

    public static Vector3D operator +(Vector3D a, Vector3D b)
    {
      return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public static Vector3D operator -(Vector3D left, Vector3D right)
    {
      return new Vector3D()
      {
        X = left.X - right.X,
        Y = left.Y - right.Y,
        Z = left.Z - right.Z
      };
    }

    public static Vector3D Multiply(Vector3D vector, float scale)
    {
      return new Vector3D(vector.X * scale, vector.Y * scale, vector.Z * scale);
    }

    public static Vector3D Multiply(Vector3D vector, int scale)
    {
      return new Vector3D(vector.X * (float) scale, vector.Y * (float) scale, vector.Z * (float) scale);
    }

    public static Vector3D Multiply(Vector3D vector, Vector3D scale)
    {
      return new Vector3D(vector.X * scale.X, vector.Y * scale.Y, vector.Z * scale.Z);
    }

    public static Vector3D Multiply(Vector3D vec, Quaternion3D q)
    {
      float num1 = Convert.ToSingle(2) * (float) ((double) q.i * (double) vec.X + (double) q.j * (double) vec.Y + (double) q.k * (double) vec.Z);
      float num2 = Convert.ToSingle(2) * q.real;
      float num3 = num2 * q.real - Convert.ToSingle(1);
      return new Vector3D((float) ((double) num3 * (double) vec.X + (double) num1 * (double) q.i + (double) num2 * ((double) q.k * (double) vec.Z - (double) q.k * (double) vec.Y)), (float) ((double) num3 * (double) vec.Y + (double) num1 * (double) q.j + (double) num2 * ((double) q.k * (double) vec.X - (double) q.i * (double) vec.Z)), (float) ((double) num3 * (double) vec.Z + (double) num1 * (double) q.k + (double) num2 * ((double) q.i * (double) vec.Y - (double) q.j * (double) vec.X)));
    }

    public static Vector3D operator *(Vector3D left, float right)
    {
      return Vector3D.Multiply(left, right);
    }

    public static Vector3D operator *(Vector3D left, int right)
    {
      return Vector3D.Multiply(left, right);
    }

    public static Vector3D operator *(float left, Vector3D right)
    {
      return Vector3D.Multiply(right, left);
    }

    public static Vector3D operator *(Vector3D left, Vector3D right)
    {
      return Vector3D.Multiply(left, right);
    }

    public static Vector3D operator *(Vector3D left, Quaternion3D right)
    {
      return Vector3D.Multiply(left, right);
    }

    public static Vector3D operator /(Vector3D vec, float scale)
    {
      float num = 1f / scale;
      vec.X *= num;
      vec.Y *= num;
      vec.Z *= num;
      return vec;
    }

    public static Vector3D Cross(Vector3D left, Vector3D right)
    {
      return new Vector3D((float) ((double) left.Y * (double) right.Z - (double) left.Z * (double) right.Y), (float) ((double) left.Z * (double) right.X - (double) left.X * (double) right.Z), (float) ((double) left.X * (double) right.Y - (double) left.Y * (double) right.X));
    }

    public static float Dot(Vector3D left, Vector3D right)
    {
      return (float) ((double) left.X * (double) right.X + (double) left.Y * (double) right.Y + (double) left.Z * (double) right.Z);
    }

    public Vector3D Normalized()
    {
      Vector3D vector3D = this;
      vector3D.Normalize();
      return vector3D;
    }

    public void Normalize()
    {
      float num = 1f / this.Length();
      this.X *= num;
      this.Y *= num;
      this.Z *= num;
    }

    public static Vector3D Normalize(Vector3D vec)
    {
      float num = 1f / vec.Length();
      vec.X *= num;
      vec.Y *= num;
      vec.Z *= num;
      return vec;
    }

    public float this[int index]
    {
      get
      {
        if (index == 0)
          return this.X;
        if (index == 1)
          return this.Y;
        if (index == 2)
          return this.Z;
        throw new IndexOutOfRangeException("You tried to access this vector at index: " + (object) index);
      }
      set
      {
        if (index == 0)
          this.X = value;
        else if (index == 1)
        {
          this.Y = value;
        }
        else
        {
          if (index != 2)
            throw new IndexOutOfRangeException("You tried to set this vector at index: " + (object) index);
          this.Z = value;
        }
      }
    }

    public float LengthSquared
    {
      get
      {
        return (float) ((double) this.X * (double) this.X + (double) this.Y * (double) this.Y + (double) this.Z * (double) this.Z);
      }
    }

    public string WriteString()
    {
      return string.Format("{0} {1} {2}", (object) this.X, (object) this.Y, (object) this.Z);
    }
  }
}
