// Decompiled with JetBrains decompiler
// Type: BRS.PEHeaderReader
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EA706D30-A741-450F-BB06-DFDA2E2B50C1
// Assembly location: G:\Games x2\AoTTG\BRS Mod v4\BRS Mod v4.0.7_Data\Managed\Assembly-CSharp.dll

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BRS
{
  public class PEHeaderReader
  {
    private PEHeaderReader.IMAGE_DOS_HEADER image_DOS_HEADER_0;
    private PEHeaderReader.IMAGE_FILE_HEADER image_FILE_HEADER_0;
    private PEHeaderReader.IMAGE_OPTIONAL_HEADER32 image_OPTIONAL_HEADER32_0;
    private PEHeaderReader.IMAGE_OPTIONAL_HEADER64 image_OPTIONAL_HEADER64_0;
    private PEHeaderReader.IMAGE_SECTION_HEADER[] image_SECTION_HEADER_0;

    public PEHeaderReader(string filePath)
    {
      using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
      {
        BinaryReader reader = new BinaryReader((Stream) fileStream);
        this.image_DOS_HEADER_0 = PEHeaderReader.FromBinaryReader<PEHeaderReader.IMAGE_DOS_HEADER>(reader);
        fileStream.Seek((long) this.image_DOS_HEADER_0.e_lfanew, SeekOrigin.Begin);
        int num = (int) reader.ReadUInt32();
        this.image_FILE_HEADER_0 = PEHeaderReader.FromBinaryReader<PEHeaderReader.IMAGE_FILE_HEADER>(reader);
        if (this.Is32BitHeader)
          this.image_OPTIONAL_HEADER32_0 = PEHeaderReader.FromBinaryReader<PEHeaderReader.IMAGE_OPTIONAL_HEADER32>(reader);
        else
          this.image_OPTIONAL_HEADER64_0 = PEHeaderReader.FromBinaryReader<PEHeaderReader.IMAGE_OPTIONAL_HEADER64>(reader);
        this.image_SECTION_HEADER_0 = new PEHeaderReader.IMAGE_SECTION_HEADER[(int) this.image_FILE_HEADER_0.NumberOfSections];
        for (int index = 0; index < this.image_SECTION_HEADER_0.Length; ++index)
          this.image_SECTION_HEADER_0[index] = PEHeaderReader.FromBinaryReader<PEHeaderReader.IMAGE_SECTION_HEADER>(reader);
      }
    }

    public static PEHeaderReader GetCallingAssemblyHeader()
    {
      return new PEHeaderReader(Assembly.GetCallingAssembly().Location);
    }

    public static PEHeaderReader GetAssemblyHeader()
    {
      return new PEHeaderReader(Assembly.GetAssembly(typeof (PEHeaderReader)).Location);
    }

    public static T FromBinaryReader<T>(BinaryReader reader)
    {
      GCHandle gcHandle = GCHandle.Alloc((object) reader.ReadBytes(Marshal.SizeOf(typeof (T))), GCHandleType.Pinned);
      T structure = (T) Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof (T));
      gcHandle.Free();
      return structure;
    }

    public bool Is32BitHeader
    {
      get
      {
        return (256 & (int) this.FileHeader.Characteristics) == 256;
      }
    }

    public PEHeaderReader.IMAGE_FILE_HEADER FileHeader
    {
      get
      {
        return this.image_FILE_HEADER_0;
      }
    }

    public PEHeaderReader.IMAGE_OPTIONAL_HEADER32 OptionalHeader32
    {
      get
      {
        return this.image_OPTIONAL_HEADER32_0;
      }
    }

    public PEHeaderReader.IMAGE_OPTIONAL_HEADER64 OptionalHeader64
    {
      get
      {
        return this.image_OPTIONAL_HEADER64_0;
      }
    }

    public PEHeaderReader.IMAGE_SECTION_HEADER[] ImageSectionHeaders
    {
      get
      {
        return this.image_SECTION_HEADER_0;
      }
    }

    public DateTime TimeStamp
    {
      get
      {
        DateTime time = new DateTime(1970, 1, 1, 0, 0, 0);
        time = time.AddSeconds((double) this.image_FILE_HEADER_0.TimeDateStamp);
        return time + TimeZone.CurrentTimeZone.GetUtcOffset(time);
      }
    }

    public struct IMAGE_DOS_HEADER
    {
      public ushort e_magic;
      public ushort e_cblp;
      public ushort e_cp;
      public ushort e_crlc;
      public ushort e_cparhdr;
      public ushort e_minalloc;
      public ushort e_maxalloc;
      public ushort e_ss;
      public ushort e_sp;
      public ushort e_csum;
      public ushort e_ip;
      public ushort e_cs;
      public ushort e_lfarlc;
      public ushort e_ovno;
      public ushort e_res_0;
      public ushort e_res_1;
      public ushort e_res_2;
      public ushort e_res_3;
      public ushort e_oemid;
      public ushort e_oeminfo;
      public ushort e_res2_0;
      public ushort e_res2_1;
      public ushort e_res2_2;
      public ushort e_res2_3;
      public ushort e_res2_4;
      public ushort e_res2_5;
      public ushort e_res2_6;
      public ushort e_res2_7;
      public ushort e_res2_8;
      public ushort e_res2_9;
      public uint e_lfanew;
    }

    public struct IMAGE_DATA_DIRECTORY
    {
      public uint VirtualAddress;
      public uint Size;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IMAGE_OPTIONAL_HEADER32
    {
      public ushort Magic;
      public byte MajorLinkerVersion;
      public byte MinorLinkerVersion;
      public uint SizeOfCode;
      public uint SizeOfInitializedData;
      public uint SizeOfUninitializedData;
      public uint AddressOfEntryPoint;
      public uint BaseOfCode;
      public uint BaseOfData;
      public uint ImageBase;
      public uint SectionAlignment;
      public uint FileAlignment;
      public ushort MajorOperatingSystemVersion;
      public ushort MinorOperatingSystemVersion;
      public ushort MajorImageVersion;
      public ushort MinorImageVersion;
      public ushort MajorSubsystemVersion;
      public ushort MinorSubsystemVersion;
      public uint Win32VersionValue;
      public uint SizeOfImage;
      public uint SizeOfHeaders;
      public uint CheckSum;
      public ushort Subsystem;
      public ushort DllCharacteristics;
      public uint SizeOfStackReserve;
      public uint SizeOfStackCommit;
      public uint SizeOfHeapReserve;
      public uint SizeOfHeapCommit;
      public uint LoaderFlags;
      public uint NumberOfRvaAndSizes;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY ExportTable;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY ImportTable;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY ResourceTable;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY ExceptionTable;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY CertificateTable;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY BaseRelocationTable;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY Debug;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY Architecture;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY GlobalPtr;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY TLSTable;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY LoadConfigTable;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY BoundImport;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY IAT;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY DelayImportDescriptor;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY CLRRuntimeHeader;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY Reserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IMAGE_OPTIONAL_HEADER64
    {
      public ushort Magic;
      public byte MajorLinkerVersion;
      public byte MinorLinkerVersion;
      public uint SizeOfCode;
      public uint SizeOfInitializedData;
      public uint SizeOfUninitializedData;
      public uint AddressOfEntryPoint;
      public uint BaseOfCode;
      public ulong ImageBase;
      public uint SectionAlignment;
      public uint FileAlignment;
      public ushort MajorOperatingSystemVersion;
      public ushort MinorOperatingSystemVersion;
      public ushort MajorImageVersion;
      public ushort MinorImageVersion;
      public ushort MajorSubsystemVersion;
      public ushort MinorSubsystemVersion;
      public uint Win32VersionValue;
      public uint SizeOfImage;
      public uint SizeOfHeaders;
      public uint CheckSum;
      public ushort Subsystem;
      public ushort DllCharacteristics;
      public ulong SizeOfStackReserve;
      public ulong SizeOfStackCommit;
      public ulong SizeOfHeapReserve;
      public ulong SizeOfHeapCommit;
      public uint LoaderFlags;
      public uint NumberOfRvaAndSizes;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY ExportTable;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY ImportTable;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY ResourceTable;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY ExceptionTable;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY CertificateTable;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY BaseRelocationTable;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY Debug;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY Architecture;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY GlobalPtr;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY TLSTable;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY LoadConfigTable;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY BoundImport;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY IAT;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY DelayImportDescriptor;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY CLRRuntimeHeader;
      public PEHeaderReader.IMAGE_DATA_DIRECTORY Reserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IMAGE_FILE_HEADER
    {
      public ushort Machine;
      public ushort NumberOfSections;
      public uint TimeDateStamp;
      public uint PointerToSymbolTable;
      public uint NumberOfSymbols;
      public ushort SizeOfOptionalHeader;
      public ushort Characteristics;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct IMAGE_SECTION_HEADER
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
      [FieldOffset(0)]
      public char[] Name;
      [FieldOffset(8)]
      public uint VirtualSize;
      [FieldOffset(12)]
      public uint VirtualAddress;
      [FieldOffset(16)]
      public uint SizeOfRawData;
      [FieldOffset(20)]
      public uint PointerToRawData;
      [FieldOffset(24)]
      public uint PointerToRelocations;
      [FieldOffset(28)]
      public uint PointerToLinenumbers;
      [FieldOffset(32)]
      public ushort NumberOfRelocations;
      [FieldOffset(34)]
      public ushort NumberOfLinenumbers;
      [FieldOffset(36)]
      public PEHeaderReader.DataSectionFlags Characteristics;

      public string Section
      {
        get
        {
          return new string(this.Name);
        }
      }
    }

    [Flags]
    public enum DataSectionFlags : uint
    {
      TypeReg = 0,
      TypeDsect = 1,
      TypeNoLoad = 2,
      TypeGroup = 4,
      TypeNoPadded = 8,
      TypeCopy = 16, // 0x00000010
      ContentCode = 32, // 0x00000020
      ContentInitializedData = 64, // 0x00000040
      ContentUninitializedData = 128, // 0x00000080
      LinkOther = 256, // 0x00000100
      LinkInfo = 512, // 0x00000200
      TypeOver = 1024, // 0x00000400
      LinkRemove = 2048, // 0x00000800
      LinkComDat = 4096, // 0x00001000
      NoDeferSpecExceptions = 16384, // 0x00004000
      RelativeGP = 32768, // 0x00008000
      MemPurgeable = 131072, // 0x00020000
      Memory16Bit = MemPurgeable, // 0x00020000
      MemoryLocked = 262144, // 0x00040000
      MemoryPreload = 524288, // 0x00080000
      Align1Bytes = 1048576, // 0x00100000
      Align2Bytes = 2097152, // 0x00200000
      Align4Bytes = Align2Bytes | Align1Bytes, // 0x00300000
      Align8Bytes = 4194304, // 0x00400000
      Align16Bytes = Align8Bytes | Align1Bytes, // 0x00500000
      Align32Bytes = Align8Bytes | Align2Bytes, // 0x00600000
      Align64Bytes = Align32Bytes | Align1Bytes, // 0x00700000
      Align128Bytes = 8388608, // 0x00800000
      Align256Bytes = Align128Bytes | Align1Bytes, // 0x00900000
      Align512Bytes = Align128Bytes | Align2Bytes, // 0x00A00000
      Align1024Bytes = Align512Bytes | Align1Bytes, // 0x00B00000
      Align2048Bytes = Align128Bytes | Align8Bytes, // 0x00C00000
      Align4096Bytes = Align2048Bytes | Align1Bytes, // 0x00D00000
      Align8192Bytes = Align2048Bytes | Align2Bytes, // 0x00E00000
      LinkExtendedRelocationOverflow = 16777216, // 0x01000000
      MemoryDiscardable = 33554432, // 0x02000000
      MemoryNotCached = 67108864, // 0x04000000
      MemoryNotPaged = 134217728, // 0x08000000
      MemoryShared = 268435456, // 0x10000000
      MemoryExecute = 536870912, // 0x20000000
      MemoryRead = 1073741824, // 0x40000000
      MemoryWrite = 2147483648, // 0x80000000
    }
  }
}
