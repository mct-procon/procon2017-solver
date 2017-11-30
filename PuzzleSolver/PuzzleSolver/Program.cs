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
using Microsoft.Win32;
using System.IO;

namespace PuzzleSolver
{
	/// <summary>
	/// メイン
	/// </summary>
	class Program
	{
		static Read read;                   //実体. ファイル読み込み
		static Controller controller;       //実体. 1問解く／表示する

		static IntPtr MainWindowHWND;       //DxLibが作ったウィンドウのハンドル
		static NativeWindow MainWindow;     //DxLibが作ったウィンドウ（MessageBox表示用）
		static Network.WCF WCFServer;       //支援システムとの通信を管理するクラス．

		/// <summary>
		/// メイン
		/// </summary>
		/// <param name="args">引数</param>
		[STAThread]
		static void Main(string[] args)
		{
			DX.SetDoubleStartValidFlag(1);

			DX.SetMainWindowText("BeamSearch");
			DX.ChangeWindowMode(true);                        // Set Window Mode against FullScreen Mode.
			DX.SetBackgroundColor(255, 255, 255);             // Set Background Color of Screen.
			DX.SetWindowSizeChangeEnableFlag(1, false);    // Set Resizable and non-Scaling.
			DX.SetWindowSize(1400, 1000);                       // Set Window Size
			DX.SetAlwaysRunFlag(true);                        // Set to run even when the window is not active.
			DX.SetGraphMode(1920, 1080, 32);                  // Set Main Render's Size.
			if (DX.Init() == DX.Result.Error) Environment.Exit(-1);
			DX.SetDrawScreen(DX.Screen.Back);

			MainWindowHWND = DX.GetMainWindowHandle();
			MainWindow = NativeWindow.FromHandle(MainWindowHWND);

			read = new Read();
			controller = new Controller(new Point(-50, -30), 9.20, 1400, 1000);

			DX.ClsDx();

			WCFServer = new Network.WCF();
			try
			{
				WCFServer.Open();
			}
			catch (System.ServiceModel.AddressAccessDeniedException exp)
			{
#if !DEBUG
				MessageBox.Show(MainWindow, "HTTPサーバーの作成でアクセス拒否が起きました．netsh等で，権限付与してくださいな．", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
#endif
			}
			catch (Exception ex)
			{
#if !DEBUG
				MessageBox.Show(MainWindow, $"エラーが起きました．\n{ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
#endif
			}

			string directoryName = GetReadDirectoryName(@"C:\Users\hashimotolab\Documents\GitHub\procon2017-solver\PuzzleSolver\PuzzleSolver\readDirectoryName.txt");
			List<Puzzle> initialPuzzles = SetInitialPuzzles(directoryName);

			foreach (Puzzle initialPuzzle in initialPuzzles)
			{
				DxLib.DX.WriteLineDx(initialPuzzle.initPieceNum.ToString());
				bool res = controller.Syakunetsukun(initialPuzzle);
				if (!res) { break; }
			}

			WCFServer.Close();

			DX.Finalize();
		}

		// どのフォルダからテストケースを読み込むか？（QRコードの場合は、QRを返す）
		static string GetReadDirectoryName(string directoryNameFile)
		{
			StreamReader reader = new StreamReader(directoryNameFile);

			if (reader == null) { return "QR"; }
			try
			{
				return reader.ReadLine();
			} catch
			{
				return "QR";
			}
		}

		// 指定したディレクトリからパズル列を読み込む
		static List<Puzzle> SetInitialPuzzles(string directoryName)
		{
			List<Puzzle> puzzles = new List<Puzzle>();

			if (directoryName.CompareTo("QR") != 0)
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(directoryName);
				FileInfo[] files = directoryInfo.GetFiles();
				List<string> fileNames = new List<string>();

				foreach (FileInfo file in files)
				{
					fileNames.Add(file.FullName);
				}

				foreach (string fileName in fileNames)
				{
					Puzzle puzzle = ReadFile(fileName);
					if (puzzle != null)
					{
						puzzles.Add(puzzle);
					}
				}
			}

			else
			{
				while (DX.ScreenFlip() == 0 && DX.ProcessMessage() == 0 && DX.ClearDrawScreen() == 0 && !DX.CheckHitKey(DX.KeyInput.Escape))
				{
					if (Network.ProconPuzzleService.IsQrCodeReceived) { break; }
				}
				if (Network.ProconPuzzleService.IsQrCodeReceived)
				{
					Puzzle puzzle = read.ReadFromQRCode(Network.ProconPuzzleService.QrCode);
					if (puzzle != null) { puzzles.Add(puzzle); }
				}
			}
			return puzzles;
		}

		/// <summary>
		/// 問題ファイルを読み込みます
		/// </summary>
		/// <param name="FilePath">読み込むファイルパス</param>
		/// <returns>読み込んだPuzzleの参照先 (失敗時はnullを返す) </returns>
		static Puzzle ReadFile(string FilePath)
		{
			try
			{
				read.ReadFile(FilePath);
				return read.ReadFile(FilePath);
			}
			catch (Exception ex)
			{
				DX.WriteLineDx("File Input Error.\n{0}", ex);
				DX.ScreenFlip();
				if (MessageBox.Show(MainWindow, $"{FilePath}が読み込めませんでした．\n別のファイルを読み込みますか？", "エラー", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
				{
					OpenFileDialog openFileDialog = new OpenFileDialog();
					openFileDialog.InitialDirectory = Environment.CurrentDirectory;
					openFileDialog.Filter = "All Files|*.*";
					openFileDialog.FilterIndex = 0;
					openFileDialog.Title = "問題ファイルをえらんでね";
					openFileDialog.Multiselect = false;
					if (openFileDialog.ShowDialog() == DialogResult.OK)
						return ReadFile(openFileDialog.FileName);
				}
				while (DX.ProcessMessage() == DX.Result.Success)
				{
					if (DX.GetMouseInput() == DX.MouseInput.Left)
						break;
				}
				DX.Finalize();
				return null;
			}
		}
	}
}