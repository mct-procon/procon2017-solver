using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuzzleSolver.Geometry;

namespace PuzzleSolver.Geometry
{
	public class Circle
	{
		public Point c { get; }
		public double r { get; }

		public Circle(Point c, double r)
		{
			this.c = c;
			this.r = r;
		}

		public static Circle MakeCircle3(Point a, Point b, Point c)
		{
			double A = (b - c).Norm;
			double B = (c - a).Norm;
			double C = (a - b).Norm;
			double S = Point.Cross(b - a, c - a);
			Point p = (A * (B + C - A) * a + B * (C + A - B) * b + C * (A + B - C) * c) / (4 * S * S);
			double r = (p - a).Abs;
			return new Circle(p, r);
		}

		public static Circle MakeCircle2(Point a, Point b)
		{
			Point c = (a + b) / 2;
			double r = (c - a).Abs;
			return new Circle(c, r);
		}

		public bool IsInCircle(Point p)
		{
			double eps = 1e-10;
			return (p - c).Norm <= this.r * this.r + eps;
		}
	}
}