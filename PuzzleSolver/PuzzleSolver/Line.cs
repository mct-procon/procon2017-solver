using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuzzleSolver;

namespace PuzzleSolver
{
	class Line
	{
		public Point start { get; private set; }
		public Point end { get; private set; }

		//コンストラクタ
		public Line() {}
		public Line(Point s, Point e) { start = s; end = e; }

		//平行移動
		public void Trans(Point t) { start += t; end += t; }
		
		//乗算（回転）
		public void Mul(Point mulValue) { start *= mulValue; end *= mulValue; }

		//反転（y=0が対称軸)
		public void Reverse() { start = start.Conj; end = end.Conj; }
		
		//当たり判定(T字, 同一直線上は交差とみなさない)
		public static bool IsHit(Line line1, Line line2)
		{
			int a = Point.Ccw(line1.start, line1.end, line2.start);
			int b = Point.Ccw(line1.start, line1.end, line2.end);
			if (a * b != -1) { return false; }
			int c = Point.Ccw(line2.start, line2.end, line1.start);
			int d = Point.Ccw(line2.start, line2.end, line1.end);
			if (c * d != -1) { return false; }
			return true;
		}

		//交点(line1とline2は平行ではないとする)
		public static Point CrossPoint(Line line1, Line line2)
		{
			double A = Point.Cross(line1.end - line1.start, line2.end - line2.start);
			double B = Point.Cross(line1.end - line1.start, line1.end - line2.start);
			return line2.start + B / A * (line2.end - line2.start);
		}

		//点と線分の距離
		public double Distance(Point point)
		{
			if (Point.Dot(end - start, point) < 0) { return (start - point).Abs; }
			if (Point.Dot(start - end, point) < 0) { return (end - point).Abs; }
			return Point.Cross(end - start, point) / (end - start).Abs;
		}
	}
}
