using UnityEngine;
using System.Collections;

public class FollowTarget : MonoBehaviour {

	public string RunTimeTarget;
	public Transform target;

	public float distance = 2F;
	public float height = 2F;

	void LateUpdate()
	{
		if(target == null) return;

		this.transform.position = new Vector3(target.transform.position.x + distance, target.transform.position.y + height, target.transform.position.z);
	}
}
