namespace Woohoo.IO.Compression.Internal.LZMA;

/// <summary>
/// The exception that is thrown when an error in input stream occurs during decoding.
/// </summary>
public class DataErrorException : Exception
{
    public DataErrorException() : base("Data Error") { }
}
