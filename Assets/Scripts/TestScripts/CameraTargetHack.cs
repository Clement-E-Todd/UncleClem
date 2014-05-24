using UnityEngine;
using System.Collections;

public class CameraTargetHack : MonoBehaviour {

	public int weight = 0;
	
	void Update () {
		if (UCCamera.activeCamera)
			UCCamera.activeCamera.SetTargetWeight(gameObject, (ushort)weight);
	}
}
