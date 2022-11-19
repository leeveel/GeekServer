using NLog;

namespace Geek.Server.Core.Utils;

public struct LNumber : IComparable<LNumber>, IEquatable<LNumber>
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

    public const int FRACTION_BITS = 14;                                                    // 小数位位数 14
    private const int INTEGER_BITS = sizeof(long) * 8 - FRACTION_BITS;          // 整数位位数 50
    
    private const int FRACTION_MASK = (int)(uint.MaxValue >> INTEGER_BITS);    // 2^14-1 = 16384-1 =16383 == 01111111111111
    private const int INTEGER_MASK = (int)(-1 & ~FRACTION_MASK);                    // -16384
    private const int FRACTION_RANGE = FRACTION_MASK + 1;                             // 16384 == 10000000000000

    public const long Max = 562949953421311;     // 2^50 = 1125899906842624 - 1
    public const long FMax = 9999;                       //  2^14-1 = 16384-1 =16383 == 01111111111111

    public static readonly LNumber MaxValue = Create_Row(Max);
    public static readonly LNumber MinValue = Create_Row(-Max);
    public static readonly LNumber zero = Create_Row(0);
    public static readonly LNumber one = 1;
    public static readonly LNumber minus_one = -one;
    public static readonly LNumber epsilon = Create_Row(1);
    public static readonly LNumber Zero = new LNumber();

    private const int Muti_FACTOR = 16384;

    public long raw;

    public long Ceiling
    {
        get
        {
            LNumber r;
            r.raw = (raw + FRACTION_MASK) & INTEGER_MASK;

            return (long)r;
        }
    }

    public long Floor
    {
        get
        {
            LNumber r;
            r.raw = raw & INTEGER_MASK;

            return (long)r;
        }
    }

    public static LNumber Create(long i, long f)
    {
#if XNUMBER_CHECK
        if (i > Max || i < -Max || f > FMax || f < -FMax)
            Debug.LogError("Xnumber 创建失败！ " + i + "." + f);
#endif

        int sign = (i ^ f) >= 0 ? 1 : -1;

        if (i < 0) i = -i;
        if (f < 0) f = -f;

        i = i << FRACTION_BITS;
        f = (f << FRACTION_BITS) / 10000;

        LNumber ret;
        ret.raw = sign * (i + f);
        return ret;
    }

    public static LNumber Create_Row(long i)
    {
        LNumber ret;
        ret.raw = i;
        return ret;
    }

    public int CompareTo(LNumber other)
    {
        return CompareTo(other.raw);
    }

    private int CompareTo(long other)
    {
        return raw.CompareTo(other);
    }

    public bool Equals(LNumber other)
    {
        return raw == other.raw;
    }

    public override bool Equals(object obj)
    {
        return (obj is LNumber && ((LNumber)obj) == this);
    }

    public override int GetHashCode()
    {
        return raw.GetHashCode();
    }

    public override string ToString()
    {
        return ((double)this).ToString("f4");
    }

    public string ToString(string str)
    {
        return ((double)this).ToString(str);
    }


    /**********************操作符号重载*****************************/

    // 二元操作符 +
    public static LNumber operator +(LNumber lhs, LNumber rhs)
    {
        LNumber r;
        r.raw = lhs.raw + rhs.raw;
        return r;
    }

    // 二元操作符 -
    public static LNumber operator -(LNumber lhs, LNumber rhs)
    {
        LNumber r;
        r.raw = lhs.raw - rhs.raw;
        return r;
    }

    // 二元操作符 *
    public static LNumber operator *(LNumber lhs, LNumber rhs)
    {
#if XNUMBER_CHECK
        var tmp = (int)lhs * (int)rhs;
        if (tmp > max || tmp < -max)
            Debug.LogError("Number数据超上限了 " + lhs + " * " + rhs);
#endif
        LNumber r;
        if (lhs.raw > int.MaxValue || rhs.raw > int.MaxValue || lhs.raw < int.MinValue || rhs.raw < int.MinValue)
        {
            //可能越界
            BigInteger a = lhs.raw;
            BigInteger b = rhs.raw;
            BigInteger c = (a * b + (FRACTION_RANGE >> 1)) >> FRACTION_BITS;

            if (c > long.MinValue && c < long.MaxValue)
                r.raw = long.Parse(c.ToString()); //未越界
            else if ((lhs > 0 && rhs > 0) || (lhs < 0 && rhs < 0))
            {
                LOGGER.Error("LNumber*已越界>" + c.ToString());
                r.raw = long.MaxValue;
            }
            else
            {
                LOGGER.Error("LNumber*已越界>" + c.ToString());
                r.raw = long.MinValue;
            }
        }else
        {
            r.raw = (lhs.raw * rhs.raw + (FRACTION_RANGE >> 1)) >> FRACTION_BITS;
        }
        return r;
    }

    // 二元操作符 /
    public static LNumber operator /(LNumber lhs, LNumber rhs)
    {
        if (lhs.raw == 0)
        {
            return 0;
        }
        var factor = 1;
        if (rhs.raw < 0)
            factor = -1;

        if ((rhs.raw + factor) >> 1 == 0)
        {
            //Debug.LogError("除0了");
            return 0;
        }
        
        LNumber r;
        if(lhs.raw > (1L << (62 - FRACTION_BITS)))
        {
            //可能越界了
            BigInteger a = lhs.raw;
            BigInteger b = rhs.raw;
            BigInteger c = ((a << (FRACTION_BITS + 1)) / b + factor) >> 1;

            if (c > long.MinValue && c < long.MaxValue)
                r.raw = long.Parse(c.ToString()); //未越界
            else if ((lhs > 0 && rhs > 0) || (lhs < 0 && rhs < 0))
            {
                LOGGER.Error("LNumber/已越界>" + c.ToString());
                r.raw = long.MaxValue;
            }
            else
            {
                LOGGER.Error("LNumber/已越界>" + c.ToString());
                r.raw = long.MinValue;
            }
        }
        else
        {
            r.raw = ((lhs.raw << (FRACTION_BITS + 1)) / rhs.raw + factor) >> 1;
        }
        return r;
    }

    // 一元操作符 - (负数操作)
    public static LNumber operator -(LNumber x)
    {
        LNumber r;
        r.raw = -x.raw;
        return r;
    }

    // 二元操作符 %
    public static LNumber operator %(LNumber lhs, LNumber rhs)
    {
        LNumber r;
        r.raw = lhs.raw % rhs.raw;
        return r;
    }

    // 比较运算符 <
    public static bool operator <(LNumber lhs, LNumber rhs)
    {
        return lhs.raw < rhs.raw;
    }

    // 比较运算符 <=
    public static bool operator <=(LNumber lhs, LNumber rhs)
    {
        return lhs.raw <= rhs.raw;
    }

    // 比较运算符 >
    public static bool operator >(LNumber lhs, LNumber rhs)
    {
        return lhs.raw > rhs.raw;
    }

    // 比较运算符 >=
    public static bool operator >=(LNumber lhs, LNumber rhs)
    {
        return lhs.raw >= rhs.raw;
    }

    // 比较运算符 ==
    public static bool operator ==(LNumber lhs, LNumber rhs)
    {
        return lhs.raw == rhs.raw;
    }

    // 比较运算符 !=
    public static bool operator !=(LNumber lhs, LNumber rhs)
    {
        return lhs.raw != rhs.raw;
    }

    /**********************数据类型转换*****************************/

    // long类型转换
    public static explicit operator long(LNumber number)
    {
        if (number.raw > 0)
            return number.raw >> FRACTION_BITS;
        else
            return (number.raw + FRACTION_MASK) >> FRACTION_BITS;
    }

    // double类型转换
    public static explicit operator double(LNumber number)
    {
        return (number.raw >> FRACTION_BITS) + (number.raw & FRACTION_MASK) / (double)FRACTION_RANGE;
    }

    // float 类型转换
    public static implicit operator float(LNumber number)
    {
        return (float)(double)number;
    }

    /**********************赋值运算*****************************/
    /// <summary>
    /// 赋值运算
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator LNumber(long value)
    {
#if XNUMBER_CHECK
        var tmp = value;
        if (tmp > Max || tmp < -Max)
            Debug.LogError("Number数据超上限了 " + value);
#endif
        //LNumber r;
        //r.raw = value << FRACTION_BITS;
        //return r;
        return Create(value, 0);
    }

    public static implicit operator LNumber(int value)
    {
#if XNUMBER_CHECK
        var tmp = value;
        if (tmp > Max || tmp < -Max)
            Debug.LogError("Number数据超上限了 " + value);
#endif
        //LNumber r;
        //r.raw = value << FRACTION_BITS;
        return Create(value, 0);
    }

    /*public static implicit operator LNumber(float number)
{
#if XNUMBER_CHECK
    var tmp = (long)number;
    if (tmp > Max || tmp < -Max)
        Debug.LogError("Number数据超上限了 " + number);
#endif
    return Convert(number);
}

public static LNumber Convert(float f)
{
    LNumber r;
    r.raw = (long)(f * Muti_FACTOR);
    return r;
}*/

}