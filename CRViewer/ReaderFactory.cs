using System;
using System.Collections.Generic;
using System.IO;

namespace CRViewer
{
    public static class ReaderFactory
    {

        public static HashSet<string> SupportedExtensions = new HashSet<string> { ".cr2", ".cr3" };

        public static IJpegReader GetReader(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();

            if (ext == ".cr2")
                return new Cr2Reader(fileName);

            if (ext == ".cr3")
                return new Cr3Reader(fileName);

            throw new NotImplementedException("No reader for " + ext);
        }
    }
}
