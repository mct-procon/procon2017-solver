﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLib;
using PuzzleSolver.Core;
using PuzzleSolver.Geometry;

namespace PuzzleSolver.UI
{
    public class Controller
    {
		private Puzzle initialPuzzle;	//最初のパズル
        private View view;		//実体. 描画.
        private Solve solve;	//実体. 回答.

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
                    view.Draw(States[nowDepth].MaxValue());
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
						solve.SetNextStates(nowPuzzle, beamWidth, States[nowDepth + 1], puzzlesInHeap);
					}
					States[nowDepth] = backupLogForDisplay;
					if (States[nowDepth + 1].Count > 0) { nowDepth++; }
				}
            }
        }
    }
}
