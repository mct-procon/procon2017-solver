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
		StreamWriter ReadLog = new StreamWriter("ReadLog.txt");

		//コンストラクタ
		public Read() { }

		//QRコードから読み取ったデータをPuzzle型に変換する.
		public Puzzle ReadFromQRCode(Procon2017MCTProtocol.QRCodeData QRCode)
		{
			List<Poly> wakus = new List<Poly>();
			List<Line> wakuLines = new List<Line>();
			List<List<Poly>> pieceTable = new List<List<Poly>>();

			//枠
			for (int i = 0; i < QRCode.Frames.Count; i++)
			{
				Poly poly = Poly.ParsePolyFromQRCode(QRCode.Frames[i], false, -1);
				poly.UpdateMinestPointId();
				wakus.Add(poly);
			}

			//ピース
			for (int i = 0; i < QRCode.Polygons.Count; i++)
			{
				Poly piece = Poly.ParsePolyFromQRCode(QRCode.Polygons[i], true, (sbyte)i);
				pieceTable.Add(GetPieceList(piece));
			}

			//枠辺
			for (int i = 0; i < wakus.Count; i++)
			{
				for (int j = 0; j < wakus[i].Count; j++)
				{
					wakuLines.Add(new Line(wakus[i].points[j], wakus[i].points[j + 1], -1));
				}
			}

			Puzzle puzzle = new Puzzle(wakus, wakuLines, pieceTable, 0, pieceTable.Count);

			//盤面評価値, 盤面ハッシュ
			puzzle.setBoardScore(0);
			puzzle.setBoardHash();
			ReadLog.Close();

			return puzzle;
		}

		//ファイルからパズルを読み込んで返す。
		public Puzzle ReadFile(string fileName)
		{
			StreamReader reader = new StreamReader(fileName);
			int n;
			string s;

			List<Poly> wakus = new List<Poly>();
			List<Line> wakuLines = new List<Line>();
			List<List<Poly>> pieceTable = new List<List<Poly>>();
			
			//枠
			s = ReadLine(reader);
			if (s == null) { return null; }
			n = int.Parse(s);
			for (int i = 0; i < n; i++)
			{
				Poly poly = ReadPoly(reader, false);
				if (poly == null || poly.Count < 3) { return null; }

				//時計回りの頂点列にする
				if (poly.Area > 0) { poly.points.Reverse(); }

				poly.UpdateMinestPointId();
				wakus.Add(poly);
			}

			//ピース
			s = ReadLine(reader);
			if (s == null) { return null; }
			n = int.Parse(s);
			for (int i = 0; i < n; i++)
			{
				Poly poly = ReadPoly(reader, true, (sbyte)i);
				if (poly == null || poly.Count < 3) { return null; }

				//反時計回りの頂点列にする
				if (poly.Area < 0) { poly.points.Reverse(); }

				List<Poly> pieceList = GetPieceList(poly);
				pieceTable.Add(pieceList);
			}

			//枠辺
			for (int i = 0; i < wakus.Count; i++)
			{
				for (int j = 0; j < wakus[i].Count; j++)
				{
					wakuLines.Add(new Line(wakus[i].points[j], wakus[i].points[j + 1], -1));
				}
			}

			Puzzle puzzle = new Puzzle(wakus, wakuLines, pieceTable, 0, pieceTable.Count);

			//盤面評価値, 盤面ハッシュ
			puzzle.setBoardScore(0);
			puzzle.setBoardHash();
			reader.Close();
			ReadLog.Close();

			return puzzle;
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

		//Validな(回転, 反転)方法をすべて返す
		private List<Poly> GetPieceList(Poly piece)
		{
			List<Poly> res = GetRotatedPieceList(piece);
			List<Poly> ret = new List<Poly>(res);

			for (int i = 0; i < res.Count; i++)
			{
				Poly poly = res[i].Clone();
				poly.Turn(true);
				ret.Add(poly);
			}
			return ret;
		}

		//Validな回転方法 (すべての点座標が整数になる原点中心の回転をしたあとのピース）をすべて返す
		private List<Poly> GetRotatedPieceList(Poly piece)
		{
			for (int k = 0; k < piece.Count; k++)
			{
				Point p = piece.points[k + 1] - piece.points[k];
			}

			List<Point> muls = GetRotateCandidate(piece.points[1] - piece.points[0]);
			List<Poly> ret = new List<Poly>();
			int i, j;
			double AllowError = 1e-8;

			for (i = 0; i < muls.Count; i++)
			{
				Point mul = muls[i];
				Poly poly = piece.Clone();

				//piece.points[0]を中心に回転する！！！！（重要なので(ry)
				//rotation center must be [piece.points[0]], not (0, 0). (that is important)
				Point p = poly.points[0];
				poly.Trans(p * (-1), true);
				poly.Mul(mul, true);
				poly.Trans(p, true);

				for (j = 0; j < poly.points.Count; j++)
				{
					int Re = GetMinErrorInteger(poly[j].Re);
					int Im = GetMinErrorInteger(poly[j].Im);
					if (Math.Abs(poly[j].Re - Re) > AllowError || Math.Abs(poly[j].Im - Im) > AllowError)
					{
						break;
					}
					poly.points[j] = new Point((double)Re, (double)Im);
				}
				poly.UpdateMinestPointId();

				if (j == poly.points.Count)
				{
					ret.Add(poly);
				}
			}
			return ret;
		}

		//回転の候補を返す
		private List<Point> GetRotateCandidate(Point vec)
		{
			int r = (int)(vec.Abs);
			List<Point> ret = new List<Point>();

			for (int x = -r; x <= r; x++)
			{
				double y = Math.Sqrt(vec.Norm - x * x);
				int yi = GetMinErrorInteger(y);
				if (Math.Abs(y - yi) < 1e-9)
				{
					Point p1 = new Point((double)x, (double)yi) / vec;
					ret.Add(p1);
					if (Math.Abs(yi) > 0)
					{
						Point p2 = new Point((double)x, (double)-yi) / vec;
						ret.Add(p2);
					}
				}
			}
			return ret;
		}

		//|X - ret|が最小になるretを返す. 複数retが考えられる場合は小さい方を返す.
		private int GetMinErrorInteger(double X)
		{
			int ret = (int)X - 1;
			for (int i = 0; i <= 1; i++)
			{
				if (Math.Abs(X - ret) > Math.Abs(X - ((int)X + i)))
				{
					ret = (int)X + i;
				}
			}
			return ret;
		}
	}
}
