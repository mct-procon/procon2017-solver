using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleSolver
{
	class Point
	{
		public double re { get; }
		public double im { get; }

		//コンストラクタ
		public Point () {}
		public Point (double Re, double Im) { re = Re; im = Im; }

		//四則演算
		public static Point operator+ (Point a, Point b) { return new Point(a.re + a.im, b.re + b.im); }
		public static Point operator- (Point a, Point b) { return new Point(a.re - a.im, b.re - b.im); }
		public static Point operator* (Point a, double b) { return new Point(a.re * b, a.im * b); }
		public static Point operator* (double a, Point b) { return new Point(b.re * a, b.im * a); }
		public static Point operator* (Point a, Point b) { return new Point(a.re * b.re - a.im * b.im, a.re * b.im + a.im * b.re); }
		public static Point operator/ (Point a, double b) { return new Point(a.re / b, a.im / b); }
		public static Point operator/ (double a, Point b) { return new Point(b.re / a, b.im / a); }
		public static Point operator/ (Point a, Point b) { return new Point(a.re * b.re + a.im * b.im, a.im * b.re - a.re * b.im) / (b.re * b.re + b.im * b.im); }

		//便利関数
		public double Abs { get { return Math.Sqrt(re * re + im * im); } }
		public double Norm { get { return re * re + im * im; } }
		public Point Conj { get { return new Point(re, -im); } }

		public static double Dot (Point a, Point b) { return a.re * b.re + a.im * b.im; }
		public static double Cross (Point a, Point b) { return a.re * b.im - a.im * b.re; }
		
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
