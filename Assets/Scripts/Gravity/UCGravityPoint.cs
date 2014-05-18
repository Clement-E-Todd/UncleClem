using UnityEngine;
using System.Collections;

public class UCGravityPoint : UCGravity {

	public float strength = 0.98f;
	public float range = 25.0f;
	public bool fadeAtEdgeOfRange = false;


	void Update ()
	{
		for (int i = UCObject.objectsInScene.Count-1; i >= 0; i--) {
			if (UCObject.objectsInScene [i].gameObject.activeInHierarchy) {
				if (Vector3.Distance(transform.position, UCObject.objectsInScene [i].transform.position) < range)
					UCObject.objectsInScene [i].gravitySources.Add (this);
			}
		}
	}
	
	public override Vector3 GetGravityForObject (UCObject target)
	{
		Vector3 gravity = (transform.position - target.transform.position).normalized * strength;
		if (fadeAtEdgeOfRange)
			gravity *= 1 - (Vector3.Distance(transform.position, target.transform.position) / range);

//		if (target.debugMessages) Debug.Log("Gravity: (" + gravity.x + ", " + gravity.y + ", " + gravity.z + ")");

		return gravity;
	}
}
