﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (CharacterController))]

public class UCObject : MonoBehaviour
{
	// Physics viariables
	CharacterController characterController;
	public GameObject modelObject;
	protected Vector3 velocity;
	protected bool isGrounded = true;
	protected bool isSliding = false;
	protected Vector3 gravityNormal = new Vector3 (1, -1, 0).normalized;
	protected float gravityStrength = 0.98f;
	protected Vector3 movingPlatformVelocity = Vector3.zero;
	protected Vector3 groundNormal = Vector3.up;
	public const float slideSpeedLimit = 20;
	Vector3 previousPosition;
	List<UCForce> appliedForces = new List<UCForce> ();

	public bool debugMessages = false;

	protected virtual void Start ()
	{
		characterController = (CharacterController)GetComponent (typeof(CharacterController));

		// Give gravity a head start so that the object can be considered "grounded" in its first frame if applicable
		velocity += gravityNormal * gravityStrength;
	}

	protected virtual void Update ()
	{
		// Reduce velocity based on the friction of the ground
		velocity -= GetGroundResistance ();
		Vector3 horizontalVelocity = velocity - (Vector3.Dot (velocity, -gravityNormal) * -gravityNormal);
		if (horizontalVelocity.magnitude < 1)
			velocity -= horizontalVelocity;

		// Be moved by any colliders that are applying force against us
		ApplyForces ();

		// Debug messages written to the console
		if (debugMessages) {
//			Debug.Log("Velocity: (" + velocity.x + ", " + velocity.y + ", " + velocity.z + ") Magnitude: " + velocity.magnitude);
		}

		// Move the character controller
		isGrounded = false;
		previousPosition = transform.position;
		characterController.Move (velocity * Time.deltaTime);

		// Disable sliding once we come to a stop
		if (horizontalVelocity.magnitude < 1)
			isSliding = false;

		// If we are not on the ground, our groundNormal should instead relfect an oposition of gravity
		if (!isGrounded && !isSliding)
			groundNormal = -gravityNormal;

		// While on the ground, maintain a sensible downward velocity
		if (isGrounded) {
			Vector3 slope = groundNormal - (Vector3.Dot (groundNormal, gravityNormal) * gravityNormal);
			float slopeFactor = (horizontalVelocity != Vector3.zero) ? -Vector3.Dot(slope, horizontalVelocity.normalized) : 0.0f;
			
			if (slopeFactor < 0)	// downhill
				velocity = velocity - (Vector3.Dot (velocity, groundNormal) * groundNormal);
			else 	 				// uphill or level ground
				velocity = horizontalVelocity;
		}

		// Apply gravity
		velocity += gravityNormal * gravityStrength;

		// The transform's "up" vector should always be the opposite of gravity. 
		transform.up = -gravityNormal;
		characterController.transform.up = transform.up;
	}

	// Collision event when THIS moves into a COLLIDER
	void OnControllerColliderHit (ControllerColliderHit hit)
	{
		// See what we hit
		Transform hitTarget = hit.collider.transform;

		// See if the hit target is a platform that we are now standing on
		if (Vector3.Dot(gravityNormal, hit.moveDirection) > 0.9f) {

			groundNormal = hit.normal;
			float slopeLimit = Mathf.Sin((1 - ((float)characterController.slopeLimit/90.0f)) * ((Mathf.PI/2)));

			if (Vector3.Dot(hit.normal, -gravityNormal) > slopeLimit) {
				if (!isSliding) isGrounded = true;
				UCForce force = new UCForce ();
				force.source = hitTarget;
				force.isFloor = true;
				force.normal = -gravityNormal;

				force.globalPoint = transform.position;
				force.localPoint = force.source.InverseTransformPoint (transform.position);
				force.globalRotation = modelObject.transform.rotation;
				force.localRotation = Quaternion.Inverse (force.source.rotation) * modelObject.transform.rotation;

				appliedForces.Add (force);
			} else {
				// If the slope is too steep, slide down
				isSliding = true;
			}

			if (isSliding) {
				velocity = velocity - (Vector3.Dot (velocity, hit.normal) * hit.normal) + (gravityNormal * gravityStrength);

				if (Vector3.Dot(hit.normal, -gravityNormal) <= slopeLimit) {
					Vector3 slideDirection = (gravityNormal - (Vector3.Dot (gravityNormal, hit.normal) * hit.normal)).normalized;

					Vector3 originalVelocity = velocity;
					velocity += slideDirection * gravityStrength * (1 - Vector3.Dot(hit.normal, -gravityNormal));
					if (velocity.magnitude > slideSpeedLimit) {
						velocity = velocity.normalized * Mathf.Max (originalVelocity.magnitude, slideSpeedLimit);
					}
				}
			}
		}

		// Move the character controller just outside of the collider if they are overlapped
		Vector3 distanceMoved = transform.position - previousPosition;
		if (distanceMoved.magnitude > hit.moveLength) {
			previousPosition = transform.position;
			characterController.Move (hit.normal * (hit.moveLength - distanceMoved.magnitude));
		}
	}

	// Collision events when a COLLIDER moves into THIS
	void OnCollisionEnter (Collision collision)
	{
		HandleCollision (collision);
	}

	void OnCollisionStay (Collision collision)
	{
		HandleCollision (collision);
	}

	void HandleCollision (Collision collision)
	{
		// Get pushed by a moving platform that we are not standing on
		UCPlatform platform = (UCPlatform)collision.collider.GetComponent (typeof(UCPlatform));
		if (platform) {
			Vector3 averageNormal = new Vector3 (0, 0, 0);
			foreach (ContactPoint contact in collision.contacts) {
				averageNormal += contact.normal;
			}
			averageNormal = averageNormal.normalized;

			// Since the collider hit the character, it should apply force to it
			UCForce force = new UCForce ();
			force.source = collision.collider.transform;
			force.isFloor = false;
			force.normal = averageNormal;

			platform.AssumePreviousTransform ();
			force.globalPoint = transform.position;
			force.localPoint = force.source.InverseTransformPoint (transform.position);
			force.globalRotation = modelObject.transform.rotation;
			force.localRotation = Quaternion.Inverse (force.source.rotation) * modelObject.transform.rotation;
			platform.AssumeCurrentTransform ();

			appliedForces.Add (force);
		}
	}

	void ApplyForces ()
	{
		movingPlatformVelocity = Vector3.zero;

		while (appliedForces.Count > 0) {
			// Follow the position of the force's source
			Vector3 newGlobalPoint = appliedForces [0].source.TransformPoint (appliedForces [0].localPoint);
			Vector3 moveDistance = (newGlobalPoint - appliedForces [0].globalPoint);
			if (moveDistance != Vector3.zero) {
				// Limit velocity based on the force's surface normal
				if (!appliedForces [0].isFloor) {
					velocity = velocity - (Vector3.Dot (velocity, appliedForces [0].normal) * appliedForces [0].normal) + (gravityNormal * gravityStrength);
//					Debug.Log("Applied force pushed object.");
				}

				// Allow the force to move the object
				if (appliedForces [0].isFloor) {
					previousPosition = transform.position;
					characterController.Move (moveDistance);
					movingPlatformVelocity += moveDistance / Time.deltaTime;
//					if (moveDistance.magnitude != 0)
//						Debug.Log("Object moved by ground underneath it.");
				} else {
					velocity += appliedForces [0].normal * moveDistance.magnitude / Time.deltaTime;
				}
			}
			
			// Follow the rotation of the force's source
			Quaternion newGlobalRotation = appliedForces [0].source.rotation * appliedForces [0].localRotation;
			Quaternion rotationDiff = newGlobalRotation * Quaternion.Inverse (appliedForces [0].globalRotation);
			rotationDiff = Quaternion.FromToRotation (rotationDiff * modelObject.transform.up, modelObject.transform.up) * rotationDiff;
			modelObject.transform.rotation = rotationDiff * modelObject.transform.rotation;

			// Now that we've used the force it is no longer valid and must be registered again if it is still applicable
			appliedForces.Remove (appliedForces [0]);
		}
	}

	protected float GetGroundFriction ()
	{
		int totalFloors = 0;
		float friction = 0.0f;
		
		foreach (UCForce force in appliedForces) {
			if (force.isFloor && force.source.GetComponent (typeof(UCPlatform)) != null) {
				friction += ((UCPlatform)force.source.GetComponent (typeof(UCPlatform))).friction;
			} else {
				friction += 1.0f;
			}
			totalFloors++;
		}

		if (totalFloors > 0)
			friction /= totalFloors;

		if (isSliding)
			friction *= 0.35f;

//		Debug.Log ("Friction: " + friction);
		return friction;
	}

	protected Vector3 GetGroundResistance ()
	{
		Vector3 resistance = Vector3.zero;

		if (isGrounded || isSliding) {
			Vector3 horizontalVelocity = velocity - Vector3.Dot (velocity, -gravityNormal) * -gravityNormal;
			resistance = horizontalVelocity * Time.deltaTime * 10 * GetGroundFriction ();
		}

		return resistance;
	}
}
