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
		private Puzzle initialPuzzle;	//最初のパズル
        private View view;		//実体. 描画.
        private Solve solve;    //実体. 回答..

        //コンストラクタ
        public Controller() { }
        public Controller(Point t, double scale, int windowSizeX, int windowSizeY)
        {
            view	    	= new View(t, scale, windowSizeX, windowSizeY);
            solve  			= new Solve();
        }

        //問題を解く
        public void Solve(Puzzle initalPuzzle)
        {
			this.initialPuzzle = initalPuzzle;

            ValueTuple<DX.KeyState, DX.Result> prev_key;
            ValueTuple<DX.KeyState, DX.Result> key;
			List<SkewHeap> States = new List<SkewHeap>();
			int beamWidth = 5;
			int nowDepth = 0;
			int maxDepth = initialPuzzle.wakus.Count + initialPuzzle.pieces.Count - 1;

			key = DX.GetHitKeyStateAll();
			for (int i = 0; i < 100; i++) { States.Add(new SkewHeap()); }
			States[0].Push(initialPuzzle);

			int strongDrawPieceId = -1; //強調表示するピースの番号

			while (true)
            {
                bool breakFlag = false;
                while (DX.ScreenFlip() == 0 && DX.ProcessMessage() == 0 && DX.ClearDrawScreen() == 0)
                {
                    prev_key = key;
                    key = DX.GetHitKeyStateAll();

                    if (key.Item1[DX.KeyInput.Escape])
                        return;

                    if (!prev_key.Item1[DX.KeyInput.NumPadEnter] && key.Item1[DX.KeyInput.NumPadEnter])
                    {
                        breakFlag = true;
                        break;
                    }
                    if (!prev_key.Item1[DX.KeyInput.Back] && key.Item1[DX.KeyInput.Back])
                    {
						//1手戻す
						if (nowDepth > 0)
						{
							nowDepth--;
						}
                    }

                    view.UpdateDrawInfo();

					//表示したいパズルを渡す
					Puzzle ViewPuzzle = States[nowDepth].MaxValue();
					if (Network.ProconPuzzleService.IsPolygonReceived)
					{
						//支援システムから、Webカメラに写った多角形を読み取る。もし読み取れて、かつ、該当する多角形が存在すればViewクラスで表示。
						Poly recivedPiece = Poly.ParsePolyFromQRCode(Network.ProconPuzzleService.Polygon, true, -1);
						int id = CalcPieceId(ViewPuzzle, recivedPiece);
						//DX.WriteLineDx("recived!");
						if (id != -1) { strongDrawPieceId = id; }
					}

					//パズルの表示
					view.Draw(ViewPuzzle);

					//強調表示
					if (strongDrawPieceId != -1)
					{
						view.DrawPieceStrong(ViewPuzzle, strongDrawPieceId, false);
					}

					//デバッグとして、ビームサーチの評価値を表示してみよう。
					SkewHeap hoge = States[nowDepth].CloneShallow();
					while (hoge.Count > 0)
					{
						DX.DrawString(100 + (hoge.Count / 25) * 400, 200 + (hoge.Count % 25) * 22, 0, hoge.Pop().boardScore.ToString());
					}
                }
                if (!breakFlag) { return; }

				//パズルを解く (1手進める) → ビームサーチ
				if (nowDepth < maxDepth)
				{
					HashSet<long> puzzlesInHeap = new HashSet<long>();
					SkewHeap backupLogForDisplay = States[nowDepth].CloneShallow();
					States[nowDepth + 1] = new SkewHeap();

					while (States[nowDepth].Count > 0)
					{
						Puzzle nowPuzzle = States[nowDepth].Pop().Clone();

						int doBeamWidth = beamWidth;
						//if (nowDepth % 6 == 0) { doBeamWidth = 3; }

						solve.SetNextStates(nowPuzzle, doBeamWidth, States[nowDepth + 1], puzzlesInHeap);
					}
					States[nowDepth] = backupLogForDisplay;
					if (States[nowDepth + 1].Count > 0) { nowDepth++; }
				}
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
