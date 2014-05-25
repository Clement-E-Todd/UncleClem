using UnityEngine;
using System.Collections;

public class AutoRotate : MonoBehaviour {

	public Vector3 constantRotation = Vector3.up;

	void Update () {
		transform.Rotate(constantRotation);
	}
}
