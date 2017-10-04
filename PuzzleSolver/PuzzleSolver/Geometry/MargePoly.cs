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
		Poly debugDstPoly;
		Poly debugSrcPoly;

		//2多角形をマージする。srcPolyが移動してきた多角形(ピースであることが保証される)
		public List<Poly> Marge(Poly dstPoly, Poly srcPoly)
		{
			debugDstPoly = dstPoly;
			debugSrcPoly = srcPoly;
			pointList = new List<Point>();
			for (int i = 0; i < dstPoly.Count; i++) { AddPointList(dstPoly.points[i]); }
			for (int i = 0; i < srcPoly.Count; i++) { AddPointList(srcPoly.points[i]); }

			edgeTo = new List<int>[pointList.Count];
			for (int i = 0; i < pointList.Count; i++) { edgeTo[i] = new List<int>(); }
			for (int i = 0; i < dstPoly.Count; i++) { AddEdgeTo(new Line(dstPoly.points[i], dstPoly.points[i + 1]), dstPoly, srcPoly); }
			for (int i = 0; i < srcPoly.Count; i++) { AddEdgeTo(new Line(srcPoly.points[i], srcPoly.points[i + 1]), dstPoly, srcPoly); }
			used = new List<bool>[pointList.Count];
			for (int i = 0; i < pointList.Count; i++) { used[i] = new List<bool>(); }
			for (int i = 0; i < pointList.Count; i++) { for (int j = 0; j < edgeTo[i].Count; j++) { used[i].Add(false); } }

			//サイクル検出
			List<Poly> polys = new List<Poly>();
			List<Line> lines = new List<Line>();        //実体をコピーします！
			if (dstPoly.isPiece)
			{
				for (int i = 0; i < dstPoly.lines.Count; i++) { lines.Add(dstPoly.lines[i].Clone()); }
				for (int i = 0; i < srcPoly.lines.Count; i++) { lines.Add(srcPoly.lines[i].Clone()); }
			}

			for (int i = 0; i < pointList.Count; i++)
			{
				for (int j = 0; j < edgeTo[i].Count; j++)
				{
					if (used[i][j]) { continue; }
					polys.AddRange(getPolys(i, j, dstPoly.isPiece, lines));
				}
			}

			//エラー処理
			if (dstPoly.isPiece && polys.Count > 1) { return new List<Poly>(); }	//2ピースの内部に穴があるケース
			if (!dstPoly.isPiece && polys.Count == 0) { polys.Add(new Poly(new List<Point>(), lines, false)); return polys; }	//枠穴に完全にピースが収まったケース

			//冗長点削除 + 辺生成
			List<Poly> ret = new List<Poly>();
			for (int i = 0; i < polys.Count; i++)
			{
				ret.Add(fixPoly(polys[i].points, lines, polys[i].isPiece));
			}

			//枠とピースが頂点で接していて, 枠が単純多角形ではなくなるケースの処理
			if (!dstPoly.isPiece && ret.Count == 2)
			{
				double area = 0;
				for (int i = 0; i < 2; i++)
				{
					area += Math.Abs(ret[i].Area);
				}
				if (area > -dstPoly.Area)
				{
					//頂点列の結合をして, 1つの枠穴として返す.
					ret = MargeTwoWaku(ret);
				}
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

			for (int i = 0; i < pointId.Count - 1; i++)
			{
				for (int j = pointId.Count - 1; j > i; j--)
				{
					double a = (pointList[pointId[j - 1]] - line.start).Norm;
					double b = (pointList[pointId[j]] - line.start).Norm;
					if (a > b) { int t = pointId[j - 1]; pointId[j - 1] = pointId[j]; pointId[j] = t; }
				}
			}

			double dist = 1e-6 * 5;
			for (int i = 0; i < pointId.Count - 1; i++)
			{
				Point a = pointList[pointId[i]];
				Point b = pointList[pointId[i + 1]];
				Point mid = (a + b) / 2;
				Point l = (b - a) * new Point(0, 1);  l /= l.Abs; l *= dist; l += mid;
				Point r = (b - a) * new Point(0, -1); r /= r.Abs; r *= dist; r += mid;
				//右手が壁じゃなかったら辺をはる
				if (!IsWall(dstPoly, srcPoly, r)) { edgeTo[pointId[i]].Add(pointId[i + 1]); }
				if (!IsWall(dstPoly, srcPoly, l)) { edgeTo[pointId[i + 1]].Add(pointId[i]); }
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
			int prevPos = startPointId;
			used[startPointId][startEdgeId] = true;

			while (pos != startPointId)
			{
				cycle.Add(pos);
				int edgeId = getNextEdgeId(prevPos, pos, isPiece);

				if (edgeId < 0)
				{
					System.IO.StreamWriter writer = new System.IO.StreamWriter("margeErrorLog.txt");
					writer.WriteLine(debugDstPoly.Count.ToString());
					for (int id = 0; id < debugDstPoly.Count; id++) {
						writer.WriteLine(debugDstPoly[id].Re.ToString() + " " + debugDstPoly[id].Im.ToString());
					}
					writer.WriteLine(debugSrcPoly.Count.ToString());
					for (int id = 0; id < debugSrcPoly.Count; id++) {
						writer.WriteLine(debugSrcPoly[id].Re.ToString() + " " + debugSrcPoly[id].Im.ToString());
					}
					writer.Close();
					throw new Exception("多角形マージ処理：サイクルを検出できませんでした。");
				}

				used[pos][edgeId] = true;
				prevPos = pos;
				pos = edgeTo[pos][edgeId];
			}
			cycle.Add(startPointId);
			return cycle;
		}

		//cycleがvisual studio 2015のマークみたいな形になっていたら, 複数のサイクルに分解する。
		private List<List<int>> BreakDownCycle(List<int> cycle)
		{
			int[] cnt = new int[pointList.Count];
			for (int i = 0; i < pointList.Count; i++) { cnt[i] = 0; }

			List<List<int>> cycles = new List<List<int>>();
			Stack<int> stack = new Stack<int>();

			for (int i = 0; i < cycle.Count; i++)
			{
				stack.Push(cycle[i]);
				cnt[cycle[i]]++;
				if (cnt[cycle[i]] == 2)  //値cycle[i]がstackに2個入っていたら
				{
					List<int> suzuki = new List<int>();
					suzuki.Add(stack.Peek());
					cnt[stack.Peek()]--;
					stack.Pop();
					while (stack.Peek() != cycle[i]) { suzuki.Add(stack.Peek()); cnt[stack.Peek()]--; stack.Pop(); }
					suzuki.Add(stack.Peek());
					suzuki.Reverse();
					cycles.Add(suzuki);
				}
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

		//サイクルの検出で用いる。次に頂点posから、何番の辺を使って次の頂点に移動するかを探す。
		//なければ-1を返す.
		int getNextEdgeId(int prevPos, int pos, bool isPiece)
		{
			const double PAI = 3.14159265358979;
			int i;

			double minArg = 114514;
			int ret = -1;
			Point a = pointList[prevPos] - pointList[pos];
			a /= a.Abs;

			for (i = 0; i < edgeTo[pos].Count; i++)
			{
				if (used[pos][i]) { continue; }
				if (isPiece) { return i; }
				int nextPos = edgeTo[pos][i];

				Point b = pointList[nextPos] - pointList[pos];
				b /= b.Abs;

				double arg = (b / a).Arg;
				if (arg < 0) { arg += 2 * PAI; }
				if (minArg > arg) { minArg = arg; ret = i; }
			}

			return ret;
		}

		//1頂点で接している2つの枠穴を, 1つの枠穴にする。（接している頂点を起点として、頂点列をマージ）.
		//使用時の前提：2つの枠穴の頂点列の向きは異なる向きになっている。
		List<Poly> MargeTwoWaku(List<Poly> wakus)
		{
			if (wakus[0].Area > 0)
			{
				Poly t = wakus[0];
				wakus[0] = wakus[1];
				wakus[1] = t;
			}

			Dictionary<Point, int> Counts = new Dictionary<Point, int>();
			int i, j, k;

			for (i = 0; i < wakus.Count; i++)
			{
				for (j = 0; j < wakus[i].Count; j++)
				{
					if (!Counts.ContainsKey(wakus[i][j]))
					{
						Counts[wakus[i][j]] = 0;
					}
					Counts[wakus[i][j]]++;
				}
			}

			if (!Counts.ContainsValue(2)) { throw new Exception("共通の頂点が見つかりません。"); }

			Point p = new Point(0, 0);
			foreach (KeyValuePair<Point, int> item in Counts)
			{
				if (item.Value == 2)
				{
					p = item.Key;
					break;
				}
			}

			List<Point> points = new List<Point>();
			for (i = 0; i < wakus.Count; i++)
			{
				for (j = 0; j < wakus[i].Count; j++)
				{
					if (Point.Compare(wakus[i][j], p) == 0)
					{
						break;
					}
				}

				for (k = 0; k < wakus[i].Count; k++)
				{
					points.Add(wakus[i][k + j]);
				}
			}
			points.Add(points[0]);

			Poly poly = new Poly(points, new List<Line>(), false);

			if (poly.Area > 0)
			{
				poly.points.Reverse();
			}
			poly.UpdateMinestPointId();

			List<Poly> ret = new List<Poly>();
			ret.Add(poly);
			return ret;
		}
	}
}
