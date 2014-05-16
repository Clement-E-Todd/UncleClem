using UnityEngine;
using System.Collections;

public class SimpleFollowCam : MonoBehaviour {

	public GameObject target;
	public int mode = 0;
	Vector3 distanceToTarget;

	void Start () {
		if (target) {
			distanceToTarget = transform.position - target.transform.position;
		}
	}

	void Update () {
		if (mode == 0) {
			transform.position = Vector3.Lerp(transform.position, target.transform.position + distanceToTarget, Time.deltaTime*5) ;
		} else if (mode == 1) {
			transform.position = new Vector3(target.transform.position.x, 20, target.transform.position.z + 20);
			transform.rotation = Quaternion.LookRotation((target.transform.position - transform.position).normalized, transform.up);
		}
	}
}
