using UnityEngine;
using System.Collections;

public class UCGravityGlobal : UCGravity
{
	public Vector3 force = new Vector3 (0, -0.98f, 0);

	void Update ()
	{
		for (int i = UCObject.objectsInScene.Count-1; i >= 0; i--) {
			if (UCObject.objectsInScene [i].gameObject.activeInHierarchy)
				UCObject.objectsInScene [i].gravitySources.Add (this);
		}
	}

	public override Vector3 GetGravityForObject (UCObject target)
	{
		return force;
	}
}
