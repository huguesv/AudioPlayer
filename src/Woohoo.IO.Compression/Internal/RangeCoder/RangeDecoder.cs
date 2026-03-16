namespace Woohoo.IO.Compression.Internal.RangeCoder;

internal sealed class RangeDecoder
{
    public const uint kTopValue = (1 << 24);
    public uint Range;
    public uint Code = 0;
    // public Buffer.InBuffer Stream = new Buffer.InBuffer(1 << 16);
    public System.IO.Stream Stream;
    public long Total;

    public void Init(System.IO.Stream stream)
    {
        // Stream.Init(stream);
        Stream = stream;

        Code = 0;
        Range = 0xFFFFFFFF;
        for (int i = 0; i < 5; i++)
            Code = (Code << 8) | (byte)Stream.ReadByte();
        Total = 5;
    }

    public void ReleaseStream()
    {
        // Stream.ReleaseStream();
        Stream = null;
    }

    public void CloseStream()
    {
        Stream.Dispose();
    }

    public void Normalize()
    {
        while (Range < kTopValue)
        {
            Code = (Code << 8) | (byte)Stream.ReadByte();
            Range <<= 8;
            Total++;
        }
    }

    public void Normalize2()
    {
        if (Range < kTopValue)
        {
            Code = (Code << 8) | (byte)Stream.ReadByte();
            Range <<= 8;
            Total++;
        }
    }

    public uint GetThreshold(uint total)
    {
        return Code / (Range /= total);
    }

    public void Decode(uint start, uint size)
    {
        Code -= start * Range;
        Range *= size;
        this.Normalize();
    }

    public uint DecodeDirectBits(int numTotalBits)
    {
        uint range = Range;
        uint code = Code;
        uint result = 0;
        for (int i = numTotalBits; i > 0; i--)
        {
            range >>= 1;
            /*
            result <<= 1;
            if (code >= range)
            {
                code -= range;
                result |= 1;
            }
            */
            uint t = (code - range) >> 31;
            code -= range & (t - 1);
            result = (result << 1) | (1 - t);

            if (range < kTopValue)
            {
                code = (code << 8) | (byte)Stream.ReadByte();
                range <<= 8;
                Total++;
            }
        }
        Range = range;
        Code = code;
        return result;
    }

    public uint DecodeBit(uint size0, int numTotalBits)
    {
        uint newBound = (Range >> numTotalBits) * size0;
        uint symbol;
        if (Code < newBound)
        {
            symbol = 0;
            Range = newBound;
        }
        else
        {
            symbol = 1;
            Code -= newBound;
            Range -= newBound;
        }
        this.Normalize();
        return symbol;
    }

    public bool IsFinished
    {
        get
        {
            return Code == 0;
        }
    }

    // ulong GetProcessedSize() {return Stream.GetProcessedSize(); }
}
