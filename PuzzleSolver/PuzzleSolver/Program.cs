using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLib;
using PuzzleSolver.Core;
using PuzzleSolver.Geometry;
using PuzzleSolver.UI;
using System.Windows.Forms;

namespace PuzzleSolver
{
	class Program
	{
		static Backup backup;				//実体. パズルをStackで管理する
		static Read read;					//実体. ファイル読み込み
		static Controller	controller;		//実体. 1問解く／表示する

		[STAThread]
		static void Main(string[] args)
		{
			DX.ChangeWindowMode(true);
			DX.SetBackgroundColor(255, 255, 255);
			DX.SetGraphMode(800, 600, 32);
			if (DX.Init() == DX.Result.Error) Environment.Exit(-1);
			DX.SetDrawScreen(DX.Screen.Back);

			backup = new Backup();
			read = new Read(backup);
			controller = new Controller(backup, new Point(0, 0), 5.0, 800, 600);
            try {
                read.ReadFile("sample.txt");
            } catch(Exception ex) {
                DX.WriteLineDx("File Input Error.\n{0}", ex);
                DX.ScreenFlip();
                while (DX.ProcessMessage() == DX.Result.Success) {
                    if (DX.GetMouseInput() == DX.MouseInput.Left)
                        break;
                }
                DX.Finalize();
                return;
            }
			controller.Solve();

			DX.Finalize();
		}
	}
}
