// Decompiled with JetBrains decompiler
// Type: BRS.Importer.ImageType
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA706D30-A741-450F-BB06-DFDA2E2B50C1
// Assembly location: G:\Games x2\AoTTG\BRS Mod v4\BRS Mod v4.0.7_Data\Managed\Assembly-CSharp.dll

namespace BRS.Importer
{
  public enum ImageType : byte
  {
    NO_IMAGE_DATA = 0,
    UNCOMPRESSED_COLOR_MAPPED = 1,
    UNCOMPRESSED_TRUE_COLOR = 2,
    UNCOMPRESSED_BLACK_AND_WHITE = 3,
    RUN_LENGTH_ENCODED_COLOR_MAPPED = 9,
    RUN_LENGTH_ENCODED_TRUE_COLOR = 10, // 0x0A
    RUN_LENGTH_ENCODED_BLACK_AND_WHITE = 11, // 0x0B
  }
}
