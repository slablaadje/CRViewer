using System.IO;

namespace CRViewer
{
    public static class RawHelper
    {
        public static BinaryReader GetReader(string filename)
        {
            return new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read));
        }

        // Swap bytes used by CR2 reader
        public static ushort SwapBytes(ushort x)
        {
            return (ushort)(((x & 0x00ff) << 8) +
                          ((x & 0xff00) >> 8));
        }

        // Swap byte used by CR3 reader
        public static uint SwapBytes(uint x)
        {
            return ((x & 0x000000ff) << 24) +
                   ((x & 0x0000ff00) << 8) +
                   ((x & 0x00ff0000) >> 8) +
                   ((x & 0xff000000) >> 24);
        }
    }
}
