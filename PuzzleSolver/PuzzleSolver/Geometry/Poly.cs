using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		private bool isPiece;

		//コンストラクタ
		Poly() { }
		Poly(List<Point> points, List<Line> lines, bool isPiece)
		{
			this.points  = points;
			this.lines   = lines;
			this.isPiece = isPiece; 
		}

		//多角形の頂点数
		int Count { get { return points.Count - 1; } }

		//面積
		double Area
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
		bool isHitLine(Poly poly)
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

		//平行移動
		void Trans(Point t, bool isUpdateLines)
		{
			for (int i = 0; i < points.Count; i++) { points[i] += t; }
			if (isUpdateLines) { for (int i = 0; i < lines.Count; i++) { lines[i].Trans(t); } }
		}

		//反転 (軸はy = 0)
		void Reverse(bool isUpdateLines)
		{
			for (int i = 0; i < points.Count; i++) { points[i] = points[i].Conj; }
			if (isUpdateLines) { for (int i = 0; i < lines.Count; i++) { lines[i].Reverse(); } }
		}

		//乗算 (回転)
		void Mul (Point mulValue, bool isUpdateLines)
		{
			for (int i = 0; i < points.Count; i++) { points[i] = points[i] * mulValue; }
			if (isUpdateLines) { for (int i = 0; i < lines.Count; i++) { lines[i].Mul(mulValue); } }
		}
	}
}
