using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PuzzleSolver.Geometry;
using DxLib;

namespace PuzzleSolver.Core
{
	public class Read
	{
		//コンストラクタ
		public Read() { }


		//QRコードから読み取ったデータをPuzzle型に変換する.
		public Puzzle ReadFromQRCode(Procon2017MCTProtocol.QRCodeData QRCode)
		{
			Puzzle puzzle = new Puzzle(new List<Poly>(), new List<Poly>(), new List<Line>());
			int i, j;

			//枠
			for (i = 0; i < QRCode.Frames.Count; i++)
			{
				puzzle.wakus.Add(ParsePolyFromQRCode(QRCode.Frames[i], false, -1));
			}

			//ピース
			for (i = 0; i < QRCode.Polygons.Count; i++)
			{
				puzzle.pieces.Add(ParsePolyFromQRCode(QRCode.Polygons[i], true, (sbyte)i));
			}

			//枠辺
			for (i = 0; i < puzzle.wakus.Count; i++)
			{
				for (j = 0; j < puzzle.wakus[i].Count; j++)
				{
					puzzle.wakuLines.Add(new Line(puzzle.wakus[i].points[j], puzzle.wakus[i].points[j + 1], -1));
				}
			}

			//初期ピース数, 盤面評価値, 盤面ハッシュ
			puzzle.setInitPieceNum(puzzle.pieces.Count);
			puzzle.setBoardScore(0);
			puzzle.setBoardHash();
			return puzzle;
		}

		/// <param name="fileName">fairu</param>
		/// <returns></returns>
		public Puzzle ReadFile(string fileName)
		{
			StreamReader reader = new StreamReader(fileName);
			int n;
			string s;
			Puzzle puzzle = new Puzzle(new List<Poly>(), new List<Poly>(), new List<Line>());

			//枠
			s = ReadLine(reader);
			if (s == null) { return null; }
			n = int.Parse(s);
			for (int i = 0; i < n; i++)
			{
				Poly poly = ReadPoly(reader, false);
				if (poly == null) { return null; }

				//時計回りの頂点列にする
				if (poly.Area > 0) { poly.points.Reverse(); }

				puzzle.wakus.Add(poly);
			}

			//ピース
			s = ReadLine(reader);
			if (s == null) { return null; }
			n = int.Parse(s);
			for (int i = 0; i < n; i++)
			{
				Poly poly = ReadPoly(reader, true, (sbyte)i);
				if (poly == null) { return null; }

				//反時計回りの頂点列にする
				if (poly.Area < 0) { poly.points.Reverse(); }

				puzzle.pieces.Add(poly);
			}

			//枠辺
			for (int i = 0; i < puzzle.wakus.Count; i++)
			{
				for (int j = 0; j < puzzle.wakus[i].Count; j++)
				{
					puzzle.wakuLines.Add(new Line(puzzle.wakus[i].points[j], puzzle.wakus[i].points[j + 1], -1));
				}
			}

			//初期ピース数, 盤面評価値, 盤面ハッシュ
			puzzle.setInitPieceNum(puzzle.pieces.Count);
			puzzle.setBoardScore(0);
			puzzle.setBoardHash();
			reader.Close();

			return puzzle;
		}

		private Poly ParsePolyFromQRCode(Procon2017MCTProtocol.SendablePolygon polygon, bool isPiece, sbyte initPieceId = -1)
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

			return new Poly(points, lines, isPiece);
		}

		//多角形を読み込んで返す. エラー時はnullを返す.
		private Poly ReadPoly(StreamReader reader, bool isPiece, sbyte initPieceId = -1)
		{
			string s = ReadLine(reader);
			if (s == null) { return null; }
			int n = int.Parse(s);
			List<Point> points = new List<Point>();
			List<Line> lines = new List<Line>();

			//n頂点分のデータを読み込む
			for (int i = 0; i < n; i++)
			{
				s = ReadLine(reader);
				if (s == null) { return null; }
				List<double> values = s.Split(' ').Select(x=>double.Parse(x)).ToList();
				if (values.Count != 2) { return null; }
				Point point = new Point(values[0], values[1]);
				points.Add(point);
			}

			//リング形式にする
			points.Add(points[0]);

			//表示する線分の設定 (ピースのみ)
			if (isPiece)
			{
				for (int i = 0; i < points.Count - 1; i++)
				{
					lines.Add(new Line(points[i], points[i + 1], initPieceId));
				}
			}

			return new Poly(points, lines, isPiece);
		}

		//空文字ではない行を返す. エラー時はnullを返す.
		private string ReadLine(StreamReader reader)
		{
			string s;
			while (!reader.EndOfStream)
			{
				s = reader.ReadLine();
				if (s.Length == 0) { continue; }
				return s;
			}
			return null;
		}
	}
}
