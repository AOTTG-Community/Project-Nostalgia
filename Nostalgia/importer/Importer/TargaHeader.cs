// Decompiled with JetBrains decompiler
// Type: BRS.Importer.TargaHeader
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA706D30-A741-450F-BB06-DFDA2E2B50C1
// Assembly location: G:\Games x2\AoTTG\BRS Mod v4\BRS Mod v4.0.7_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

namespace BRS.Importer
{
  public class TargaHeader
  {
    private VerticalTransferOrder verticalTransferOrder_0 = VerticalTransferOrder.UNKNOWN;
    private HorizontalTransferOrder horizontalTransferOrder_0 = HorizontalTransferOrder.UNKNOWN;
    private string string_0 = string.Empty;
    private List<Color> list_0 = new List<Color>();
    private byte byte_0;
    private ColorMapType colorMapType_0;
    private ImageType imageType_0;
    private short short_0;
    private short short_1;
    private byte byte_1;
    private short short_2;
    private short short_3;
    private short short_4;
    private short short_5;
    private byte byte_2;
    private byte byte_3;
    private byte byte_4;

    public byte ImageIDLength
    {
      get
      {
        return this.byte_0;
      }
    }

    protected internal void SetImageIDLength(byte bImageIDLength)
    {
      this.byte_0 = bImageIDLength;
    }

    public ColorMapType ColorMapType
    {
      get
      {
        return this.colorMapType_0;
      }
    }

    protected internal void SetColorMapType(ColorMapType eColorMapType)
    {
      this.colorMapType_0 = eColorMapType;
    }

    public ImageType ImageType
    {
      get
      {
        return this.imageType_0;
      }
    }

    protected internal void SetImageType(ImageType eImageType)
    {
      this.imageType_0 = eImageType;
    }

    public short ColorMapFirstEntryIndex
    {
      get
      {
        return this.short_0;
      }
    }

    protected internal void SetColorMapFirstEntryIndex(short sColorMapFirstEntryIndex)
    {
      this.short_0 = sColorMapFirstEntryIndex;
    }

    public short ColorMapLength
    {
      get
      {
        return this.short_1;
      }
    }

    protected internal void SetColorMapLength(short sColorMapLength)
    {
      this.short_1 = sColorMapLength;
    }

    public byte ColorMapEntrySize
    {
      get
      {
        return this.byte_1;
      }
    }

    protected internal void SetColorMapEntrySize(byte bColorMapEntrySize)
    {
      this.byte_1 = bColorMapEntrySize;
    }

    public short XOrigin
    {
      get
      {
        return this.short_2;
      }
    }

    protected internal void SetXOrigin(short sXOrigin)
    {
      this.short_2 = sXOrigin;
    }

    public short YOrigin
    {
      get
      {
        return this.short_3;
      }
    }

    protected internal void SetYOrigin(short sYOrigin)
    {
      this.short_3 = sYOrigin;
    }

    public short Width
    {
      get
      {
        return this.short_4;
      }
    }

    protected internal void SetWidth(short sWidth)
    {
      this.short_4 = sWidth;
    }

    public short Height
    {
      get
      {
        return this.short_5;
      }
    }

    protected internal void SetHeight(short sHeight)
    {
      this.short_5 = sHeight;
    }

    public byte PixelDepth
    {
      get
      {
        return this.byte_2;
      }
    }

    protected internal void SetPixelDepth(byte bPixelDepth)
    {
      this.byte_2 = bPixelDepth;
    }

    protected internal byte ImageDescriptor
    {
      get
      {
        return this.byte_3;
      }
      set
      {
        this.byte_3 = value;
      }
    }

    public FirstPixelDestination FirstPixelDestination
    {
      get
      {
        if (this.verticalTransferOrder_0 == VerticalTransferOrder.UNKNOWN || this.horizontalTransferOrder_0 == HorizontalTransferOrder.UNKNOWN)
          return FirstPixelDestination.UNKNOWN;
        if (this.verticalTransferOrder_0 == VerticalTransferOrder.BOTTOM && this.horizontalTransferOrder_0 == HorizontalTransferOrder.LEFT)
          return FirstPixelDestination.BOTTOM_LEFT;
        if (this.verticalTransferOrder_0 == VerticalTransferOrder.BOTTOM && this.horizontalTransferOrder_0 == HorizontalTransferOrder.RIGHT)
          return FirstPixelDestination.BOTTOM_RIGHT;
        return this.verticalTransferOrder_0 == VerticalTransferOrder.TOP && this.horizontalTransferOrder_0 == HorizontalTransferOrder.LEFT ? FirstPixelDestination.TOP_LEFT : FirstPixelDestination.TOP_RIGHT;
      }
    }

    public VerticalTransferOrder VerticalTransferOrder
    {
      get
      {
        return this.verticalTransferOrder_0;
      }
    }

    protected internal void SetVerticalTransferOrder(VerticalTransferOrder eVerticalTransferOrder)
    {
      this.verticalTransferOrder_0 = eVerticalTransferOrder;
    }

    public HorizontalTransferOrder HorizontalTransferOrder
    {
      get
      {
        return this.horizontalTransferOrder_0;
      }
    }

    protected internal void SetHorizontalTransferOrder(HorizontalTransferOrder eHorizontalTransferOrder)
    {
      this.horizontalTransferOrder_0 = eHorizontalTransferOrder;
    }

    public byte AttributeBits
    {
      get
      {
        return this.byte_4;
      }
    }

    protected internal void SetAttributeBits(byte bAttributeBits)
    {
      this.byte_4 = bAttributeBits;
    }

    public string ImageIDValue
    {
      get
      {
        return this.string_0;
      }
    }

    protected internal void SetImageIDValue(string strImageIDValue)
    {
      this.string_0 = strImageIDValue;
    }

    public List<Color> ColorMap
    {
      get
      {
        return this.list_0;
      }
    }

    public int ImageDataOffset
    {
      get
      {
        int num1 = 18 + (int) this.byte_0;
        int num2 = 0;
        switch (this.byte_1)
        {
          case 15:
            num2 = 2;
            break;
          case 16:
            num2 = 2;
            break;
          case 24:
            num2 = 3;
            break;
          case 32:
            num2 = 4;
            break;
        }
        return num1 + (int) this.short_1 * num2;
      }
    }

    public int BytesPerPixel
    {
      get
      {
        return (int) this.byte_2 / 8;
      }
    }
  }
}
