﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuzzleSolver.Geometry;

namespace PuzzleSolver.Core
{
	public class Solver
	{
		private MargePoly margePoly;                    //実体. 多角形マージ用の関数を集めた.
		public List<Puzzle> ViewPuzzles { get; }        //ViewPuzzles.Count … 何手まで調べたか(最初も含む)、ViewPuzzles[i] … i手目の結果
		public List<List<int>> evalScores { get; }		//評価値, デバッグ用

		//コンストラクタ
		public Solver()
		{
			margePoly = new MargePoly();
			ViewPuzzles = new List<Puzzle>();
			evalScores = new List<List<int>>();
		}


		//問題を解く
		public void Solve(Puzzle initialPuzzle)
		{
			List<SkewHeap> States = new List<SkewHeap>();
			int beamWidth = 1000;
			int nowDepth = 0;
			int maxDepth = initialPuzzle.initPieceNum;

			for (int i = 0; i <= maxDepth; i++) { States.Add(new SkewHeap()); }
			States[0].Push(initialPuzzle);
			ViewPuzzles.Add(initialPuzzle); //0手目の結果

			for (int i = 0; i <= maxDepth; i++) { evalScores.Add(new List<int>()); }
			evalScores[0].Add(initialPuzzle.boardScore);

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

				//デバッグ
				SkewHeap sh = States[nowDepth].CloneShallow();
				while (sh.Count > 0)
				{
					evalScores[nowDepth].Add(sh.MinValue().boardScore);
					sh.Pop();
				}
				evalScores[nowDepth].Reverse();
			}
		}


		//ビームサーチの次状態更新
		public void SetNextStates(Puzzle puzzle, int beamWidth, SkewHeap heap, HashSet<long> puzzlesInHeap)
		{
			int wakuId = GetDistinationWakuId(puzzle.wakus);
			if (wakuId < 0) { return; }

			for (int srcPolyId = 0; srcPolyId < puzzle.pieceTable.Count; srcPolyId++)
			{
				if (!puzzle.isPieceExist[srcPolyId]) { continue; }
				List<Poly> srcPolys = puzzle.pieceTable[srcPolyId];

				Poly dstPoly = puzzle.wakus[wakuId];
				for (int i = 0; i < srcPolys.Count; i++)
				{
					int score = GetScore(dstPoly, srcPolys[i], heap.Count < beamWidth ? -1 : heap.MinValue().boardScore - puzzle.boardScore + 1);
					if (score < 0) { continue; }
					Puzzle nextPuzzle = GetNextPuzzle(puzzle, wakuId, srcPolyId, i, score);
					if (IsUpdateBeam(heap, puzzlesInHeap, nextPuzzle, beamWidth))
					{
						UpdateBeam(heap, puzzlesInHeap, nextPuzzle, beamWidth);
					}
				}
			}
		}

		//dstination頂点の属する可能性のある枠の番号を1つ返す. dstination頂点 = 枠Xの頂点番号Yのとき, Xを返す.
		//枠の頂点がない場合は, nullを返す。
		public int GetDistinationWakuId(List<Poly> wakus)
		{
			int X = -1;

			for (int i = 0; i < wakus.Count; i++)
			{
				if (!wakus[i].isExist || wakus[i].Count <= 0) { continue; }
				if (X == -1 || Point.Compare(wakus[X].points[wakus[X].minestPointId], wakus[i].points[wakus[i].minestPointId]) > 0)
				{
					X = i;
				}
			}
			if (X == -1) { return -1; }
			return X;
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
		private int GetScore(Poly dstPoly, Poly srcPoly, int bestScore)
		{
			Poly clonedSrcPoly = srcPoly.Clone();
			move(dstPoly, clonedSrcPoly, false);
			int score = Poly.Evaluation(dstPoly, clonedSrcPoly, dstPoly.minestPointId, clonedSrcPoly.minestPointId);
			if (score < bestScore || clonedSrcPoly.isHitLine(dstPoly)) { score = -1; }

			List<Point> points = clonedSrcPoly.SizingPoly();
			for (int i = 0; i < points.Count; i++)
			{
				if (!dstPoly.isCover(points[i]))
				{
					score = -1;
					break;
				}
			}

			return score;
		}

		//多角形srcPolyを(平行)移動する。多角形dstPolyのx座標最小点と, 多角形srcPolyのx座標最小点が一致するように移動量を決める。
		private void move(Poly dstPoly, Poly srcPoly, bool isUpdateLines)
		{
			srcPoly.Trans(dstPoly[dstPoly.minestPointId] - srcPoly[srcPoly.minestPointId], isUpdateLines);
		}

		//結合後のパズルを得る。(マージ不可な場合は, nullを返す.) (使用時の前提：当たり判定は完了している)
		//引数：結合前のPuzzle(const), くっつけ方(枠番号, ピースnowDepthの候補番号), 結合度.
		private Puzzle GetNextPuzzle(Puzzle puzzle, int wakuId, int pieceId, int pieceListId, int score)
		{
			Puzzle ret = puzzle.Clone();
			Poly dstPoly, srcPoly;

			dstPoly = ret.wakus[wakuId];
			srcPoly = ret.pieceTable[pieceId][pieceListId].Clone();

			//ピースの移動
			move(dstPoly, srcPoly, true);

			//マージ判定
			List<Poly> polys = margePoly.Marge(dstPoly, srcPoly);
			if (polys.Count == 0) { return null; }

			//マージ処理
			for (int i = 0; i < polys.Count; i++)
			{
				if (!polys[i].isPiece)
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
			//srcPoly.isExist = false; srcPoly.points.Clear(); srcPoly.lines.Clear();

			//盤面の深さ, 盤面評価, ハッシュの登録
			ret.setIsPieceExist(pieceId, false);
			ret.setBoardScore(puzzle.boardScore + score);
			ret.setBoardHash();

			return ret;
		}
	}
}
