using System.Security.Cryptography;

namespace HashNCopy;
public class HashedStream : IBlockStream
{
    public int HashSize { get; } = 64;
    public Stream? DataStream { get; set; }
    public Stream? HashStream { get; set; }
    public string? Name { get; set; }
    public int? BlockSize { get; set; }

    public long? BlockIdx
    {
        get { return DataStream != null ? DataStream.Position / BlockSize : null; }
        set { DataStream?.SeekStream(value, BlockSize!.Value); HashStream?.SeekStream(value, HashSize); }
    }

    public long? BlockCount
    {
        get { return DataStream != null ? (long)Math.Ceiling((double)DataStream.Length / (double)BlockSize!) : null; }
    }

    public long? DataCount => DataStream?.Length;

    public HashedStream(Stream? dataStream, Stream? hashStream, int blockSize = 64 * 1024)
    {
        DataStream = dataStream; HashStream = hashStream; BlockSize = blockSize;
    }

    public byte[]? ReadBlock(int? blockSize = null)
    {
        var data = Tools.ReadStream(DataStream, blockSize ?? BlockSize!.Value);
        if (HashStream != null && data != null)
        {
            var hashResult = Tools.BinaryCompare(SHA512.HashData(data), Tools.ReadStream(HashStream, HashSize));
            if (hashResult != null && !hashResult.Value) return null;
        }
        return data;
    }

    public void WriteBlock(byte[] data)
    {
        DataStream?.Write(data, 0, data.Length);
        HashStream?.Write(SHA512.HashData(data), 0, HashSize);
    }

    public virtual void Dispose()
    {
        DataStream?.Dispose();
        HashStream?.Dispose();
    }
}