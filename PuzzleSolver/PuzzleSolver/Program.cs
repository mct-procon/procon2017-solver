﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLib;
using PuzzleSolver.Core;
using PuzzleSolver.Geometry;
using PuzzleSolver.UI;
using System.Windows.Forms;
using Microsoft.Win32;

namespace PuzzleSolver
{
    class Program
    {
        static Backup backup;				//実体. パズルをStackで管理する
        static Read read;					//実体. ファイル読み込み
        static Controller	controller;		//実体. 1問解く／表示する

        static IntPtr MainWindowHWND;       //DxLibが作ったウィンドウのハンドル
        static NativeWindow MainWindow;

        [STAThread]
        static void Main(string[] args)
        {
            DX.ChangeWindowMode(true);
            DX.SetBackgroundColor(255, 255, 255);
            DX.SetGraphMode(800, 600, 32);
            if (DX.Init() == DX.Result.Error) Environment.Exit(-1);
            DX.SetDrawScreen(DX.Screen.Back);

            MainWindowHWND = DX.GetMainWindowHandle();
            MainWindow = NativeWindow.FromHandle(MainWindowHWND);


            backup = new Backup();
            read = new Read(backup);
            controller = new Controller(backup, new Point(0, 0), 5.0, 800, 600);

            if (!ReadFile("sample.txt"))
                return;
            DX.ClsDx();

            controller.Solve();

            DX.Finalize();
        }

        static bool ReadFile(string FilePath) {
            try {
                read.ReadFile(FilePath);
                return true;
            } catch (Exception ex) {
                DX.WriteLineDx("File Input Error.\n{0}", ex);
                DX.ScreenFlip();
                if (MessageBox.Show(MainWindow, "sample.txtが読み込めませんでした．\n別のファイルを読み込みますか？", "エラー", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes) {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.InitialDirectory = Environment.CurrentDirectory;
                    openFileDialog.Filter = "All Files|*.*";
                    openFileDialog.FilterIndex = 0;
                    openFileDialog.Title = "問題ファイルをえらんでね";
                    openFileDialog.Multiselect = false;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                        return ReadFile(openFileDialog.FileName);
                }
                while (DX.ProcessMessage() == DX.Result.Success) {
                    if (DX.GetMouseInput() == DX.MouseInput.Left)
                        break;
                }
                DX.Finalize();
                return false;
            }
        }
    }
}
