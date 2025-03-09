namespace HashNCopy;
public interface IBlockStream : IDisposable
{
    string? Name { get; set; }
    int? BlockSize { get; set; }
    long? BlockIdx { get; set; }
    long? BlockCount { get; }
    long? DataCount { get; }
    byte[]? ReadBlock(int? blockSize = null);
    void WriteBlock(byte[] data);
}