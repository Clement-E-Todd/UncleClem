using UnityEngine;
using System.Collections;

public class UCPlatform : MonoBehaviour {

	public float friction = 1.0f;

	public Vector3 constantVelocity = new Vector3(0,0,0);
	public Vector3 constantAngularVelocity = new Vector3(0,0,0);

	Vector3 currentPosition;
	Vector3 previousPosition;

	Quaternion currentRotation;
	Quaternion previousRotation;

	void Update () {
		previousPosition = currentPosition;
		previousRotation = currentRotation;

		transform.Translate(constantVelocity, Space.World);
		transform.Rotate(constantAngularVelocity, Space.Self);

		currentPosition = transform.position;
		currentRotation = transform.rotation;
	}

	public Vector3 GetPushAmount (Vector3 globalStartPoint) {

		Vector3 localStartPoint = transform.InverseTransformPoint(globalStartPoint);

		Vector3 originalPosition = transform.position;
		Quaternion originalRotation = transform.rotation;

		transform.position += constantVelocity;
		Vector3 eulerRotation = transform.rotation.eulerAngles + constantAngularVelocity;
		transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, eulerRotation.z);
		
		Vector3 newGlobalPoint = transform.TransformPoint(localStartPoint);

		transform.position = originalPosition;
		transform.rotation = originalRotation;

		return (newGlobalPoint - globalStartPoint) / Time.deltaTime;
	}

	public void AssumePreviousTransform () {
		transform.position = previousPosition;
		transform.rotation = previousRotation;
	}

	public void AssumeCurrentTransform () {
		transform.position = currentPosition;
		transform.rotation = currentRotation;
	}

}
