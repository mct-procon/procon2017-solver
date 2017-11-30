using System;
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
		//
		//返り値：True：正常終了
		public bool Syakunetsukun(Puzzle initialPuzzle)
		{
			//ソルバーを初期化（ログを消すなど）
			solve = new Solver();

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
				if (!prev_key.Item1[DX.KeyInput.Escape] && key.Item1[DX.KeyInput.Escape])
					return true;

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

				//ViewPuzzles.Count == 0だったら表示に移らない
				if (solve.ViewPuzzles.Count == 0) { continue; }

				//表示したいパズルを渡す
				Puzzle ViewPuzzle = solve.ViewPuzzles[cursor].Clone();
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
			return false;
		}

		//配置情報の多角形poly, 形状情報initialPuzzleが与えられるので、何番のピースが与えられたかを特定せよ。
		//ピース番号, dstPointId, srcPointId, turnflagをTuple<int, int, int, bool> で返す。
		//どれにも当てはまらない場合はnullを返すこと。これは支援システムと違って、かなり厳密に動作する。
		private Tuple<int, int, int, bool> DetectPiece(Puzzle initialPuzzle, Poly poly)
		{
			for (int i = 0; i < initialPuzzle.initPieceNum; i++)
			{
				Poly srcPoly = initialPuzzle.pieces[i].Clone();
				if (srcPoly.Count != poly.Count) { continue; }
				if (Math.Abs(srcPoly.Area - poly.Area) >= 0.5) { continue; }
				for (int dstPointId = 0; dstPointId < poly.Count; dstPointId++)
				{
					double dArg = poly.Arg(dstPointId);
					if (dArg < 0) { dArg += 2 * Math.PI; }
					for (int srcPointId = 0; srcPointId < srcPoly.Count; srcPointId++)
					{
						double sArg = srcPoly.Arg(srcPointId);
						if (sArg < 0) { sArg += 2 * Math.PI; }
						if (Math.Abs(dArg - sArg) * 180 / Math.PI > 0.1) { continue; }  //枝刈り

						//実際に動かしてみる
						move(poly, srcPoly, dstPointId, srcPointId, false, false);
						if (IsSamePoly(poly, srcPoly, dstPointId, srcPointId)) { return new Tuple<int, int, int, bool>(i, dstPointId, srcPointId, false); }
						move(poly, srcPoly, dstPointId, srcPointId, true, false);
						if (IsSamePoly(poly, srcPoly, dstPointId, srcPointId)) { return new Tuple<int, int, int, bool>(i, dstPointId, srcPointId, true); }
						srcPoly.Turn(false);
					}
				}
			}
			return null;
		}

		//Detect情報の更新 (srcPolyの更新)
		private void SetDetectPiece(Poly hintPiece, Poly puzzlePiece, int dstPointId, int srcPointId, bool turnflag)
		{
			move(hintPiece, puzzlePiece, dstPointId, srcPointId, turnflag, true);
			puzzlePiece.DetectPosition();
		}
		
		//srcPolyとdstPolyを2点で合わせる
		private void move(Poly dstPoly, Poly srcPoly, int dstPointId, int srcPointId, bool turnflag, bool isUpdateLine)
		{
			if (turnflag) { srcPoly.Turn(isUpdateLine); }
			Point mul = (dstPoly[dstPointId + 1] - dstPoly[dstPointId]) / (srcPoly[srcPointId + 1] - srcPoly[srcPointId]);
			mul /= mul.Abs;
			srcPoly.Mul(mul, isUpdateLine);
			srcPoly.Trans(dstPoly[dstPointId] - srcPoly[srcPointId], isUpdateLine);
		}

		//2つの多角形が同じ点列であるかを調べる。（ただし、頂点番号のrotationがある可能性はある）
		bool IsSamePoly(Poly dstPoly, Poly srcPoly, int dstPointId, int srcPointId)
		{
			if (dstPoly.Count != srcPoly.Count) { return false; }
			int n = dstPoly.Count;
			double eps = 1e-10;

			for (int i = 0; i < n; i++)
			{
				if ((dstPoly[i + dstPointId] - srcPoly[i + srcPointId]).Norm >= eps)
				{
					return false;
				}
			}
			return true;
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

			for (i = 0; i < puzzle.pieces.Count; i++)
			{
				if (!puzzle.pieces[i].isExist)
				{
					continue;
				}
				for (j = 0; j < puzzle.pieces[i].lines.Count; j++)
				{
					Line line = puzzle.pieces[i].lines[j];
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
				Poly initPiece = new Poly(initPieces[i], new List<Line>(), true, false);
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

				poly = new Poly(newPoints, new List<Line>(), true, false);
			}
		}
	}
}