using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleSolver
{
	class Puzzle
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
	}
}
