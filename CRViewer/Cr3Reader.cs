using System;
using System.IO;

namespace CRViewer
{
    public class Cr3Reader : ReaderBase
    {
        private static int soiOffsetIndex = 0xe2;
        private static int lenOffsetIndex = 0x34;

        public Cr3Reader(string fileName)
            : base(fileName)
        {

        }

        public override Orientation Orientation()
        {
            Reader.BaseStream.Seek(0x12C, SeekOrigin.Begin); //magic!
            uint count = Reader.ReadUInt32();
            int dunno = Reader.ReadUInt16();
            for (int i = 0; i < count; i++)
            {
                uint tagId = Reader.ReadUInt16();
                uint type = Reader.ReadUInt16();
                uint c = Reader.ReadUInt32();
                uint d = Reader.ReadUInt32();
                if(tagId == 0x0112)
                {
                    return new Orientation((int)d);
                }
            }
            return null;
        }

        public override byte[] GetData()
        {
            try
            {
                Reader.BaseStream.Seek(0, SeekOrigin.Begin);
                Reader.ReadBytes(soiOffsetIndex);

                uint offset = RawHelper.SwapBytes(Reader.ReadUInt32());
                Reader.BaseStream.Seek(offset + lenOffsetIndex, SeekOrigin.Begin);
                uint len = RawHelper.SwapBytes(Reader.ReadUInt32());
                byte[] data = Reader.ReadBytes((int)len);
                return data;
            }
            catch(Exception e)
            {
                return null;
            }
        }
    }
}