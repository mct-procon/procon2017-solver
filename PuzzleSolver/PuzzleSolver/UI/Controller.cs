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
			DX.KeyState key = new DX.KeyState();
			DX.KeyState bkey = new DX.KeyState();

			DX.GetHitKeyStateAll(ref key);

			while (true)
			{
				bkey = new DX.KeyState();
				while (DX.ScreenFlip() == 0 && DX.ProcessMessage() == 0 && DX.ClearDrawScreen() == 0 && !DX.CheckHitKey(DX.KeyInput.Escape))
				{
					view.Update();
					view.Draw(History.Peek());
				}

				Puzzle puzzle = History.Peek().Clone();
				puzzle = solve.ConnectAuto(puzzle);
				History.Push(puzzle);
			}
		}
	}
}
