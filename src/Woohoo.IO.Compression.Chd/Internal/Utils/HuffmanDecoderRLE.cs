namespace Woohoo.IO.Compression.Chd.Internal.Utils;

internal class HuffmanDecoderRLE : HuffmanDecoder
{
    private int rlecount = 0;
    private uint prevdata = 0;

    public HuffmanDecoderRLE(uint numcodes, byte maxbits, BitStream bitbuf, ushort[] buffLookup) : base(numcodes, maxbits, bitbuf, buffLookup)
    {
    }

    public void Reset()
    {
        this.rlecount = 0;
        this.prevdata = 0;
    }

    public void FlushRLE()
    {
        this.rlecount = 0;
    }

    public new uint DecodeOne()
    {
        // return RLE data if we still have some
        if (this.rlecount != 0)
        {
            this.rlecount--;
            return this.prevdata;
        }

        // fetch the data and process
        uint data = base.DecodeOne();
        if (data < 0x100)
        {
            this.prevdata += data;
            return this.prevdata;
        }
        else
        {
            this.rlecount = CodeToRLECount((int)data);
            this.rlecount--;
            return this.prevdata;
        }
    }

    public static int CodeToRLECount(int code)
    {
        if (code == 0x00)
        {
            return 1;
        }

        if (code <= 0x107)
        {
            return 8 + (code - 0x100);
        }

        return 16 << (code - 0x108);
    }
}
