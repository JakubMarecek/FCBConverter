// Author: id-daemon
// https://zenhax.com/viewtopic.php?f=5&t=12842

// Decompiled with JetBrains decompiler
// Type: UE4.actor
// Assembly: UE4, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DF65A7A4-1036-462D-9045-BC0208CBF69F
// Assembly location: D:\test\fc5mod_clothes.exe

using APPLIB;

namespace UE4
{
  internal class actor
  {
    public Vector3D pos = new Vector3D();
    public Vector3D rot = new Vector3D();
    public Vector3D scale = new Vector3D(1f, 1f, 1f);
    public int parent = -1;
    public string meshname;
    public int[] overmat;
  }
}
