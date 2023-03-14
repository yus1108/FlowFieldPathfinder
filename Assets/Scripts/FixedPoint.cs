using System;
using System.Text;

public class FixedPoint
{
    public const byte MAX_POINT_DIGITS = 8;
    public const int MULTIPLIER_OFFSET = 100000000;

    public int HighValue { get; private set; }
    public int LowValue { get; private set; }
    public long RawValue
    {
        get { return ((long)HighValue * MULTIPLIER_OFFSET) + LowValue; }
        set { HighValue = (int)(value / MULTIPLIER_OFFSET); LowValue = (int)(value % MULTIPLIER_OFFSET); }
    }

    public FixedPoint()
    {
        HighValue = 0;
        LowValue = 0;
    }
    public FixedPoint(Int32 num)
    {
        HighValue = num;
        LowValue = 0;
    }
    public FixedPoint(float num)
    {
        HighValue = (int)num;
        LowValue = (int)((num - (int)num) * MULTIPLIER_OFFSET);
    }
    public FixedPoint(double num)
    {
        HighValue = (int)num;
        LowValue = (int)((num - (int)num) * MULTIPLIER_OFFSET);
    }

    public static FixedPoint operator -(FixedPoint a, FixedPoint b) => new FixedPoint() { RawValue = a.RawValue - b.RawValue };
    public static FixedPoint operator -(FixedPoint a, int b) => new FixedPoint() { RawValue = a.RawValue - new FixedPoint(b).RawValue };
    public static FixedPoint operator -(int a, FixedPoint b) => new FixedPoint() { RawValue = new FixedPoint(a).RawValue - b.RawValue };

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder(HighValue.ToString() + ".");
        string lowValueStr = LowValue.ToString();
        for (int i = 0; i < MAX_POINT_DIGITS - lowValueStr.Length; i++)
        {
            sb.Append('0');
        }
        sb.Append(lowValueStr);
        return sb.ToString();
    }

    public string ToString(string fmt)
    {
        StringBuilder sb = new StringBuilder(HighValue.ToString() + ".");
        string lowValueStr = LowValue.ToString();
        if (fmt.StartsWith("F") || fmt.StartsWith("f"))
        {
            StringBuilder lowValueStringBuilder = new StringBuilder();
            for (int i = 0; i < MAX_POINT_DIGITS - lowValueStr.Length; i++)
            {
                lowValueStringBuilder.Append('0');
            }
            lowValueStringBuilder.Append(lowValueStr);
            lowValueStr = lowValueStringBuilder.ToString();

            int digits = Convert.ToInt32(fmt.Substring(1));
            for (int i = 0; i < digits; i++)
            {
                sb.Append(lowValueStr[i]);
            }
            return sb.ToString();
        }

        for (int i = 0; i < MAX_POINT_DIGITS - lowValueStr.Length; i++)
        {
            sb.Append('0');
        }
        sb.Append(lowValueStr);
        return sb.ToString();
    }

    public int ToInt32()
    {
        return HighValue;
    }

    public float ToFloat()
    {
        float result = HighValue + LowValue / (float)MULTIPLIER_OFFSET;
        return HighValue + LowValue / (float)MULTIPLIER_OFFSET;
    }
}
