// Decompiled with JetBrains decompiler
// Type: BRS.Importer.TargaExtensionArea
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA706D30-A741-450F-BB06-DFDA2E2B50C1
// Assembly location: G:\Games x2\AoTTG\BRS Mod v4\BRS Mod v4.0.7_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BRS.Importer
{
  public class TargaExtensionArea
  {
    private string string_0 = string.Empty;
    private string string_1 = string.Empty;
    private DateTime dateTime_0 = DateTime.Now;
    private string string_2 = string.Empty;
    private TimeSpan timeSpan_0 = TimeSpan.Zero;
    private string string_3 = string.Empty;
    private string string_4 = string.Empty;
    private List<int> list_0 = new List<int>();
    private List<Color> list_1 = new List<Color>();
    private int int_0;
    private Color color_0;
    private int int_1;
    private int int_2;
    private int int_3;
    private int int_4;
    private int int_5;
    private int int_6;
    private int int_7;
    private int int_8;

    public int ExtensionSize
    {
      get
      {
        return this.int_0;
      }
    }

    protected internal void SetExtensionSize(int intExtensionSize)
    {
      this.int_0 = intExtensionSize;
    }

    public string AuthorName
    {
      get
      {
        return this.string_0;
      }
    }

    protected internal void SetAuthorName(string strAuthorName)
    {
      this.string_0 = strAuthorName;
    }

    public string AuthorComments
    {
      get
      {
        return this.string_1;
      }
    }

    protected internal void SetAuthorComments(string strAuthorComments)
    {
      this.string_1 = strAuthorComments;
    }

    public DateTime DateTimeStamp
    {
      get
      {
        return this.dateTime_0;
      }
    }

    protected internal void SetDateTimeStamp(DateTime dtDateTimeStamp)
    {
      this.dateTime_0 = dtDateTimeStamp;
    }

    public string JobName
    {
      get
      {
        return this.string_2;
      }
    }

    protected internal void SetJobName(string strJobName)
    {
      this.string_2 = strJobName;
    }

    public TimeSpan JobTime
    {
      get
      {
        return this.timeSpan_0;
      }
    }

    protected internal void SetJobTime(TimeSpan dtJobTime)
    {
      this.timeSpan_0 = dtJobTime;
    }

    public string SoftwareID
    {
      get
      {
        return this.string_3;
      }
    }

    protected internal void SetSoftwareID(string strSoftwareID)
    {
      this.string_3 = strSoftwareID;
    }

    public string SoftwareVersion
    {
      get
      {
        return this.string_4;
      }
    }

    protected internal void SetSoftwareVersion(string strSoftwareVersion)
    {
      this.string_4 = strSoftwareVersion;
    }

    public Color KeyColor
    {
      get
      {
        return this.color_0;
      }
    }

    protected internal void SetKeyColor(Color cKeyColor)
    {
      this.color_0 = cKeyColor;
    }

    public int PixelAspectRatioNumerator
    {
      get
      {
        return this.int_1;
      }
    }

    protected internal void SetPixelAspectRatioNumerator(int intPixelAspectRatioNumerator)
    {
      this.int_1 = intPixelAspectRatioNumerator;
    }

    public int PixelAspectRatioDenominator
    {
      get
      {
        return this.int_2;
      }
    }

    protected internal void SetPixelAspectRatioDenominator(int intPixelAspectRatioDenominator)
    {
      this.int_2 = intPixelAspectRatioDenominator;
    }

    public float PixelAspectRatio
    {
      get
      {
        if (this.int_2 > 0)
          return (float) this.int_1 / (float) this.int_2;
        return 0.0f;
      }
    }

    public int GammaNumerator
    {
      get
      {
        return this.int_3;
      }
    }

    protected internal void SetGammaNumerator(int intGammaNumerator)
    {
      this.int_3 = intGammaNumerator;
    }

    public int GammaDenominator
    {
      get
      {
        return this.int_4;
      }
    }

    protected internal void SetGammaDenominator(int intGammaDenominator)
    {
      this.int_4 = intGammaDenominator;
    }

    public float GammaRatio
    {
      get
      {
        if (this.int_4 > 0)
          return (float) Math.Round((double) ((float) this.int_3 / (float) this.int_4), 1);
        return 1f;
      }
    }

    public int ColorCorrectionOffset
    {
      get
      {
        return this.int_5;
      }
    }

    protected internal void SetColorCorrectionOffset(int intColorCorrectionOffset)
    {
      this.int_5 = intColorCorrectionOffset;
    }

    public int PostageStampOffset
    {
      get
      {
        return this.int_6;
      }
    }

    protected internal void SetPostageStampOffset(int intPostageStampOffset)
    {
      this.int_6 = intPostageStampOffset;
    }

    public int ScanLineOffset
    {
      get
      {
        return this.int_7;
      }
    }

    protected internal void SetScanLineOffset(int intScanLineOffset)
    {
      this.int_7 = intScanLineOffset;
    }

    public int AttributesType
    {
      get
      {
        return this.int_8;
      }
    }

    protected internal void SetAttributesType(int intAttributesType)
    {
      this.int_8 = intAttributesType;
    }

    public List<int> ScanLineTable
    {
      get
      {
        return this.list_0;
      }
    }

    public List<Color> ColorCorrectionTable
    {
      get
      {
        return this.list_1;
      }
    }
  }
}
