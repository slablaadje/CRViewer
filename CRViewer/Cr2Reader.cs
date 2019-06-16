using System;
using System.IO;
using System.Linq;

namespace CRViewer
{
    public class Cr2Reader : ReaderBase
    {
        private uint bigDataPos;
        private uint bigDataLen;

        private uint smallDataPos = 0;
        private uint smallDataLen = 0;

        private Orientation _Orientation;

        public Cr2Reader(string fileName)
            : base(fileName)
        {
            ReadTiffHeader();
            FindSubImage();
        }

        private void ReadTiffHeader()
        {
            Reader.BaseStream.Seek(0x10, SeekOrigin.Begin);
            uint count = Reader.ReadUInt16();
            for (int i = 0; i < count; i++)
            {
                uint tagId = Reader.ReadUInt16();
                uint type = Reader.ReadUInt16();
                uint c = Reader.ReadUInt32();
                uint d = Reader.ReadUInt32();
                if (tagId == 0x0111)
                    bigDataPos = d;
                if (tagId == 0x0117)
                    bigDataLen = d;
                if (tagId == 0x0112)
                    _Orientation = new Orientation((int)d);
            }
        }


        private void FindSubImage()
        {
            Reader.BaseStream.Seek(bigDataPos, SeekOrigin.Begin);
            short s = Reader.ReadInt16(); //ffd8
            ushort marker = 0;
            while (marker != 0xffda)
            {
                marker = RawHelper.SwapBytes(Reader.ReadUInt16());
                ushort len = RawHelper.SwapBytes(Reader.ReadUInt16());
                if (marker == 0xffe2)
                {
                    ParseMPF();
                    break;
                }
                else
                    Reader.BaseStream.Seek(len - 2, SeekOrigin.Current);
            }
        }

        private void ParseMPF()
        {
            long l = Reader.BaseStream.Position;
            byte[] mpfHeader = Reader.ReadBytes(4);
            if (mpfHeader.SequenceEqual(new byte[] { 0x4D, 0x50, 0x46, 0 }))
            {
                Reader.BaseStream.Seek(8, SeekOrigin.Current); //byteorder, offset to first IFD
                ushort numEntries = Reader.ReadUInt16();
                int numImages = 0;

                for (int i = 0; i < numEntries; i++)
                {
                    ushort tag = Reader.ReadUInt16();
                    ushort type = Reader.ReadUInt16();
                    uint taglen = Reader.ReadUInt32();
                    if (tag == 0xb000)
                        Reader.ReadUInt32(); // should be the text '0100'
                    if (tag == 0xb001)
                        numImages = Reader.ReadInt32(); // should be 2
                    if (tag == 0xb002)
                    {//read image mess
                        byte[] d = Reader.ReadBytes((16 + 33) * numImages);
                        uint offset = BitConverter.ToUInt32(d, 32) + 10; //10?
                        smallDataLen = bigDataLen - offset;
                        smallDataPos = bigDataPos + offset;
                        return;
                    }
                }
            }
        }

        public override byte[] GetData()
        {
            if (smallDataLen == 0)
                return BigJpegData();
            else
                return SmallJpegData();
        }

        public byte[] BigJpegData()
        {
            Reader.BaseStream.Seek(bigDataPos, SeekOrigin.Begin);
            byte[] data = Reader.ReadBytes((int)bigDataLen);
            return data;
        }

        public byte[] SmallJpegData()
        {
            if (smallDataLen == 0)
                return null;
            Reader.BaseStream.Seek(smallDataPos, SeekOrigin.Begin);
            byte[] data = Reader.ReadBytes((int)smallDataLen);

            return data;
        }

        public override Orientation Orientation()
        {
            return _Orientation;
        }
    }
}