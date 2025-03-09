namespace HashNCopy;
public class HashedFile : HashedStream, IBlockStream
{
    public HashedFile(string fileName, bool createMode, int blockSize = 64 * 1024) : base(Tools.FileStream(fileName, createMode),
                                                                                          Tools.FileStream($"{fileName}.shb512", createMode),
                                                                                          blockSize) { Name = fileName; }
    public override void Dispose() => base.Dispose();
}