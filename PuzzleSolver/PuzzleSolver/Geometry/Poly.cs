using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuzzleSolver.Geometry;

namespace PuzzleSolver.Geometry
{
	public class Poly
	{
        /// <summary>
        /// 点列(始点 = 終点). 実体.
        /// </summary>
		public List<Point> points { get; private set; }
        /// <summary>
        /// 線分の集合. 実体.
        /// </summary>
		public List<Line> lines { get; private set; }
		public bool isPiece { get; }

		//コンストラクタ
		public Poly() { }
		public Poly(List<Point> points, List<Line> lines, bool isPiece)
		{
			this.points  = points;
			this.lines   = lines;
			this.isPiece = isPiece;
		}

		//多角形の頂点数
		public int Count { get { return points.Count - 1; } }

		//面積
		public double Area
		{
			get
			{
				double area = 0;
				for (int i = 0; i < Count; i++)
				{
					area += Point.Cross(points[i], points[i + 1]);
				}
				area /= 2;
				return area;
			}
		}

		//線分が接触しているか
		public bool isHitLine(Poly poly)
		{
			for (int i = 0; i < Count; i++)
			{
				Line line1 = new Line(points[i], points[i + 1]);
				for (int j = 0; j < poly.Count; j++)
				{
					Line line2 = new Line(poly.points[i], poly.points[i + 1]);
					if (Line.IsHit(line1, line2))
					{
						return true;
					}
				}
			}
			return false;
		}

		//点pointが多角形thisの内部に含まれるか？ (境界はfalse)
		public bool isCover(Point point)
		{
			bool flag = false;
			double eps = 1e-10;
			for (int i = 0; i < Count; i++)
			{
				Point a = points[i] - point;
				Point b = points[i + 1] - point;
				if (a.Im > b.Im) { Generic.Swap(ref a, ref b); }
				if (a.Im <= 0 && 0 <= b.Im && Point.Cross(a, b) > eps) { flag = !flag; }
				if (Point.Cross(a, b) == eps && Point.Dot(a, b) <= eps) { return false; }
			}
			return flag;
		}

		//平行移動
		public void Trans(Point t, bool isUpdateLines)
		{
			for (int i = 0; i < points.Count; i++) { points[i] += t; }
			if (isUpdateLines) { for (int i = 0; i < lines.Count; i++) { lines[i].Trans(t); } }
		}

		//反転 (軸はy = 0)
		public void Reverse(bool isUpdateLines)
		{
			for (int i = 0; i < points.Count; i++) { points[i] = points[i].Conj; }
			if (isUpdateLines) { for (int i = 0; i < lines.Count; i++) { lines[i].Reverse(); } }
		}

		//乗算 (回転)
		public void Mul (Point mulValue, bool isUpdateLines)
		{
			for (int i = 0; i < points.Count; i++) { points[i] = points[i] * mulValue; }
			if (isUpdateLines) { for (int i = 0; i < lines.Count; i++) { lines[i].Mul(mulValue); } }
		}
	}
}
