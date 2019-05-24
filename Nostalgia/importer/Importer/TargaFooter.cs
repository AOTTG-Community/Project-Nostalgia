// Decompiled with JetBrains decompiler
// Type: BRS.Importer.TargaFooter
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA706D30-A741-450F-BB06-DFDA2E2B50C1
// Assembly location: G:\Games x2\AoTTG\BRS Mod v4\BRS Mod v4.0.7_Data\Managed\Assembly-CSharp.dll

namespace BRS.Importer
{
  public class TargaFooter
  {
    private string string_0 = string.Empty;
    private string string_1 = string.Empty;
    private int int_0;
    private int int_1;

    public int ExtensionAreaOffset
    {
      get
      {
        return this.int_0;
      }
    }

    protected internal void SetExtensionAreaOffset(int intExtensionAreaOffset)
    {
      this.int_0 = intExtensionAreaOffset;
    }

    public int DeveloperDirectoryOffset
    {
      get
      {
        return this.int_1;
      }
    }

    protected internal void SetDeveloperDirectoryOffset(int intDeveloperDirectoryOffset)
    {
      this.int_1 = intDeveloperDirectoryOffset;
    }

    public string Signature
    {
      get
      {
        return this.string_0;
      }
    }

    protected internal void SetSignature(string strSignature)
    {
      this.string_0 = strSignature;
    }

    public string ReservedCharacter
    {
      get
      {
        return this.string_1;
      }
    }

    protected internal void SetReservedCharacter(string strReservedCharacter)
    {
      this.string_1 = strReservedCharacter;
    }
  }
}
