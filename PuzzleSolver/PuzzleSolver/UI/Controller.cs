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
			bool[] bkey = new bool[256];
			bool[] key = new bool[256];
			for (int i = 0; i < 256; i++) { key[i] = false; }

			while (true)
			{
				bool breakFlag = false;
				while (DX.ScreenFlip() == 0 && DX.ProcessMessage() == 0 && DX.ClearDrawScreen() == 0 && !DX.CheckHitKey(DX.KeyInput.Escape))
				{
					for (int i = 0; i < 256; i++) { bkey[i] = key[i]; }
					for (int i = 0; i < 256; i++) { key[i] = DX.CheckHitKey((DX.KeyInput)i); }

					if (!bkey[(int)DX.KeyInput.NumPadEnter] && key[(int)DX.KeyInput.NumPadEnter])
					{
						breakFlag = true;
						break;
					}
					if (!bkey[(int)DX.KeyInput.Back] && key[(int)DX.KeyInput.Back])
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
