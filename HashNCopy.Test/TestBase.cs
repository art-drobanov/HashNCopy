namespace HashNCopy.Test;
public class TestBase
{
    protected const int KB = 1024;
    protected const int MB = KB * KB;
    protected const int BLOCK_SIZE = 64 * KB;

    protected static class Random
    {
        private static System.Random _rnd = new System.Random((int)(DateTime.Now.Ticks % int.MaxValue));
        private static object _syncRoot = new object();
        public static double GetDouble() { lock (_syncRoot) return _rnd.NextDouble(); }
        public static int GetInteger(int min, int maxExcl) { lock (_syncRoot) return _rnd.Next(min, maxExcl); }
        public static byte[] GetBuffer(int size) { var buffer = new byte[size]; lock (_syncRoot) _rnd.NextBytes(buffer); return buffer; }
        public static int[] GetDistinctValues(int minValue, int maxValue, int N)
        {
            var values = Enumerable.Range(minValue, (maxValue - minValue) + 1).ToList();
            while (values.Count > N) values.RemoveAt(Random.GetInteger(0, values.Count));
            return values.ToArray();
        }
    }
}