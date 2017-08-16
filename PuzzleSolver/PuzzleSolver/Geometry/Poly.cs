using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuzzleSolver.Geometry;

namespace PuzzleSolver.Geometry
{
    public class Poly : ICloneable
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

        //コンストラクタ
        public Poly() { }
        public Poly(List<Point> points, List<Line> lines, bool isPiece)
        {
            this.points  = points;
            this.lines   = lines;
            this.isPiece = isPiece;
            this.isExist = true;
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
                    Line line2 = new Line(poly.points[j], poly.points[j + 1]);
                    if (Line.IsHit(line1, line2))
                    {
                        return true;
                    }
                }
            }
            return false;
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


        //結合度計算
        //多角形dstPolyの頂点dstPointId, 多角形srcPolyの頂点srcPointIdが同じ位置にある。
        public static int Evaluation(Poly dstPoly, Poly srcPoly, int dstPointId, int srcPointId)
        {
            const double eps = 1e-10;
            int d = dstPointId;
            int s = srcPointId;
            int[] count = { 0, 0, 0, 0 };   //count = {一致辺の個数, 始点と方向は一致してるが一致辺ではない辺の個数, 180°角の個数, 360°角の個数};
            int[] weight = { 4, 1, 2, 3 };  //weight…各項目の点数重み (定数)
            Point a, b;

            //走査1
            while ((dstPoly[d] - srcPoly[s]).Norm <= eps) { d++; s--; }
            count[0] += d - dstPointId - 1;

            a = dstPoly[d] - dstPoly[d - 1];
            b = srcPoly[s] - srcPoly[s + 1];
            if (Math.Abs(Point.Cross(a, b)) <= eps) { //平行
                if (Point.Dot(a, b) >= 0) { count[1]++; }
                else { count[2]++; }
            }

            //走査2
            d = dstPointId; s = srcPointId;
            while ((dstPoly[d] - srcPoly[s]).Norm <= eps) { d--; s++; }
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

        //クローン (深いコピー)
        public Poly Clone()
        {
            Poly ret = new Poly();
            ret.lines = new List<Line>(this.lines);
            ret.points = new List<Point>(this.points);
            for (int i = 0; i < ret.lines.Count; i++) { ret.lines[i] = ret.lines[i].Clone(); }
            ret.isPiece = this.isPiece;
            ret.isExist = this.isExist;
            return ret;
        }

        /// <summary>
        /// Deep Clone. (Called by ICloneable.Clone())
        /// </summary>
        /// <returns>Clone Object</returns>
        object ICloneable.Clone() {
            return Clone();
        }
    }
}
