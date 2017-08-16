using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuzzleSolver.Geometry;

namespace PuzzleSolver.Core
{
    public class Puzzle : ICloneable
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
            ret.wakus  = new List<Poly>(this.wakus.Count);
            ret.pieces = new List<Poly>(this.pieces.Count);
            for (int n = 0; n < this.wakus.Count; ++n)
                ret.wakus.Add(this.wakus[n]);
            for (int n = 0; n < this.pieces.Count; ++n)
                ret.pieces.Add(this.pieces[n]);
            return ret;
        }


        /// <summary>
        /// Deep Clone.(Called by ICloneable.Clone())
        /// </summary>
        /// <returns>Clone</returns>
        object ICloneable.Clone() {
            return Clone();
        }
    }
}
