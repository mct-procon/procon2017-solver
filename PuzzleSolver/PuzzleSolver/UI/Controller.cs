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
			int beamWidth = 50;
			int nowDepth = 0;
			int maxDepth = initialPuzzle.wakus.Count + initialPuzzle.pieces.Count - 1;

			key = DX.GetHitKeyStateAll();
			for (int i = 0; i < 100; i++) { States.Add(new SkewHeap()); }
			States[0].Push(initialPuzzle);

			int strongDrawPieceId = -1;	//強調表示するピースの番号

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
						if (id != -1) { strongDrawPieceId = id; }
					}

					//パズルの表示
					view.Draw(ViewPuzzle);

					//強調表示
					if (strongDrawPieceId != -1)
					{
						view.DrawPieceStrong(ViewPuzzle, strongDrawPieceId);
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
				double a = EvalLogScale(initPiece, piece.Clone());
				if (minA > a)
				{
					minA = a;
					ret = i;
				}
			}
			return ret;
		}


		//なんかあれ
		private double EvalLogScale(Poly poly1, Poly poly2)
		{
			int i, j;
			double ret = 1145141919;
			double errorDeg = 10;

			poly1 = RemovePoints(poly1, errorDeg);
			poly2 = RemovePoints(poly2, errorDeg);
			if (poly1.Count != poly2.Count) { return 1145141919; }
			Scaling(poly1);
			Scaling(poly2);

			int n = poly1.Count;
			for (i = 0; i < n; i++)	//poly1[0], poly2[i]から走査
			{
				double maxLogScale = 0;
				for (j = 0; j < n; j++)
				{
					Point a = poly1[j + 1] - poly1[j];
					Point b = poly2[i + j + 1] - poly2[i + j];

					if (a.Abs < 1e-9 || b.Abs < 1e-9)
					{
						return 1145141919;
					}
					maxLogScale = Math.Max(maxLogScale, Math.Abs(Math.Log(a.Abs / b.Abs)));
				}
				ret = Math.Min(ret, maxLogScale);
			}
			return ret;
		}

		///角度が180度に近い頂点を取り除く
		private Poly RemovePoints(Poly poly, double errorDegree)
		{
			int i;
			const double PAI = 3.14159265358979;
			Poly ret = new Poly(new List<Point>(), new List<Line>(), true);

			for (i = 0; i < poly.Count; i++)
			{
				Point a = poly[i - 1];
				Point b = poly[i];
				Point c = poly[i + 1];

				Point ba = a - b;
				Point cb = c - b;
				Point hoge = (ba / cb);

				double deg = Math.Atan2(hoge.Im, hoge.Re) * 180 / PAI;
				if (deg < 0) { deg += 360; }

				if (Math.Abs(deg - 180) <= errorDegree)
				{
					continue;
				}

				ret.points.Add(b);
			}
			ret.points.Add(ret.points[0]);

			return ret;
		}

		//正規化 (最も短い辺の長さが1になるように拡大（縮小）する）
		private void Scaling(Poly poly)
		{
			double minLength = 1145141919;
			int i;

			poly.Trans(poly.points[0], false);

			for (i = 0; i < poly.Count; i++)
			{
				minLength = Math.Min(minLength, (poly.points[i + 1] - poly.points[i]).Abs);
			}

			//minLength倍縮小する。
			for (i = 0; i < poly.Count; i++)
			{
				poly[i] /= minLength;
			}
		}
    }
}
