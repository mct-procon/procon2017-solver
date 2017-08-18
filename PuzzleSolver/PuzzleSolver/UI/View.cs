﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLib;
using PuzzleSolver.Core;
using PuzzleSolver.Geometry;

namespace PuzzleSolver.UI
{
    /// <summary>
    /// パズルを表示します。
    /// t = (0, 0)のときの注視点は, 描画座標「ウィンドウ中央」, データ座標「(0, 0)」にあります。
    /// </summary>
    public class View
    {
        /// <summary>
        /// ビューの中央座標
        /// (画面の)平行移動. 注視点の移動量とは向きが逆なので注意. 平行移動 → 拡大の順で行う.
        /// </summary>
        Point CenterPoint;

        /// <summary>
        /// 拡大率
        /// </summary>
        double Scale;
        /// <summary>
        /// ウィンドウの横幅
        /// </summary>
        int WindowSizeX;
        /// <summary>
        /// ウィンドウの高さ
        /// </summary>
        int WindowSizeY;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public View () { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="t">ビューの中央座標</param>
        /// <param name="scale">ビューの拡大率</param>
        /// <param name="windowSizeX">メインウィンドウの幅</param>
        /// <param name="windowSizeY">メインウィンドウの高さ</param>
        public View (Point t, double scale, int windowSizeX, int windowSizeY)
        {
            this.CenterPoint = t;
            this.Scale = scale;
            this.WindowSizeX = windowSizeX;
            this.WindowSizeY = windowSizeY;
        }

        /// <summary>
        /// 描画クエリ
        /// </summary>
        /// <param name="puzzle">描画するパズル</param>
        public void Draw(Puzzle puzzle)
        {	
			//線分
			for (int i = 0; i < puzzle.wakus.Count; i++) { if (!puzzle.wakus[i].isExist) continue; for (int j = 0; j < puzzle.wakus[i].lines.Count; j++) { DrawLine(puzzle.wakus[i].lines[j]); } }
			for (int i = 0; i < puzzle.pieces.Count; i++) { if (!puzzle.pieces[i].isExist) continue; for (int j = 0; j < puzzle.pieces[i].lines.Count; j++) { DrawLine(puzzle.pieces[i].lines[j]); } }
			//頂点列
            for (int i = 0; i < puzzle.wakus.Count; i++) { if (!puzzle.wakus[i].isExist) continue; DrawPoly(puzzle.wakus[i]); }
            for (int i = 0; i < puzzle.pieces.Count; i++) { if (!puzzle.pieces[i].isExist) continue; DrawPoly(puzzle.pieces[i]); }

			//サイジング多角形（デバッグ）
			/*for (int i = 0; i < puzzle.wakus.Count; i++)
			{
				List<Point> points = puzzle.wakus[i].SizingPoly();
				Poly poly = new Poly(points, new List<Line>(), false);
				DrawPoly(poly);
			}*/
        }

        /// <summary>
        /// 更新クエリ
        /// （現在はキーボード操作のみですが, そのうち, マウス操作も入れると思います。）
        /// </summary>
        public void UpdateDrawInfo()
        {
            //平行移動
            if (DX.CheckHitKey(DX.KeyInput.Up)) { CenterPoint += new Point(0, 3) / Scale; }
            if (DX.CheckHitKey(DX.KeyInput.Right)) { CenterPoint += new Point(-3, 0) / Scale; }
            if (DX.CheckHitKey(DX.KeyInput.Down)) { CenterPoint += new Point(0, -3) / Scale; }
            if (DX.CheckHitKey(DX.KeyInput.Left)) { CenterPoint += new Point(3, 0) / Scale; }

            //縮小／拡大
            if (DX.CheckHitKey(DX.KeyInput.Z)) { Scale *= 0.99; }
            if (DX.CheckHitKey(DX.KeyInput.X)) { Scale /= 0.99; }
        }

        /// <summary>
        /// 多角形の描画クエリ
        /// Draw(Puzzle)の中で呼ばれます
        /// </summary>
        /// <param name="poly">描画する多角形</param>
        private void DrawPoly(Poly poly)
        {
            DX.Color color;
            if (poly.isPiece) { color = new DX.Color(255, 0, 0); }
            else { color = new DX.Color(255, 0, 255); }

            for (int i = 0; i < poly.Count; i++)
            {
                Point s = toDrawPoint(poly.points[i]);
                Point e = toDrawPoint(poly.points[i + 1]);
                DX.DrawLine((int)s.Re, (int)s.Im, (int)e.Re, (int)e.Im, color, 2);
				DX.DrawString((float)s.Re, (float)s.Im, 255, i.ToString());
            }
        }

		//線分の描画
		private void DrawLine(Line line)
		{
			DX.Color color = new DX.Color(0, 255, 0);

			Point s = toDrawPoint(line.start);
			Point e = toDrawPoint(line.end);
			DX.DrawLine((int)s.Re, (int)s.Im, (int)e.Re, (int)e.Im, color, 2);
		}

        /// <summary>
        /// 座標変換
        /// </summary>
        /// <param name="cellPoint"></param>
        /// <returns></returns>
        private Point toDrawPoint(Point cellPoint)
        {
            Point center = new Point(WindowSizeX / 2, WindowSizeY / 2);
            Point p = cellPoint + center + CenterPoint;
            return (p - center) * Scale + center;
        }
    }
}
