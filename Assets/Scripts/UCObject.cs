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
	protected Vector3 constantVelocity;
	protected bool isGrounded = true;
	protected bool isSliding = false;
	protected Vector3 gravity = Vector3.zero;
	protected Vector3 movingPlatformVelocity = Vector3.zero;
	bool movedByPlatform = false;
	protected Vector3 groundNormal = Vector3.up;
	protected float groundFriction = 1.0f;
	public const float slideSpeedLimit = 20;
	public List<UCGravity> gravitySources = new List<UCGravity> ();
	public bool debugMessages = false;
	
	Transform activePlatform = null;
	Vector3 previousRelativePoint;
	Vector3 previousGlobalPoint;
	Quaternion previousRelativeRotation;
	Quaternion previousGlobalRotation;
	protected bool followPlatformRotation = true;

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
		// Since horizontal velocity is needed a lot in this method, record it now so we don't have to reclculate every time
		Vector3 horizontalVelocity = HorizontalVelocity();
	
		// Disable sliding once we come to a stop
		if (horizontalVelocity.magnitude < 1)
			isSliding = false;
			
		// Reduce velocity based on the friction of the ground
		velocity -= GetGroundResistance ();
		if (horizontalVelocity.magnitude < 1)
			velocity -= horizontalVelocity;
		
		// Apply gravity from all relevant gravity sources
		ApplyGravity();
		
		// The transform's "up" vector should always be the opposite of gravity
		transform.up = Vector3.Slerp(transform.up, -gravity.normalized, Time.deltaTime*(isGrounded?10:1));
		characterController.transform.up = transform.up;
		
		// Get movement of the platform we are standing on if applicable
		movingPlatformVelocity = Vector3.zero;
		if (activePlatform) {
			// Follow collision's position...
			Vector3 newGlobalPoint = activePlatform.TransformPoint (previousRelativePoint);
			movingPlatformVelocity = newGlobalPoint - previousGlobalPoint;
			
			// Follow collision's rotation...
			if (followPlatformRotation) {
				Quaternion newGlobalRotation = activePlatform.rotation * previousRelativeRotation;
				Quaternion rotationDiff = newGlobalRotation * Quaternion.Inverse (modelObject.transform.rotation);
				rotationDiff *= Quaternion.FromToRotation (rotationDiff * modelObject.transform.up, modelObject.transform.up);
				modelObject.transform.rotation = rotationDiff * modelObject.transform.rotation;
			}
			
			// Prevent a bug that causes upward-moving platforms to move the object slightly too far
			
//			if (debugMessages) Debug.Log("Velocity vs Gravity: " + Vector3.Dot(velocity, gravity));
			if (Vector3.Dot(movingPlatformVelocity, -gravity.normalized) > 0 &&
			    Vector3.Dot(velocity, gravity) > 0) {
				Vector3 upwardMovement = Vector3.Dot(movingPlatformVelocity, -gravity.normalized) * -gravity.normalized;
				upwardMovement = upwardMovement.normalized * Mathf.Min(upwardMovement.magnitude*5, gravity.magnitude);
				movingPlatformVelocity -= upwardMovement;
			}
		}
		activePlatform = null;
		followPlatformRotation = true;
		
		// Debug messages written to the console
		if (debugMessages) {
			Debug.Log("Velocity: (" + velocity.x + ", " + velocity.y + ", " + velocity.z + ") Magnitude: " + velocity.magnitude);
		}

		// Move the character controller
		isGrounded = false;
		movedByPlatform = false;
		characterController.Move (velocity*Time.deltaTime + movingPlatformVelocity);
		
		// If the object we collided with is a moving platform, record our relative and global orientations
		if (activePlatform) {
			previousGlobalPoint = transform.position;
			previousRelativePoint = activePlatform.InverseTransformPoint (transform.position);
			previousGlobalRotation = transform.rotation;
			previousRelativeRotation = Quaternion.Inverse (activePlatform.rotation) * modelObject.transform.rotation;
		}

		// If we are not on the ground, set our ground data to a neutral
		if (!isGrounded && !isSliding) {
			groundNormal = -gravity.normalized;
			groundFriction = 0.0f;
		}

		// Debug - Draw Rays reflecting the transform
		Debug.DrawRay(transform.position, transform.up*5, Color.green, 0.0f, false);
		Debug.DrawRay(transform.position, transform.right*5, Color.red, 0.0f, false);
		Debug.DrawRay(transform.position, transform.forward*5, Color.blue, 0.0f, false);
	}

	// Collision event when THIS moves into a COLLIDER
	void OnControllerColliderHit (ControllerColliderHit hit)
	{
		HandleCollision(hit.collider, hit.point, hit.normal, true);
	}

	// Collision events when a COLLIDER moves into THIS
	void OnCollisionEnter (Collision collision)
	{
		OnCollisionStay(collision);
	}

	void OnCollisionStay (Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts) {
			HandleCollision(collision.collider, contact.point, contact.normal, false);
		}
	}

	void HandleCollision (Collider hitCollider, Vector3 hitPoint, Vector3 hitNormal, bool canLandOnFloors)
	{
		// Find out the steepness of the slope in degrees
		float slope = 1+Mathf.Asin(Vector3.Dot (hitNormal, gravity.normalized)) / (Mathf.PI/2);
		float slopeDegrees = slope * 90;
//		if (debugMessages) Debug.Log ("Slope: " + slope + " (" + slopeDegrees + " degrees)");
		
		// Get more details on the collision
		bool hitIsFloor = canLandOnFloors && (slopeDegrees < 85 && Vector3.Dot (velocity.normalized, hitNormal) < 0);
		isGrounded = !isGrounded ? hitIsFloor : true;
		isSliding = !isSliding ? (hitIsFloor && slopeDegrees > characterController.slopeLimit) : true;
		UCFriction frictionInfo = ((UCFriction)hitCollider.GetComponent(typeof(UCFriction)));

		if (hitIsFloor) {
			activePlatform = hitCollider.transform;
			groundNormal = hitNormal;
			groundFriction = frictionInfo ? frictionInfo.friction : 1.0f;
		}		
		
		// If this UCObject's velocity is pushing into the object it collided with its velocity should be cut
		if (Vector3.Dot (velocity, hitNormal) < 0)
			velocity = velocity - (Vector3.Dot (velocity, hitNormal) * hitNormal);
			
		// Pop out of the hitCollider if overlapping
		Vector3 centerPoint = transform.position + transform.up * characterController.center.y;
		if (Vector3.Distance (centerPoint, hitPoint) < characterController.radius * 0.9f) {
			transform.position += hitNormal * (characterController.radius - Vector3.Distance (centerPoint, hitPoint));
		}
	}
	
	Vector3 VerticalVelocity() {
		return Vector3.Dot (velocity, -gravity.normalized) * -gravity.normalized;
	}
	
	Vector3 HorizontalVelocity() {
		return velocity - VerticalVelocity();
	}

	protected Vector3 GetGroundResistance ()
	{
		Vector3 resistance = Vector3.zero;

		if (isGrounded || isSliding) {
			resistance = HorizontalVelocity() * Time.deltaTime * 10 * groundFriction;
		}

		return resistance;
	}

	void ApplyGravity()
	{
//		if (debugMessages) Debug.Log("Gravity Source: " + gravitySources.Count);
		if (gravitySources.Count > 0) {
			int gravityPriority = -1;
			while (gravitySources.Count > 0) {
//				if (debugMessages) Debug.Log("Priority: " + gravitySources[0].priority);
				if (gravitySources [0].priority > gravityPriority) {
					gravityPriority = gravitySources [0].priority;
					gravity = Vector3.zero;
//					if (debugMessages) Debug.Log("Gravity Reset");
				}
				if (gravitySources [0].priority == gravityPriority) {
					gravity += gravitySources [0].GetGravityForObject (this);
//					if (debugMessages) Debug.Log("Gravity Added");
				}
				gravitySources.RemoveAt (0);
			}
		} else {
			gravity *= 1 - Time.deltaTime*4;
		}
		velocity += gravity;
	}
}
