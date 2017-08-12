using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleSolver.Core
{
    /// <summary>
    /// 履歴管理
    /// </summary>
	public class Backup
	{
        /// <summary>
        /// 履歴の実体
        /// </summary>
		private Stack<Puzzle> History;

        /// <summary>
		/// 要素を入れる
        /// </summary>
        /// <param name="puzzle"></param>
        public void Push(Puzzle puzzle)
		{
            History.Push(puzzle);
		}

        /// <summary>
        /// 最後に入れた要素を取り出す. (Stackの中身は変化しない)
        /// </summary>
        /// <returns></returns>
        public Puzzle Peek()
		{
			return History.Peek();
		}

        /// <summary>
        /// 要素数が2以上のとき, 最後に入れた要素を削除する.
        /// </summary>
        public void Pop()
		{
			if (History.Count >= 2)
			{
                History.Pop();
			}
		}

		//要素を空にする
		public void Clear()
		{
			History.Clear();
		}
	}
}
