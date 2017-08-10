using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLib;
using PuzzleSolver.Core;

namespace PuzzleSolver.UI
{
	public class Controller
	{
		public Backup History;	//参照. パズルをStackで管理.
		public View View;		//実体. 描画.
		public Solve Solver;	//実体. 回答.

		Controller(Backup backup)
		{
			this.History	= backup;
			View	    	= new View();
			Solver  		= new Solve();
		}
	}
}
