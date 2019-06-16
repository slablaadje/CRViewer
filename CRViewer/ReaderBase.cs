using System;
using System.Drawing;
using System.IO;

namespace CRViewer
{
    public abstract class ReaderBase : IJpegReader, IDisposable
    {
        public string FileName { get; set; }

        public virtual Image GetImage()
        {
            try
            {
                return (Image)new ImageConverter().ConvertFrom(GetData());
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected BinaryReader Reader;

        public abstract byte[] GetData();

        public ReaderBase(string fileName)
        {
            FileName = fileName;
            Reader = RawHelper.GetReader(FileName);
        }

        public void Dispose()
        {
            Reader.Dispose();
        }

        public abstract Orientation Orientation();
    }
}
