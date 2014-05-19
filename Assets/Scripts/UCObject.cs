using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(CharacterController))]

public class UCObject : MonoBehaviour
{
	public static List<UCObject> objectsInScene = new List<UCObject> ();

	CharacterController characterController;
	public GameObject modelObject;
	protected Vector3 velocity;
	protected bool isGrounded = true;
	protected bool isSliding = false;
	protected Vector3 gravity = Vector3.zero;
	protected Vector3 movingPlatformVelocity = Vector3.zero;
	protected Vector3 groundNormal = Vector3.up;
	public const float slideSpeedLimit = 20;
	List<UCForce> appliedForces = new List<UCForce> ();
	public List<UCGravity> gravitySources = new List<UCGravity> ();
	public bool debugMessages = false;

	// TODO - Remove this temporary method of adding objects to the main camera targets
	public int tempCamWeight = 0;

	protected virtual void Start ()
	{
		objectsInScene.Add (this);
		characterController = (CharacterController)GetComponent (typeof(CharacterController));

		// Give gravity a head start so that the object can be considered "grounded" in its first frame if applicable
		velocity -= transform.up;

		// Start off with a tiny amount of forwad momentum to trick the collision check into initializing
		velocity += transform.forward * 0.01f;
	}

	protected virtual void Update ()
	{
		// TODO - Remove this temporary AND VERY INEFFICIENT method of adding objects to the main camera targets
		if (tempCamWeight > 0) {
			UCCamera.activeCamera.SetTargetWeight(gameObject, (ushort)tempCamWeight);
		}
	
		// Reduce velocity based on the friction of the ground
		velocity -= GetGroundResistance ();
		Vector3 horizontalVelocity = velocity - (Vector3.Dot (velocity, -gravity.normalized) * -gravity.normalized);
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
		characterController.Move (velocity * Time.deltaTime);

		// Disable sliding once we come to a stop
		if (horizontalVelocity.magnitude < 1)
			isSliding = false;

		// If we are not on the ground, our groundNormal should instead relfect an oposition of gravity
		if (!isGrounded && !isSliding)
			groundNormal = -gravity.normalized;

		// While on the ground, maintain a sensible downward velocity
		if (isGrounded) {
			Vector3 slope = groundNormal - (Vector3.Dot (groundNormal, gravity.normalized) * gravity.normalized);
			float slopeFactor = (horizontalVelocity != Vector3.zero) ? -Vector3.Dot (slope, horizontalVelocity.normalized) : 0.0f;

			if (slopeFactor < 0)	// downhill
				velocity = velocity - (Vector3.Dot (velocity, groundNormal) * groundNormal);
			else 	 				// uphill or level ground
				velocity = horizontalVelocity;
		}

		// Apply gravity
		ApplyGravity();

		// The transform's "up" vector should always be the opposite of gravity.
		transform.up = -gravity.normalized;
		characterController.transform.up = transform.up;

		// Debug - Draw Rays reflecting the transform
		Debug.DrawRay(transform.position, transform.up*5, Color.green, 0.0f, false);
		Debug.DrawRay(transform.position, transform.right*5, Color.red, 0.0f, false);
		Debug.DrawRay(transform.position, transform.forward*5, Color.blue, 0.0f, false);
	}

	// Collision event when THIS moves into a COLLIDER
	void OnControllerColliderHit (ControllerColliderHit hit)
	{
		// See what we hit
		Transform hitTarget = hit.collider.transform;

//		if (debugMessages)
//			Debug.Log("Slope: " + Vector3.Dot (hit.normal, -gravity.normalized) + ", Limit (Min): " + getSlopeLimit());

		if (Vector3.Dot (hit.normal, -gravity.normalized) > 0.1f &&
			Vector3.Dot (velocity.normalized, hit.normal) < 0) {

			groundNormal = hit.normal;

			if (Vector3.Dot (hit.normal, -gravity.normalized) > getSlopeLimit()) {
				if (!isSliding)
					isGrounded = true;
				UCForce force = new UCForce ();
				force.source = hitTarget;
				force.isFloor = true;
				force.point = hit.point;
				force.normal = -gravity.normalized;

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
				velocity = velocity - (Vector3.Dot (velocity, hit.normal) * hit.normal) + gravity;

				if (Vector3.Dot (hit.normal, -gravity.normalized) <= getSlopeLimit()) {
					Vector3 slideDirection = (gravity.normalized - (Vector3.Dot (gravity.normalized, hit.normal) * hit.normal)).normalized;
					Debug.DrawRay(transform.position, slideDirection*5, Color.blue, 0.0f, false);

					Vector3 originalVelocity = velocity;
					velocity += slideDirection * gravity.magnitude * (1 - Vector3.Dot (hit.normal, -gravity.normalized));
					if (velocity.magnitude > slideSpeedLimit) {
						velocity = velocity.normalized * Mathf.Max (originalVelocity.magnitude, slideSpeedLimit);
					}
				}
			}
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
		foreach (ContactPoint contact in collision.contacts) {
			UCForce force = new UCForce ();
			force.source = collision.collider.transform;
			force.isFloor = false;
			force.point = contact.point;
			force.normal = contact.normal;

			UCPlatform platform = (UCPlatform)collision.collider.GetComponent (typeof(UCPlatform));

			if (platform)
				platform.AssumePreviousTransform ();
			force.globalPoint = transform.position;
			force.localPoint = force.source.InverseTransformPoint (transform.position);
			force.globalRotation = modelObject.transform.rotation;
			force.localRotation = Quaternion.Inverse (force.source.rotation) * modelObject.transform.rotation;
			if (platform)
				platform.AssumeCurrentTransform ();

			appliedForces.Add (force);
		}
	}

	void ApplyForces ()
	{
		movingPlatformVelocity = Vector3.zero;

		while (appliedForces.Count > 0) {
			// Pop out of the force's source's collider if overlapping
			Vector3 centerPoint = transform.position + transform.up * characterController.center.y;
			if (Vector3.Distance (centerPoint, appliedForces [0].point) < characterController.radius * 0.9f) {
				transform.position += appliedForces [0].normal * (characterController.radius - Vector3.Distance (centerPoint, appliedForces [0].point));
			}

			// Get pushed or dragged by the force
			Vector3 newGlobalPoint = appliedForces [0].source.TransformPoint (appliedForces [0].localPoint);
			Vector3 moveDistance = (newGlobalPoint - appliedForces [0].globalPoint);
			if (moveDistance != Vector3.zero) {
				// Limit velocity based on the force's surface normal
				if (!appliedForces [0].isFloor) {
					velocity = velocity - (Vector3.Dot (velocity, appliedForces [0].normal) * appliedForces [0].normal) + gravity;
//					Debug.Log("Applied force pushed object.");
				}

				// Allow the force to move the object
				if (appliedForces [0].isFloor) {
					characterController.Move (moveDistance);
					movingPlatformVelocity += moveDistance / Time.deltaTime;
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
			appliedForces.RemoveAt (0);
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
			Vector3 horizontalVelocity = velocity - Vector3.Dot (velocity, -gravity.normalized) * -gravity.normalized;
			resistance = horizontalVelocity * Time.deltaTime * 10 * GetGroundFriction ();
		}

		return resistance;
	}

	void ApplyGravity()
	{
		if (gravitySources.Count > 0) {
			int gravityPriority = -1;
			while (gravitySources.Count > 0) {
				if (gravitySources [0].priority > gravityPriority) {
					gravityPriority = gravitySources [0].priority;
					gravity = Vector3.zero;
				}
				gravity += gravitySources [0].GetGravityForObject (this);
				gravitySources.RemoveAt (0);
			}
		} else {
			gravity *= 1 - Time.deltaTime*4;
		}
		velocity += gravity;
	}

	float getSlopeLimit() {
		return Mathf.Sin ((1 - ((float)characterController.slopeLimit / 90.0f)) * ((Mathf.PI / 2)));
	}
}
