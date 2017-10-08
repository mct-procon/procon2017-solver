﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLib;
using PuzzleSolver.Core;
using PuzzleSolver.Geometry;
using PuzzleSolver.Network;

namespace PuzzleSolver.UI
{
	public class Controller
	{
		private View view;      //実体. 描画.
		private Solver solve;    //実体. 回答..

		//コンストラクタ
		public Controller() { }
		public Controller(Point t, double scale, int windowSizeX, int windowSizeY)
		{
			view = new View(t, scale, windowSizeX, windowSizeY);
			solve = new Solver();
		}


		// 1問につき、1回実行される
		// ・ビュー（パズルの表示, 強調表示）
		// ・ソルバ（計算）
		// でそれぞれ1つずつのスレッドを立ち上げる。
		// ソルバのViewPuzzleの中身をビューで表示する。
		public void Syakunetsukun(Puzzle initialPuzzle)
		{
			//ソルバのスレッドを立ち上げる
			Task.Run(() => solve.Solve(initialPuzzle));
			//solve.Solve(initialPuzzle);

			//コントロール(表示指令）
			ValueTuple<DX.KeyState, DX.Result> prev_key;
			ValueTuple<DX.KeyState, DX.Result> key;
			int strongDrawPieceId = -1; //強調表示するピースの番号
			int cursor = 0;         //カーソル

			key = DX.GetHitKeyStateAll();
			while (DX.ScreenFlip() == 0 && DX.ProcessMessage() == 0 && DX.ClearDrawScreen() == 0)
			{
				prev_key = key;
				key = DX.GetHitKeyStateAll();

				//キー操作
				if (key.Item1[DX.KeyInput.Escape])
					return;

				if (!prev_key.Item1[DX.KeyInput.NumPadEnter] && key.Item1[DX.KeyInput.NumPadEnter])
				{
					if (cursor + 1 < solve.ViewPuzzles.Count)
					{
						cursor++;
					}
				}

				if (!prev_key.Item1[DX.KeyInput.Back] && key.Item1[DX.KeyInput.Back])
				{
					//表示を1手戻す
					if (cursor > 0)
					{
						cursor--;
					}
				}

				view.UpdateDrawInfo();

				//ヒントが送られてきたら、initialPuzzleと照合してヒントとして与えられたピースを検索し、ソルバを再起動する。
				if (Network.ProconPuzzleService.IsQrCodeReceived /*&& Network.ProconPuzzleService.QrCode.IsHint*/)
				{
					System.Diagnostics.Debug.WriteLine("hinted");
					solve.Cancel();
					System.Threading.Thread.Sleep(100);

					List<Poly> polys = new List<Poly>();
					for (int i = 0; i < Network.ProconPuzzleService.QrCode.Polygons.Count; i++)
					{
						polys.Add(Poly.ParsePolyFromQRCode(Network.ProconPuzzleService.QrCode.Polygons[i], true));
					}
					List<Tuple<int, int>> hints = new List<Tuple<int, int>>();
					for (int i = 0; i < polys.Count; i++)
					{
						Tuple<int, int> hint = GetHint(initialPuzzle, polys[i]);	//(polyId, rotateId)
						if (hint == null) { continue; }
						hints.Add(hint);
					}
					initialPuzzle.UpdateHint(hints);
					cursor = 0;

					Task.Run(() => solve.Solve(initialPuzzle));
				}

				//ViewPuzzles.Count == 0だったら表示に移らない
				if (solve.ViewPuzzles.Count == 0) { continue; }

				//表示したいパズルを渡す
				Puzzle ViewPuzzle = solve.ViewPuzzles[cursor].Clone();

				//最初の, 枠穴の反転, 回転を打ち消す変換をする。
				revertInitialWakuTransform(ViewPuzzle);

				if (Network.ProconPuzzleService.IsPolygonReceived)
				{
					//支援システムから、Webカメラに写った多角形を読み取る。もし読み取れて、かつ、該当する多角形が存在すればViewクラスで表示。
					Poly recivedPiece = Poly.ParsePolyFromQRCode(Network.ProconPuzzleService.Polygon, true, -1);
					int id = CalcPieceId(ViewPuzzle, recivedPiece);
					if (id != -1) { strongDrawPieceId = id; }
				}

				//パズルの表示
				view.Draw(ViewPuzzle);

				//強調表示
				if (strongDrawPieceId != -1)
				{
					view.DrawPieceStrong(ViewPuzzle, strongDrawPieceId, false);
				}
			}
		}

		//ヒントを返す。initialPuzzleと照合する。(多角形番号, pieceTable[polyId][向き番号]の向き番号)を返す。
		private Tuple<int, int> GetHint(Puzzle initialPuzzle, Poly poly)
		{
			int i, j, k;

			for (i = 0; i < initialPuzzle.pieceTable.Count; i++)
			{
				List<Poly> pieceList = initialPuzzle.pieceTable[i];
				for (j = 0; j < pieceList.Count; j++)
				{
					//pieceList[j]とpolyが平行移動で一致できる関係にあるか？
					if (IsMacheByTransrate(pieceList[j], poly))
					{
						return new Tuple<int, int>(i, j);
					}
				}
			}
			return null;
		}

		//poly1とpoly2が平行移動で一致できる関係にあるか？
		private bool IsMacheByTransrate(Poly poly1, Poly poly2)
		{
			if (poly1.points.Count != poly2.points.Count) { return false; }
			if (poly1.points.Count == 0) { return false; }

			Point t = poly1.points[0] - poly2.points[0];
			for (int i = 1; i < poly1.Count; i++)
			{
				if (Point.Compare(t, poly1.points[i] - poly2.points[i]) != 0)
				{
					return false;
				}
			}
			return true;
		}

		//最初の枠穴の反転, 回転を打ち消す変換をする。(表示用データの生成. 計算には用いない！）
		//枠穴と枠線の変換をすればよい。
		void revertInitialWakuTransform(Puzzle puzzle)
		{
			bool TurnFlag = puzzle.wakuInitialTurnFlag;
			Point Mul = puzzle.wakuInitialMul;
			Mul = new Point(1, 0) / Mul;

			for (int i = 0; i < puzzle.wakus.Count; i++)
			{
				if (!puzzle.wakus[i].isExist) { continue; }

				//変換
				puzzle.wakus[i].Mul(Mul, true);
				if (TurnFlag) { puzzle.wakus[i].Turn(true); }
				puzzle.wakus[i].UpdateMinestPointId();
			}

			for (int i = 0; i < puzzle.wakuLines.Count; i++)
			{
				puzzle.wakuLines[i].Mul(Mul);
				if (TurnFlag) { puzzle.wakuLines[i].Turn(); }
			}
		}

		//引数：表示するPuzzle, 支援システムから受け取った多角形
		//戻り値：強調表示すべきピースの番号. 該当するものがなければ-1を返す.
		private int CalcPieceId(Puzzle puzzle, Poly piece)
		{
			List<List<Point>> initPieces = new List<List<Point>>();
			int i, j;

			for (i = 0; i < puzzle.initPieceNum; i++)
			{
				initPieces.Add(new List<Point>());
			}

			for (i = 0; i < puzzle.wakuLines.Count; i++)
			{
				Line line = puzzle.wakuLines[i];
				if (line.initPieceId >= 0)
				{
					initPieces[line.initPieceId].Add(line.start);
				}
			}

			for (i = 0; i < puzzle.initPieceNum; i++)
			{
				if (!puzzle.isPieceExist[i]) { continue; }
				Poly poly = puzzle.pieceTable[i][0];
				for (j = 0; j < poly.lines.Count; j++)
				{
					Line line = poly.lines[j];
					if (line.initPieceId >= 0)
					{
						initPieces[line.initPieceId].Add(line.start);
					}
				}
			}

			double minA = 11451419;
			int ret = -1;
			for (i = 0; i < puzzle.initPieceNum; i++)
			{
				initPieces[i].Add(initPieces[i][0]);
				Poly initPiece = new Poly(initPieces[i], new List<Line>(), true);
				double eval1 = EvalCircleRadiusLog(initPiece.Clone(), piece.Clone());
				double eval2 = EvalSumLengthLog(initPiece, piece.Clone());
				double a = Math.Max(eval1, eval2);

				if (minA > a)
				{
					minA = a;
					ret = i;
				}
			}
			return ret;
		}


		//類似度計算。
		//アルゴリズム：
		//poly1とpoly2を「面積が1」になるように拡大・縮小する。
		//外接円の半径r1, r2をそれぞれ求める。
		//Abs(Log(r1 / r2))を返す。
		private double EvalCircleRadiusLog(Poly poly1, Poly poly2)
		{
			if (Math.Abs(poly1.Area) < 1e-9 || Math.Abs(poly2.Area) < 1e-9)
			{
				return 1145141919;
			}

			Scaling(poly1, 1.0 / Math.Sqrt(Math.Abs(poly1.Area)));
			Scaling(poly2, 1.0 / Math.Sqrt(Math.Abs(poly2.Area)));
			FixPoly(poly1, 0.01);
			FixPoly(poly2, 0.01);

			Circle c1 = poly1.MinestCoverCircle();
			Circle c2 = poly2.MinestCoverCircle();
			double r1 = c1.r;
			double r2 = c2.r;
			if (r1 < 1e-9 || r2 < 1e-9) { return 1145141919; }
			return Math.Abs(Math.Log(r1 / r2));
		}

		//類似度計算2
		//アルゴリズム：
		//poly1とpoly2を「面積が1」になるように拡大・縮小する。
		//周の長さl1, l2をそれぞれ求める。
		//Abs(Log(l1 / l2))を返す。
		private double EvalSumLengthLog(Poly poly1, Poly poly2)
		{
			if (Math.Abs(poly1.Area) < 1e-9 || Math.Abs(poly2.Area) < 1e-9)
			{
				return 1145141919;
			}

			Scaling(poly1, 1.0 / Math.Sqrt(Math.Abs(poly1.Area)));
			Scaling(poly2, 1.0 / Math.Sqrt(Math.Abs(poly2.Area)));
			FixPoly(poly1, 0.01);
			FixPoly(poly2, 0.01);

			int i;
			double sumLength1 = 0;
			double sumLength2 = 0;

			for (i = 0; i < poly1.Count; i++) { sumLength1 += (poly1[i + 1] - poly1[i]).Abs; }
			for (i = 0; i < poly2.Count; i++) { sumLength2 += (poly2[i + 1] - poly2[i]).Abs; }
			if (sumLength1 < 1e-9 || sumLength2 < 1e-9) { return 1145141919; }
			return Math.Abs(Math.Log(sumLength1 / sumLength2));
		}

		//scale倍拡大する。scale < 1なら縮小。原点の座標が(0, 0)になる。
		private void Scaling(Poly poly, double scale)
		{
			poly.Trans(poly.points[0], false);

			//scale倍拡大する。
			for (int i = 0; i < poly.points.Count; i++)
			{
				poly.points[i] *= scale;
			}
		}

		//以下を繰り返す
		//・隣接する3頂点a,b,cで三角形を作ったときに、高さがdist以下だったら、bを取り除く.多角形を更新して、頂点0から再度探索する.
		private void FixPoly(Poly poly, double distError)
		{
			int i;

			while (poly.Count > 3)
			{
				for (i = 0; i < poly.Count; i++)
				{
					Point a = poly[i - 1];
					Point b = poly[i];
					Point c = poly[i + 1];
					double area = Math.Abs(Point.Cross(c - a, b - a));
					if ((c - a).Abs < 1e-9) { return; }
					double dist = area / (c - a).Abs;
					if (dist <= distError)
					{
						break;
					}
				}
				if (i == poly.Count) { break; }

				List<Point> newPoints = new List<Point>();
				int id = i;
				for (i = 0; i < poly.Count; i++)
				{
					if (i == id) { continue; }
					newPoints.Add(poly.points[i]);
				}
				newPoints.Add(newPoints[0]);

				poly = new Poly(newPoints, new List<Line>(), true);
			}
		}
	}
}