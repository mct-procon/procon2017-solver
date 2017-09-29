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
        public List<Poly> wakus;									//クローンは実体. 枠の集合 (各枠のlinesの中身は空っぽにします)
		public List<Line> wakuLines;                                //枠の表示辺 (探索することを考えているので, 深いコピーを使って, メモリ独立な実体を作ります.)

		public List<List<Poly>> pieceTable { get; }					//クローンは参照. pieceTable[i][j] = ピースiのj番目の(回転, 反転フラグ)候補.
		public int nowDepth { get; private set; }					//ピースnowDepthから結合していく.

		public int initPieceNum { get; }				//最初のピースの個数 (Viewで使う)
		public int boardScore { get; private set; }		//盤面スコア.
		public long boardHash { get; private set; }     //盤面ハッシュ値. (枠穴の)頂点列に使われている線分の集合とハッシュ値を対応させている。

        //コンストラクタ
        public Puzzle() { }
        public Puzzle(List<Poly> wakus, List<Line> wakuLines, List<List<Poly>> pieceTable, int nowDepth, int initPieceNum)
        {
            this.wakus = wakus;
			this.wakuLines = wakuLines;
			this.pieceTable = pieceTable;
			this.nowDepth = nowDepth;
			this.initPieceNum = initPieceNum;
        }

		//nowDepthの更新. Solverクラスの多角形マージでのみ使用
		public void setNowDepth(int depth)
		{
			nowDepth = depth;
		}

		//boardScoreの更新. Solverクラスの多角形マージでのみ使用
		public void setBoardScore(int score)
		{
			boardScore = score;
		}

		//boardHashの更新. Solverクラスの多角形マージでのみ使用
		public void setBoardHash()
		{
			boardHash = GetHash();
		}

        //クローン(深いコピー)
        //evalNoteはまだ使っていないので, コピーしていません）
        public Puzzle Clone()
        {
			Puzzle ret = new Puzzle(new List<Poly>(this.wakus), new List<Line>(this.wakuLines), this.pieceTable, this.nowDepth, this.initPieceNum);
			for (int i = 0; i < ret.wakus.Count; i++) { ret.wakus[i] = ret.wakus[i].Clone(); }
			for (int i = 0; i < ret.wakuLines.Count; i++) { ret.wakuLines[i] = ret.wakuLines[i].Clone(); }
			ret.boardScore = this.boardScore;
			ret.boardHash = this.boardHash;
            return ret;
        }

		//ハッシュ値を計算する理由…Dictionaryでパズルを管理して重複を取り除きたいから。(あまりにも愚直にやると時間がかかりすぎる)
		//ハッシュ値の計算 (コンストラクタで1回だけ呼ぶ), Base進数, (Mod1, Mod2は素数)
		//lines(頂点列に使われている辺の集合)をローリングハッシュに変換する。
		//ゾブリストハッシュとは違い, 値の順序の違いを保存できるので, linesを「集合として同じなら同じ列になるように上手くソートして」, (lines1.start.Re, lines1.start.Im, …)と並べたもの1つ1つを文字として見た時の
		//ローリングハッシュ値を求める。
		private const long Base = 454559593;
		private const long Mod1 = 1000000007;
		private const long Mod2 = 1234567891;
		private long GetHash()
		{
			int i, j;
			List<Line> lines = new List<Line>();
			
			for (i = 0; i < wakus.Count; i++) { if (!wakus[i].isExist) continue; for (j = 0; j < wakus[i].Count; j++) { lines.Add(new Line(wakus[i].points[j], wakus[i].points[j + 1])); } }
			lines.Sort((a, b) => Point.Compare(a.start, b.start) != 0 ? Point.Compare(a.start, b.start) : Point.Compare(a.end, b.end));

			long mul1 = 1, sum1 = 0;
			long mul2 = 1, sum2 = 0;

			for (i = 0; i < lines.Count; i++)
			{
				//座標値を0.1刻みの精度で整数にする。値域は(-Base, Base)になっていること。
				long[] a = { (long)(lines[i].start.Re * 10), (long)(lines[i].start.Im * 10), (long)(lines[i].end.Re * 10), (long)(lines[i].end.Im * 10) };
				for (j = 0; j < 4; j++) { if (a[j] < 0) { a[j] += Base; } }

				//ローリングほげほげ
				for (j = 0; j < 4; j++)
				{
					sum1 = (sum1 + a[j] * mul1) % Mod1;
					sum2 = (sum2 + a[j] * mul2) % Mod2;
					mul1 *= Base; mul1 %= Mod1;
					mul2 *= Base; mul2 %= Mod2;
				}
			}

			//2つのmodで作ったハッシュを繋ぎ合わせて返す (これで衝突を回避)
			return sum1 * ((long)1 << 32) + sum2;
		}
    }
}
