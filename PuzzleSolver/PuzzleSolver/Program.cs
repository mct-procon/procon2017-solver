using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLib;
using PuzzleSolver.Core;
using PuzzleSolver.Geometry;
using PuzzleSolver.UI;

namespace PuzzleSolver
{
	class Program
	{
		Read		read;			//実体. ファイル読み込み
		Controller	controller;		//実体. 1問解く／表示する
		Backup		backup;			//実体. パズルをStackで管理する

		[STAThread]
		static void Main(string[] args)
		{
			DX.ChangeWindowMode(true);
			DX.SetBackgroundColor(255, 255, 255);
			DX.SetGraphMode(800, 600, 32);
			if (DX.Init() == DX.Result.Error) Environment.Exit(-1);
			DX.SetDrawScreen(DX.Screen.Back);

			DX.WriteLineDx("{0} {1}", 123, "なにこれ？");
			while(DX.ScreenFlip()==0 && DX.ProcessMessage()==0 && DX.ClearDrawScreen()==0 && !DX.CheckHitKey(DX.KeyInput.Escape)) {
				DX.DrawCircle(300, 300, 50, new DX.Color(255, 0, 0), true);
				DX.DrawLine(0, 0, 100, 100, new DX.Color(255, 0, 255));
			}
			DX.Finalize();
		}
	}
}
