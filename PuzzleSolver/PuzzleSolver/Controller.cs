using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLib;

namespace PuzzleSolver
{
	class Controller
	{
		Backup backup;	//参照. パズルをStackで管理.
		View view;		//実体. 描画.
		Solve solve;	//実体. 回答.

		Controller(Backup backup)
		{
			this.backup	= backup;
			view		= new View();
			solve		= new Solve();
		}
	}
}
