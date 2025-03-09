namespace HashNCopy;
public static class Tools
{
    public static Stream? FileStream(string fileName, bool createMode)
    {
        Stream? stream = null;
        try { stream = new FileStream(fileName, createMode ? FileMode.Create : FileMode.Open); } catch { }
        return stream;
    }

    public static void SeekStream(this Stream? stream, long? blockIdx, int blockSize)
    {
        if (blockIdx is not null) stream?.Seek(blockIdx.Value * blockSize, SeekOrigin.Begin);
    }

    public static byte[]? ReadStream(this Stream? stream, int count)
    {
        if (stream is null) return null;
        var buffer = new byte[count];
        var read = 0; while ((read < count) && (stream.Position < stream.Length)) { read += stream.Read(buffer, read, count - read); }
        return buffer.Length == read ? buffer : buffer.GetSubArray(0, read);
    }

    public static bool? BinaryCompare(this byte[]? a1, byte[]? a2)
    {
        if ((a1 is null) || (a2 is null) || (a1.Length != a2.Length)) return null;
        for (var i = 0; i < a1.Length; i++) if (a1[i] != a2[i]) return false;
        return true;
    }

    public static byte[] GetSubArray(this byte[] source, int offset, int count)
    {
        var result = new byte[count];
        Array.Copy(source, offset, result, 0, count);
        return result;
    }
}