using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UCCamera : MonoBehaviour {

	List<UCCameraTarget> targets = new List<UCCameraTarget>();

	public static UCCamera activeCamera = null;
	public bool isActiveCamera = false;
	protected bool wasActiveCamera = false;

	float zoomDistance = 10;
	public float zoomDistanceMin = 5;
	public float zoomDistanceMax = 15;
	public float heightToDistanceRatio = 0.5f;
	public float positionalBuffer = 3.0f;
	public float offsetStiffness = 0.5f;
	
	Vector3 targetPoint = Vector3.zero;
	Vector3 targetUp = Vector3.up;
	Vector3 offset = Vector3.zero;
	
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

		// Follow any targets assigned to this camera
		if (targets.Count > 0)
			FollowTarget();
	}

	void FollowTarget() {
		// Find the target position and up 
		targetPoint = Vector3.zero;
		targetUp = Vector3.zero;
		int totalWeight = 0;
		
		foreach (UCCameraTarget target in targets) {
			for (int i = 0; i < Mathf.Max(target.weightPercent,1); i++) {
				targetPoint += target.targetTransform.position;
				targetUp += target.targetTransform.up;
				totalWeight++;
			}
		}
		
		targetPoint /= totalWeight;
		targetUp /= totalWeight;
		targetUp = (targetUp != Vector3.zero) ? targetUp.normalized : transform.up;
		
		// Figure out our target position and move toward it
		Vector3 distance = transform.position - targetPoint;
		Vector3 distanceVertical = Mathf.Abs(Vector3.Dot(distance, targetUp)) * targetUp;
		Vector3 distanceHorizontal = distance - distanceVertical;
		
		Vector3 goalPosition = targetPoint+(distanceVertical.normalized*zoomDistance*heightToDistanceRatio + distanceHorizontal.normalized*zoomDistance);
		
		transform.position = Vector3.Lerp(transform.position, goalPosition, Time.deltaTime*2.5f);
		
		// Add some rigidity to the camera's movement
		if (offset != Vector3.zero)
			transform.position = Vector3.Lerp(transform.position, targetPoint+offset, offsetStiffness);
		offset = transform.position - targetPoint;
		
		// Find out our target rotation and turn toward it
		Quaternion currentRotation = transform.rotation;
		transform.LookAt(targetPoint, targetUp);
		transform.rotation = Quaternion.Slerp(currentRotation, transform.rotation, Time.deltaTime*10);
	}

	public void ZoomIn(float amount) {
		zoomDistance = Mathf.Clamp(zoomDistance-amount, zoomDistanceMin, zoomDistanceMax);
	}
	
	public void RotateAround(float amount) {
		Vector3 distance = transform.position - targetPoint;
		Vector3 distanceVertical = Mathf.Abs(Vector3.Dot(distance, targetUp)) * targetUp;
		Vector3 distanceHorizontal = distance - distanceVertical;
		
		transform.position = targetPoint + distanceVertical + Vector3.RotateTowards(distanceHorizontal,
																 					Vector3.Cross(distanceVertical, distanceHorizontal).normalized*zoomDistance,
		                                                        					-amount*Time.deltaTime*4, zoomDistance);
		transform.LookAt(targetPoint, targetUp);
	}
}
