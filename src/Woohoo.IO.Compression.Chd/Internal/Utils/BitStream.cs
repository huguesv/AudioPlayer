namespace Woohoo.IO.Compression.Chd.Internal.Utils;

internal class BitStream
{
    private uint buffer;
    private int bits;
    private readonly byte[] readBuffer;
    private int doffset;
    private readonly int dlength;

    private readonly int initialOffset = 0;

    public bool Overflow()
    {
        return this.doffset - (this.bits / 8) > this.dlength;
    }

    public BitStream(byte[] src, int offset, int length)
    {
        this.buffer = 0;
        this.bits = 0;
        this.readBuffer = src;
        this.doffset = this.initialOffset = offset;
        this.dlength = offset + length;
    }

    // fetch the requested number of bits but don't advance the input pointer
    public uint Peek(int numbits)
    {
        if (numbits == 0)
        {
            return 0;
        }

        // fetch data if we need more
        if (numbits > this.bits)
        {
            while (this.bits <= 24)
            {
                if (this.doffset < this.dlength)
                {
                    this.buffer |= (uint)this.readBuffer[this.doffset] << (24 - this.bits);
                }

                this.doffset++;
                this.bits += 8;
            }
        }

        // return the data
        return this.buffer >> (32 - numbits);
    }

    // advance the input pointer by the specified number of bits
    public void Remove(int numbits)
    {
        this.buffer <<= numbits;
        this.bits -= numbits;
    }

    // fetch the requested number of bits
    public uint Read(int numbits)
    {
        uint result = this.Peek(numbits);
        this.Remove(numbits);
        return result;
    }

    // flush to the nearest byte
    public int Flush()
    {
        while (this.bits >= 8)
        {
            this.doffset--;
            this.bits -= 8;
        }

        this.bits = 0;
        this.buffer = 0;
        return this.doffset - this.initialOffset;
    }
}
