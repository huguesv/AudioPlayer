namespace Woohoo.IO.Compression.Internal.RangeCoder;

internal struct RangeBitTreeDecoder
{
    RangeBitDecoder[] Models;
    int NumBitLevels;

    public RangeBitTreeDecoder(int numBitLevels)
    {
        NumBitLevels = numBitLevels;
        Models = new RangeBitDecoder[1 << numBitLevels];
    }

    public void Init()
    {
        for (uint i = 1; i < (1 << NumBitLevels); i++)
            Models[i].Init();
    }

    public uint Decode(RangeCoder.RangeDecoder rangeDecoder)
    {
        uint m = 1;
        for (int bitIndex = NumBitLevels; bitIndex > 0; bitIndex--)
            m = (m << 1) + Models[m].Decode(rangeDecoder);
        return m - ((uint)1 << NumBitLevels);
    }

    public uint ReverseDecode(RangeCoder.RangeDecoder rangeDecoder)
    {
        uint m = 1;
        uint symbol = 0;
        for (int bitIndex = 0; bitIndex < NumBitLevels; bitIndex++)
        {
            uint bit = Models[m].Decode(rangeDecoder);
            m <<= 1;
            m += bit;
            symbol |= (bit << bitIndex);
        }
        return symbol;
    }

    public static uint ReverseDecode(RangeBitDecoder[] Models, uint startIndex,
        RangeCoder.RangeDecoder rangeDecoder, int NumBitLevels)
    {
        uint m = 1;
        uint symbol = 0;
        for (int bitIndex = 0; bitIndex < NumBitLevels; bitIndex++)
        {
            uint bit = Models[startIndex + m].Decode(rangeDecoder);
            m <<= 1;
            m += bit;
            symbol |= (bit << bitIndex);
        }
        return symbol;
    }
}
