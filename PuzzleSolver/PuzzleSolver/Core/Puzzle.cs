using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuzzleSolver.Geometry;

namespace PuzzleSolver.Core
{
    public class Puzzle
    {
        public List<Poly> wakus;		//実体. 枠の集合 (各枠のlinesの中身は空っぽにします)
        public List<Poly> pieces;		//実体. ピースの集合
        public EvalNote evalNote;       //実体. 評価値のメモ
		public List<Line> wakuLines;    //枠の表示辺 (探索することを考えているので, 深いコピーを使って, メモリ独立な実体を作ります.)
		public int initPieceNum { get; private set; } //最初のピースの個数 (View, 多角形マージ時の更新で使う)

        //コンストラクタ
        public Puzzle() { }
        public Puzzle(List<Poly> wakus, List<Poly> pieces, EvalNote evalNote, List<Line> wakuLines)
        {
            this.wakus = wakus;
            this.pieces = pieces;
            this.evalNote = evalNote;
			this.wakuLines = wakuLines;
        }

		//ピース数. Readクラスでのみ使用.
		public void setInitPieceNum(int num)
		{
			initPieceNum = num;
		}

        //クローン(深いコピー)
        //evalNoteはまだ使っていないので, コピーしていません）
        public Puzzle Clone()
        {
            Puzzle ret = new Puzzle();
			ret.wakus = new List<Poly>(this.wakus);
			ret.pieces = new List<Poly>(this.pieces);
			ret.wakuLines = new List<Line>(this.wakuLines);
			for (int i = 0; i < ret.wakus.Count; i++) { ret.wakus[i] = ret.wakus[i].Clone(); }
			for (int i = 0; i < ret.pieces.Count; i++) { ret.pieces[i] = ret.pieces[i].Clone(); }
			for (int i = 0; i < ret.wakuLines.Count; i++) { ret.wakuLines[i] = ret.wakuLines[i].Clone(); }
			ret.initPieceNum = this.initPieceNum;
            return ret;
        }
    }
}
