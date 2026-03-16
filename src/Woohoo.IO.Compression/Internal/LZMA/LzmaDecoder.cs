namespace Woohoo.IO.Compression.Internal.LZMA;

using Woohoo.IO.Compression.Internal.RangeCoder;

internal sealed class LzmaDecoder
{
    class LenDecoder
    {
        RangeBitDecoder m_Choice = new RangeBitDecoder();
        RangeBitDecoder m_Choice2 = new RangeBitDecoder();
        RangeBitTreeDecoder[] m_LowCoder = new RangeBitTreeDecoder[LzmaBase.kNumPosStatesMax];
        RangeBitTreeDecoder[] m_MidCoder = new RangeBitTreeDecoder[LzmaBase.kNumPosStatesMax];
        RangeBitTreeDecoder m_HighCoder = new RangeBitTreeDecoder(LzmaBase.kNumHighLenBits);
        uint m_NumPosStates = 0;

        public void Create(uint numPosStates)
        {
            for (uint posState = m_NumPosStates; posState < numPosStates; posState++)
            {
                m_LowCoder[posState] = new RangeBitTreeDecoder(LzmaBase.kNumLowLenBits);
                m_MidCoder[posState] = new RangeBitTreeDecoder(LzmaBase.kNumMidLenBits);
            }
            m_NumPosStates = numPosStates;
        }

        public void Init()
        {
            m_Choice.Init();
            for (uint posState = 0; posState < m_NumPosStates; posState++)
            {
                m_LowCoder[posState].Init();
                m_MidCoder[posState].Init();
            }
            m_Choice2.Init();
            m_HighCoder.Init();
        }

        public uint Decode(RangeCoder.RangeDecoder rangeDecoder, uint posState)
        {
            if (m_Choice.Decode(rangeDecoder) == 0)
                return m_LowCoder[posState].Decode(rangeDecoder);
            else
            {
                uint symbol = LzmaBase.kNumLowLenSymbols;
                if (m_Choice2.Decode(rangeDecoder) == 0)
                    symbol += m_MidCoder[posState].Decode(rangeDecoder);
                else
                {
                    symbol += LzmaBase.kNumMidLenSymbols;
                    symbol += m_HighCoder.Decode(rangeDecoder);
                }
                return symbol;
            }
        }
    }

    class LiteralDecoder
    {
        struct Decoder2
        {
            RangeBitDecoder[] m_Decoders;
            public void Create() { m_Decoders = new RangeBitDecoder[0x300]; }
            public void Init() { for (int i = 0; i < 0x300; i++) m_Decoders[i].Init(); }

            public byte DecodeNormal(RangeCoder.RangeDecoder rangeDecoder)
            {
                uint symbol = 1;
                do
                    symbol = (symbol << 1) | m_Decoders[symbol].Decode(rangeDecoder);
                while (symbol < 0x100);
                return (byte)symbol;
            }

            public byte DecodeWithMatchByte(RangeCoder.RangeDecoder rangeDecoder, byte matchByte)
            {
                uint symbol = 1;
                do
                {
                    uint matchBit = (uint)(matchByte >> 7) & 1;
                    matchByte <<= 1;
                    uint bit = m_Decoders[((1 + matchBit) << 8) + symbol].Decode(rangeDecoder);
                    symbol = (symbol << 1) | bit;
                    if (matchBit != bit)
                    {
                        while (symbol < 0x100)
                            symbol = (symbol << 1) | m_Decoders[symbol].Decode(rangeDecoder);
                        break;
                    }
                }
                while (symbol < 0x100);
                return (byte)symbol;
            }
        }

        Decoder2[] m_Coders;
        int m_NumPrevBits;
        int m_NumPosBits;
        uint m_PosMask;

        public void Create(int numPosBits, int numPrevBits)
        {
            if (m_Coders != null && m_NumPrevBits == numPrevBits &&
                m_NumPosBits == numPosBits)
                return;
            m_NumPosBits = numPosBits;
            m_PosMask = ((uint)1 << numPosBits) - 1;
            m_NumPrevBits = numPrevBits;
            uint numStates = (uint)1 << (m_NumPrevBits + m_NumPosBits);
            m_Coders = new Decoder2[numStates];
            for (uint i = 0; i < numStates; i++)
                m_Coders[i].Create();
        }

        public void Init()
        {
            uint numStates = (uint)1 << (m_NumPrevBits + m_NumPosBits);
            for (uint i = 0; i < numStates; i++)
                m_Coders[i].Init();
        }

        uint GetState(uint pos, byte prevByte)
        { return ((pos & m_PosMask) << m_NumPrevBits) + (uint)(prevByte >> (8 - m_NumPrevBits)); }

        public byte DecodeNormal(RangeCoder.RangeDecoder rangeDecoder, uint pos, byte prevByte)
        { return m_Coders[this.GetState(pos, prevByte)].DecodeNormal(rangeDecoder); }

        public byte DecodeWithMatchByte(RangeCoder.RangeDecoder rangeDecoder, uint pos, byte prevByte, byte matchByte)
        { return m_Coders[this.GetState(pos, prevByte)].DecodeWithMatchByte(rangeDecoder, matchByte); }
    };

    LZ.LzOutWindow m_OutWindow;

    RangeBitDecoder[] m_IsMatchDecoders = new RangeBitDecoder[LzmaBase.kNumStates << LzmaBase.kNumPosStatesBitsMax];
    RangeBitDecoder[] m_IsRepDecoders = new RangeBitDecoder[LzmaBase.kNumStates];
    RangeBitDecoder[] m_IsRepG0Decoders = new RangeBitDecoder[LzmaBase.kNumStates];
    RangeBitDecoder[] m_IsRepG1Decoders = new RangeBitDecoder[LzmaBase.kNumStates];
    RangeBitDecoder[] m_IsRepG2Decoders = new RangeBitDecoder[LzmaBase.kNumStates];
    RangeBitDecoder[] m_IsRep0LongDecoders = new RangeBitDecoder[LzmaBase.kNumStates << LzmaBase.kNumPosStatesBitsMax];

    RangeBitTreeDecoder[] m_PosSlotDecoder = new RangeBitTreeDecoder[LzmaBase.kNumLenToPosStates];
    RangeBitDecoder[] m_PosDecoders = new RangeBitDecoder[LzmaBase.kNumFullDistances - LzmaBase.kEndPosModelIndex];

    RangeBitTreeDecoder m_PosAlignDecoder = new RangeBitTreeDecoder(LzmaBase.kNumAlignBits);

    LenDecoder m_LenDecoder = new LenDecoder();
    LenDecoder m_RepLenDecoder = new LenDecoder();

    LiteralDecoder m_LiteralDecoder = new LiteralDecoder();

    int m_DictionarySize;

    uint m_PosStateMask;

    LzmaBase.State state = new LzmaBase.State();
    uint rep0, rep1, rep2, rep3;

    public LzmaDecoder()
    {
        m_DictionarySize = -1;
        for (int i = 0; i < LzmaBase.kNumLenToPosStates; i++)
            m_PosSlotDecoder[i] = new RangeBitTreeDecoder(LzmaBase.kNumPosSlotBits);
    }

    void CreateDictionary()
    {
        if (m_DictionarySize < 0)
            throw new InvalidParamException();
        m_OutWindow = new LZ.LzOutWindow();
        int blockSize = Math.Max(m_DictionarySize, (1 << 12));
        m_OutWindow.Create(blockSize);
    }

    void SetLiteralProperties(int lp, int lc)
    {
        if (lp > 8)
            throw new InvalidParamException();
        if (lc > 8)
            throw new InvalidParamException();
        m_LiteralDecoder.Create(lp, lc);
    }

    void SetPosBitsProperties(int pb)
    {
        if (pb > LzmaBase.kNumPosStatesBitsMax)
            throw new InvalidParamException();
        uint numPosStates = (uint)1 << pb;
        m_LenDecoder.Create(numPosStates);
        m_RepLenDecoder.Create(numPosStates);
        m_PosStateMask = numPosStates - 1;
    }

    void Init()
    {
        uint i;
        for (i = 0; i < LzmaBase.kNumStates; i++)
        {
            for (uint j = 0; j <= m_PosStateMask; j++)
            {
                uint index = (i << LzmaBase.kNumPosStatesBitsMax) + j;
                m_IsMatchDecoders[index].Init();
                m_IsRep0LongDecoders[index].Init();
            }
            m_IsRepDecoders[i].Init();
            m_IsRepG0Decoders[i].Init();
            m_IsRepG1Decoders[i].Init();
            m_IsRepG2Decoders[i].Init();
        }

        m_LiteralDecoder.Init();
        for (i = 0; i < LzmaBase.kNumLenToPosStates; i++)
            m_PosSlotDecoder[i].Init();
        // m_PosSpecDecoder.Init();
        for (i = 0; i < LzmaBase.kNumFullDistances - LzmaBase.kEndPosModelIndex; i++)
            m_PosDecoders[i].Init();

        m_LenDecoder.Init();
        m_RepLenDecoder.Init();
        m_PosAlignDecoder.Init();

        state.Init();
        rep0 = 0;
        rep1 = 0;
        rep2 = 0;
        rep3 = 0;
    }

    public void Code(System.IO.Stream inStream, System.IO.Stream outStream,
        long inSize, long outSize, ICodeProgress progress)
    {
        if (m_OutWindow == null)
            this.CreateDictionary();
        m_OutWindow.Init(outStream);
        if (outSize > 0)
            m_OutWindow.SetLimit(outSize);
        else
            m_OutWindow.SetLimit(long.MaxValue - m_OutWindow.Total);

        RangeCoder.RangeDecoder rangeDecoder = new RangeCoder.RangeDecoder();
        rangeDecoder.Init(inStream);

        this.Code(m_DictionarySize, m_OutWindow, rangeDecoder);

        m_OutWindow.ReleaseStream();
        rangeDecoder.ReleaseStream();

        if (!rangeDecoder.IsFinished || (inSize > 0 && rangeDecoder.Total != inSize))
            throw new DataErrorException();
        if (m_OutWindow.HasPending)
            throw new DataErrorException();
        m_OutWindow = null;
    }

    internal bool Code(int dictionarySize, LZ.LzOutWindow outWindow, RangeCoder.RangeDecoder rangeDecoder)
    {
        int dictionarySizeCheck = Math.Max(dictionarySize, 1);

        outWindow.CopyPending();

        while (outWindow.HasSpace)
        {
            uint posState = (uint)outWindow.Total & m_PosStateMask;
            if (m_IsMatchDecoders[(state.Index << LzmaBase.kNumPosStatesBitsMax) + posState].Decode(rangeDecoder) == 0)
            {
                byte b;
                byte prevByte = outWindow.GetByte(0);
                if (!state.IsCharState())
                    b = m_LiteralDecoder.DecodeWithMatchByte(rangeDecoder,
                        (uint)outWindow.Total, prevByte, outWindow.GetByte((int)rep0));
                else
                    b = m_LiteralDecoder.DecodeNormal(rangeDecoder, (uint)outWindow.Total, prevByte);
                outWindow.PutByte(b);
                state.UpdateChar();
            }
            else
            {
                uint len;
                if (m_IsRepDecoders[state.Index].Decode(rangeDecoder) == 1)
                {
                    if (m_IsRepG0Decoders[state.Index].Decode(rangeDecoder) == 0)
                    {
                        if (m_IsRep0LongDecoders[(state.Index << LzmaBase.kNumPosStatesBitsMax) + posState].Decode(rangeDecoder) == 0)
                        {
                            state.UpdateShortRep();
                            outWindow.PutByte(outWindow.GetByte((int)rep0));
                            continue;
                        }
                    }
                    else
                    {
                        uint distance;
                        if (m_IsRepG1Decoders[state.Index].Decode(rangeDecoder) == 0)
                        {
                            distance = rep1;
                        }
                        else
                        {
                            if (m_IsRepG2Decoders[state.Index].Decode(rangeDecoder) == 0)
                                distance = rep2;
                            else
                            {
                                distance = rep3;
                                rep3 = rep2;
                            }
                            rep2 = rep1;
                        }
                        rep1 = rep0;
                        rep0 = distance;
                    }
                    len = m_RepLenDecoder.Decode(rangeDecoder, posState) + LzmaBase.kMatchMinLen;
                    state.UpdateRep();
                }
                else
                {
                    rep3 = rep2;
                    rep2 = rep1;
                    rep1 = rep0;
                    len = LzmaBase.kMatchMinLen + m_LenDecoder.Decode(rangeDecoder, posState);
                    state.UpdateMatch();
                    uint posSlot = m_PosSlotDecoder[LzmaBase.GetLenToPosState(len)].Decode(rangeDecoder);
                    if (posSlot >= LzmaBase.kStartPosModelIndex)
                    {
                        int numDirectBits = (int)((posSlot >> 1) - 1);
                        rep0 = ((2 | (posSlot & 1)) << numDirectBits);
                        if (posSlot < LzmaBase.kEndPosModelIndex)
                            rep0 += RangeBitTreeDecoder.ReverseDecode(m_PosDecoders,
                                    rep0 - posSlot - 1, rangeDecoder, numDirectBits);
                        else
                        {
                            rep0 += (rangeDecoder.DecodeDirectBits(
                                numDirectBits - LzmaBase.kNumAlignBits) << LzmaBase.kNumAlignBits);
                            rep0 += m_PosAlignDecoder.ReverseDecode(rangeDecoder);
                        }
                    }
                    else
                        rep0 = posSlot;
                }
                if (rep0 >= outWindow.Total || rep0 >= dictionarySizeCheck)
                {
                    if (rep0 == 0xFFFFFFFF)
                        return true;
                    throw new DataErrorException();
                }
                outWindow.CopyBlock((int)rep0, (int)len);
            }
        }
        return false;
    }

    public void SetDecoderProperties(byte[] properties)
    {
        if (properties.Length < 1)
            throw new InvalidParamException();
        int lc = properties[0] % 9;
        int remainder = properties[0] / 9;
        int lp = remainder % 5;
        int pb = remainder / 5;
        if (pb > LzmaBase.kNumPosStatesBitsMax)
            throw new InvalidParamException();
        this.SetLiteralProperties(lp, lc);
        this.SetPosBitsProperties(pb);
        this.Init();
        if (properties.Length >= 5)
        {
            m_DictionarySize = 0;
            for (int i = 0; i < 4; i++)
                m_DictionarySize += properties[1 + i] << (i * 8);
        }
    }

    public void Train(System.IO.Stream stream)
    {
        if (m_OutWindow == null)
            this.CreateDictionary();
        m_OutWindow.Train(stream);
    }

    /*
    public override bool CanRead { get { return true; }}
    public override bool CanWrite { get { return true; }}
    public override bool CanSeek { get { return true; }}
    public override long Length { get { return 0; }}
    public override long Position
    {
        get { return 0;	}
        set { }
    }
    public override void Flush() { }
    public override int Read(byte[] buffer, int offset, int count) 
    {
        return 0;
    }
    public override void Write(byte[] buffer, int offset, int count)
    {
    }
    public override long Seek(long offset, System.IO.SeekOrigin origin)
    {
        return 0;
    }
    public override void SetLength(long value) {}
    */
}
