Console.CursorVisible = false;
Console.WriteLine("HashNCopy 1.00   Copyright (c) 2024 Artem Drobanov   20 Feb 2024");
if ((args.Length == 2) && File.Exists(args[0]) && Directory.Exists(args[1]))
{
    var sourceFile = args[0];
    var targetDir = Path.Combine(args[1], Path.GetFileName(sourceFile));
    Console.Write($"Source: {sourceFile}\nTarget: {targetDir}\n");
    using (var hash = System.Security.Cryptography.SHA256.Create())
    using (var source = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read))
    using (var target = new FileStream(targetDir, FileMode.Create, FileAccess.Write, FileShare.None))
    {
        var progressThr = new Thread(() =>
        {
            while (true)
            {
                try { Console.Write($"Progress: {((source.Position / (double)source.Length) * 100):F2} %\r"); } catch { } finally { Thread.Sleep(100); }
            }
        }) { IsBackground = true }; progressThr.Start();

        var buffer = new byte[4096];
        while (source.Position < source.Length)
        {
            var read = source.Read(buffer, 0, buffer.Length);
            target.Write(buffer, 0, read);
            hash.TransformBlock(buffer, 0, read, buffer, 0);
        }
        hash.TransformFinalBlock(buffer, 0, 0);
        Array.Clear(buffer);
        File.WriteAllText(targetDir + ".sha256", $"{Convert.ToHexString(hash.Hash).ToLower()}  {Path.GetFileName(args[0])}\n");
    }
} else Console.WriteLine("Usage: HashNCopy <source file> <target directory>");