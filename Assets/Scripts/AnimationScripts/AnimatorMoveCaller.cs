using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimatorMoveCaller : MonoBehaviour {

	private PlayerMoveCC player;
	void Start()
	{
		if (GameRuleManager.instance == null) { this.enabled = false; return; }
		player = GameRuleManager.instance.playerMove;
		if (player == null)
			this.enabled = false;
	}

	void OnAnimatorMove()
	{
		player?.OnPlayerMove();
	}
}
