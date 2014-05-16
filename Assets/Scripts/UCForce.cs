using UnityEngine;
using System.Collections;

// A structure which represents a force applied to a GrumpMovableObject by another collider, primarily moving platforms. 

public struct UCForce {
	public Transform source;
	public bool isFloor;
	public Vector3 normal;
	public Vector3 localPoint;
	public Vector3 globalPoint;
	public Quaternion localRotation;
	public Quaternion globalRotation;
}
