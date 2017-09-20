using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuzzleSolver.Core;

namespace PuzzleSolver.Geometry
{
	//Nはデータ数.
	//追加：ならしO(logN)
	//最小値の削除：ならしO(logN)
	//最小値の取得：O(1)
	//最大値の取得：O(N)
	//ちなみに, Puzzle.boardScoreをキーにしています。
	public class SkewHeap
	{
		public SkewHeapNode root;
		public int Count { get; private set; }

		//空のヒープを作る
		public SkewHeap()
		{
			root = null;
			Count = 0;
		}

		//2つのヒープをマージする (ここの実装が本質！）
		private SkewHeapNode Meld(SkewHeapNode a, SkewHeapNode b)
		{
			if (a == null) return b;
			if (b == null) return a;
			if (a.val.boardScore > b.val.boardScore) Generic.Swap<SkewHeapNode>(ref a, ref b);
			a.r = Meld(a.r, b);
			Generic.Swap<SkewHeapNode>(ref a.l, ref a.r);
			return a;
		}

		//要素xを追加
		public void Push(Puzzle x)
		{
			root = Meld(root, new SkewHeapNode(x));
			Count++;
		}

		//Puzzle.boardScoreが最小となるPuzzleを取得
		public Puzzle MinValue()
		{
			return root.val;
		}

		//最小要素(root)を削除
		public Puzzle Pop()
		{
			Puzzle ret = root.val;
			root = Meld(root.l, root.r);
			Count--;
			return ret;
		}


		//最大値を取得
		public Puzzle MaxValue()
		{
			return MaxValueSub(root);
		}

		public Puzzle MaxValueSub(SkewHeapNode a)
		{
			if (a == null) { return null; }

			Puzzle ret = MaxValueSub(a.l);
			Puzzle res = MaxValueSub(a.r);
			if (res != null && (ret == null || ret.boardScore < res.boardScore)) { ret = res; }
			if (ret == null || ret.boardScore < a.val.boardScore) { ret = a.val; }
			return ret;
		}

		//浅いクローン (node.val(Puzzle)の実体は複製せず参照値をコピー) (木の各ノードを複製する)
		public SkewHeap CloneShallow()
		{
			SkewHeapNode node = CloneShallowSub(root);
			SkewHeap ret = new SkewHeap();
			ret.root = node;
			ret.Count = Count;
			return ret;
		}

		private SkewHeapNode CloneShallowSub(SkewHeapNode a)
		{
			if (a == null) { return null; }
			return new SkewHeapNode(CloneShallowSub(a.l), CloneShallowSub(a.r), a.val);
		}
	}
}
