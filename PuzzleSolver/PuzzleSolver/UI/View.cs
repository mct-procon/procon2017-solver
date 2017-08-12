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
	//パズルを表示します。t = (0, 0)のときの注視点は, 描画座標「ウィンドウ中央」, データ座標「(0, 0)」にあります。
	public class View
	{
		Point t;            //(画面の)平行移動. 注視点の移動量とは向きが逆なので注意. 平行移動 → 拡大の順で行う.
		double scale;       //拡大率
		int windowSizeX;    //横方向のウィンドウサイズ
		int windowSizeY;	//縦方向のウィンドウサイズ

		//コンストラクタ
		public View () { }
		public View (Point t, double scale, int windowSizeX, int windowSizeY)
		{
			this.t = t;
			this.scale = scale;
			this.windowSizeX = windowSizeX;
			this.windowSizeY = windowSizeY;
		}

		//表示クエリ
		public void Draw(Puzzle puzzle)
		{	
			for (int i = 0; i < puzzle.wakus.Count; i++) { DrawPoly(puzzle.wakus[i]); }
			for (int i = 0; i < puzzle.pieces.Count; i++) { DrawPoly(puzzle.pieces[i]); }		
		}

		//変更クエリ (現在はキーボード操作のみですが, そのうち, マウス操作も入れると思います。）
		public void Update()
		{
			//平行移動
			if (DX.CheckHitKey(DX.KeyInput.Up)) { t += new Point(0, 3) / scale; }
			if (DX.CheckHitKey(DX.KeyInput.Right)) { t += new Point(-3, 0) / scale; }
			if (DX.CheckHitKey(DX.KeyInput.Down)) { t += new Point(0, -3) / scale; }
			if (DX.CheckHitKey(DX.KeyInput.Left)) { t += new Point(3, 0) / scale; }

			//縮小／拡大
			if (DX.CheckHitKey(DX.KeyInput.Z)) { scale *= 0.99; }
			if (DX.CheckHitKey(DX.KeyInput.X)) { scale /= 0.99; }
		}

		//多角形を表示する
		private void DrawPoly(Poly poly)
		{
			DX.Color color;
			if (poly.isPiece) { color = new DX.Color(0, 255, 0); }
			else { color = new DX.Color(255, 0, 255); }

			for (int i = 0; i < poly.lines.Count; i++)
			{
				Point s = toDrawPoint(poly.lines[i].start);
				Point e = toDrawPoint(poly.lines[i].end);
				DX.DrawLine((int)s.Re, (int)s.Im, (int)e.Re, (int)e.Im, color, 2);
			}
		}

		//座標変換
		private Point toDrawPoint(Point cellPoint)
		{
			Point center = new Point(windowSizeX / 2, windowSizeY / 2);
			Point p = cellPoint + center + t;
			return (p - center) * scale + center;
		}
	}
}
