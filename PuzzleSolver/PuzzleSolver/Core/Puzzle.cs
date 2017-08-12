﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuzzleSolver.Geometry;

namespace PuzzleSolver.Core
{
	public class Puzzle
	{
		public List<Poly> wakus = new List<Poly>();		//実体. 枠の集合
		public List<Poly> pieces = new List<Poly>();		//実体. ピースの集合
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
