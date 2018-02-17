using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tower
{
	public enum GameStatus 
	{
		ConnectionWait = 1 << 0,
		PlayerTurn = 1 << 1,
		EnemyTurn = 1 << 2,
	};

}