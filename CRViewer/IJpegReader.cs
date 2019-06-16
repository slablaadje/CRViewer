using System;
using System.Drawing;

namespace CRViewer
{
    public interface IJpegReader : IDisposable
    {
        string FileName { get; set; }
        Image GetImage();
        Orientation Orientation();
    }
}
