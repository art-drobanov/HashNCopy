using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HashNCopy.Test;

[TestClass]
public class DataCorrectonTests : TestBase
{
    [TestMethod]
    public void BitVoteArrayTest()
    {
        DataCorrectonTest("!BitVoteArrayTest.MBps.txt", (List<byte[]?>? copies) =>
        {
            ApplyErasuresByRandom(copies, GetMaxVoteErrorCount(copies!.Count));
            return BitVote.Process(copies);
        });
    }

    [TestMethod]
    public void BitVoteHashedStreamListTest()
    {
        DataCorrectonTest("!BitVoteHashedStreamListTest.MBps.txt", (List<byte[]?>? copies) =>
        {
            ApplyErasuresByRandom(copies, GetMaxVoteErrorCount(copies!.Count));
            var hashedStreams = copies.Select((dataStream) => new HashedStream(new MemoryStream(), null)).ToArray();
            foreach (var (stream, idx) in hashedStreams.Select((stream, idx) => (stream, idx))) stream.WriteBlock(copies![idx]!);
            using (var hashedStreamList = new HashedStreamList(hashedStreams, voteMode: true))
            {
                hashedStreamList.BlockIdx = 0;
                return hashedStreamList.ReadBlock(copies!.First()!.Length);
            }
        });
    }

    [TestMethod]
    public void OneAliveCopyHashedStreamListTest()
    {
        DataCorrectonTest("!OneAliveCopyHashedStreamListTest.MBps.txt", (List<byte[]?>? copiesNormal) =>
        {
            var copiesDamaged = copiesNormal!.Select((copyNormal) => copyNormal!.ToArray()).ToList();
            ApplyErasuresByCopies(copiesDamaged!, copiesDamaged!.Count - 1);

            var hashedStreamsNormal = copiesNormal!.Select((copyNormal) => new HashedStream(new MemoryStream(), new MemoryStream())).ToArray();
            var hashedStreamsDamaged = copiesDamaged.Select((copyDamaged) => new HashedStream(new MemoryStream(), null)).ToArray();

            foreach (var (stream, idx) in hashedStreamsNormal.Select((stream, idx) => (stream, idx))) stream.WriteBlock(copiesNormal![idx]!);
            foreach (var (stream, idx) in hashedStreamsDamaged.Select((stream, idx) => (stream, idx))) stream.WriteBlock(copiesDamaged![idx]!);
            for (int i = 0; i < hashedStreamsNormal.Length - 1; i++) hashedStreamsDamaged[i].HashStream = hashedStreamsNormal[i].HashStream;
            using (var hashedStreamListDamaged = new HashedStreamList(hashedStreamsDamaged, voteMode: false))
            {
                hashedStreamListDamaged.BlockIdx = 0;
                return hashedStreamListDamaged.ReadBlock(copiesNormal!.First()!.Length);
            }
        });
    }

    private void DataCorrectonTest(string reportName,
                                   Func<List<byte[]?>?, byte[]?> processing,
                                   int copiesMin = 1, int copiesMax = 9)
    {
        var sw = new Stopwatch();
        var processedTotal = 0;
        for (int N = copiesMin; N <= copiesMax; N++)
        {
            var original = Random.GetBuffer(MB);
            var copies = Enumerable.Range(0, N).Select((i) => original.ToArray()).ToList();
            sw.Start(); var corrected = processing(copies!); sw.Stop();
            if (corrected != null)
            {
                Assert.IsTrue(Tools.BinaryCompare(corrected, original));
                processedTotal += corrected.Length * copies.Count;
            }
        }
        var spdMBPS = (processedTotal / MB) / sw.Elapsed.TotalSeconds;
        File.WriteAllText(reportName, spdMBPS.ToString("F1"));
    }

    private void ApplyErasuresByRandom(List<byte[]?>? copies, int errorCount)
    {
        for (int byteIdx = 0; byteIdx < copies!.First()!.Length; byteIdx++)
        {
            var errPosList = Random.GetDistinctValues(0, copies!.Count - 1, errorCount);
            foreach (var errPos in errPosList) copies![errPos]![byteIdx] ^= 0xFF;
        }
    }

    private void ApplyErasuresByCopies(List<byte[]?>? copies, int errorCount)
    {
        var errPosList = Random.GetDistinctValues(0, copies!.Count - 1, errorCount);
        foreach (var errPos in errPosList)
            for (int byteIdx = 0; byteIdx < copies!.First()!.Length; byteIdx++)
                copies![errPos]![byteIdx] ^= 0xFF;
    }

    private int GetMaxVoteErrorCount(int N) => (N - ((N + 1) % 2)) / 2;
}