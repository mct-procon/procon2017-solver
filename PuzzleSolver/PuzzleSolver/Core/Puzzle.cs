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
		public List<Poly> wakus;		//実体. 枠の集合
		public List<Poly> pieces;		//実体. ピースの集合
		public EvalNote evalNote;		//実体. 評価値のメモ

		//コンストラクタ
		public Puzzle() { }
		public Puzzle(List<Poly> wakus, List<Poly> pieces, EvalNote evalNote)
		{
			this.wakus = wakus;
			this.pieces = pieces;
			this.evalNote = evalNote;
		}

		//クローン(深いコピー)
		//evalNoteはまだ使っていないので, コピーしていません）
		public Puzzle Clone()
		{
			Puzzle ret = new Puzzle();
			ret.wakus  = new List<Poly>(this.wakus);
			ret.pieces = new List<Poly>(this.pieces);
			for (int i = 0; i < ret.wakus.Count; i++) { ret.wakus[i] = ret.wakus[i].Clone(); }
			for (int i = 0; i < ret.pieces.Count; i++) { ret.pieces[i] = ret.pieces[i].Clone(); }
			return ret;
		}
	}
}
