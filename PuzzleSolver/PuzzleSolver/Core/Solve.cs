using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuzzleSolver.Geometry;
using T = System.Tuple<int, int, PuzzleSolver.Geometry.Poly, int, int, int, bool>;

namespace PuzzleSolver.Core
{
	public class Solve
	{
		private MargePoly margePoly;			//実体. 多角形マージ用の関数を集めた.

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

			//scoreTable[i] = {dstPoly == polys[i]のときの, (scoreの最大値M, score==MになるようなsrcPoly(相方)の個数, score==MとなるsrcPoly, dstPointId, srcPointId, direction, turnflag)}
			T[] scoreTable = new T[polys.Count];
			for (int i = 0; i < polys.Count; i++) { scoreTable[i] = new T(-1, 0, new Poly(), -1, -1, -1, false); }

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
								int score = getScore(dstPoly, srcPoly, dstPointId, srcPointId, direction, turnflag);
								bestScore = Math.Max(bestScore, score);

								//スコアテーブルの更新
								if (scoreTable[dstPolyId].Item1 < score)
								{
									scoreTable[dstPolyId] = new T(score, 1, srcPoly, dstPointId, srcPointId, direction, turnflag);
								}
								else if (scoreTable[dstPolyId].Item1 == score)
								{
									scoreTable[dstPolyId] = new T(score, scoreTable[dstPolyId].Item2 + 1, scoreTable[dstPolyId].Item3,
										scoreTable[dstPolyId].Item4, scoreTable[dstPolyId].Item5, scoreTable[dstPolyId].Item6, scoreTable[dstPolyId].Item7);
								}
							}
						}
					}
				}
			}

			//結合度最大のペアを探す
			int rowId = 0;
			for (int i = 1; i < polys.Count; i++)
			{
				if (scoreTable[rowId].Item1 < scoreTable[i].Item1 || (scoreTable[rowId].Item1 == scoreTable[i].Item1 && scoreTable[rowId].Item2 > scoreTable[i].Item2))
				{
					rowId = i;
				}
			}

			//結合度最大のペアでくっつける
			if (scoreTable[rowId].Item1 > 0)
			{
				Poly dstPoly = polys[rowId];
				Poly srcPoly = scoreTable[rowId].Item3;
				int dstPointId = scoreTable[rowId].Item4;
				int srcPointId = scoreTable[rowId].Item5;
				int direction = scoreTable[rowId].Item6;
				bool turnflag = scoreTable[rowId].Item7;

				if (turnflag) { srcPoly.Turn(true); }
				move(dstPoly, srcPoly, dstPointId, srcPointId, direction, true);

				List<Poly> margedPolyList = margePoly.Marge(dstPoly, srcPoly);
				DxLib.DX.WriteLineDx(margedPolyList.Count.ToString() + " " + getScore(dstPoly, srcPoly, dstPointId, srcPointId, direction, turnflag).ToString());
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
				}
			}
			return puzzle;
		}

		//結合度を得る
		private int getScore(Poly dstPoly, Poly srcPoly, int dstPointId, int srcPointId, int direction, bool turnflag)
		{
			List<Point> backupPointList = new List<Point>(srcPoly.points);
			if (turnflag) { srcPoly.Turn(false); }
			move(dstPoly, srcPoly, dstPointId, srcPointId, direction);
			int score;
			if (dstPoly.isHitLine(srcPoly)) { score = -1; }
			else { score = Poly.Evaluation(dstPoly, srcPoly, dstPointId, srcPointId); }
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
