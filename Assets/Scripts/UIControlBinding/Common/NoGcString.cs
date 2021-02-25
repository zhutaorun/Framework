using UnityEngine.UI;
using System;

public class NoGcString
{

    private static readonly char[] number = new char[] { '0','1','2','3','4','5', '6', '7','8','9','0' };
    private int m_ChunkLength;

    private int m_MaxCapacity;
    private char[] int_parser = new char[20];
    private int i;
    private int count;
    private string s;
    private bool isDirty = true;

    public int Length
    {
        get
        {
            return m_ChunkLength;
        }
    }

    public NoGcString(int capacity)
    {
        m_MaxCapacity = capacity;
        s = new string((char)0, m_MaxCapacity);
    }
    //更新UI显示通过这个方法，直接使用Getstring不会更新显示内容
    public void ApplyForUIText(Text text)
    {
        if(isDirty)
        {
            text.text = getString();
            text.cachedTextGenerator.Invalidate();
            text.SetVerticesDirty();
            isDirty = false;
        }
    }

    public unsafe string getString()
    {
        return s;
    }

    public override string ToString()
    {
        return getString();
    }

    //注意支持的类型！！！ 不够自己的补充
    public string Format(params object[] args)
    {
        Clear();
        for(int i=0,length = args.Length;i<length;i++)
        {
            object arg = args[i];
            if (arg.GetType() == typeof(int))
                Append((int)arg);
            else if (arg.GetType() == typeof(uint))
                Append((uint)arg);
            else if (arg.GetType() == typeof(long))
                Append((long)arg);
            else if (arg.GetType() == typeof(ulong))
                Append((ulong)arg);
            else if (arg.GetType() == typeof(float))
                Append((float)arg);
            else if (arg.GetType() == typeof(char))
                Append((char)arg);
            else if (arg.GetType() == typeof(ushort))
                Append((ushort)arg);
            else
                Append((string)arg);
        }
        return getString();
    }

    public unsafe NoGcString Clear()
    {
        if(m_ChunkLength!=0)
        {
            fixed (char* p = s)
            {
                p[0] = '\0';
            }
            isDirty = true;
        }
        m_ChunkLength = 0;
        return this;
    }

    public unsafe NoGcString Append(char[] value)
    {
        if((value!= number) && (value.Length!=0))
        {
            fixed(char* chRef = value)
            {
                this.Append(chRef, value.Length);
            }
        }
        return this;
    }

    public unsafe NoGcString Append(char* value,int valueCount)
    {
        if(valueCount<0)
        {
            throw new ArgumentOutOfRangeException("valueCount", "Count cannot be less than zero.");
        }
        int num = valueCount + this.m_ChunkLength;
        if(num<= this.m_MaxCapacity)
        {
            fixed(char* p = s)
            {
                wstrcpy(p + m_ChunkLength, value, count);
                m_ChunkLength = num;

                int* pInt = (int*)p;
                *(pInt - 1) = m_ChunkLength;
            }
            isDirty = true;
        }
        return this;
    }

    public unsafe NoGcString Append(char value)
    {
        if(this.m_ChunkLength<m_MaxCapacity)
        {
            int chunkLength = this.m_ChunkLength;
            this.m_ChunkLength = chunkLength + 1;
            fixed(char* p = s)
            {
                p[chunkLength] = value;
                int* pInt = (int*)p;
                *(pInt - 1) = m_ChunkLength;
            }
            isDirty = true;
        }
        return this;
    }

    public NoGcString Append(string value)
    {
        for (int i = 0; i < value.Length; i++)
            Append(value[i]);
        return this;
    }

    public NoGcString Append(int value)
    {
        if(value>=0)
        {
            count = ToCharArray((uint)value, int_parser, 0);
        }
        else
        {
            int_parser[0] = '_';
            count = ToCharArray((uint)-value, int_parser, 1) + 1;
        }

        for(int i=0;i<count;i++)
        {
            Append(int_parser[i]);
        }
        return this;
    }
    public NoGcString Append(uint value)
    {
        if (value >= 0)
        {
            count = ToCharArray((uint)value, int_parser, 0);
        }
        else
        {
            int_parser[0] = '_';
            count = ToCharArray((uint)-value, int_parser, 1) + 1;
        }

        for (int i = 0; i < count; i++)
        {
            Append(int_parser[i]);
        }
        return this;
    }

    public NoGcString Append(long value)
    {
        if (value >= 0)
        {
            count = ToCharArray((ulong)value, int_parser, 0);
        }
        else
        {
            int_parser[0] = '_';
            count = ToCharArray((ulong)-value, int_parser, 1) + 1;
        }

        for (int i = 0; i < count; i++)
        {
            Append(int_parser[i]);
        }
        return this;
    }
    public NoGcString Append(ulong value)
    {
        count = ToCharArray((ulong)value, int_parser, 0);
        for (int i = 0; i < count; i++)
        {
            Append(int_parser[i]);
        }
        return this;
    }

    private int FloatMantissaSize(float data)
    {
        int len = 0;
        while(data - Math.Floor(data)>float.Epsilon)
        {
            len++;
            data *= 10;
        }
        return len;
    }

    public NoGcString Append(float value,int displayDecimalNum=0)
    {
        int integer = (int)value;
        int mantissa = (int)Math.Floor((value - integer) * Math.Pow(10, FloatMantissaSize(Math.Abs(value))));

        if(value>=0)
        {
            count = ToCharArray((uint)integer, int_parser, 0);
            for(i=0;i<count;i++)
            {
                Append(int_parser[i]);
            }
            count = ToCharArray((uint)mantissa, int_parser, 0);
            if (count > 0)
                Append(".");
            if (count >= 6)
                count--;
            if (displayDecimalNum > 0 && displayDecimalNum < count)
                count = displayDecimalNum;
            for (i = 0; i < count; i++)
            {
                Append(int_parser[i]);
            }
        }
        else
        {
            int_parser[0] = '-';
            count = ToCharArray((uint)-integer, int_parser, 1)+1;
            for (i = 0; i < count; i++)
            {
                Append(int_parser[i]);
            }
            count = ToCharArray((uint)Math.Abs(mantissa), int_parser, 0);
            if (count > 0)
                Append(".");
            if (count >= 6)
                count--;
            if (displayDecimalNum > 0 && displayDecimalNum < count)
                count = displayDecimalNum;
            for (i = 0; i < count; i++)
            {
                Append(int_parser[i]);
            }
        }
        return this;
        
    }

    private static int ToCharArray(ulong value,char[] buffer,int bufferIndex)
    {
        if(value==0)
        {
            buffer[bufferIndex] = '0';
            return 1;
        }
        int len = 1;
        for(ulong rem = value/10;rem>0;rem/=10)
        {
            len++;
        }
        for(int i = len-1; i>=0;i--)
        {
            buffer[bufferIndex + i] = number[value % 10];
            value /= 10;
        }
        return len;
    }

    public static int ToCharArray(uint value,char[] buffer,int bufferIndex)
    {
        if(value==0)
        {
            buffer[bufferIndex] = '0';
            return 1;
        }
        int len = 1;
        for(uint rem = value/10;rem>0; rem/=10)
        {
            len++;
        }
        for(int i = len-1;i>=0;i--)
        {
            buffer[bufferIndex + i] = (char)('0' + (value % 10));
            value /= 10;
        }
        return len;
    }

    private static unsafe void ThreadSafeCopy(char* sourcePtr,char[]  destination,int destinationIndex,int count)
    {
        if(count>0)
        {
            if((destinationIndex> destination.Length)|| ((destinationIndex+ count)>destination.Length))
            {
                throw new ArgumentOutOfRangeException("destinationIndex", "Index was out of range.Must be non-negative and less than the size of the collection.");
            }
            fixed(char* chRef = &(destination[destinationIndex]))
            {
                wstrcpy(chRef, sourcePtr, count);
            }
        }
    }

    internal static unsafe void wstrcpy(char* dmem,char* smem, int charCount)
    {
        Memcpy((byte*)dmem, (byte*)dmem, charCount * 2);
    }
    static unsafe void Memcpy(byte* dest,byte* src,int size)
    {
        if (((((int)dest) | ((int)src)) & 3) != 0)
        {
            if((((((int)dest)&1)!= 0) && ((((int)src) & 1) != 0)) && (size >= 1))
            {
                dest[0] = src[0];
                dest++;
                src++;
                size--;
            }
            if ((((((int)dest) & 2) != 0) && ((((int)src) & 2) != 0)) && (size >= 2))
            {
                *((short*)dest) = *((short*)src);
                dest +=2;
                src +=2;
                size -=2;
            }
            if (((((int)dest) | ((int)src)) & 1) != 0)
            {
                memcpy1(dest, src, size);
                return;
            }
            if (((((int)dest) | ((int)src)) & 2) != 0)
            {
                memcpy2(dest, src, size);
                return;
            }
        }
        memcpy4(dest, src, size);
    }

    private static unsafe void memcpy1(byte* dest, byte* src, int size)
    {
        while (size >= 8)
        {
            dest[0] = src[0];
            dest[1] = src[1];
            dest[2] = src[2];
            dest[3] = src[3];
            dest[4] = src[4];
            dest[5] = src[5];
            dest[6] = src[6];
            dest[7] = src[7];
            dest += 8;
            src += 8;
            size -= 8;
        }
        while (size >= 2)
        {
            dest[0] = src[0];
            dest[1] = src[1];
            dest += 2;
            src += 2;
            size -= 2;
        }
        if (size > 0)
        {
            dest[0] = src[0];
        }
    }

    static unsafe void memcpy2(byte* dest,byte* src,int size)
    {
        while (size >= 8)
        {
            *((short*)dest) = *((short*)src);
            *((short*)dest + 2) = *((short*)src + 2);
            *((short*)(dest + (2 * 2))) = *((short*)(src + (2 * 2)));
            *((short*)(dest + (3 * 2))) = *((short*)(src + (3 * 2)));
            dest += 8;
            src += 8;
            size -= 8;
        }

        while(size>=2)
        {
            *((short*)dest) = *((short*)src);
            dest += 2;
            src += 2;
            size -= 2;
        }
        if(size>0)
        {
            dest[0] = src[0];
        }
    }

    static unsafe void memcpy4(byte* dest, byte* src, int size)
    {
        while (size >= 0x10)
        {
            *((int*)dest) = *((int*)src);
            *((int*)(dest + 4)) = *((int*)(src + 4));
            *((int*)(dest + (2 * 4))) = *((int*)(src + (2 * 4)));
            *((int*)(dest + (3 * 4))) = *((int*)(src + (3 * 4)));
            dest += 0x10;
            src += 0x10;
            size -= 0x10;
        }
        while (size >= 4)
        {
            *((int*)dest) = *((int*)src);
            dest += 4;
            src += 4;
            size -= 4;
        }
        while(size>0)
        {
            dest[0] = src[0];
            dest++;
            src++;
            size--;
        }
    }
}
