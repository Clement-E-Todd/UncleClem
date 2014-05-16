using UnityEngine;
using System.Collections;

public class SimpleFollowCam : MonoBehaviour {

	public GameObject target;
	Vector3 distanceToTarget;

	void Start () {
		if (target) {
			distanceToTarget = transform.position - target.transform.position;
		}
	}

	void Update () {
		transform.position = Vector3.Lerp(transform.position, target.transform.position + distanceToTarget, Time.deltaTime*5) ;
	}
}
