// Decompiled with JetBrains decompiler
// Type: BRS.Importer.TargaImage
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA706D30-A741-450F-BB06-DFDA2E2B50C1
// Assembly location: G:\Games x2\AoTTG\BRS Mod v4\BRS Mod v4.0.7_Data\Managed\Assembly-CSharp.dll

using ns18;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace BRS.Importer
{
  public class TargaImage
  {
    private string string_0 = string.Empty;
    private List<List<byte>> list_0 = new List<List<byte>>();
    private List<byte> list_1 = new List<byte>();
    private TargaHeader targaHeader_0;
    private TargaExtensionArea targaExtensionArea_0;
    private TargaFooter targaFooter_0;
    private Texture2D texture2D_0;
    private Texture2D texture2D_1;
    private TGAFormat tgaformat_0;
    private int int_0;
    private int int_1;

    public TargaImage()
    {
      this.targaFooter_0 = new TargaFooter();
      this.targaHeader_0 = new TargaHeader();
      this.targaExtensionArea_0 = new TargaExtensionArea();
      this.texture2D_0 = (Texture2D) null;
      this.texture2D_1 = (Texture2D) null;
    }

    public TargaHeader Header
    {
      get
      {
        return this.targaHeader_0;
      }
    }

    public TargaExtensionArea ExtensionArea
    {
      get
      {
        return this.targaExtensionArea_0;
      }
    }

    public TargaFooter Footer
    {
      get
      {
        return this.targaFooter_0;
      }
    }

    public TGAFormat Format
    {
      get
      {
        return this.tgaformat_0;
      }
    }

    public Texture Image
    {
      get
      {
        return (Texture) this.texture2D_0;
      }
    }

    public Texture Thumbnail
    {
      get
      {
        return (Texture) this.texture2D_1;
      }
    }

    public string FileName
    {
      get
      {
        return this.string_0;
      }
    }

    public int Stride
    {
      get
      {
        return this.int_0;
      }
    }

    public int Padding
    {
      get
      {
        return this.int_1;
      }
    }

    public TargaImage(string strFileName)
      : this()
    {
      if (!(Path.GetExtension(strFileName).ToLower() == ".tga"))
        throw new Exception("Error loading file, file '" + strFileName + "' must have an extension of '.tga'.");
      if (!File.Exists(strFileName))
        throw new Exception("Error loading file, could not find file '" + strFileName + "' on disk.");
      this.string_0 = strFileName;
      byte[] buffer = File.ReadAllBytes(this.string_0);
      if (buffer == null || buffer.Length <= 0)
        throw new Exception("Error loading file, could not read file from disk.");
      MemoryStream memoryStream;
      using (memoryStream = new MemoryStream(buffer))
      {
        if (memoryStream == null || memoryStream.Length <= 0L || !memoryStream.CanSeek)
          throw new Exception("Error loading file, could not read file from disk.");
        BinaryReader binReader;
        using (binReader = new BinaryReader((Stream) memoryStream))
        {
          this.method_0(binReader);
          this.method_1(binReader);
          this.method_2(binReader);
          this.method_4(binReader);
        }
      }
    }

    private void method_0(BinaryReader binReader)
    {
      if (binReader != null && binReader.BaseStream != null && binReader.BaseStream.Length > 0L)
      {
        if (binReader.BaseStream.CanSeek)
        {
          try
          {
            binReader.BaseStream.Seek(-18L, SeekOrigin.End);
            string str = Encoding.ASCII.GetString(binReader.ReadBytes(16)).TrimEnd(new char[1]);
            if (string.Compare(str, "TRUEVISION-XFILE") == 0)
            {
              this.tgaformat_0 = TGAFormat.NEW_TGA;
              binReader.BaseStream.Seek(-26L, SeekOrigin.End);
              int intExtensionAreaOffset = binReader.ReadInt32();
              int intDeveloperDirectoryOffset = binReader.ReadInt32();
              binReader.ReadBytes(16);
              string strReservedCharacter = Encoding.ASCII.GetString(binReader.ReadBytes(1)).TrimEnd(new char[1]);
              this.targaFooter_0.SetExtensionAreaOffset(intExtensionAreaOffset);
              this.targaFooter_0.SetDeveloperDirectoryOffset(intDeveloperDirectoryOffset);
              this.targaFooter_0.SetSignature(str);
              this.targaFooter_0.SetReservedCharacter(strReservedCharacter);
              return;
            }
            this.tgaformat_0 = TGAFormat.ORIGINAL_TGA;
            return;
          }
          catch (Exception ex)
          {
            this.method_7();
            throw ex;
          }
        }
      }
      this.method_7();
      throw new Exception("Error loading file, could not read file from disk.");
    }

    private void method_1(BinaryReader binReader)
    {
      if (binReader != null && binReader.BaseStream != null && binReader.BaseStream.Length > 0L)
      {
        if (binReader.BaseStream.CanSeek)
        {
          try
          {
            binReader.BaseStream.Seek(0L, SeekOrigin.Begin);
            this.targaHeader_0.SetImageIDLength(binReader.ReadByte());
            this.targaHeader_0.SetColorMapType((ColorMapType) binReader.ReadByte());
            this.targaHeader_0.SetImageType((ImageType) binReader.ReadByte());
            this.targaHeader_0.SetColorMapFirstEntryIndex(binReader.ReadInt16());
            this.targaHeader_0.SetColorMapLength(binReader.ReadInt16());
            this.targaHeader_0.SetColorMapEntrySize(binReader.ReadByte());
            this.targaHeader_0.SetXOrigin(binReader.ReadInt16());
            this.targaHeader_0.SetYOrigin(binReader.ReadInt16());
            this.targaHeader_0.SetWidth(binReader.ReadInt16());
            this.targaHeader_0.SetHeight(binReader.ReadInt16());
            byte bPixelDepth = binReader.ReadByte();
            switch (bPixelDepth)
            {
              case 8:
              case 16:
              case 24:
              case 32:
                this.targaHeader_0.SetPixelDepth(bPixelDepth);
                byte b = binReader.ReadByte();
                this.targaHeader_0.SetAttributeBits((byte) Class64.smethod_0(b, 0, 4));
                this.targaHeader_0.SetVerticalTransferOrder((VerticalTransferOrder) Class64.smethod_0(b, 5, 1));
                this.targaHeader_0.SetHorizontalTransferOrder((HorizontalTransferOrder) Class64.smethod_0(b, 4, 1));
                if (this.targaHeader_0.ImageIDLength > (byte) 0)
                {
                  this.targaHeader_0.SetImageIDValue(Encoding.ASCII.GetString(binReader.ReadBytes((int) this.targaHeader_0.ImageIDLength)).TrimEnd(new char[1]));
                  break;
                }
                break;
              default:
                this.method_7();
                throw new Exception("Targa Image only supports 8, 16, 24, or 32 bit pixel depths.");
            }
          }
          catch (Exception ex)
          {
            this.method_7();
            throw ex;
          }
          if (this.targaHeader_0.ColorMapType == ColorMapType.COLOR_MAP_INCLUDED)
          {
            if (this.targaHeader_0.ImageType != ImageType.UNCOMPRESSED_COLOR_MAPPED && this.targaHeader_0.ImageType != ImageType.RUN_LENGTH_ENCODED_COLOR_MAPPED)
              return;
            if (this.targaHeader_0.ColorMapLength > (short) 0)
            {
              try
              {
                for (int index = 0; index < (int) this.targaHeader_0.ColorMapLength; ++index)
                {
                  switch (this.targaHeader_0.ColorMapEntrySize)
                  {
                    case 15:
                      byte[] numArray1 = binReader.ReadBytes(2);
                      this.targaHeader_0.ColorMap.Add(Class64.smethod_1(numArray1[1], numArray1[0]));
                      break;
                    case 16:
                      byte[] numArray2 = binReader.ReadBytes(2);
                      this.targaHeader_0.ColorMap.Add(Class64.smethod_1(numArray2[1], numArray2[0]));
                      break;
                    case 24:
                      byte b1 = binReader.ReadByte();
                      byte g1 = binReader.ReadByte();
                      this.targaHeader_0.ColorMap.Add((Color) new Color32(binReader.ReadByte(), g1, b1, byte.MaxValue));
                      break;
                    case 32:
                      byte a = binReader.ReadByte();
                      byte b2 = binReader.ReadByte();
                      byte g2 = binReader.ReadByte();
                      this.targaHeader_0.ColorMap.Add((Color) new Color32(binReader.ReadByte(), g2, b2, a));
                      break;
                    default:
                      this.method_7();
                      throw new Exception("TargaImage only supports ColorMap Entry Sizes of 15, 16, 24 or 32 bits.");
                  }
                }
                return;
              }
              catch (Exception ex)
              {
                this.method_7();
                throw ex;
              }
            }
            else
            {
              this.method_7();
              throw new Exception("Image Type requires a Color Map and Color Map Length is zero.");
            }
          }
          else
          {
            if (this.targaHeader_0.ImageType != ImageType.UNCOMPRESSED_COLOR_MAPPED && this.targaHeader_0.ImageType != ImageType.RUN_LENGTH_ENCODED_COLOR_MAPPED)
              return;
            this.method_7();
            throw new Exception("Image Type requires a Color Map and there was not a Color Map included in the file.");
          }
        }
      }
      this.method_7();
      throw new Exception("Error loading file, could not read file from disk.");
    }

    private void method_2(BinaryReader binReader)
    {
      if (binReader != null && binReader.BaseStream != null && (binReader.BaseStream.Length > 0L && binReader.BaseStream.CanSeek))
      {
        if (this.targaFooter_0.ExtensionAreaOffset <= 0)
          return;
        try
        {
          binReader.BaseStream.Seek((long) this.targaFooter_0.ExtensionAreaOffset, SeekOrigin.Begin);
          this.targaExtensionArea_0.SetExtensionSize((int) binReader.ReadInt16());
          this.targaExtensionArea_0.SetAuthorName(Encoding.ASCII.GetString(binReader.ReadBytes(41)).TrimEnd(new char[1]));
          this.targaExtensionArea_0.SetAuthorComments(Encoding.ASCII.GetString(binReader.ReadBytes(324)).TrimEnd(new char[1]));
          short num1 = binReader.ReadInt16();
          short num2 = binReader.ReadInt16();
          short num3 = binReader.ReadInt16();
          short num4 = binReader.ReadInt16();
          short num5 = binReader.ReadInt16();
          short num6 = binReader.ReadInt16();
          DateTime result;
          if (DateTime.TryParse(num1.ToString() + "/" + num2.ToString() + "/" + num3.ToString() + " " + num4.ToString() + ":" + num5.ToString() + ":" + num6.ToString(), out result))
            this.targaExtensionArea_0.SetDateTimeStamp(result);
          this.targaExtensionArea_0.SetJobName(Encoding.ASCII.GetString(binReader.ReadBytes(41)).TrimEnd(new char[1]));
          num4 = binReader.ReadInt16();
          num5 = binReader.ReadInt16();
          short num7 = binReader.ReadInt16();
          this.targaExtensionArea_0.SetJobTime(new TimeSpan((int) num4, (int) num5, (int) num7));
          this.targaExtensionArea_0.SetSoftwareID(Encoding.ASCII.GetString(binReader.ReadBytes(41)).TrimEnd(new char[1]));
          this.targaExtensionArea_0.SetSoftwareID(((float) binReader.ReadInt16() / 100f).ToString("F2") + Encoding.ASCII.GetString(binReader.ReadBytes(1)).TrimEnd(new char[1]));
          byte a1 = binReader.ReadByte();
          byte r1 = binReader.ReadByte();
          byte b1 = binReader.ReadByte();
          byte g1 = binReader.ReadByte();
          this.targaExtensionArea_0.SetKeyColor((Color) new Color32(r1, g1, b1, a1));
          this.targaExtensionArea_0.SetPixelAspectRatioNumerator((int) binReader.ReadInt16());
          this.targaExtensionArea_0.SetPixelAspectRatioDenominator((int) binReader.ReadInt16());
          this.targaExtensionArea_0.SetGammaNumerator((int) binReader.ReadInt16());
          this.targaExtensionArea_0.SetGammaDenominator((int) binReader.ReadInt16());
          this.targaExtensionArea_0.SetColorCorrectionOffset(binReader.ReadInt32());
          this.targaExtensionArea_0.SetPostageStampOffset(binReader.ReadInt32());
          this.targaExtensionArea_0.SetScanLineOffset(binReader.ReadInt32());
          this.targaExtensionArea_0.SetAttributesType((int) binReader.ReadByte());
          if (this.targaExtensionArea_0.ScanLineOffset > 0)
          {
            binReader.BaseStream.Seek((long) this.targaExtensionArea_0.ScanLineOffset, SeekOrigin.Begin);
            for (int index = 0; index < (int) this.targaHeader_0.Height; ++index)
              this.targaExtensionArea_0.ScanLineTable.Add(binReader.ReadInt32());
          }
          if (this.targaExtensionArea_0.ColorCorrectionOffset <= 0)
            return;
          binReader.BaseStream.Seek((long) this.targaExtensionArea_0.ColorCorrectionOffset, SeekOrigin.Begin);
          for (int index = 0; index < 256; ++index)
          {
            byte a2 = Convert.ToByte(binReader.ReadInt16());
            byte r2 = Convert.ToByte(binReader.ReadInt16());
            byte b2 = Convert.ToByte(binReader.ReadInt16());
            byte g2 = Convert.ToByte(binReader.ReadInt16());
            this.targaExtensionArea_0.ColorCorrectionTable.Add((Color) new Color32(r2, g2, b2, a2));
          }
        }
        catch (Exception ex)
        {
          this.method_7();
          throw ex;
        }
      }
      else
      {
        this.method_7();
        throw new Exception("Error loading file, could not read file from disk.");
      }
    }

    private byte[] method_3(BinaryReader binReader)
    {
      if (binReader != null && binReader.BaseStream != null && (binReader.BaseStream.Length > 0L && binReader.BaseStream.CanSeek))
      {
        if (this.targaHeader_0.ImageDataOffset > 0)
        {
          byte[] buffer = new byte[this.int_1];
          binReader.BaseStream.Seek((long) this.targaHeader_0.ImageDataOffset, SeekOrigin.Begin);
          int num1 = (int) this.targaHeader_0.Width * this.targaHeader_0.BytesPerPixel;
          int capacity = num1 * (int) this.targaHeader_0.Height;
          if (this.targaHeader_0.ImageType != ImageType.RUN_LENGTH_ENCODED_BLACK_AND_WHITE && this.targaHeader_0.ImageType != ImageType.RUN_LENGTH_ENCODED_COLOR_MAPPED && this.targaHeader_0.ImageType != ImageType.RUN_LENGTH_ENCODED_TRUE_COLOR)
          {
            for (int index = 0; index < (int) this.targaHeader_0.Height; ++index)
            {
              this.list_1.AddRange((IEnumerable<byte>) binReader.ReadBytes(num1));
              this.list_0.Add(this.list_1);
              this.list_1 = new List<byte>(num1);
            }
          }
          else
          {
            int num2 = 0;
            int num3 = 0;
            while (num2 < capacity)
            {
              byte b = binReader.ReadByte();
              int num4 = Class64.smethod_0(b, 7, 1);
              int num5 = Class64.smethod_0(b, 0, 7) + 1;
              switch (num4)
              {
                case 0:
                  int num6 = num5 * this.targaHeader_0.BytesPerPixel;
                  for (int index = 0; index < num6; ++index)
                  {
                    this.list_1.Add(binReader.ReadByte());
                    ++num2;
                    ++num3;
                    if (num3 == num1)
                    {
                      this.list_0.Add(this.list_1);
                      this.list_1 = new List<byte>(num1);
                      num3 = 0;
                    }
                  }
                  continue;
                case 1:
                  byte[] numArray = binReader.ReadBytes(this.targaHeader_0.BytesPerPixel);
                  for (int index = 0; index < num5; ++index)
                  {
                    foreach (byte num7 in numArray)
                      this.list_1.Add(num7);
                    num3 += numArray.Length;
                    num2 += numArray.Length;
                    if (num3 == num1)
                    {
                      this.list_0.Add(this.list_1);
                      this.list_1 = new List<byte>(capacity);
                      num3 = 0;
                    }
                  }
                  continue;
                default:
                  continue;
              }
            }
          }
          bool flag1 = true;
          bool flag2 = false;
          switch (this.targaHeader_0.FirstPixelDestination)
          {
            case FirstPixelDestination.UNKNOWN:
            case FirstPixelDestination.BOTTOM_RIGHT:
              flag1 = false;
              flag2 = false;
              break;
            case FirstPixelDestination.TOP_LEFT:
              flag1 = true;
              flag2 = true;
              break;
            case FirstPixelDestination.TOP_RIGHT:
              flag1 = true;
              flag2 = false;
              break;
            case FirstPixelDestination.BOTTOM_LEFT:
              flag1 = false;
              flag2 = true;
              break;
          }
          MemoryStream memoryStream;
          using (memoryStream = new MemoryStream())
          {
            if (flag1)
              this.list_0.Reverse();
            for (int index = 0; index < this.list_0.Count; ++index)
            {
              if (flag2)
                this.list_0[index].Reverse();
              byte[] array = this.list_0[index].ToArray();
              memoryStream.Write(array, 0, array.Length);
              memoryStream.Write(buffer, 0, buffer.Length);
            }
            return memoryStream.ToArray();
          }
        }
        else
        {
          this.method_7();
          throw new Exception("Error loading file, No image data in file.");
        }
      }
      else
      {
        this.method_7();
        throw new Exception("Error loading file, could not read file from disk.");
      }
    }

    private void method_4(BinaryReader binReader)
    {
      this.int_0 = ((int) this.targaHeader_0.Width * (int) this.targaHeader_0.PixelDepth + 31 & -32) >> 3;
      this.int_1 = this.int_0 - ((int) this.targaHeader_0.Width * (int) this.targaHeader_0.PixelDepth + 7) / 8;
      byte[] data = this.method_3(binReader);
      TextureFormat textureFormat = this.method_5();
      this.texture2D_0 = new Texture2D((int) this.targaHeader_0.Width, (int) this.targaHeader_0.Height, textureFormat, false);
      if (textureFormat == TextureFormat.BGRA32)
      {
        int index = 0;
        while (index < data.Length)
        {
          byte num = data[index];
          data[index] = data[index + 2];
          data[index + 2] = num;
          index += 4;
        }
      }
      this.texture2D_0.LoadRawTextureData(data);
      this.texture2D_0.Apply(false, false);
      this.method_6(binReader, textureFormat);
    }

    private TextureFormat method_5()
    {
      TextureFormat textureFormat = TextureFormat.ARGB32;
      switch (this.targaHeader_0.PixelDepth)
      {
        case 8:
          textureFormat = TextureFormat.Alpha8;
          break;
        case 16:
          if (this.Format == TGAFormat.NEW_TGA)
          {
            switch (this.targaExtensionArea_0.AttributesType)
            {
              case 0:
              case 1:
              case 2:
                textureFormat = TextureFormat.RGB565;
                break;
              case 3:
                textureFormat = TextureFormat.RGB565;
                break;
            }
          }
          else
          {
            textureFormat = TextureFormat.RGB565;
            break;
          }
        case 24:
          textureFormat = TextureFormat.RGB24;
          break;
        case 32:
          if (this.Format == TGAFormat.NEW_TGA)
          {
            switch (this.targaExtensionArea_0.AttributesType)
            {
              case 0:
              case 3:
                textureFormat = TextureFormat.BGRA32;
                break;
              case 1:
              case 2:
                textureFormat = TextureFormat.BGRA32;
                break;
              case 4:
                textureFormat = TextureFormat.BGRA32;
                break;
            }
          }
          else
          {
            textureFormat = TextureFormat.BGRA32;
            break;
          }
      }
      return textureFormat;
    }

    private void method_6(BinaryReader binReader, TextureFormat pfPixelFormat)
    {
      byte[] data = (byte[]) null;
      if (binReader != null && binReader.BaseStream != null && (binReader.BaseStream.Length > 0L && binReader.BaseStream.CanSeek))
      {
        if (this.ExtensionArea.PostageStampOffset > 0)
        {
          binReader.BaseStream.Seek((long) this.ExtensionArea.PostageStampOffset, SeekOrigin.Begin);
          int width = (int) binReader.ReadByte();
          int height = (int) binReader.ReadByte();
          int length = ((width * (int) this.targaHeader_0.PixelDepth + 31 & -32) >> 3) - (width * (int) this.targaHeader_0.PixelDepth + 7) / 8;
          List<List<byte>> byteListList = new List<List<byte>>();
          List<byte> byteList = new List<byte>();
          byte[] buffer = new byte[length];
          bool flag1 = false;
          bool flag2 = false;
          MemoryStream memoryStream;
          using (memoryStream = new MemoryStream())
          {
            int num = width * ((int) this.targaHeader_0.PixelDepth / 8);
            for (int index = 0; index < height; ++index)
            {
              byteList.AddRange((IEnumerable<byte>) binReader.ReadBytes(num));
              byteListList.Add(byteList);
              byteList = new List<byte>(num);
            }
            switch (this.targaHeader_0.FirstPixelDestination)
            {
              case FirstPixelDestination.UNKNOWN:
              case FirstPixelDestination.BOTTOM_RIGHT:
                flag2 = true;
                flag1 = false;
                break;
              case FirstPixelDestination.TOP_RIGHT:
                flag2 = false;
                flag1 = false;
                break;
            }
            if (flag2)
              byteListList.Reverse();
            for (int index = 0; index < byteListList.Count; ++index)
            {
              if (flag1)
                byteListList[index].Reverse();
              byte[] array = byteListList[index].ToArray();
              memoryStream.Write(array, 0, array.Length);
              memoryStream.Write(buffer, 0, buffer.Length);
            }
            data = memoryStream.ToArray();
          }
          if (data == null || data.Length <= 0)
            return;
          this.texture2D_1 = new Texture2D(width, height, pfPixelFormat, false);
          this.texture2D_1.LoadRawTextureData(data);
        }
        else
        {
          UnityEngine.Object.Destroy((UnityEngine.Object) this.texture2D_1);
          this.texture2D_1 = (Texture2D) null;
        }
      }
      else
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) this.texture2D_1);
        this.texture2D_1 = (Texture2D) null;
      }
    }

    private void method_7()
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) this.texture2D_0);
      UnityEngine.Object.Destroy((UnityEngine.Object) this.texture2D_1);
      this.texture2D_0 = (Texture2D) null;
      this.texture2D_1 = (Texture2D) null;
      this.targaHeader_0 = new TargaHeader();
      this.targaExtensionArea_0 = new TargaExtensionArea();
      this.targaFooter_0 = new TargaFooter();
      this.tgaformat_0 = TGAFormat.UNKNOWN;
      this.int_0 = 0;
      this.int_1 = 0;
      this.list_0.Clear();
      this.list_1.Clear();
      this.string_0 = string.Empty;
    }

    public static Texture2D LoadTargaImage(string sFileName)
    {
      return new TargaImage(sFileName).texture2D_0;
    }
  }
}
