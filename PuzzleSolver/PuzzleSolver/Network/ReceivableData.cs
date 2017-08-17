using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroFormatter;

namespace PuzzleSolver.Network {
    [ZeroFormattable]
    public struct ReceivablePoint {
        [Index(0)]
        public int X;
        [Index(1)]
        public int Y;

        public ReceivablePoint(int X, int Y) {
            this.X = X;
            this.Y = Y;
        }

        public static implicit operator ReceivablePoint(Geometry.Point p)
            => new ReceivablePoint((int)p.Re, (int)p.Im);
    }

    [ZeroFormattable]
    public class ReceivablePolygon {
        [Index(0)]
        public virtual List<ReceivablePoint> Points { get; set; }

        public ReceivablePolygon() { }

        public ReceivablePolygon(Geometry.Point[] pts) {
            Points = new List<ReceivablePoint>(pts.Length);
            foreach (var p in pts) {
                Points.Add(p);
            }
        }
    }
}
