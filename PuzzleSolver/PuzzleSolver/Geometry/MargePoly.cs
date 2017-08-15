using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleSolver.Geometry
{
	//2つの多角形をマージする。あんまり多く呼び出さないため, 定数倍高速化よりも保守性を考えるべき。
	public class MargePoly
	{
		List<Point> pointList;
		List<int>[] edgeTo;
		List<bool>[] used;

		//2多角形をマージする。srcPolyが移動してきた多角形(ピースであることが保証される)
		public List<Poly> Marge(Poly dstPoly, Poly srcPoly)
		{
			pointList = new List<Point>();
			for (int i = 0; i < dstPoly.Count; i++) { AddPointList(dstPoly.points[i]); }
			for (int i = 0; i < srcPoly.Count; i++) { AddPointList(srcPoly.points[i]); }

			edgeTo = new List<int>[pointList.Count];
			for (int i = 0; i < dstPoly.Count; i++) { AddEdgeTo(new Line(dstPoly.points[i], dstPoly.points[i + 1]), dstPoly, srcPoly); }
			for (int i = 0; i < srcPoly.Count; i++) { AddEdgeTo(new Line(srcPoly.points[i], srcPoly.points[i + 1]), dstPoly, srcPoly); }
			used = new List<bool>[pointList.Count];
			for (int i = 0; i < pointList.Count; i++) { for (int j = 0; j < edgeTo[i].Count; i++) { used[i].Add(false); } }

			//サイクル検出
			List<Poly> polys = new List<Poly>();
			List<Line> lines = dstPoly.lines;		//(表示用)線分の方は, 参照をコピーしておけばOk
			lines.AddRange(srcPoly.lines);

			for (int i = 0; i < pointList.Count; i++)
			{
				for (int j = 0; j < edgeTo[i].Count; i++)
				{
					if (used[i][j]) { continue; }
					polys.AddRange(getPolys(i, j, dstPoly.isPiece, lines));
				}
			}

			//エラー処理
			if (dstPoly.isPiece && polys.Count > 1) { return new List<Poly>(); }

			//冗長点削除 + 辺生成
			List<Poly> ret = new List<Poly>();
			for (int i = 0; i < polys.Count; i++)
			{
				ret.Add(fixPoly(polys[i].points, lines, polys[i].isPiece));
			}
			return ret;
		}

		//点リストに点p(or pと非常に近い点)がなければ, 点リストに点pを追加する.
		private void AddPointList(Point p)
		{
			double eps = 1e-10;
			for (int i = 0; i < pointList.Count; i++) { if ((pointList[i] - p).Norm <= eps) { return; } }
			pointList.Add(p);
		}

		//辺の追加
		private void AddEdgeTo(Line line, Poly dstPoly, Poly srcPoly)
		{
			double eps = 1e-5;
			List<int> pointId = new List<int>();

			for (int i = 0; i < pointList.Count; i++) { if (line.Distance(pointList[i]) <= eps) { pointId.Add(i); } }
			pointId.Sort(delegate (int a, int b) {
				double diff = (line.start - pointList[pointId[a]]).Norm - (line.start - pointList[pointId[b]]).Norm;
				if (diff < 0) return -1;
				if (diff == 0) return 0;
				return 1;
			});

			double dist = 1e-2;
			for (int i = 0; i < pointId.Count - 1; i++)
			{
				Point a = pointList[pointId[i]];
				Point b = pointList[pointId[i + 1]];
				Point mid = (a + b) / 2;
				Point l = (b - a) * new Point(0, 1);  l /= l.Abs; l *= dist; l += (a + b) / 2;
				Point r = (b - a) * new Point(0, -1); r /= r.Abs; r *= dist; r += (a + b) / 2;
				if (IsWall(dstPoly, srcPoly, l)) { edgeTo[pointId[i]].Add(pointId[i + 1]); }
				if (IsWall(dstPoly, srcPoly, r)) { edgeTo[pointId[i + 1]].Add(pointId[i]); }
			}
		}
	
		//サイクルを検出して多角形を作る
		private List<Poly> getPolys(int startPointId, int startEdgeId, bool isPiece, List<Line> lines)
		{
			List<int> cycle = getCycle(startPointId, startEdgeId, isPiece);
			List<List<int>> cycles = BreakDownCycle(cycle);
			List<Poly> ret = new List<Poly>();

			for (int i = 0; i < cycles.Count; i++)
			{
				List<Point> points = new List<Point>();
				for (int j = 0; j < cycles[i].Count; j++)
				{
					points.Add(pointList[cycles[i][j]]);
				}

				ret.Add(fixPoly(points, lines, isPiece));
			}
			return ret;
		}

		//点pointが壁に含まれるか？(境界の結果はPoly.isCover関数に依存)
		//壁の定義：
		//ピース同士の場合   … マージする2ピースの和領域
		//枠穴とピースの場合 … {枠穴 - ピース}以外の領域
		private bool IsWall(Poly dstPoly, Poly srcPoly, Point point)
		{
			if (dstPoly.isPiece)
			{
				return dstPoly.isCover(point) || srcPoly.isCover(point);
			}
			else
			{
				if (srcPoly.isCover(point)) { return true; }
				else if (dstPoly.isCover(point)) { return false; }
				else { return true; }
			}
		}

		//サイクルを検出する
		private List<int> getCycle(int startPointId, int startEdgeId, bool isPiece)
		{
			List<int> cycle = new List<int>();
			cycle.Add(startPointId);

			int pos = edgeTo[startPointId][startEdgeId];
			used[startPointId][startEdgeId] = true;

			while (pos != startPointId)
			{
				cycle.Add(pos);
				int i;
				for (i = 0; i < edgeTo[pos].Count; i++)
				{
					if (!used[pos][i]) { break; }
				}
				if (i == edgeTo[pos].Count) { throw new Exception("多角形マージ処理：サイクルを検出できませんでした。"); }

				used[pos][i] = true;
				pos = edgeTo[pos][i];
			}
			cycle.Add(startPointId);
			return cycle;
		}

		//cycleがvisual studio 2015のマークみたいな形になっていたら, 複数のサイクルに分解する。
		private List<List<int>> BreakDownCycle(List<int> cycle)
		{
			bool[] visited = new bool[pointList.Count];
			for (int i = 0; i < pointList.Count; i++) { visited[i] = false; }

			List<List<int>> cycles = new List<List<int>>();
			Stack<int> stack = new Stack<int>();

			for (int i = 0; i < cycle.Count; i++)
			{
				stack.Push(cycle[i]);
				if (visited[cycle[i]])  //値cycle[i]がstackに2個入っていたら
				{
					List<int> suzuki = new List<int>();
					suzuki.Add(stack.Pop());
					while (stack.Peek() != cycle[i]) { suzuki.Add(stack.Pop()); }
					suzuki.Add(stack.Peek());
					cycles.Add(suzuki);
				}
				visited[cycle[i]] = true;
			}
			return cycles;
		}

		//冗長点を削除する
		Poly fixPoly(List<Point> points, List<Line> lines, bool isPiece)
		{
			List<Point> ret = new List<Point>();

			for (int i = 0; i < points.Count - 1; i++)
			{
				Point a = (i == 0) ? points[points.Count - 2] : points[i - 1];
				Point b = points[i];
				Point c = points[i + 1];
				if (Point.Ccw(a, b, c) != -2) { ret.Add(b); }
			}
			ret.Add(ret[0]);

			return new Poly(ret, lines, isPiece);
		}
	}
}
