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
		public List<Poly> Frames;		//実体. 枠の集合
		public List<Poly> Pieces;      //実体. ピースの集合
		EvalNote EvalNote;		//実体. 評価値のメモ
	}
}
