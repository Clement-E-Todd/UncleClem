using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UCCamera : MonoBehaviour {

	public static UCCamera activeCamera = null;

	public bool isActiveCamera = false;
	protected bool wasActiveCamera = false;

	List<UCCameraTarget> targets = new List<UCCameraTarget>();

	public Vector3 offsetDirection = new Vector3(0, 7.5f, 10).normalized;
	float offsetDistance = 20;
	public float minDistance = 15;
	public float maxDistance = 30;
	public float zoomSpeed = 2.0f;
	public float centeringLenience = 0.0f;
	public Vector3 targetUp;
	
	public float stiffness = 0.083f;
	public bool matchUpVector = false;

	void Update () {
		// If the active camera flag has just been set, unset it for previous active camera
		if (isActiveCamera && !wasActiveCamera) {
			if (activeCamera) {
				activeCamera.isActiveCamera = false;
				activeCamera.wasActiveCamera = false;
			}
			activeCamera = this;
			wasActiveCamera = true;
		}

		// Follow the transform of the targets
		Vector3 targetPoint = Vector3.zero;
		int totalWeight = 0;

		foreach (UCCameraTarget target in targets) {
			for (int i = 0; i < target.weightPercent; i++) {
				targetPoint += target.targetTransform.position;
				if (matchUpVector) targetUp += target.targetTransform.up;
				totalWeight++;
			}
		}

		if (totalWeight == 0)
			return;

		targetPoint /= totalWeight;
		if (matchUpVector) {
			targetUp /= totalWeight;
			targetUp.Normalize ();
		} else {
			targetUp = transform.up;
		}
		
		// Zoom in or out to try and capture all targets in the shot
		float distanceBetweenTargets = 0;
		// Project each target point onto the screen and find the greatest distance between any two of them
		for (int left = 0; left < targets.Count; left++) {
			for (int right = left+1; right < targets.Count; right++) {
				distanceBetweenTargets = Mathf.Max (distanceBetweenTargets,
				                                    Vector3.Distance(targets[left].targetTransform.position - (Vector3.Dot (targets[left].targetTransform.position, offsetDirection) * offsetDirection),
				                 									 targets[right].targetTransform.position - (Vector3.Dot (targets[right].targetTransform.position, offsetDirection) * offsetDirection)));
			}
		}
		
		float goalDistance = distanceBetweenTargets*1.5f;
		goalDistance = Mathf.Clamp(goalDistance, minDistance, maxDistance);
		offsetDistance = Mathf.Lerp(offsetDistance, goalDistance, Time.deltaTime*zoomSpeed);
		
		// Finally, update the camera's transform
		transform.position = targetPoint + (offsetDirection*offsetDistance);
		
		
		// Center the screen on the target somewhat
		Vector3 screenPlane = transform.position - (Vector3.Dot (transform.position, transform.forward) * transform.forward);
		Vector3 onScreenPos = targetPoint - (Vector3.Dot (targetPoint, transform.forward) * transform.forward);
		Vector3 distanceFromCenter = (onScreenPos-screenPlane) - (onScreenPos-screenPlane).normalized*centeringLenience;
		Vector3 centeredPosition = transform.position + distanceFromCenter*Mathf.Clamp(stiffness, 0, 1);
		offsetDirection = (centeredPosition-targetPoint).normalized;
		
		transform.up = targetUp;
		transform.forward = -offsetDirection;
		Debug.Log("Offset Direction: (" + offsetDirection.x + ", " + offsetDirection.y + ", " + offsetDirection.z + ")");
	}

	public void SetTargetWeight(GameObject target, ushort weightPercent) {
		
		for (int i = 0; i < targets.Count; i++) {
			if (targets[i].targetTransform == target.transform) {
				targets.Remove(targets[i]);
				break;
			}
		}
		
		if (weightPercent > 0) {
			UCCameraTarget newTarget = new UCCameraTarget();
			newTarget.targetTransform = target.transform;
			newTarget.weightPercent = weightPercent;
			targets.Add(newTarget);
		}
	}
}
