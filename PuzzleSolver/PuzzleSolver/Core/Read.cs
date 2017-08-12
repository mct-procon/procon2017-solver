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
		private Backup History;      //参照. パズルの集合

		//コンストラクタ
		public Read() { }
		public Read(Backup backup) { History = backup; }

		//Historyを全消去する. その後,
		//ファイルからパズルを読み込んで, Historyに追加する。
		public void ReadFile(string fileName)
		{
			History.Clear();
			try
			{
				StreamReader reader = new StreamReader(fileName);
				int n;
				string s;
				Puzzle puzzle = new Puzzle();

				//枠
				s = ReadLine(reader);
				if (s == null) { return; }
				n = int.Parse(s);
				for (int i = 0; i < n; i++)
				{
					Poly poly = ReadPoly(reader, false);
					if (poly == null) { return; }
                    puzzle.wakus.Add(poly);
				}

				//ピース
				s = ReadLine(reader);
				if (s == null) { return; }
				n = int.Parse(s);
				for (int i = 0; i < n; i++)
				{
					Poly poly = ReadPoly(reader, true);
					if (poly == null) { return; }
					puzzle.pieces.Add(poly);
				}

				//パズルを突っ込む
				History.Push(puzzle);

				reader.Close();
			}
			catch { DX.WriteLineDx("file open error"); }
		}

		//多角形を読み込んで返す. エラー時はnullを返す.
		private Poly ReadPoly(StreamReader reader, bool isPiece)
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
				List<int> values = s.Split(' ').Select(x=>int.Parse(x)).ToList();
				if (values.Count != 2) { return null; }
				Point point = new Point((double)values[0], (double)values[1]);
				points.Add(point);
			}

			//リング形式にする
			points.Add(points[0]);

			//表示する線分の設定
			for (int i = 0; i < points.Count - 1; i++)
			{
				lines.Add(new Line(points[i], points[i + 1]));
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
