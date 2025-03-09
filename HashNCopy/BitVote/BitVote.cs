namespace HashNCopy;
public static class BitVote
{
    public static byte[]? Process(List<byte[]?>? copies)
    {
        copies = copies?.Where((item) => item is not null).ToList();
        if (copies is null || copies.Count < 2) return copies?.FirstOrDefault();
        var result = new byte[copies.First()!.Length];
        Parallel.For(0, result.Length, (int byteIdx) => result[byteIdx] = GetByte(copies!, byteIdx));
        return result;
    }

    private static byte GetByte(List<byte[]> copies, int byteIdx)
    {
        const int b1 = 1, b2 = 2, b3 = 4, b4 = 8, b5 = 16, b6 = 32, b7 = 64, b8 = 128;
        int c1 = 0, c2 = 0, c3 = 0, c4 = 0, c5 = 0, c6 = 0, c7 = 0, c8 = 0;
        for (var copyIdx = 0; copyIdx < copies.Count; copyIdx++)
        {
            var copy = copies[copyIdx][byteIdx];
            c1 += (copy & b1) != 0 ? 1 : -1;
            c2 += (copy & b2) != 0 ? 1 : -1;
            c3 += (copy & b3) != 0 ? 1 : -1;
            c4 += (copy & b4) != 0 ? 1 : -1;
            c5 += (copy & b5) != 0 ? 1 : -1;
            c6 += (copy & b6) != 0 ? 1 : -1;
            c7 += (copy & b7) != 0 ? 1 : -1;
            c8 += (copy & b8) != 0 ? 1 : -1;
        }
        int result = 0;
        if (c1 > 0) result |= b1;
        if (c2 > 0) result |= b2;
        if (c3 > 0) result |= b3;
        if (c4 > 0) result |= b4;
        if (c5 > 0) result |= b5;
        if (c6 > 0) result |= b6;
        if (c7 > 0) result |= b7;
        if (c8 > 0) result |= b8;
        return (byte)result;
    }
}