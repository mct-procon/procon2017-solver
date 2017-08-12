using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleSolver.Geometry
{
    /// <summary>
    /// 点
    /// </summary>
    public struct Point
    {
        /// <summary>
        /// 実部（X座標）
        /// </summary>
        public double Re;
        /// <summary>
        /// 虚部（Y座標）
        /// </summary>
        public double Im;

        /// <summary>
        /// 初期化コンストラクタ
        /// </summary>
        /// <param name="Re">実部</param>
        /// <param name="Im">虚部</param>
        public Point (double Re, double Im) { this.Re = Re; this.Im = Im; }

        public static Point operator+ (Point a, Point b) => new Point(a.Re + b.Re, a.Im + b.Im);
        public static Point operator- (Point a, Point b) => new Point(a.Re - b.Re, a.Im - b.Im);
        public static Point operator* (Point a, double b) => new Point(a.Re * b, a.Im * b);
        public static Point operator* (double a, Point b) => new Point(b.Re * a, b.Im * a);
        public static Point operator* (Point a, Point b) => new Point(a.Re * b.Re - a.Im * b.Im, a.Re * b.Im + a.Im * b.Re);
        public static Point operator/ (Point a, double b) => new Point(a.Re / b, a.Im / b);
        public static Point operator/ (double a, Point b) => new Point(b.Re / a, b.Im / a);
        public static Point operator/ (Point a, Point b) => new Point(a.Re * b.Re + a.Im * b.Im, a.Im * b.Re - a.Re * b.Im) / (b.Re * b.Re + b.Im * b.Im);

        /// <summary>
        /// ベクトルの大きさ（平方根の計算をするので速度が遅い、高速な計算が必要なときはNorm関数を使ってください）
        /// </summary>
        public double Abs => Math.Sqrt(Re * Re + Im * Im);
        /// <summary>
        /// ベクトルの大きさの二乗
        /// </summary>
        public double Norm => Re * Re + Im * Im;
        /// <summary>
        /// 共役複素数
        /// </summary>
        public Point Conj => new Point(Re, -Im);

        /// <summary>
        /// ベクトルの内積
        /// </summary>
        public static double Dot (Point a, Point b) => a.Re * b.Re + a.Im * b.Im;
        /// <summary>
        /// ベクトルの外積
        /// </summary>
        public static double Cross(Point a, Point b) => a.Re * b.Im - a.Im * b.Re;
        
        /// <summary>
        /// 点の進行方向を判定する
        /// </summary>
        public static int Ccw(Point a, Point b, Point c)
        {
            double eps = 1e-10;        //わずかな変化(浮動小数点数の誤差)を無視するための微小量.
            b -= a;
            c -= a;
            if (Cross(b, c) > eps)  { return +1; }    //counter clockwise
            if (Cross(b, c) < -eps) { return -1; }    //clockwise
            if (Dot(b, c) < -eps)   { return +2; }    //c--a--b on line
            if (b.Norm < c.Norm) { return -2; }        //a--b--c on line
            return 0;
        }
    }
}
