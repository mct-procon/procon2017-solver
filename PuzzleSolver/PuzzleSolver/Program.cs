using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLib;

namespace PuzzleSolver
{
	class Program
	{
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
