using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Collider))]

public class UCGravityZone : UCGravity
{
	public Vector3 force = new Vector3 (0, -0.98f, 0);
	
	public override Vector3 GetGravityForObject (UCObject target)
	{
		return force;
	}

	void OnTriggerEnter (Collider collider)
	{
		HandleCollision (collider);
	}
	
	void OnTriggerStay (Collider collider)
	{
		HandleCollision (collider);
	}
	
	void HandleCollision (Collider collider)
	{
		if (collider.GetComponent(typeof(UCObject))) {
			((UCObject)collider.GetComponent(typeof(UCObject))).gravitySources.Add(this);
		}
	}
}