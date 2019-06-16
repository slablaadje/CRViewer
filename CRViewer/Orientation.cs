using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRViewer
{
    public class Orientation
    {
        private int flags;

        public Orientation(int flags)
        {
            this.flags = flags-1;
        }

        public bool FlipDiagonal {
            get {
                return (flags & 0b100) != 0;
            }
        }
        public bool Rotate180 {
            get {
                return (flags & 0b010) != 0;
            }
        }
        public bool FlipHorizontal {
            get {
                return (flags & 0b001) != 0;
            }
        }

        public override string ToString()
        {
            return $"Flag: {flags}, FlipDiagonal: {FlipDiagonal}, Rotate180: {Rotate180}, FlipHorizontal: {FlipHorizontal}";
        }
    }
}
