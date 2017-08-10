using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleSolver
{
	class Backup
	{
		private Stack<Puzzle> stack;

		//要素を入れる
		public void Push(Puzzle puzzle)
		{
			stack.Push(puzzle);
		}

		//最後に入れた要素を取り出す. (Stackの中身は変化しない)
		public Puzzle Peek()
		{
			return stack.Peek();
		}

		//要素数が2以上のとき, 最後に入れた要素を削除する.
		public void Pop()
		{
			if (stack.Count >= 2)
			{
				stack.Pop();
			}
		}
	}
}
