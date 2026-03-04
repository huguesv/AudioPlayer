namespace Woohoo.IO.Compression.Internal.LZMA;

/// <summary>
/// The exception that is thrown when the value of an argument is outside the allowable range.
/// </summary>
internal sealed class InvalidParamException : Exception
{
    public InvalidParamException() : base("Invalid Parameter") { }
}
