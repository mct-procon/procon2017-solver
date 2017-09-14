using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuzzleSolver.Core;

namespace PuzzleSolver.Geometry
{
	public class SkewHeapNode
	{
		public SkewHeapNode l, r;
		public Puzzle val;

		public SkewHeapNode(Puzzle puzzle)
		{
			val = puzzle;
			l = null;
			r = null;
		}

		public SkewHeapNode(SkewHeapNode l, SkewHeapNode r, Puzzle puzzle)
		{
			this.l = l;
			this.r = r;
			this.val = puzzle;
		}
	}
}
