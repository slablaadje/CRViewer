using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        public Matrix Matrix {
            get {
                Matrix matrix = new Matrix();
                if (FlipDiagonal)
                    matrix.Multiply(new Matrix(0, 1, 1, 0, 0, 0), MatrixOrder.Append);
                if (Rotate180)
                    matrix.Multiply(new Matrix(-1, 0, 0, -1, 0, 0), MatrixOrder.Append);
                if (FlipHorizontal)
                    matrix.Multiply(new Matrix(-1, 0, 0, 1, 0, 0), MatrixOrder.Append);
                return matrix;
            }
        }

        public Point CalculateOffset(Point offset, int imageWidth, int imageHeight)
        {
            Point[] points = new Point[] {
                    new Point(0,0),
                    new Point(0, imageHeight),
                    new Point(imageWidth, 0),
                    new Point(imageWidth, imageHeight)};

            Matrix.TransformPoints(points);
            Point min = new Point(points.Min(p => p.X), points.Min(p => p.Y));
            points = new Point[] { min };
            Matrix.TransformPoints(points);
            min = points[0];
            Matrix inverted = Matrix.Clone();
            inverted.Invert();
            points = new Point[] { new Point(0, 40) };
            inverted.TransformPoints(points);
            return new Point(min.X + points[0].X, min.Y + points[0].Y);
        }

        public override string ToString()
        {
            return $"Flag: {flags}, FlipDiagonal: {FlipDiagonal}, Rotate180: {Rotate180}, FlipHorizontal: {FlipHorizontal}";
        }
    }
}
