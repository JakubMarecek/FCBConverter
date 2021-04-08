// Author: id-daemon
// https://zenhax.com/viewtopic.php?f=5&t=12842

// Decompiled with JetBrains decompiler
// Type: APPLIB.C3D
// Assembly: UE4, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DF65A7A4-1036-462D-9045-BC0208CBF69F
// Assembly location: D:\test\fc5mod_clothes.exe

using System;

namespace APPLIB
{
  internal static class C3D
  {
    private const double FLT_EPSILON = 1E-05;

    public static float FlipFloat(float inputF)
    {
      return -inputF;
    }

    private static double deg2rad(double deg)
    {
      double num = Math.PI / 180.0;
      return deg * num;
    }

    private static double rad2deg(double rad)
    {
      double num = 180.0 / Math.PI;
      return rad * num;
    }

    public static float NanSafe(float val)
    {
      return float.IsNaN(val) || (double) val + 0.0 == 0.0 ? 0.0f : Convert.ToSingle(val);
    }

    public static Vector3D Quat2Euler_UBISOFT(Quaternion3D quat)
    {
      Vector3D vector3D = new Vector3D();
      double num1 = Math.PI / 2.0;
      float num2 = (float) ((double) quat.i * (double) quat.j + (double) quat.k * (double) quat.real);
      if ((double) num2 > 0.499000012874603)
      {
        vector3D.X = 2f * (float) Math.Atan2((double) quat.i, (double) quat.real);
        vector3D.Y = (float) num1;
        vector3D.Z = 0.0f;
      }
      else if ((double) num2 < -0.499000012874603)
      {
        vector3D.X = -2f * (float) Math.Atan2((double) quat.i, (double) quat.real);
        vector3D.Y = (float) -num1;
        vector3D.Z = 0.0f;
      }
      else
      {
        float num3 = quat.i * quat.i;
        float num4 = quat.j * quat.j;
        float num5 = quat.k * quat.k;
        vector3D.X = (float) Math.Atan2(2.0 * (double) quat.j * (double) quat.real - 2.0 * (double) quat.i * (double) quat.k, 1.0 - 2.0 * (double) num4 - 2.0 * (double) num5);
        vector3D.Y = Convert.ToSingle(Math.Asin(2.0 * (double) num2));
        vector3D.Z = (float) Math.Atan2(2.0 * (double) quat.i * (double) quat.real - 2.0 * (double) quat.j * (double) quat.k, 1.0 - 2.0 * (double) num3 - 2.0 * (double) num5);
      }
      if (float.IsNaN(vector3D.X))
        vector3D.X = 0.0f;
      if (float.IsNaN(vector3D.Y))
        vector3D.Y = 0.0f;
      if (float.IsNaN(vector3D.Z))
        vector3D.Z = 0.0f;
      return vector3D;
    }

    public static Vector3D QuaternionToEuler(Quaternion3D quat)
    {
      Vector3D vector3D = new Vector3D();
      float num1 = quat.real * quat.real;
      float num2 = quat.i * quat.i;
      float num3 = quat.j * quat.j;
      float num4 = quat.k * quat.k;
      vector3D.Z = (float) C3D.rad2deg(Math.Atan2(2.0 * ((double) quat.j * (double) quat.k + (double) quat.i * (double) quat.real), -(double) num2 - (double) num3 + (double) num4 + (double) num1));
      vector3D.X = (float) C3D.rad2deg(Math.Asin(-2.0 * ((double) quat.i * (double) quat.k - (double) quat.j * (double) quat.real)));
      vector3D.Y = (float) C3D.rad2deg(Math.Atan2(2.0 * ((double) quat.i * (double) quat.j + (double) quat.k * (double) quat.real), (double) num2 - (double) num3 - (double) num4 + (double) num1));
      if (float.IsNaN(vector3D.X))
        vector3D.X = 0.0f;
      if (float.IsNaN(vector3D.Y))
        vector3D.Y = 0.0f;
      if (float.IsNaN(vector3D.Z))
        vector3D.Z = 0.0f;
      return vector3D;
    }

    public static Vector3D QuaternionToEulerRAD(Quaternion3D quat)
    {
      Vector3D vector3D = new Vector3D();
      float num1 = quat.real * quat.real;
      float num2 = quat.i * quat.i;
      float num3 = quat.j * quat.j;
      float num4 = quat.k * quat.k;
      vector3D.Z = (float) Math.Atan2(2.0 * ((double) quat.j * (double) quat.k + (double) quat.i * (double) quat.real), -(double) num2 - (double) num3 + (double) num4 + (double) num1);
      vector3D.X = (float) Math.Asin(-2.0 * ((double) quat.i * (double) quat.k - (double) quat.j * (double) quat.real));
      vector3D.Y = (float) Math.Atan2(2.0 * ((double) quat.i * (double) quat.j + (double) quat.k * (double) quat.real), (double) num2 - (double) num3 - (double) num4 + (double) num1);
      if (float.IsNaN(vector3D.X))
        vector3D.X = 0.0f;
      if (float.IsNaN(vector3D.Y))
        vector3D.Y = 0.0f;
      if (float.IsNaN(vector3D.Z))
        vector3D.Z = 0.0f;
      return vector3D;
    }

    public static Vector3D QuaternionToEulerRAD2(Quaternion3D quat)
    {
      Vector3D vector3D = new Vector3D();
      float real = quat.real;
      float i = quat.i;
      float j = quat.j;
      float k = quat.k;
      float num1 = i * i;
      float num2 = j * j;
      float num3 = k * k;
      vector3D.Z = (float) Math.Atan2(2.0 * ((double) real * (double) i + (double) j * (double) k), 1.0 - 2.0 * ((double) num1 + (double) num2));
      vector3D.X = (float) Math.Asin(2.0 * ((double) real * (double) j - (double) k * (double) i));
      vector3D.Y = (float) Math.Atan2(2.0 * ((double) real * (double) k + (double) i * (double) j), 1.0 - 2.0 * ((double) num2 + (double) num3));
      return vector3D;
    }

    public static Quaternion3D EulerAnglesToQuaternion(
      float yaw,
      float pitch,
      float roll)
    {
      double num1 = (double) C3D.NormalizeAngle(yaw);
      double num2 = (double) C3D.NormalizeAngle(pitch);
      double num3 = (double) C3D.NormalizeAngle(roll);
      double num4 = Math.Cos(num1);
      double num5 = Math.Cos(num2);
      double num6 = Math.Cos(num3);
      double num7 = Math.Sin(num1);
      double num8 = Math.Sin(num2);
      double num9 = Math.Sin(num3);
      return new Quaternion3D()
      {
        real = (float) (num4 * num5 * num6 - num7 * num8 * num9),
        i = (float) (num7 * num8 * num6 + num4 * num5 * num9),
        j = (float) (num7 * num5 * num6 + num4 * num8 * num9),
        k = (float) (num4 * num8 * num6 - num7 * num5 * num9)
      };
    }

    public static Quaternion3D DEG_EulerAnglesToQuaternion(
      float yaw,
      float pitch,
      float roll)
    {
      double num1 = C3D.deg2rad((double) yaw);
      double num2 = C3D.deg2rad((double) pitch);
      double num3 = C3D.deg2rad((double) roll);
      double num4 = Math.Cos(num1);
      double num5 = Math.Cos(num2);
      double num6 = Math.Cos(num3);
      double num7 = Math.Sin(num1);
      double num8 = Math.Sin(num2);
      double num9 = Math.Sin(num3);
      return new Quaternion3D()
      {
        real = (float) (num4 * num5 * num6 - num7 * num8 * num9),
        i = (float) (num7 * num8 * num6 + num4 * num5 * num9),
        j = (float) (num7 * num5 * num6 + num4 * num8 * num9),
        k = (float) (num4 * num8 * num6 - num7 * num5 * num9)
      };
    }

    public static Quaternion3D Euler2Quat(Vector3D orientation)
    {
      Quaternion3D quaternion3D = new Quaternion3D();
      float returnSin1 = 0.0f;
      float returnSin2 = 0.0f;
      float returnSin3 = 0.0f;
      float returnCos1 = 0.0f;
      float returnCos2 = 0.0f;
      float returnCos3 = 0.0f;
      C3D.MathUtil.SinCos(ref returnSin1, ref returnCos1, orientation.X * 0.5f);
      C3D.MathUtil.SinCos(ref returnSin2, ref returnCos2, orientation.Y * 0.5f);
      C3D.MathUtil.SinCos(ref returnSin3, ref returnCos3, orientation.Z * 0.5f);
      quaternion3D.real = (float) ((double) returnCos3 * (double) returnCos1 * (double) returnCos2 + (double) returnSin3 * (double) returnSin1 * (double) returnSin2);
      quaternion3D.i = (float) (-(double) returnCos3 * (double) returnSin1 * (double) returnCos2 - (double) returnSin3 * (double) returnCos1 * (double) returnSin2);
      quaternion3D.j = (float) ((double) returnCos3 * (double) returnSin1 * (double) returnSin2 - (double) returnSin3 * (double) returnCos2 * (double) returnCos1);
      quaternion3D.k = (float) ((double) returnSin3 * (double) returnSin1 * (double) returnCos2 - (double) returnCos3 * (double) returnCos1 * (double) returnSin2);
      return quaternion3D;
    }

    private static float NormalizeAngle(float input)
    {
      return (float) ((double) input * Math.PI / 360.0);
    }

    public static Vector3D ToEulerAngles(Quaternion3D q)
    {
      return C3D.Eul_FromQuat(q, 0, 1, 2, 0, C3D.EulerParity.Even, C3D.EulerRepeat.No, C3D.EulerFrame.S);
    }

    private static Vector3D Eul_FromQuat(
      Quaternion3D q,
      int i,
      int j,
      int k,
      int h,
      C3D.EulerParity parity,
      C3D.EulerRepeat repeat,
      C3D.EulerFrame frame)
    {
      double[,] M = new double[4, 4];
      double num1 = (double) q.i * (double) q.i + (double) q.j * (double) q.j + (double) q.k * (double) q.k + (double) q.real * (double) q.real;
      double num2 = num1 <= 0.0 ? 0.0 : 2.0 / num1;
      double num3 = (double) q.i * num2;
      double num4 = (double) q.j * num2;
      double num5 = (double) q.k * num2;
      double num6 = (double) q.real * num3;
      double num7 = (double) q.real * num4;
      double num8 = (double) q.real * num5;
      double num9 = (double) q.i * num3;
      double num10 = (double) q.i * num4;
      double num11 = (double) q.i * num5;
      double num12 = (double) q.j * num4;
      double num13 = (double) q.j * num5;
      double num14 = (double) q.k * num5;
      M[0, 0] = 1.0 - (num12 + num14);
      M[0, 1] = num10 - num8;
      M[0, 2] = num11 + num7;
      M[1, 0] = num10 + num8;
      M[1, 1] = 1.0 - (num9 + num14);
      M[1, 2] = num13 - num6;
      M[2, 0] = num11 - num7;
      M[2, 1] = num13 + num6;
      M[2, 2] = 1.0 - (num9 + num12);
      M[3, 3] = 1.0;
      return C3D.Eul_FromHMatrix(M, i, j, k, h, parity, repeat, frame);
    }

    private static Vector3D Eul_FromHMatrix(
      double[,] M,
      int i,
      int j,
      int k,
      int h,
      C3D.EulerParity parity,
      C3D.EulerRepeat repeat,
      C3D.EulerFrame frame)
    {
      Vector3D vector3D = new Vector3D();
      if (repeat == C3D.EulerRepeat.Yes)
      {
        double y = Math.Sqrt(M[i, j] * M[i, j] + M[i, k] * M[i, k]);
        if (y > 0.00016)
        {
          vector3D.X = (float) Math.Atan2(M[i, j], M[i, k]);
          vector3D.Y = (float) Math.Atan2(y, M[i, i]);
          vector3D.Z = (float) Math.Atan2(M[j, i], -M[k, i]);
        }
        else
        {
          vector3D.X = (float) Math.Atan2(-M[j, k], M[j, j]);
          vector3D.Y = (float) Math.Atan2(y, M[i, i]);
          vector3D.Z = 0.0f;
        }
      }
      else
      {
        double x = Math.Sqrt(M[i, i] * M[i, i] + M[j, i] * M[j, i]);
        if (x > 0.00016)
        {
          vector3D.X = (float) Math.Atan2(M[k, j], M[k, k]);
          vector3D.Y = (float) Math.Atan2(-M[k, i], x);
          vector3D.Z = (float) Math.Atan2(M[j, i], M[i, i]);
        }
        else
        {
          vector3D.X = (float) Math.Atan2(-M[j, k], M[j, j]);
          vector3D.Y = (float) Math.Atan2(-M[k, i], x);
          vector3D.Z = 0.0f;
        }
      }
      if (parity == C3D.EulerParity.Odd)
      {
        vector3D.X = -vector3D.X;
        vector3D.Y = -vector3D.Y;
        vector3D.Z = -vector3D.Z;
      }
      if (frame == C3D.EulerFrame.R)
      {
        double x = (double) vector3D.X;
        vector3D.X = vector3D.Z;
        vector3D.Z = (float) x;
      }
      return vector3D;
    }

    private enum EulerParity
    {
      Even,
      Odd,
    }

    private enum EulerRepeat
    {
      No,
      Yes,
    }

    private enum EulerFrame
    {
      S,
      R,
    }

    public class MathUtil
    {
      public static float kPi;
      public static float k2Pi;
      public static float kPiOver2;
      public static float k1OverPi;
      public static float k1Over2Pi;
      public static float kPiOver180;
      public static float k180OverPi;
      public static Vector3D kZeroVector;

      public MathUtil()
      {
        C3D.MathUtil.kPi = 3.141593f;
        C3D.MathUtil.k2Pi = C3D.MathUtil.kPi * 2f;
        C3D.MathUtil.kPiOver2 = C3D.MathUtil.kPi / 2f;
        C3D.MathUtil.k1OverPi = 1f / C3D.MathUtil.kPi;
        C3D.MathUtil.k1Over2Pi = 1f / C3D.MathUtil.k2Pi;
        C3D.MathUtil.kPiOver180 = C3D.MathUtil.kPi / 180f;
        C3D.MathUtil.k180OverPi = 180f / C3D.MathUtil.kPi;
        C3D.MathUtil.kZeroVector = new Vector3D(0.0f, 0.0f, 0.0f);
      }

      public static void SinCos(ref float returnSin, ref float returnCos, float theta)
      {
        returnSin = Convert.ToSingle(Math.Sin(Convert.ToDouble(C3D.deg2rad((double) theta))));
        returnCos = Convert.ToSingle(Math.Cos(Convert.ToDouble(C3D.deg2rad((double) theta))));
      }
    }
  }
}
