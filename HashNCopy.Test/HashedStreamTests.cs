using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace HashNCopy.Test;

[TestClass]
public partial class HashedStreamTests : TestBase
{
    [TestMethod]
    public void HashedMemoryStreamTest()
    {
        using (var dataStream = new MemoryStream())
        using (var hashStream = new MemoryStream())
        using (var hashedStream = new HashedStream(dataStream, hashStream))
        {
            HashedStreamTest(hashedStream, "MemoryStream");
        }
    }

    [TestMethod]
    public void HashedFileStreamTest()
    {
        using (var dataStream = Tools.FileStream("!HashedFileStream.bin", true))
        using (var hashStream = Tools.FileStream("!HashedFileStream.bin.shb512", true))
        using (var hashedStream = new HashedStream(dataStream, hashStream))
        {
            HashedStreamTest(hashedStream, "FileStream");
        }
    }

    [TestMethod]
    public void HashedFileTest()
    {
        using (var hashedStream = new HashedFile("!HashedFile.bin", true))
        {
            HashedStreamTest(hashedStream, "File");
        }
    }

    [TestMethod]
    public void HashedStreamListTest()
    {
        using (var s1 = new HashedFile("!HashedFile1.bin", true))
        using (var s2 = new HashedFile("!HashedFile2.bin", true))
        using (var s3 = new HashedFile("!HashedFile3.bin", true))
        using (var s4 = new HashedFile("!HashedFile4.bin", true))
        using (var hashedStreamList = new HashedStreamList(new HashedFile[] { s1, s2, s3, s4 }))
        {
            HashedStreamTest(hashedStreamList, "File");
        }
    }

    private void HashedStreamTest(IBlockStream hashedStream, string testName, int N = 10000)
    {
        // Data
        byte[][] originals = new byte[N][];
        var usedBlockIdxs = new List<long?>();

        // Rnd data
        for (int blockIdx = 0; blockIdx < N; blockIdx++) originals[blockIdx] = Random.GetBuffer(BLOCK_SIZE);

        // Sequential rec.
        var sw = Stopwatch.StartNew();
        for (int blockIdx = 0; blockIdx < N; blockIdx++) hashedStream.WriteBlock(originals[blockIdx]);
        sw.Stop(); var spdMBPS = ((N * BLOCK_SIZE) / MB) / sw.Elapsed.TotalSeconds;
        File.WriteAllText($"!{testName}.WriteBlock({BLOCK_SIZE}).MBps.txt", spdMBPS.ToString("F1"));

        // Sequential read & verification
        sw = Stopwatch.StartNew();
        hashedStream.BlockIdx = 0;
        for (int blockIdx = 0; blockIdx < N; blockIdx++)
        {
            usedBlockIdxs.Add(hashedStream.BlockIdx);
            var data = hashedStream.ReadBlock();
            Assert.IsTrue(Tools.BinaryCompare(data, originals[blockIdx]));
        }
        Assert.IsTrue(usedBlockIdxs.Distinct().Count() == N);
        sw.Stop(); spdMBPS = ((N * BLOCK_SIZE) / MB) / sw.Elapsed.TotalSeconds;
        File.WriteAllText($"!{testName}.ReadBlock({BLOCK_SIZE}).MBps.txt", spdMBPS.ToString("F1"));

        // Random read & verification
        usedBlockIdxs.Clear();
        var blockIdxList = Enumerable.Range(0, N).ToList();
        while (blockIdxList.Any())
        {
            var blockIdxRnd = blockIdxList[Random.GetInteger(0, blockIdxList.Count)]; blockIdxList.Remove(blockIdxRnd);
            hashedStream.BlockIdx = blockIdxRnd;
            usedBlockIdxs.Add(hashedStream.BlockIdx);
            var data = hashedStream.ReadBlock();
            Assert.IsTrue(Tools.BinaryCompare(data, originals[blockIdxRnd]));
        }
        Assert.IsTrue(usedBlockIdxs.Distinct().Count() == N);
    }
}