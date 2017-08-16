using System;
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
        private Backup History;	//参照. パズルをStackで管理.
        private View view;		//実体. 描画.
        private Solve solve;	//実体. 回答.

        //コンストラクタ
        public Controller() { }
        public Controller(Backup backup, Point t, double scale, int windowSizeX, int windowSizeY)
        {
            this.History	= backup;
            view	    	= new View(t, scale, windowSizeX, windowSizeY);
            solve  			= new Solve();
        }

        //問題を解く
        public void Solve()
        {

            ValueTuple<DX.KeyState, DX.Result> prev_key;
            ValueTuple<DX.KeyState, DX.Result> key;

            key = DX.GetHitKeyStateAll();

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
                        History.Pop();
                    }

                    view.Update();
                    view.Draw(History.Peek());
                }
                if (!breakFlag) { return; }

                Puzzle puzzle = History.Peek().Clone();
                puzzle = solve.ConnectAuto(puzzle);
                History.Push(puzzle);
            }
        }
    }
}
