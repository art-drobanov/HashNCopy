using HashNCopy;
Console.CursorVisible = false; Console.WriteLine("HashNCopy 2.00   Copyright (c) 2024 Artem Drobanov   09 Mar 2024");
if (!args.Any()) { Console.WriteLine("Usage: HashNCopy <source file(s)> <target directory/file>"); return; }
var targetPath = Directory.Exists(args.Last()) ? Path.Combine(args.Last(), Path.GetFileName(args.First())) : args.Last();
using (var source = new HashedStreamList(args.Take(args.Length - 1).Select((arg) => new HashedFile(arg, createMode: false)), voteMode: targetPath == args.Last()))
using (var target = new HashedStreamList(new[] { new HashedFile(targetPath, createMode: true) }, voteMode: targetPath == args.Last()))
using (var hash = System.Security.Cryptography.SHA256.Create())
{
    Console.Write($"{String.Join("\n", source.Select(sourceFile => $"Source: {sourceFile.Name}\n"))}Target: {target.First().Name}\n");
    var progressThr = new Thread(() =>
    {
        while (true) try { Console.Write($"Progress: {(source.BlockIdx / (double)source.BlockCount!) * 100:F2} %\r"); } catch { } finally { Thread.Sleep(100); }
    }) { IsBackground = true }; progressThr.Start();
    byte[]? block = null;
    for (int blockIdx = 0; blockIdx < source.BlockCount; blockIdx++)
    {
        block = source.ReadBlock(); if (block is null) throw new Exception($"Can't get valid block({blockIdx}), corrupted data"); target.WriteBlock(block);
        hash.TransformBlock(block, 0, block.Length, null, 0);
    }
    if (block is not null) hash.TransformFinalBlock(block, 0, 0);
    try { File.WriteAllText(targetPath + ".sha256", $"{Convert.ToHexString(hash.Hash!).ToLower()}  {Path.GetFileName(args[0])}\n"); } catch { }
}