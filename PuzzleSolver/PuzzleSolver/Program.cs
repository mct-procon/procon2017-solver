using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLib;

namespace PuzzleSolver {
    class Program {
        [STAThread]
        static void Main(string[] args) {
            DX.ChangeWindowMode(true);
            if (DX.Init() == DX.Result.Error) Environment.Exit(-1);
            DX.DrawLine(0, 0, 100, 100, new DX.Color(255, 255, 255));
            DX.WriteLineDx("{0} {1}", 123, "なにこれ？");
            while(DX.ProcessMessage() == DX.Result.Success) {
            }
            DX.Finalize();
        }
    }
}
