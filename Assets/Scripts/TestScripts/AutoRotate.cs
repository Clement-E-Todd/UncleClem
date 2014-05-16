using UnityEngine;
using System.Collections;

public class AutoRotate : MonoBehaviour {

	bool activated = true;
	public bool reverse = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.R))
			activated = !activated;

		if (activated)
			transform.Rotate(new Vector3(0, reverse ? -1 : 1, 0));
	}
}
