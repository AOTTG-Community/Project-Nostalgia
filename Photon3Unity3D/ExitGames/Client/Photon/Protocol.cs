using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ExitGames.Client.Photon
{
  public class Protocol
  {
    internal static readonly Dictionary<Type, CustomType> TypeDict = new Dictionary<Type, CustomType>();
    internal static readonly Dictionary<byte, CustomType> CodeDict = new Dictionary<byte, CustomType>();
    public const string protocolType = "GpBinaryV16";

    internal static bool TryRegisterType(Type type, byte typeCode, SerializeMethod serializeFunction, DeserializeMethod deserializeFunction)
    {
      if (Protocol.CodeDict.ContainsKey(typeCode) || Protocol.TypeDict.ContainsKey(type))
        return false;
      CustomType customType = new CustomType(type, typeCode, serializeFunction, deserializeFunction);
      Protocol.CodeDict.Add(typeCode, customType);
      Protocol.TypeDict.Add(type, customType);
      return true;
    }

    private static bool SerializeCustom(MemoryStream dout, object serObject)
    {
      CustomType customType;
      if (!Protocol.TypeDict.TryGetValue(serObject.GetType(), out customType))
        return false;
      byte[] buffer = customType.SerializeFunction(serObject);
      dout.WriteByte((byte) 99);
      dout.WriteByte(customType.Code);
      Protocol.SerializeShort(dout, (short) buffer.Length, false);
      dout.Write(buffer, 0, buffer.Length);
      return true;
    }

    private static object DeserializeCustom(MemoryStream din, byte customTypeCode)
    {
      short num = Protocol.DeserializeShort(din);
      byte[] numArray = new byte[(int) num];
      din.Read(numArray, 0, (int) num);
      CustomType customType;
      if (Protocol.CodeDict.TryGetValue(customTypeCode, out customType))
        return customType.DeserializeFunction(numArray);
      return (object) null;
    }

    public static byte[] Serialize(object obj)
    {
      MemoryStream dout = new MemoryStream();
      Protocol.Serialize(dout, obj, true);
      return dout.ToArray();
    }

    public static object Deserialize(byte[] serializedData)
    {
      MemoryStream din = new MemoryStream(serializedData);
      return Protocol.Deserialize(din, (byte) din.ReadByte());
    }

    internal static object DeserializeMessage(MemoryStream stream)
    {
      return Protocol.Deserialize(stream, (byte) stream.ReadByte());
    }

    internal static byte[] DeserializeRawMessage(MemoryStream stream)
    {
      return (byte[]) Protocol.Deserialize(stream, (byte) stream.ReadByte());
    }

    internal static void SerializeMessage(MemoryStream ms, object msg)
    {
      Protocol.Serialize(ms, msg, true);
    }

    private static Type GetTypeOfCode(byte typeCode)
    {
      byte num = typeCode;
      if ((uint) num <= 42U)
      {
        if ((int) num == 0 || (int) num == 42)
          return typeof (object);
      }
      else
      {
        switch (num)
        {
          case 68:
            return typeof (IDictionary);
          case 97:
            return typeof (string[]);
          case 98:
            return typeof (byte);
          case 99:
            return typeof (CustomType);
          case 100:
            return typeof (double);
          case 101:
            return typeof (EventData);
          case 102:
            return typeof (float);
          case 104:
            return typeof (Hashtable);
          case 105:
            return typeof (int);
          case 107:
            return typeof (short);
          case 108:
            return typeof (long);
          case 110:
            return typeof (int[]);
          case 111:
            return typeof (bool);
          case 112:
            return typeof (OperationResponse);
          case 113:
            return typeof (OperationRequest);
          case 115:
            return typeof (string);
          case 120:
            return typeof (byte[]);
          case 121:
            return typeof (Array);
          case 122:
            return typeof (object[]);
        }
      }
      Debug.WriteLine("missing type: " + (object) typeCode);
      throw new Exception("deserialize(): " + (object) typeCode);
    }

    private static byte GetCodeOfType(Type type)
    {
      switch (Type.GetTypeCode(type))
      {
        case TypeCode.Boolean:
          return 111;
        case TypeCode.Byte:
          return 98;
        case TypeCode.Int16:
          return 107;
        case TypeCode.Int32:
          return 105;
        case TypeCode.Int64:
          return 108;
        case TypeCode.Single:
          return 102;
        case TypeCode.Double:
          return 100;
        case TypeCode.String:
          return 115;
        default:
          if (type.IsArray)
            return type == typeof (byte[]) ? (byte) 120 : (byte) 121;
          if (type == typeof (Hashtable))
            return 104;
          if (type.IsGenericType && typeof (Dictionary<,>) == type.GetGenericTypeDefinition())
            return 68;
          if (type == typeof (EventData))
            return 101;
          if (type == typeof (OperationRequest))
            return 113;
          return type == typeof (OperationResponse) ? (byte) 112 : (byte) 0;
      }
    }

    private static Array CreateArrayByType(byte arrayType, short length)
    {
      return Array.CreateInstance(Protocol.GetTypeOfCode(arrayType), (int) length);
    }

    internal static void SerializeOperationRequest(MemoryStream memStream, OperationRequest serObject, bool setType)
    {
      Protocol.SerializeOperationRequest(memStream, serObject.OperationCode, serObject.Parameters, setType);
    }

    internal static void SerializeOperationRequest(MemoryStream memStream, byte operationCode, Dictionary<byte, object> parameters, bool setType)
    {
      if (setType)
        memStream.WriteByte((byte) 113);
      memStream.WriteByte(operationCode);
      Protocol.SerializeParameterTable(memStream, parameters);
    }

    internal static OperationRequest DeserializeOperationRequest(MemoryStream din)
    {
      return new OperationRequest()
      {
        OperationCode = Protocol.DeserializeByte(din),
        Parameters = Protocol.DeserializeParameterTable(din)
      };
    }

    internal static void SerializeOperationResponse(MemoryStream memStream, OperationResponse serObject, bool setType)
    {
      if (setType)
        memStream.WriteByte((byte) 112);
      memStream.WriteByte(serObject.OperationCode);
      Protocol.SerializeShort(memStream, serObject.ReturnCode, false);
      if (string.IsNullOrEmpty(serObject.DebugMessage))
        memStream.WriteByte((byte) 42);
      else
        Protocol.SerializeString(memStream, serObject.DebugMessage, false);
      Protocol.SerializeParameterTable(memStream, serObject.Parameters);
    }

    internal static OperationResponse DeserializeOperationResponse(MemoryStream memoryStream)
    {
      return new OperationResponse()
      {
        OperationCode = Protocol.DeserializeByte(memoryStream),
        ReturnCode = Protocol.DeserializeShort(memoryStream),
        DebugMessage = Protocol.Deserialize(memoryStream, Protocol.DeserializeByte(memoryStream)) as string,
        Parameters = Protocol.DeserializeParameterTable(memoryStream)
      };
    }

    internal static void SerializeEventData(MemoryStream memStream, EventData serObject, bool setType)
    {
      if (setType)
        memStream.WriteByte((byte) 101);
      memStream.WriteByte(serObject.Code);
      Protocol.SerializeParameterTable(memStream, serObject.Parameters);
    }

    internal static EventData DeserializeEventData(MemoryStream din)
    {
      return new EventData()
      {
        Code = Protocol.DeserializeByte(din),
        Parameters = Protocol.DeserializeParameterTable(din)
      };
    }

    private static void SerializeParameterTable(MemoryStream memStream, Dictionary<byte, object> parameters)
    {
      if (parameters == null || parameters.Count == 0)
      {
        Protocol.SerializeShort(memStream, (short) 0, false);
      }
      else
      {
        Protocol.SerializeShort(memStream, (short) parameters.Count, false);
        foreach (KeyValuePair<byte, object> parameter in parameters)
        {
          memStream.WriteByte(parameter.Key);
          Protocol.Serialize(memStream, parameter.Value, true);
        }
      }
    }

    private static Dictionary<byte, object> DeserializeParameterTable(MemoryStream memoryStream)
    {
      short num = Protocol.DeserializeShort(memoryStream);
      Dictionary<byte, object> dictionary = new Dictionary<byte, object>((int) num);
      for (int index1 = 0; index1 < (int) num; ++index1)
      {
        byte index2 = (byte) memoryStream.ReadByte();
        object obj = Protocol.Deserialize(memoryStream, (byte) memoryStream.ReadByte());
        dictionary[index2] = obj;
      }
      return dictionary;
    }

    private static void Serialize(MemoryStream dout, object serObject, bool setType)
    {
      if (serObject == null)
      {
        if (!setType)
          return;
        dout.WriteByte((byte) 42);
      }
      else
      {
        Type type = serObject.GetType();
        switch (Type.GetTypeCode(type))
        {
          case TypeCode.Boolean:
            Protocol.SerializeBoolean(dout, (bool) serObject, setType);
            break;
          case TypeCode.Byte:
            Protocol.SerializeByte(dout, (byte) serObject, setType);
            break;
          case TypeCode.Int16:
            Protocol.SerializeShort(dout, (short) serObject, setType);
            break;
          case TypeCode.Int32:
            Protocol.SerializeInteger(dout, (int) serObject, setType);
            break;
          case TypeCode.Int64:
            Protocol.SerializeLong(dout, (long) serObject, setType);
            break;
          case TypeCode.Single:
            Protocol.SerializeFloat(dout, (float) serObject, setType);
            break;
          case TypeCode.Double:
            Protocol.SerializeDouble(dout, (double) serObject, setType);
            break;
          case TypeCode.String:
            Protocol.SerializeString(dout, (string) serObject, setType);
            break;
          default:
            if (serObject is Hashtable)
            {
              Protocol.SerializeHashTable(dout, (Hashtable) serObject, setType);
              break;
            }
            if (type.IsArray)
            {
              if (serObject is byte[])
              {
                Protocol.SerializeByteArray(dout, (byte[]) serObject, setType);
                break;
              }
              if (serObject is int[])
              {
                Protocol.SerializeIntArrayOptimized(dout, (int[]) serObject, setType);
                break;
              }
              if (type.GetElementType() == typeof (object))
              {
                Protocol.SerializeObjectArray(dout, serObject as object[], setType);
                break;
              }
              Protocol.SerializeArray(dout, (Array) serObject, setType);
              break;
            }
            if (serObject is IDictionary)
            {
              Protocol.SerializeDictionary(dout, (IDictionary) serObject, setType);
              break;
            }
            if (serObject is EventData)
            {
              Protocol.SerializeEventData(dout, (EventData) serObject, setType);
              break;
            }
            if (serObject is OperationResponse)
            {
              Protocol.SerializeOperationResponse(dout, (OperationResponse) serObject, setType);
              break;
            }
            if (serObject is OperationRequest)
            {
              Protocol.SerializeOperationRequest(dout, (OperationRequest) serObject, setType);
              break;
            }
            if (!Protocol.SerializeCustom(dout, serObject))
              throw new Exception("cannot serialize(): " + (object) serObject.GetType());
            break;
        }
      }
    }

    private static void SerializeByte(MemoryStream dout, byte serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 98);
      dout.WriteByte(serObject);
    }

    private static void SerializeBoolean(MemoryStream dout, bool serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 111);
      dout.Write(BitConverter.GetBytes(serObject), 0, 1);
    }

    private static void SerializeShort(MemoryStream dout, short serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 107);
      byte[] buffer = new byte[2]
      {
        (byte) ((uint) serObject >> 8),
        (byte) serObject
      };
      dout.Write(buffer, 0, 2);
    }

    public static void Serialize(short value, byte[] target, ref int targetOffset)
    {
      target[targetOffset++] = (byte) ((uint) value >> 8);
      target[targetOffset++] = (byte) value;
    }

    private static void SerializeInteger(MemoryStream dout, int serObject, bool setType)
    {
      byte[] buffer = new byte[5]
      {
        (byte) 105,
        (byte) (serObject >> 24),
        (byte) (serObject >> 16),
        (byte) (serObject >> 8),
        (byte) serObject
      };
      dout.Write(buffer, setType ? 0 : 1, setType ? 5 : 4);
    }

    public static void Serialize(int value, byte[] target, ref int targetOffset)
    {
      target[targetOffset++] = (byte) (value >> 24);
      target[targetOffset++] = (byte) (value >> 16);
      target[targetOffset++] = (byte) (value >> 8);
      target[targetOffset++] = (byte) value;
    }

    private static void SerializeLong(MemoryStream dout, long serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 108);
      byte[] bytes = BitConverter.GetBytes(serObject);
      if (BitConverter.IsLittleEndian)
      {
        byte num1 = bytes[0];
        byte num2 = bytes[1];
        byte num3 = bytes[2];
        byte num4 = bytes[3];
        bytes[0] = bytes[7];
        bytes[1] = bytes[6];
        bytes[2] = bytes[5];
        bytes[3] = bytes[4];
        bytes[4] = num4;
        bytes[5] = num3;
        bytes[6] = num2;
        bytes[7] = num1;
      }
      dout.Write(bytes, 0, 8);
    }

    private static void SerializeFloat(MemoryStream dout, float serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 102);
      byte[] bytes = BitConverter.GetBytes(serObject);
      if (BitConverter.IsLittleEndian)
      {
        byte num1 = bytes[0];
        byte num2 = bytes[1];
        bytes[0] = bytes[3];
        bytes[1] = bytes[2];
        bytes[2] = num2;
        bytes[3] = num1;
      }
      dout.Write(bytes, 0, 4);
    }

    public static void Serialize(float value, byte[] target, ref int targetOffset)
    {
      byte[] bytes = BitConverter.GetBytes(value);
      if (BitConverter.IsLittleEndian)
      {
        target[targetOffset++] = bytes[3];
        target[targetOffset++] = bytes[2];
        target[targetOffset++] = bytes[1];
        target[targetOffset++] = bytes[0];
      }
      else
      {
        target[targetOffset++] = bytes[0];
        target[targetOffset++] = bytes[1];
        target[targetOffset++] = bytes[2];
        target[targetOffset++] = bytes[3];
      }
    }

    private static void SerializeDouble(MemoryStream dout, double serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 100);
      byte[] bytes = BitConverter.GetBytes(serObject);
      if (BitConverter.IsLittleEndian)
      {
        byte num1 = bytes[0];
        byte num2 = bytes[1];
        byte num3 = bytes[2];
        byte num4 = bytes[3];
        bytes[0] = bytes[7];
        bytes[1] = bytes[6];
        bytes[2] = bytes[5];
        bytes[3] = bytes[4];
        bytes[4] = num4;
        bytes[5] = num3;
        bytes[6] = num2;
        bytes[7] = num1;
      }
      dout.Write(bytes, 0, 8);
    }

    private static void SerializeString(MemoryStream dout, string serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 115);
      byte[] bytes = Encoding.UTF8.GetBytes(serObject);
      if (bytes.Length > (int) short.MaxValue)
        throw new NotSupportedException("Strings that exceed a UTF8-encoded byte-length of 32767 (short.MaxValue) are not supported. Yours is: " + (object) bytes.Length);
      Protocol.SerializeShort(dout, (short) bytes.Length, false);
      dout.Write(bytes, 0, bytes.Length);
    }

    private static void SerializeArray(MemoryStream dout, Array serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 121);
      if (serObject.Length > (int) short.MaxValue)
        throw new NotSupportedException("String[] that exceed 32767 (short.MaxValue) entries are not supported. Yours is: " + (object) serObject.Length);
      Protocol.SerializeShort(dout, (short) serObject.Length, false);
      Type elementType = serObject.GetType().GetElementType();
      byte codeOfType = Protocol.GetCodeOfType(elementType);
      if ((int) codeOfType != 0)
      {
        dout.WriteByte(codeOfType);
        if ((int) codeOfType == 68)
        {
          bool setKeyType;
          bool setValueType;
          Protocol.SerializeDictionaryHeader(dout, (object) serObject, out setKeyType, out setValueType);
          for (int index = 0; index < serObject.Length; ++index)
          {
            object dict = serObject.GetValue(index);
            Protocol.SerializeDictionaryElements(dout, dict, setKeyType, setValueType);
          }
        }
        else
        {
          for (int index = 0; index < serObject.Length; ++index)
          {
            object serObject1 = serObject.GetValue(index);
            Protocol.Serialize(dout, serObject1, false);
          }
        }
      }
      else
      {
        CustomType customType;
        if (!Protocol.TypeDict.TryGetValue(elementType, out customType))
          throw new NotSupportedException("cannot serialize array of type " + (object) elementType);
        dout.WriteByte((byte) 99);
        dout.WriteByte(customType.Code);
        for (int index = 0; index < serObject.Length; ++index)
        {
          object customObject = serObject.GetValue(index);
          byte[] buffer = customType.SerializeFunction(customObject);
          Protocol.SerializeShort(dout, (short) buffer.Length, false);
          dout.Write(buffer, 0, buffer.Length);
        }
      }
    }

    private static void SerializeByteArray(MemoryStream dout, byte[] serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 120);
      Protocol.SerializeInteger(dout, serObject.Length, false);
      dout.Write(serObject, 0, serObject.Length);
    }

    private static void SerializeIntArrayOptimized(MemoryStream inWriter, int[] serObject, bool setType)
    {
      if (setType)
        inWriter.WriteByte((byte) 121);
      Protocol.SerializeShort(inWriter, (short) serObject.Length, false);
      inWriter.WriteByte((byte) 105);
      byte[] buffer = new byte[serObject.Length * 4];
      int num1 = 0;
      for (int index1 = 0; index1 < serObject.Length; ++index1)
      {
        byte[] numArray1 = buffer;
        int index2 = num1;
        int num2 = 1;
        int num3 = index2 + num2;
        int num4 = (int) (byte) (serObject[index1] >> 24);
        numArray1[index2] = (byte) num4;
        byte[] numArray2 = buffer;
        int index3 = num3;
        int num5 = 1;
        int num6 = index3 + num5;
        int num7 = (int) (byte) (serObject[index1] >> 16);
        numArray2[index3] = (byte) num7;
        byte[] numArray3 = buffer;
        int index4 = num6;
        int num8 = 1;
        int num9 = index4 + num8;
        int num10 = (int) (byte) (serObject[index1] >> 8);
        numArray3[index4] = (byte) num10;
        byte[] numArray4 = buffer;
        int index5 = num9;
        int num11 = 1;
        num1 = index5 + num11;
        int num12 = (int) (byte) serObject[index1];
        numArray4[index5] = (byte) num12;
      }
      inWriter.Write(buffer, 0, buffer.Length);
    }

    private static void SerializeStringArray(MemoryStream dout, string[] serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 97);
      Protocol.SerializeShort(dout, (short) serObject.Length, false);
      for (int index = 0; index < serObject.Length; ++index)
        Protocol.SerializeString(dout, serObject[index], false);
    }

    private static void SerializeObjectArray(MemoryStream dout, object[] objects, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 122);
      Protocol.SerializeShort(dout, (short) objects.Length, false);
      for (int index = 0; index < objects.Length; ++index)
      {
        object serObject = objects[index];
        Protocol.Serialize(dout, serObject, true);
      }
    }

    private static void SerializeHashTable(MemoryStream dout, Hashtable serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 104);
      Protocol.SerializeShort(dout, (short) serObject.Count, false);
      foreach (DictionaryEntry dictionaryEntry in serObject)
      {
        Protocol.Serialize(dout, dictionaryEntry.Key, true);
        Protocol.Serialize(dout, dictionaryEntry.Value, true);
      }
    }

    private static void SerializeDictionary(MemoryStream dout, IDictionary serObject, bool setType)
    {
      if (setType)
        dout.WriteByte((byte) 68);
      bool setKeyType;
      bool setValueType;
      Protocol.SerializeDictionaryHeader(dout, (object) serObject, out setKeyType, out setValueType);
      Protocol.SerializeDictionaryElements(dout, (object) serObject, setKeyType, setValueType);
    }

    private static void SerializeDictionaryHeader(MemoryStream writer, Type dictType)
    {
      bool setKeyType;
      bool setValueType;
      Protocol.SerializeDictionaryHeader(writer, (object) dictType, out setKeyType, out setValueType);
    }

    private static void SerializeDictionaryHeader(MemoryStream writer, object dict, out bool setKeyType, out bool setValueType)
    {
      Type[] genericArguments = dict.GetType().GetGenericArguments();
      setKeyType = genericArguments[0] == typeof (object);
      setValueType = genericArguments[1] == typeof (object);
      if (setKeyType)
      {
        writer.WriteByte((byte) 0);
      }
      else
      {
        GpType codeOfType = (GpType) Protocol.GetCodeOfType(genericArguments[0]);
        if (codeOfType == GpType.Unknown || codeOfType == GpType.Dictionary)
          throw new Exception("Unexpected - cannot serialize Dictionary with key type: " + (object) genericArguments[0]);
        writer.WriteByte((byte) codeOfType);
      }
      if (setValueType)
      {
        writer.WriteByte((byte) 0);
      }
      else
      {
        GpType codeOfType = (GpType) Protocol.GetCodeOfType(genericArguments[1]);
        if (codeOfType == GpType.Unknown)
          throw new Exception("Unexpected - cannot serialize Dictionary with value type: " + (object) genericArguments[0]);
        writer.WriteByte((byte) codeOfType);
        if (codeOfType == GpType.Dictionary)
          Protocol.SerializeDictionaryHeader(writer, genericArguments[1]);
      }
    }

    private static void SerializeDictionaryElements(MemoryStream writer, object dict, bool setKeyType, bool setValueType)
    {
      IDictionary dictionary = (IDictionary) dict;
      Protocol.SerializeShort(writer, (short) dictionary.Count, false);
      foreach (DictionaryEntry dictionaryEntry in dictionary)
      {
        Protocol.Serialize(writer, dictionaryEntry.Key, setKeyType);
        Protocol.Serialize(writer, dictionaryEntry.Value, setValueType);
      }
    }

    private static object Deserialize(MemoryStream din, byte type)
    {
      byte num = type;
      if ((uint) num <= 42U)
      {
        if ((int) num == 0 || (int) num == 42)
          return (object) null;
      }
      else
      {
        switch (num)
        {
          case 68:
            return (object) Protocol.DeserializeDictionary(din);
          case 97:
            return (object) Protocol.DeserializeStringArray(din);
          case 98:
            return (object) Protocol.DeserializeByte(din);
          case 99:
            byte customTypeCode = (byte) din.ReadByte();
            return Protocol.DeserializeCustom(din, customTypeCode);
          case 100:
            return (object) Protocol.DeserializeDouble(din);
          case 101:
            return (object) Protocol.DeserializeEventData(din);
          case 102:
            return (object) Protocol.DeserializeFloat(din);
          case 104:
            return (object) Protocol.DeserializeHashTable(din);
          case 105:
            return (object) Protocol.DeserializeInteger(din);
          case 107:
            return (object) Protocol.DeserializeShort(din);
          case 108:
            return (object) Protocol.DeserializeLong(din);
          case 110:
            return (object) Protocol.DeserializeIntArray(din);
          case 111:
            return (object) Protocol.DeserializeBoolean(din);
          case 112:
            return (object) Protocol.DeserializeOperationResponse(din);
          case 113:
            return (object) Protocol.DeserializeOperationRequest(din);
          case 115:
            return (object) Protocol.DeserializeString(din);
          case 120:
            return (object) Protocol.DeserializeByteArray(din);
          case 121:
            return (object) Protocol.DeserializeArray(din);
          case 122:
            return (object) Protocol.DeserializeObjectArray(din);
        }
      }
      Debug.WriteLine("missing type: " + (object) type);
      throw new Exception("deserialize(): " + (object) type);
    }

    private static byte DeserializeByte(MemoryStream din)
    {
      return (byte) din.ReadByte();
    }

    private static bool DeserializeBoolean(MemoryStream din)
    {
      return din.ReadByte() != 0;
    }

    private static short DeserializeShort(MemoryStream din)
    {
      byte[] buffer = new byte[2];
      din.Read(buffer, 0, 2);
      return (short) ((int) buffer[0] << 8 | (int) buffer[1]);
    }

    public static void Deserialize(out short value, byte[] source, ref int offset)
    {
      value = (short) ((int) source[offset++] << 8 | (int) source[offset++]);
    }

    private static int DeserializeInteger(MemoryStream din)
    {
      byte[] buffer = new byte[4];
      din.Read(buffer, 0, 4);
      return (int) buffer[0] << 24 | (int) buffer[1] << 16 | (int) buffer[2] << 8 | (int) buffer[3];
    }

    public static void Deserialize(out int value, byte[] source, ref int offset)
    {
      value = (int) source[offset++] << 24 | (int) source[offset++] << 16 | (int) source[offset++] << 8 | (int) source[offset++];
    }

    private static long DeserializeLong(MemoryStream din)
    {
      byte[] buffer = new byte[8];
      din.Read(buffer, 0, 8);
      if (BitConverter.IsLittleEndian)
        return (long) buffer[0] << 56 | (long) buffer[1] << 48 | (long) buffer[2] << 40 | (long) buffer[3] << 32 | (long) buffer[4] << 24 | (long) buffer[5] << 16 | (long) buffer[6] << 8 | (long) buffer[7];
      return BitConverter.ToInt64(buffer, 0);
    }

    private static float DeserializeFloat(MemoryStream din)
    {
      byte[] buffer = new byte[4];
      din.Read(buffer, 0, 4);
      if (BitConverter.IsLittleEndian)
      {
        byte num1 = buffer[0];
        byte num2 = buffer[1];
        buffer[0] = buffer[3];
        buffer[1] = buffer[2];
        buffer[2] = num2;
        buffer[3] = num1;
      }
      return BitConverter.ToSingle(buffer, 0);
    }

    public static void Deserialize(out float value, byte[] source, ref int offset)
    {
      if (BitConverter.IsLittleEndian)
      {
        byte[] numArray = new byte[4]
        {
          (byte) 0,
          (byte) 0,
          (byte) 0,
          source[offset++]
        };
        numArray[2] = source[offset++];
        numArray[1] = source[offset++];
        numArray[0] = source[offset++];
        value = BitConverter.ToSingle(numArray, 0);
      }
      else
      {
        value = BitConverter.ToSingle(source, offset);
        offset += 4;
      }
    }

    private static double DeserializeDouble(MemoryStream din)
    {
      byte[] buffer = new byte[8];
      din.Read(buffer, 0, 8);
      if (BitConverter.IsLittleEndian)
      {
        byte num1 = buffer[0];
        byte num2 = buffer[1];
        byte num3 = buffer[2];
        byte num4 = buffer[3];
        buffer[0] = buffer[7];
        buffer[1] = buffer[6];
        buffer[2] = buffer[5];
        buffer[3] = buffer[4];
        buffer[4] = num4;
        buffer[5] = num3;
        buffer[6] = num2;
        buffer[7] = num1;
      }
      return BitConverter.ToDouble(buffer, 0);
    }

    private static string DeserializeString(MemoryStream din)
    {
      short num = Protocol.DeserializeShort(din);
      if ((int) num == 0)
        return "";
      byte[] numArray = new byte[(int) num];
      din.Read(numArray, 0, numArray.Length);
      return Encoding.UTF8.GetString(numArray, 0, numArray.Length);
    }

    private static Array DeserializeArray(MemoryStream din)
    {
      short num1 = Protocol.DeserializeShort(din);
      byte num2 = (byte) din.ReadByte();
      Array array1;
      switch (num2)
      {
        case 121:
          Array array2 = Protocol.DeserializeArray(din);
          array1 = Array.CreateInstance(array2.GetType(), (int) num1);
          array1.SetValue((object) array2, 0);
          for (short index = 1; (int) index < (int) num1; ++index)
          {
            Array array3 = Protocol.DeserializeArray(din);
            array1.SetValue((object) array3, (int) index);
          }
          break;
        case 120:
          array1 = Array.CreateInstance(typeof (byte[]), (int) num1);
          for (short index = 0; (int) index < (int) num1; ++index)
          {
            Array array3 = (Array) Protocol.DeserializeByteArray(din);
            array1.SetValue((object) array3, (int) index);
          }
          break;
        case 99:
          byte key = (byte) din.ReadByte();
          CustomType customType;
          if (!Protocol.CodeDict.TryGetValue(key, out customType))
            throw new Exception("Cannot find deserializer for custom type: " + (object) key);
          array1 = Array.CreateInstance(customType.Type, (int) num1);
          for (int index = 0; index < (int) num1; ++index)
          {
            short num3 = Protocol.DeserializeShort(din);
            byte[] numArray = new byte[(int) num3];
            din.Read(numArray, 0, (int) num3);
            array1.SetValue(customType.DeserializeFunction(numArray), index);
          }
          break;
        case 68:
          Array arrayResult = (Array) null;
          Protocol.DeserializeDictionaryArray(din, num1, out arrayResult);
          return arrayResult;
        default:
          array1 = Protocol.CreateArrayByType(num2, num1);
          for (short index = 0; (int) index < (int) num1; ++index)
            array1.SetValue(Protocol.Deserialize(din, num2), (int) index);
          break;
      }
      return array1;
    }

    private static byte[] DeserializeByteArray(MemoryStream din)
    {
      int count = Protocol.DeserializeInteger(din);
      byte[] buffer = new byte[count];
      din.Read(buffer, 0, count);
      return buffer;
    }

    private static int[] DeserializeIntArray(MemoryStream din)
    {
      int length = Protocol.DeserializeInteger(din);
      int[] numArray = new int[length];
      for (int index = 0; index < length; ++index)
        numArray[index] = Protocol.DeserializeInteger(din);
      return numArray;
    }

    private static string[] DeserializeStringArray(MemoryStream din)
    {
      int length = (int) Protocol.DeserializeShort(din);
      string[] strArray = new string[length];
      for (int index = 0; index < length; ++index)
        strArray[index] = Protocol.DeserializeString(din);
      return strArray;
    }

    private static object[] DeserializeObjectArray(MemoryStream din)
    {
      short num = Protocol.DeserializeShort(din);
      object[] objArray = new object[(int) num];
      for (int index = 0; index < (int) num; ++index)
      {
        byte type = (byte) din.ReadByte();
        objArray.SetValue(Protocol.Deserialize(din, type), index);
      }
      return objArray;
    }

    private static Hashtable DeserializeHashTable(MemoryStream din)
    {
      int x = (int) Protocol.DeserializeShort(din);
      Hashtable hashtable = new Hashtable(x);
      for (int index1 = 0; index1 < x; ++index1)
      {
        object index2 = Protocol.Deserialize(din, (byte) din.ReadByte());
        object obj = Protocol.Deserialize(din, (byte) din.ReadByte());
        hashtable[index2] = obj;
      }
      return hashtable;
    }

    private static IDictionary DeserializeDictionary(MemoryStream din)
    {
      byte typeCode1 = (byte) din.ReadByte();
      byte typeCode2 = (byte) din.ReadByte();
      int num = (int) Protocol.DeserializeShort(din);
      bool flag1 = (int) typeCode1 == 0 || (int) typeCode1 == 42;
      bool flag2 = (int) typeCode2 == 0 || (int) typeCode2 == 42;
      IDictionary instance = Activator.CreateInstance(typeof (Dictionary<,>).MakeGenericType(Protocol.GetTypeOfCode(typeCode1), Protocol.GetTypeOfCode(typeCode2))) as IDictionary;
      for (int index = 0; index < num; ++index)
      {
        object key = Protocol.Deserialize(din, flag1 ? (byte) din.ReadByte() : typeCode1);
        object obj = Protocol.Deserialize(din, flag2 ? (byte) din.ReadByte() : typeCode2);
        instance.Add(key, obj);
      }
      return instance;
    }

    private static bool DeserializeDictionaryArray(MemoryStream din, short size, out Array arrayResult)
    {
      byte keyTypeCode;
      byte valTypeCode;
      Type type1 = Protocol.DeserializeDictionaryType(din, out keyTypeCode, out valTypeCode);
      arrayResult = Array.CreateInstance(type1, (int) size);
      for (short index1 = 0; (int) index1 < (int) size; ++index1)
      {
        IDictionary instance = Activator.CreateInstance(type1) as IDictionary;
        if (instance == null)
          return false;
        short num = Protocol.DeserializeShort(din);
        for (int index2 = 0; index2 < (int) num; ++index2)
        {
          object key;
          if ((int) keyTypeCode != 0)
          {
            key = Protocol.Deserialize(din, keyTypeCode);
          }
          else
          {
            byte type2 = (byte) din.ReadByte();
            key = Protocol.Deserialize(din, type2);
          }
          object obj;
          if ((int) valTypeCode != 0)
          {
            obj = Protocol.Deserialize(din, valTypeCode);
          }
          else
          {
            byte type2 = (byte) din.ReadByte();
            obj = Protocol.Deserialize(din, type2);
          }
          instance.Add(key, obj);
        }
        arrayResult.SetValue((object) instance, (int) index1);
      }
      return true;
    }

    private static Type DeserializeDictionaryType(MemoryStream reader, out byte keyTypeCode, out byte valTypeCode)
    {
      keyTypeCode = (byte) reader.ReadByte();
      valTypeCode = (byte) reader.ReadByte();
      GpType gpType1 = (GpType) keyTypeCode;
      GpType gpType2 = (GpType) valTypeCode;
      return typeof (Dictionary<,>).MakeGenericType(gpType1 != GpType.Unknown ? Protocol.GetTypeOfCode(keyTypeCode) : typeof (object), gpType2 != GpType.Unknown ? Protocol.GetTypeOfCode(valTypeCode) : typeof (object));
    }
  }
}
