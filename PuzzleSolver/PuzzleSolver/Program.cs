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
            if (DX.DxLib_Init() == -1) Environment.Exit(-1);

            while(DX.ProcessMessage() != -1) {

            }
            DX.DxLib_End();
        }
    }
}
