using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleSolver.Geometry
{
	public class RandomArray64		//Puzzleのハッシュ計算(ゾブリストハッシュ)で用いる乱数列. Program.csに実体を1つ置く.
	{
		private ulong[] num;
		private int minIndex, maxIndex;		//[座標値の最小値, 最大値]以上に大きな区間を取る

		private ulong y = 1;
		private ulong xorshift64()
		{
			y = y ^ (y << 13);
			y = y ^ (y >> 7);
			y = y ^ (y << 17);
			return y;
		}

		public RandomArray64(int minIndex, int maxIndex, ulong seed)
		{
			this.minIndex = minIndex;
			this.maxIndex = maxIndex;

			int n = this.maxIndex - this.minIndex + 1;
			num = new ulong[n];

			this.y = seed;
			for (int i = 0; i < n; i++)
			{
				num[i] = xorshift64();
			}
		}

		public ulong GetValue(int index)
		{
			return num[index - minIndex];
		}
	}
}
