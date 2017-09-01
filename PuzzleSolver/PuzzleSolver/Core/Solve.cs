using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuzzleSolver.Geometry;
using T = System.Tuple<int, int, int, int, int, bool>;

namespace PuzzleSolver.Core
{
	public class Solve
	{
		private MargePoly margePoly;            //実体. 多角形マージ用の関数を集めた.

		//コンストラクタ
		public Solve()
		{
			margePoly = new MargePoly();
		}

		//全自動でくっつける
		public Puzzle ConnectAuto(Puzzle puzzle)
		{
			List<Poly> polys = new List<Poly>(puzzle.wakus);
			for (int i = 0; i < puzzle.pieces.Count; i++) { polys.Add(puzzle.pieces[i]); }

			//scoreTable[dstPolyId, dstPointId] => polys[dstPolyId].points[dstPointId]に何かをくっつけたときの, 
			//(scoreの最大値M, score==MになるようなsrcPoly(相方)の個数, score==MとなるsrcPoly, srcPointId, direction, turnflag)
			Dictionary<Tuple<int, int>, T> scoreTable = new Dictionary<Tuple<int, int>, T>();
			
			for (int i = 0; i < polys.Count(); i++)
			{
				for (int j = 0; j < polys[i].Count; j++)
				{
					scoreTable.Add(new Tuple<int, int>(i, j), new T(-1, 0, -1, -1, -1, false));
				}
			}

			//2辺のくっつけ方をすべて試す
			int bestScore = -1;
			for (int dstPolyId = 0; dstPolyId < polys.Count; dstPolyId++)
			{
				Poly dstPoly = polys[dstPolyId];		
				if (!dstPoly.isExist) { continue; }
				for (int srcPolyId = 0; srcPolyId < puzzle.pieces.Count; srcPolyId++)
				{
					Poly srcPoly = puzzle.pieces[srcPolyId];
					if (!srcPoly.isExist) { continue; }
					if (dstPoly.points == srcPoly.points) { continue; }

					for (int dstPointId = 0; dstPointId < dstPoly.Count; dstPointId++)
					{
						for (int srcPointId = 0; srcPointId < srcPoly.Count; srcPointId++)
						{
							for (int option = 0; option < 4; option++)
							{
								int direction = option / 2;
								bool turnflag = (option % 2 == 1);
								int score = getScore(dstPoly, srcPoly, dstPointId, srcPointId, direction, turnflag, bestScore);
								if (bestScore < score) { bestScore = score; }

								//スコアテーブルの更新
								Tuple<int, int> Key = new Tuple<int, int>(dstPolyId, dstPointId);
								if (scoreTable[Key].Item1 < score)
								{
									scoreTable[Key] = new T(score, 1, srcPolyId, srcPointId, direction, turnflag);
								}
								else if (scoreTable[Key].Item1 == score)
								{
									scoreTable[Key] = new T(score, scoreTable[Key].Item2 + 1, scoreTable[Key].Item3,
										scoreTable[Key].Item4, scoreTable[Key].Item5, scoreTable[Key].Item6);
								}
							}
						}
					}
				}
			}

			//結合度最大のペアを探す
			Tuple<int, int> bestKey = new Tuple<int, int>(0, 0);
			foreach (var element in scoreTable)
			{
				if (scoreTable[bestKey].Item1 < element.Value.Item1 || (scoreTable[bestKey].Item1 == element.Value.Item1 && scoreTable[bestKey].Item2 > element.Value.Item2))
				{
					bestKey = element.Key;
				}
			}
			KeyValuePair<Tuple<int, int>, T> bestElement = new KeyValuePair<Tuple<int, int>, T>(bestKey, scoreTable[bestKey]);

			//結合度最大のペアでくっつける
			if (bestElement.Value.Item1 > 0)
			{
				Poly dstPoly = polys[bestElement.Key.Item1];
				int dstPointId = bestElement.Key.Item2;
				int pieceId = bestElement.Value.Item3;
				Poly srcPoly = puzzle.pieces[pieceId];
				int srcPointId = bestElement.Value.Item4;
				int direction = bestElement.Value.Item5;
				bool turnflag = bestElement.Value.Item6;

				if (turnflag) { srcPoly.Turn(true); }
				move(dstPoly, srcPoly, dstPointId, srcPointId, direction, true);
				List<Poly> margedPolyList = margePoly.Marge(dstPoly, srcPoly);

				DxLib.DX.WriteLineDx("結合度 = " + bestElement.Value.Item1.ToString() + " 候補数 = " + bestElement.Value.Item2.ToString());

				if (margedPolyList.Count > 0 && (!margedPolyList[0].isPiece || margedPolyList.Count == 1))
				{
					//リストに追加
					for (int i = 0; i < margedPolyList.Count; i++)
					{
						if (margedPolyList[i].isPiece)
						{
							puzzle.pieces.Add(margedPolyList[i]);
						}
						else
						{
							puzzle.wakus.Add(margedPolyList[i]);
						}
					}
					//非アクティブにする
					dstPoly.isExist = false;
					srcPoly.isExist = false;
					
					//枠辺の更新
					if (!dstPoly.isPiece)
					{
						puzzle.wakuLines.AddRange(srcPoly.lines);
					}

					//ピース辺の更新
					if (pieceId >= puzzle.initPieceNum)
					{
						srcPoly.lines.Clear();
					}
				}
			}
			return puzzle;
		}

		//結合度を得る
		private int getScore(Poly dstPoly, Poly srcPoly, int dstPointId, int srcPointId, int direction, bool turnflag, int bestScore)
		{
			List<Point> backupPointList = new List<Point>(srcPoly.points);
			if (turnflag) { srcPoly.Turn(false); }
			move(dstPoly, srcPoly, dstPointId, srcPointId, direction);
			int score = Poly.Evaluation(dstPoly, srcPoly, dstPointId, srcPointId);
			if (score < bestScore || dstPoly.isHitLine(srcPoly)) { score = -1; }
			srcPoly.points = backupPointList;
			return score;
		}

		//多角形srcPolyを平行移動・回転する。
		//まず, 「多角形dstPolyの頂点dstPointId --> dstPointId + direction」と「多角形srcPolyの頂点srcPointId --> srcPointId - direction」
		//が同じ方向を向くように回転する。
		//次に, 「多角形dstPolyの頂点dstPointId」と「多角形srcPolyの頂点srcPointId」がくっつくように平行移動する。
		private void move(Poly dstPoly, Poly srcPoly, int dstPointId, int srcPointId, int direction, bool isUpdateLines = false)
		{
			Point mul = (dstPoly[dstPointId + direction] - dstPoly[dstPointId]) / (srcPoly[srcPointId - direction] - srcPoly[srcPointId]);
			mul /= mul.Abs;
			srcPoly.Mul(mul, isUpdateLines);

			Point t = dstPoly.points[dstPointId] - srcPoly.points[srcPointId];
			srcPoly.Trans(t, isUpdateLines);
		}
	}
}
