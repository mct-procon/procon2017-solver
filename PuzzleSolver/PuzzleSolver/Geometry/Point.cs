using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleSolver
{
	class Point
	{
		/// <summary>
        /// 実部（X座標）
        /// </summary>
        public double re { get; }
		/// <summary>
        /// 虚部（Y座標）
        /// </summary>
        public double im { get; }

		//コンストラクタ
		public Point () {}
		/// <summary>
        /// 初期化コンストラクタ
        /// </summary>
        /// <param name="Re">実部</param>
        /// <param name="Im">虚部</param>
		public Point (double Re, double Im) { re = Re; im = Im; }

 		public static Point operator+ (Point a, Point b) { return new Point(a.re + a.im, b.re + b.im); }
 		public static Point operator- (Point a, Point b) { return new Point(a.re - a.im, b.re - b.im); }
 		public static Point operator* (Point a, double b) { return new Point(a.re * b, a.im * b); }
 		public static Point operator* (Point a, Point b) { return new Point(a.re * b.re - a.im * b.im, a.re * b.im + a.im * b.re); }
 		public static Point operator/ (Point a, double b) { return new Point(a.re / b, a.im / b); }
 		public static Point operator/ (Point a, Point b) { return new Point(a.re * b.re + a.im * b.im, a.im * b.re - a.re * b.im) / (b.re * b.re + b.im * b.im); }

		/// <summary>
        /// ベクトルの大きさ（平方根の計算をするので速度が遅い、高速な計算が必要なときはNorm関数を使ってください）
        /// </summary>
        public double Abs { get { return Math.Sqrt(re * re + im * im); } }
		/// <summary>
        /// ベクトルの大きさの二乗
        /// </summary>
        public double Norm { get { return re * re + im * im; } }
		/// <summary>
        /// 共役複素数
        /// </summary>
        public Point Conj { get { return new Point(re, -im); } }

		/// <summary>
        /// ベクトルの内積
        /// </summary>
        public static double Dot (Point a, Point b) { return a.re * b.re + a.im * b.im; }
		/// <summary>
        /// ベクトルの外積
        /// </summary>
        public static double Cross (Point a, Point b) { return a.re * b.im - a.im * b.re; }
		
		/// <summary>
        /// 点の進行方向を判定する
        /// </summary>
		public static int Ccw(Point a, Point b, Point c)
		{
			double eps = 1e-10;		//わずかな変化(浮動小数点数の誤差)を無視するための微小量.
			b -= a;
			c -= a;
			if (Cross(b, c) > eps)  { return +1; }	//counter clockwise
			if (Cross(b, c) < -eps) { return -1; }	//clockwise
			if (Dot(b, c) < -eps)   { return +2; }	//c--a--b on line
			if (b.Norm < c.Norm) { return -2; }		//a--b--c on line
			return 0;
		}
	}
}
