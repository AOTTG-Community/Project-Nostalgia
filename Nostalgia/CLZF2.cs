//TODO: Fix and uncomment

#if false
using System;

public static class CLZF2
{
    private static readonly long[] HashTable = new long[HSIZE];
    private static readonly uint HLOG = 14;
    private static readonly uint HSIZE = 0x4000;
    private static readonly uint MAX_LIT = 0x20;
    private static readonly uint MAX_OFF = 0x2000;
    private static readonly uint MAX_REF = 0x108;

    public static byte[] Compress(byte[] inputBytes)
    {
        int num = inputBytes.Length * 2;
        byte[] output = new byte[num];
        int count = lzf_compress(inputBytes, ref output);
        while (count == 0)
        {
            num *= 2;
            output = new byte[num];
            count = lzf_compress(inputBytes, ref output);
        }
        byte[] dst = new byte[count];
        Buffer.BlockCopy(output, 0, dst, 0, count);
        return dst;
    }

    public static byte[] Decompress(byte[] inputBytes)
    {
        int num = inputBytes.Length * 2;
        byte[] output = new byte[num];
        int count = lzf_decompress(inputBytes, ref output);
        while (count == 0)
        {
            num *= 2;
            output = new byte[num];
            count = lzf_decompress(inputBytes, ref output);
        }
        byte[] dst = new byte[count];
        Buffer.BlockCopy(output, 0, dst, 0, count);
        return dst;
    }

    public static int lzf_compress(byte[] input, ref byte[] output)
    {
        int length = input.Length;
        int num2 = output.Length;
        Array.Clear(HashTable, 0, (int) HSIZE);
        uint index = 0;
        uint num5 = 0;
        uint num7 = (uint) ((input[index] << 8) | input[(int) ((IntPtr) (index + 1))]);
        int num9 = 0;
    Label_0035:
        while (index < (length - 2))
        {
            long num8;
            num7 = (num7 << 8) | input[(int) ((IntPtr) (index + 2))];
            long num3 = ((num7 ^ (num7 << 5)) >> ((0x18 - HLOG) - (num7 * 5))) & (HSIZE - 1);
            long num6 = HashTable[(int) ((IntPtr) num3)];
            HashTable[(int) ((IntPtr) num3)] = index;
            if (((((num8 = (index - num6) - 1L) >= MAX_OFF) || ((index + 4) >= length)) || ((num6 <= 0L) || (input[(int) ((IntPtr) num6)] != input[index]))) || ((input[(int) ((IntPtr) (num6 + 1L))] != input[(int) ((IntPtr) (index + 1))]) || (input[(int) ((IntPtr) (num6 + 2L))] != input[(int) ((IntPtr) (index + 2))])))
            {
                goto Label_0280;
            }
            uint num10 = 2;
            uint num11 = (((uint) length) - index) - num10;
            num11 = (num11 <= MAX_REF) ? num11 : MAX_REF;
            if ((((num5 + num9) + ((ulong) 1L)) + ((ulong) 3L)) >= num2)
            {
                return 0;
            }
            do
            {
                num10++;
            }
            while ((num10 < num11) && (input[(int) ((IntPtr) (num6 + num10))] == input[index + num10]));
            if (num9 != 0)
            {
                output[num5++] = (byte) (num9 - 1);
                num9 = -num9;
                do
                {
                    output[num5++] = input[(int) ((IntPtr) (index + num9))];
                }
                while (++num9 != 0);
            }
            num10 -= 2;
            index++;
            if (num10 < 7)
            {
                output[num5++] = (byte) ((num8 >> 8) + (num10 << 5));
            }
            else
            {
                output[num5++] = (byte) ((num8 >> 8) + 0xe0L);
                output[num5++] = (byte) (num10 - 7);
            }
            output[num5++] = (byte) num8;
            index += num10 - 1;
            num7 = (uint) ((input[index] << 8) | input[(int) ((IntPtr) (index + 1))]);
            num7 = (num7 << 8) | input[(int) ((IntPtr) (index + 2))];
            HashTable[(int) ((IntPtr) (((num7 ^ (num7 << 5)) >> ((0x18 - HLOG) - (num7 * 5))) & (HSIZE - 1)))] = index;
            index++;
            num7 = (num7 << 8) | input[(int) ((IntPtr) (index + 2))];
            HashTable[(int) ((IntPtr) (((num7 ^ (num7 << 5)) >> ((0x18 - HLOG) - (num7 * 5))) & (HSIZE - 1)))] = index;
            index++;
        }
        if (index == length)
        {
            if (num9 != 0)
            {
                if (((num5 + num9) + ((ulong) 1L)) >= num2)
                {
                    return 0;
                }
                output[num5++] = (byte) (num9 - 1);
                num9 = -num9;
                do
                {
                    output[num5++] = input[(int) ((IntPtr) (index + num9))];
                }
                while (++num9 != 0);
            }
            return (int) num5;
        }
    Label_0280:
        num9++;
        index++;
        if (num9 == MAX_LIT)
        {
            if (((num5 + 1) + MAX_LIT) >= num2)
            {
                return 0;
            }
            output[num5++] = (byte) (MAX_LIT - 1);
            num9 = -num9;
            do
            {
                output[num5++] = input[(int) ((IntPtr) (index + num9))];
            }
            while (++num9 != 0);
        }
        goto Label_0035;
    }

    public static int lzf_decompress(byte[] input, ref byte[] output)
    {
        int length = input.Length;
        int num2 = output.Length;
        uint num3 = 0;
        uint num4 = 0;
        do
        {
            uint num5 = input[num3++];
            if (num5 < 0x20)
            {
                num5++;
                if ((num4 + num5) > num2)
                {
                    return 0;
                }
                do
                {
                    output[num4++] = input[num3++];
                }
                while (--num5 != 0);
            }
            else
            {
                uint num6 = num5 >> 5;
                int num7 = ((int) (num4 - ((num5 & 0x1f) << 8))) - 1;
                if (num6 == 7)
                {
                    num6 += input[num3++];
                }
                num7 -= input[num3++];
                if (((num4 + num6) + 2) > num2)
                {
                    return 0;
                }
                if (num7 < 0)
                {
                    return 0;
                }
                output[num4++] = output[num7++];
                output[num4++] = output[num7++];
                do
                {
                    output[num4++] = output[num7++];
                }
                while (--num6 != 0);
            }
        }
        while (num3 < length);
        return (int) num4;
    }
}
#endif