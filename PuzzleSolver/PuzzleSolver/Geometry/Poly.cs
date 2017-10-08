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
        public List<Point> points { get; set; }
        /// <summary>
        /// 線分の集合. 実体.
        /// </summary>
        public List<Line> lines { get; private set; }
        public bool isPiece;
        public bool isExist;
		public bool isPositionDetected { get; private set; }

        //コンストラクタ
        public Poly() { }
        public Poly(List<Point> points, List<Line> lines, bool isPiece, bool isPositionDetected)
        {
            this.points  = points;
            this.lines   = lines;
            this.isPiece = isPiece;
            this.isExist = true;
			this.isPositionDetected = isPositionDetected;
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
			List<Point> points = SizingPoly();
			for (int i = 0; i < Count; i++)
            {
                Line line1 = new Line(points[i], points[i + 1]);
                for (int j = 0; j < poly.Count; j++)
                {
                    Line line2 = new Line(poly.points[j], poly.points[j + 1]);
                    if (Line.IsHit(line1, line2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

		//多角形の縮小サイジング処理 (当たり判定で内部的に使う)
		public List<Point> SizingPoly()
		{
			List<Point> ret = new List<Point>();
			double dist = 1e-2;

			for (int i = 0; i < Count; i++)
			{
				Point a = (i == 0) ? points[Count - 1] : points[i - 1];
				Point b = points[i];
				Point c = points[i + 1];

				if (Point.Ccw(a, b, c) > 0)
				{
					ret.Add(b + ((a - b) / (a - b).Abs + (c - b) / (c - b).Abs) * dist);
				}
				else
				{
					ret.Add(b + ((b - a) / (b - a).Abs + (b - c) / (b - c).Abs) * dist);
				}
			}
			ret.Add(ret[0]);
			return ret;
		}

        //点pointが多角形thisの内部に含まれるか？ (境界はfalse)
        public bool isCover(Point point) {
            bool flag = false;
            for (int i = 0; i < Count; i++) {
                Point a = points[i] - point;
                Point b = points[i + 1] - point;
                if (a.Im > b.Im) { Point t = a; a = b; b = t; }
                if (a.Im <= 0 && 0 < b.Im && Point.Cross(a, b) > 0) { flag = !flag; }
                if (Math.Abs(Point.Cross(a, b)) == 0 && Point.Dot(a, b) <= 0) { return false; }
            }
            return flag;
        }

		//点列の場所が確定
		public void DetectPosition()
		{
			isPositionDetected = true;
		}

        //平行移動
        public void Trans(Point t, bool isUpdateLines)
        {
            for (int i = 0; i < points.Count; i++) { points[i] += t; }
            if (isUpdateLines) { for (int i = 0; i < lines.Count; i++) { lines[i].Trans(t); } }
        }

        //反転 (軸はy = 0)
        public void Turn(bool isUpdateLines)
        {
            for (int i = 0; i < points.Count; i++) { points[i] = points[i].Conj; }

            int l = 0, r = points.Count - 1;
            while (l < r) { Point t = points[l]; points[l] = points[r]; points[r] = t; l++; r--; }

            if (isUpdateLines) { for (int i = 0; i < lines.Count; i++) { lines[i].Turn(); } }
        }

        //乗算 (回転)
        public void Mul (Point mulValue, bool isUpdateLines)
        {
            for (int i = 0; i < points.Count; i++) { points[i] = points[i] * mulValue; }
            if (isUpdateLines) { for (int i = 0; i < lines.Count; i++) { lines[i].Mul(mulValue); } }
        }

        //インデクサ
        //(可毒性と速さが微粒子レベルで下がるので, 負の添え字[-1]にアクセスしたい！というとき以外はあまり使わないでください。）
        //8月15日の時点では, Solveクラス, 本クラスの結合度計算のみで使用しています.
        public Point this[int i]
        {
            set { i %= Count; if (i < 0) { i += Count; } this.points[i] = value; }
            get { i %= Count; if (i < 0) { i += Count; } return this.points[i]; }
        }

		//頂点の角度（インデクサ使用）
		public double GetAngleDegree(int id)
		{
			const double PAI = 3.14159265358979;
			Point a = this[id - 1] - this[id];
			Point b = this[id + 1] - this[id];
			Point rot = (b / a);
			double angle = Math.Atan2(rot.Im, rot.Re);
			if (angle < 0) angle += 2 * PAI;
			angle *= 180 / PAI;
			return angle;
		}

		//結合度計算
		//ピース同士ならO(N + M), 枠にピースを入れる場合は枠をM角形としてO(NM) (定数軽い)
		public static int Evaluation(Poly dstPoly, Poly srcPoly, int dstPointId, int srcPointId)
		{
			if (dstPoly.isPiece) { return EvaluationSub(dstPoly, srcPoly, dstPointId, srcPointId); }
			else
			{
				double eps = 1e-10;
				bool[] calced = new bool[srcPoly.Count];
				List<Tuple<Point, int>> pointList = new List<Tuple<Point, int>>();
				int score = 0;

				for (int i = 0; i < srcPoly.Count; i++) { calced[i] = false; }
				for (int i = 0; i < dstPoly.Count; i++) {
					pointList.Add(new Tuple<Point, int>(dstPoly.points[i], i));
				}
				pointList.Sort((a, b) => Point.Compare(a.Item1, b.Item1));

				for (int i = 0; i < srcPoly.Count; i++)
				{
					if (calced[i]) { continue; }

					int st = 0, ed = dstPoly.Count, mid;	//oooxxxx
					while (ed - st >= 2)
					{
						mid = (st + ed) / 2;
						if (Point.Compare(pointList[mid].Item1, srcPoly.points[i]) <= 0)
						{
							st = mid;
						}
						else
						{
							ed = mid;
						}
					}
					if (st == dstPoly.Count || (srcPoly.points[i] - pointList[st].Item1).Norm > eps) { continue; }

					//探索
					score += EvaluationSub(dstPoly, srcPoly, pointList[st].Item2, i, calced);
				}
				return score;
			}
		}

        //結合度計算のサブ（接している連続した部分1つについて、結合度を計算）
        //多角形dstPolyの頂点dstPointId, 多角形srcPolyの頂点srcPointIdが同じ位置にある。
        private static int EvaluationSub(Poly dstPoly, Poly srcPoly, int dstPointId, int srcPointId, bool[] calced = null)
        {
            const double eps = 1e-10;
            int d = dstPointId;
            int s = srcPointId;
            int[] count = { 0, 0, 0, 0 };   //count = {一致辺の個数, 始点と方向は一致してるが一致辺ではない辺の個数, 180°角の個数, 360°角の個数};
            int[] weight = { 4, 1, 2, 3 };  //weight…各項目の点数重み (定数)
            Point a, b;

			//走査1
			int counter = 0;
            while ((dstPoly[d] - srcPoly[s]).Norm <= eps && counter < dstPoly.Count) { if (calced != null) calced[toIndex(s, srcPoly.Count)] = true; d++; s--; counter++; }

			//例外処理
			if (counter == dstPoly.Count) { return dstPoly.Count * weight[0]; }	//完全一致
			count[0] += d - dstPointId - 1;

            a = dstPoly[d] - dstPoly[d - 1];
            b = srcPoly[s] - srcPoly[s + 1];
            if (Math.Abs(Point.Cross(a, b)) <= eps) { //平行
                if (Point.Dot(a, b) >= 0) { count[1]++; }
                else { count[2]++; }
            }

            //走査2
            d = dstPointId; s = srcPointId;
            while ((dstPoly[d] - srcPoly[s]).Norm <= eps) { if (calced != null) calced[toIndex(s, srcPoly.Count)] = true; d--; s++; }
            count[0] += s - srcPointId - 1;

            a = dstPoly[d] - dstPoly[d + 1];
            b = srcPoly[s] - srcPoly[s - 1];
            if (Math.Abs(Point.Cross(a, b)) <= eps) { //平行
                if (Point.Dot(a, b) >= 0) { count[1]++; }
                else { count[2]++; }
            }

            //角の個数は, count[0] + count[1] - 1で導出できる
            count[3] = count[0] + count[1] - 1;

            int score = 0;
            for (int i = 0; i < 4; i++) { score += weight[i] * count[i]; }
            return score;
        }

		//インデックス変換. 結合度計算で内部的に使う
		private static int toIndex(int i, int Count)
		{
			i %= Count; if (i < 0) { i += Count; }
			return i;
		}

        //クローン (深いコピー)
        public Poly Clone()
        {
            Poly ret = new Poly(new List<Point>(points), new List<Line>(lines), isPiece, isPositionDetected);
            for (int i = 0; i < ret.lines.Count; i++) { ret.lines[i] = ret.lines[i].Clone(); }
            ret.isPiece = this.isPiece;
            ret.isExist = this.isExist;
            return ret;
        }

		//型変換
		public static Poly ParsePolyFromQRCode(Procon2017MCTProtocol.SendablePolygon polygon, bool isPiece, sbyte initPieceId = -1)
		{
			int i;
			List<Point> points = new List<Point>();
			List<Line> lines = new List<Line>();

			for (i = 0; i < polygon.Points.Count; i++)
			{
				points.Add(new Point(polygon.Points[i].X, polygon.Points[i].Y));
			}
			points.Add(points[0]);

			//表示する線分の設定 (ピースのみ)
			if (isPiece)
			{
				for (i = 0; i < points.Count - 1; i++)
				{
					lines.Add(new Line(points[i], points[i + 1], initPieceId));
				}
			}

			Poly poly = new Poly(points, lines, isPiece, false);

			if (isPiece && poly.Area < 0) { poly.points.Reverse(); }
			if (!isPiece && poly.Area > 0) { poly.points.Reverse(); }
			return poly;
		}


		//最小包含円の半径. ランダムソートしておけば、期待計算量O(N), 最悪O(N^3). Nは頂点数.
		//http://tubo28.me/algorithm/minball/ を参考に作成。
		public Circle MinestCoverCircle()
		{
			Circle c = Circle.MakeCircle2(points[0], points[1]);

			for (int i = 2; i < Count; i++)
			{
				if (!c.IsInCircle(points[i]))
				{
					c = Circle.MakeCircle2(points[0], points[i]);
					for (int j = 0; j < i; j++)
					{
						if (!c.IsInCircle(points[j]))
						{
							c = Circle.MakeCircle2(points[i], points[j]);
							for (int k = 0; k < j; k++)
							{
								if (!c.IsInCircle(points[k]))
								{
									c = Circle.MakeCircle3(points[i], points[j], points[k]);
								}
							}
						}
					}
				}
			}
			return c;
		}

		//偏角
		public double Arg(int id)
		{
			Point a = this[id + 1] - this[id];
			Point b = this[id - 1] - this[id];
			Point mul = b / a;
			return Math.Atan2(mul.Im, mul.Re);
		}
	}
}
