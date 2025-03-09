namespace HashNCopy;
public class HashedStreamList : List<IBlockStream>, IBlockStream
{
    public bool VoteMode { get; set; }

    public string? Name
    {
        get { return this.FirstOrDefault()?.Name; }
        set { Parallel.ForEach(this, (stream) => stream.Name = value); }
    }

    public int? BlockSize
    {
        get { return this.FirstOrDefault()?.BlockSize; }
        set { Parallel.ForEach(this, (stream) => stream.BlockSize = value); }
    }

    public long? BlockIdx
    {
        get { return this.FirstOrDefault()?.BlockIdx; }
        set { Parallel.ForEach(this, (stream) => stream.BlockIdx = value); }
    }

    public long? BlockCount
    {
        get { return this.FirstOrDefault()?.BlockCount; }
    }

    public long? DataCount
    {
        get { return this.FirstOrDefault()?.DataCount; }
    }

    public HashedStreamList(IEnumerable<IBlockStream>? streams = null, bool voteMode = false)
    {
        if (streams != null) AddRange(streams);
        VoteMode = voteMode;
    }

    public byte[]? ReadBlock(int? blockSize = null) => VoteMode ? VoteBlockPrivate(blockSize) : ReadBlockPrivate(blockSize);

    public void WriteBlock(byte[] data) => Parallel.ForEach(this, (stream) => stream.WriteBlock(data));

    private byte[]? ReadBlockPrivate(int? blockSize = null)
    {
        byte[]? result = null;
        foreach (var stream in this) { result = stream.ReadBlock(blockSize); if (result != null) break; }
        return result;
    }

    private byte[]? VoteBlockPrivate(int? blockSize = null) => BitVote.Process(this.AsParallel().Select((stream) => stream?.ReadBlock(blockSize)).Where((item) => item != null).ToList());

    public virtual void Dispose() => Parallel.ForEach(this, (stream) => stream.Dispose());
}