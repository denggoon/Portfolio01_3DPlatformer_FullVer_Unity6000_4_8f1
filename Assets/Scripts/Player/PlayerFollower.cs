using UnityEngine;
using System.Collections;

public class PlayerFollower : MonoBehaviour {

	void Update ()
	{
		if(GameRuleManager.instance.playerMove == null) return;

		this.transform.position = GameRuleManager.instance.playerMove.transform.position;
	}
}
