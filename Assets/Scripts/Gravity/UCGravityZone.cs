using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(Collider))]

public class UCGravityZone : UCGravity
{
	public Vector3 force = new Vector3 (0, -0.98f, 0);
	List<Collider> collidersInRange = new List<Collider>();
	
	public override Vector3 GetGravityForObject (UCObject target)
	{
		return force;
	}

	void OnTriggerEnter (Collider collider)
	{
		collidersInRange.Add(collider);
	}
	
	void OnTriggerExit (Collider collider)
	{
		collidersInRange.Remove(collider);
	}
	
	void Update ()
	{
		foreach (Collider collider in collidersInRange) {
			if (collider.GetComponent(typeof(UCObject))) {
				((UCObject)collider.GetComponent(typeof(UCObject))).gravitySources.Add(this);
			}
		}
	}
}