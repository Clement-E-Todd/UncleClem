using UnityEngine;
using System.Collections;

[RequireComponent (typeof (ExampleInput))]

public class ExamplePlayer : UCObject
{
	ExampleInput playerInput = null;

	// Movement Variables
	Vector2 moveInput = Vector2.zero;
	Vector3 moveDirection = Vector3.forward;
	float moveSpeedGoal = 0;
	bool running = false;
	float jumpTime = 0;

	// Movement Stats
	protected float moveSpeedMax = 15.0f;
	protected float moveAccelTime = 0.025f;
	protected float jumpPower = 12.0f;

	// Aniamation Variables
	bool stepWithRightFootFirst = true;

	protected override void Start ()
	{
		base.Start ();
		playerInput = (ExampleInput)GetComponent (typeof(ExampleInput));
	}

	protected override void Update ()
	{
		base.Update ();
		HandleInput ();
		HandleAnimation ();

		// Rotate the model based on the player's current actions
		if (!isSliding)
			modelObject.transform.rotation = Quaternion.LookRotation(moveInput != Vector2.zero ? ((transform.right * -moveInput.x) + (transform.forward * -moveInput.y)).normalized : modelObject.transform.forward, -gravity.normalized);
		else
			modelObject.transform.rotation = Quaternion.LookRotation(new Vector3 (velocity.x, 0, velocity.z).normalized, groundNormal);
	}

	void HandleInput ()
	{
		// Walk, Run and Slide
		moveInput = playerInput.GetMovement ();
		if (moveInput.magnitude > 0.25f) {
			// Direction
			moveDirection = ((transform.right * -moveInput.x) + (transform.forward * -moveInput.y)).normalized;
//			moveDirection = new Vector3 (-movement.x, 0, -movement.y).normalized;
			if (isSliding && Vector3.Dot (moveDirection, velocity.normalized) < 0)
				moveDirection = (moveDirection - (Vector3.Dot (moveDirection, velocity.normalized) * velocity.normalized)).normalized;

			// Speed
			{
				// Since walking/running mostly concerns the horizontal plane we want to keep track of that
				Vector3 originalHorizontalVelocity = velocity - (Vector3.Dot (velocity, -gravity.normalized) * -gravity.normalized);
				// Next we need to find out the top speed we can move at given the current situation
				running = playerInput.GetRun (false);
				moveSpeedGoal = !isSliding ? (moveInput.magnitude * moveSpeedMax / (running ? 1 : 2)) : (originalHorizontalVelocity.magnitude);
				// Our speed is reduced if we are walking/running up a slope
				Vector3 slope = groundNormal - (Vector3.Dot (groundNormal, gravity.normalized) * gravity.normalized);
				float slopeFactor = Vector3.Dot(slope, originalHorizontalVelocity.normalized);
				if (slopeFactor < 0) {
					slopeFactor = 1+slopeFactor;
					moveSpeedGoal *= slopeFactor;
				}
				else {
					slopeFactor = 1.0f;
				}
				slopeFactor *= slopeFactor;
				// Accumulate the various factors which influence our velocity
				Vector3 velocityToAdd = (moveDirection * moveSpeedGoal * Time.deltaTime / Mathf.Max (moveAccelTime, 0.001f));
				velocityToAdd *= Mathf.Max (Mathf.Pow (GetGroundFriction (), 1.5f), 0.05f);
				velocityToAdd *= slopeFactor;
				if (isSliding)
					velocityToAdd *= Mathf.Max(0.5f, Vector3.Dot(velocityToAdd.normalized, velocity.normalized));
				// Finally we apply these factors to our speed
				velocity += velocityToAdd;
				// If we went over the speed limit, now is the time to pull back
				Vector3 horizontalVelocity = velocity - (Vector3.Dot (velocity, -gravity.normalized) * -gravity.normalized);
				Vector3 verticalVelocity = velocity - horizontalVelocity;
				if (horizontalVelocity.magnitude > moveSpeedGoal) {
					horizontalVelocity = horizontalVelocity.normalized * Mathf.Max (originalHorizontalVelocity.magnitude, moveSpeedGoal);
					velocity = horizontalVelocity + verticalVelocity;
				}
			}
		} else {
			moveSpeedGoal = 0;
		}
	
		// Jump
		if (playerInput.GetJump (true) && (isGrounded || isSliding)) {
			// Use the character's up vector is grounded or the ground's surface normal if sliding
			Vector3 jumpNormal = !isSliding ? transform.up : groundNormal;
			// To ensure jumps are consistent, first flatten our velocity to be parallel to the ground
			velocity = velocity - (Vector3.Dot (velocity, jumpNormal) * jumpNormal);
			// Then add the initial burst of upward speed
			velocity += transform.up * jumpPower;
			// If we were standing on a moving platform, we add its velocity to ours as well
			velocity += movingPlatformVelocity;
			// And finally we start the timer which incidates when we will be forced to start falling
			jumpTime = 0.1f;
			// If we jumped out of a slide, we are no longer sliding
			isSliding = false;

		} else if (jumpTime > 0 && playerInput.GetJump (false)) {
			velocity -= gravity * 2;
			jumpTime = Mathf.Max (0, jumpTime - Time.deltaTime);
		} else if (!playerInput.GetJump (false)) {
			jumpTime = 0;
		}
	}

	void HandleAnimation ()
	{

		// Arial Animations
		if (!isGrounded && !isSliding) {
			if (!modelObject.animation.IsPlaying ("Arial")) {
				modelObject.animation.CrossFade ("Arial", 0.05f, PlayMode.StopAll);
			}
			modelObject.animation ["Arial"].time = Mathf.Clamp ((Vector3.Dot(velocity, -gravity.normalized) + 10) / 20, 0, 1);
		}

		// Grounded Animations
		else if (!isSliding) {
			// Stand
			if (moveSpeedGoal == 0 && !modelObject.animation.IsPlaying ("Stand")) {
				modelObject.animation.CrossFade ("Stand", 0.25f, PlayMode.StopAll);
				modelObject.animation ["Stand"].wrapMode = WrapMode.Loop;
			}

			// Walk or Run
			else if (moveSpeedGoal != 0) {
				string animationName = running ? "Run" : "Walk";
				if (!modelObject.animation.IsPlaying (animationName)) {
					// - Start the animation
					modelObject.animation.CrossFade (animationName, 0.1f, PlayMode.StopAll);
					modelObject.animation [animationName].wrapMode = WrapMode.Loop;
					// - Lift the foot which has been down longest first to prevent "skiing"
					modelObject.animation [animationName].time = stepWithRightFootFirst ? 0 : modelObject.animation [animationName].length / 2;
				}
				// - Set the speed of the animation based on how fast UCPlayer is actually moving
				modelObject.animation ["Run"].speed = (moveSpeedGoal / moveSpeedMax) * (1 + (1 - GetGroundFriction ()) * 0.6f);
				modelObject.animation ["Walk"].speed = modelObject.animation ["Run"].speed * 2;
				// - Keep track of which foot should be lifted first when a new animation begins
				stepWithRightFootFirst = (modelObject.animation [animationName].time > modelObject.animation [animationName].length / 2);
			}
		}

		// Sliding
		else {
			if (!modelObject.animation.IsPlaying ("Slide"))
				modelObject.animation.CrossFade ("Slide", 0.1f, PlayMode.StopAll);
			modelObject.animation ["Slide"].time = velocity.magnitude / slideSpeedLimit;
		}
	}

}
