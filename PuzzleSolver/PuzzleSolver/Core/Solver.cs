using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuzzleSolver.Geometry;
using DxLib;

namespace PuzzleSolver.Core
{
	public class Solver
	{
		private MargePoly margePoly;					//実体. 多角形マージ用の関数を集めた.
		public List<Puzzle> ViewPuzzles { get; }		//ViewPuzzles.Count … 何手まで調べたか(最初も含む)、ViewPuzzles[i] … i手目の結果

		//コンストラクタ
		public Solver()
		{
			margePoly = new MargePoly();
			ViewPuzzles = new List<Puzzle>();
		}


		//問題を解く
		public void Solve(Puzzle initialPuzzle)
		{
			List<SkewHeap> States = new List<SkewHeap>();
			int beamWidth = 50;
			int nowDepth = 0;
			int maxDepth = initialPuzzle.pieces.Count;

			for (int i = 0; i < 100; i++) { States.Add(new SkewHeap()); }
			States[0].Push(initialPuzzle);
			ViewPuzzles.Add(initialPuzzle);	//0手目の結果

			//パズルを解く (1手進める) → ビームサーチ
			while (nowDepth < maxDepth)
			{
				HashSet<long> puzzlesInHeap = new HashSet<long>();
				States[nowDepth + 1] = new SkewHeap();

				while (States[nowDepth].Count > 0)
				{
					Puzzle nowPuzzle = States[nowDepth].Pop().Clone();
					SetNextStates(nowPuzzle, beamWidth, States[nowDepth + 1], puzzlesInHeap);
				}
				if (States[nowDepth + 1].Count > 0) { ViewPuzzles.Add(States[nowDepth + 1].MaxValue().Clone()); nowDepth++; }
				else { break; }
			}
		}

		//ビームサーチの次状態更新
		private void SetNextStates(Puzzle puzzle, int beamWidth, SkewHeap heap, HashSet<long> puzzlesInHeap)
		{
			List<Poly> polys = new List<Poly>(puzzle.wakus);
			for (int i = 0; i < puzzle.pieces.Count; i++) { polys.Add(puzzle.pieces[i]); }

			for (int dstPolyId = 0; dstPolyId < polys.Count; dstPolyId++)
			{
				Poly dstPoly = polys[dstPolyId];
				if (!dstPoly.isExist) { continue; }
				for (int srcPolyId = 0; srcPolyId < puzzle.pieces.Count; srcPolyId++)
				{
					Poly srcPoly = puzzle.pieces[srcPolyId];
					if (!srcPoly.isExist) { continue; }
					if (dstPoly == srcPoly) { continue; }

					for (int dstPointId = 0; dstPointId < dstPoly.Count; dstPointId++)
					{
						for (int srcPointId = 0; srcPointId < srcPoly.Count; srcPointId++)
						{
							for (int option = 0; option < 4; option++)
							{
								int direction = option >> 1;   // means : option / 2
								bool turnflag = (option & 1) == 1;  // means : (option % 2) == 1
								int score = getScore(dstPoly, srcPoly, dstPointId, srcPointId, direction, turnflag, heap.Count == beamWidth ? heap.MinValue().boardScore + 1 - puzzle.boardScore : -1);
								if (score < 0) { continue; }

								Puzzle nextPuzzle = GetNextPuzzle(puzzle, dstPolyId, srcPolyId, dstPointId, srcPointId, direction, turnflag, score);
								if (IsUpdateBeam(heap, puzzlesInHeap, nextPuzzle, beamWidth))
								{
									UpdateBeam(heap, puzzlesInHeap, nextPuzzle, beamWidth);
								}
							}
						}
					}
				}
			}
		}

		//ビームを更新するか
		private bool IsUpdateBeam(SkewHeap heap, HashSet<long> puzzlesInHeap, Puzzle nextPuzzle, int beamWidth)
		{
			if (nextPuzzle == null) { return false; }
			if (!puzzlesInHeap.Contains(nextPuzzle.boardHash)) { return true; }
			if (heap.Count < beamWidth) { return false; }
			if (nextPuzzle.boardHash == heap.MinValue().boardHash) { return true; }
			return false;
		}

		//ビームの更新
		private void UpdateBeam(SkewHeap heap, HashSet<long> puzzlesInHeap, Puzzle nextPuzzle, int beamWidth)
		{
			if (heap.Count == beamWidth)
			{
				puzzlesInHeap.Remove(heap.MinValue().boardHash);
				heap.Pop();
			}

			puzzlesInHeap.Add(nextPuzzle.boardHash);
			heap.Push(nextPuzzle);
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


		//結合後のパズルを得る。(マージ不可な場合は, nullを返す.) (使用時の前提：当たり判定は完了している)
		//引数：結合前のPuzzle(const), くっつけ方, 結合度.
		//dstPolyId … {枠0, 枠1, …, ピース0, ピース1, …}(0頂点の多角形含む)としたときに, 何番目の多角形にくっつけるか？ (0-indexed)
		private Puzzle GetNextPuzzle(Puzzle puzzle, int dstPolyId, int srcPolyId, int dstPointId, int srcPointId, int direction, bool turnflag, int score)
		{
			Puzzle ret = puzzle.Clone();
			Poly dstPoly, srcPoly;

			if (dstPolyId < ret.wakus.Count) { dstPoly = ret.wakus[dstPolyId]; }
			else { dstPoly = ret.pieces[dstPolyId - ret.wakus.Count]; }
			srcPoly = ret.pieces[srcPolyId];

			//ピースの移動
			if (turnflag) { srcPoly.Turn(true); }
			move(dstPoly, srcPoly, dstPointId, srcPointId, direction, true);

			//マージ判定
			List<Poly> polys = margePoly.Marge(dstPoly, srcPoly);
			if (polys.Count == 0) { return null; }

			//マージ処理
			for (int i = 0; i < polys.Count; i++)
			{
				if (polys[i].isPiece)
				{
					ret.pieces.Add(polys[i]);
				}
				else
				{
					ret.wakus.Add(polys[i]);
				}
			}

			if (!dstPoly.isPiece)
			{
				for (int i = 0; i < srcPoly.lines.Count; i++)
				{
					ret.wakuLines.Add(srcPoly.lines[i]);
				}
			}

			dstPoly.isExist = false; dstPoly.points.Clear(); dstPoly.lines.Clear();
			srcPoly.isExist = false; srcPoly.points.Clear(); srcPoly.lines.Clear();

			//盤面評価, ハッシュの登録
			ret.setBoardScore(puzzle.boardScore + score);
			ret.setBoardHash();

			return ret;
		}
	}
}
