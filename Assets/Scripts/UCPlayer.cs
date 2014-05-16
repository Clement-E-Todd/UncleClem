using UnityEngine;
using System.Collections;

public class UCPlayer : UCObject
{

	UCInput playerInput = null;

	// Movement Variables
	Vector3 moveDirection = Vector3.zero;
	float moveSpeedGoal = 0;
	bool running = false;
	float jumpTime = 0;

	// Movement Stats
	protected float moveSpeedMax = 15.0f;
	protected float moveAccelTime = 0.025f;
	protected float jumpPower = 12.0f;

	// Aniamation Variables
	bool stepWithRightFootFirst = true;

	protected void Awake ()
	{
		moveDirection = transform.TransformDirection (Vector3.forward);
	}

	protected override void Start ()
	{
		base.Start ();
		playerInput = (UCInput)GetComponent (typeof(UCInput));
	}

	protected override void Update ()
	{
		base.Update ();
		HandleInput ();
		HandleAnimation ();
	}

	void HandleInput ()
	{
		// Walk, Run and Slide
		Vector2 movement = playerInput.GetMovement ();
		if (movement.magnitude > 0.25f) {
			// Direction
			moveDirection = new Vector3 (-movement.x, 0, -movement.y).normalized;
			transform.rotation = Quaternion.LookRotation (moveDirection);
			moveDirection = (moveDirection - (Vector3.Dot (moveDirection, transform.up) * transform.up)).normalized*moveDirection.magnitude;

			// Speed
			{
				// Since walking/running mostly concerns the horizontal plane we want to keep track of that
				Vector3 originalHorizontalVelocity = velocity - (Vector3.Dot (velocity, transform.up) * transform.up);
				// Next we need to find out the top speed we can move at given the current situation
				running = playerInput.GetRun (false);
				moveSpeedGoal = !isSliding ? (movement.magnitude * moveSpeedMax / (running ? 1 : 2)) : (originalHorizontalVelocity.magnitude);
				// Our speed is reduced if we are walking/running up a slope
				Vector3 slope = groundNormal - (Vector3.Dot (groundNormal, gravityNormal) * gravityNormal);
				float slopeFactor = Vector3.Dot(slope, originalHorizontalVelocity.normalized);
				if (slopeFactor < 0) {
//					slopeFactor = 1+Mathf.Asin(slopeFactor) / (Mathf.PI/2); // Convert to a linear value
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
					velocityToAdd *= Mathf.Max(0, Vector3.Dot(velocityToAdd.normalized, velocity.normalized));
				// Finally we apply these factors to our speed
				velocity += velocityToAdd;
				// If we went over the speed limit, now is the time to pull back
				Vector3 horizontalVelocity = velocity - (Vector3.Dot (velocity, transform.up) * transform.up);
				Vector3 verticalVelocity = velocity - horizontalVelocity;
				if (horizontalVelocity.magnitude > moveSpeedGoal) {
					horizontalVelocity = horizontalVelocity.normalized * Mathf.Max (originalHorizontalVelocity.magnitude, moveSpeedGoal);
					velocity = horizontalVelocity + verticalVelocity;// + (gravityNormal*gravityStrength);
				}
			}
		
		} else {
			moveSpeedGoal = 0;
		}
	
		// Jump
		if (playerInput.GetJump (true) && (isGrounded || isSliding)) {
			// Use the character's up vector is grounded or the ground's surface normal if sliding
			Vector3 normal = isGrounded ? transform.up : groundNormal;
			// To ensure jumps are consistent, first flatten our velocity to be parallel to the ground
			velocity = velocity - (Vector3.Dot (velocity, normal) * normal);
			// Then add the initial burst of upward speed
			velocity += transform.TransformDirection (Vector3.up) * jumpPower;
			// If we were standing on a moving platform, we add its velocity to ours as well
			velocity += movingPlatformVelocity;
			// And finally we start the timer which incidates when we will be forced to start falling
			jumpTime = 0.1f;
			// If we jumped out of a slide, we are no longer sliding
			isSliding = false;
		} else if (jumpTime > 0 && playerInput.GetJump (false)) {
			velocity -= gravityNormal * gravityStrength * 2;
			jumpTime = Mathf.Max (0, jumpTime - Time.deltaTime);
		} else if (!playerInput.GetJump (false)) {
			jumpTime = 0;
		}
	}

	int TEMP = 0;
	void HandleAnimation ()
	{

		// Arial Animations
		if (!isGrounded) {
//			Debug.Log("Not Grounded (Call #" + TEMP + ")");
			TEMP++;
			if (!animation.IsPlaying ("Arial")) {
				animation.CrossFade ("Arial", 0.05f, PlayMode.StopAll);
			}
			animation ["Arial"].time = Mathf.Clamp ((velocity.y + 10) / 20, 0, 1);
		}

		// Grounded Animations
		else {
			// Stand
			if (moveSpeedGoal == 0 && !animation.IsPlaying ("Stand")) {
				animation.CrossFade ("Stand", 0.1f, PlayMode.StopAll);
				animation ["Stand"].wrapMode = WrapMode.Loop;
			}

			// Walk or Run
			else if (moveSpeedGoal != 0) {
				string animationName = running ? "Run" : "Walk";
				if (!animation.IsPlaying (animationName)) {
					// - Start the animation
					animation.CrossFade (animationName, 0.1f, PlayMode.StopAll);
					animation [animationName].wrapMode = WrapMode.Loop;
					// - Lift the foot which has been down longest first to prevent "skiing"
					animation [animationName].time = stepWithRightFootFirst ? 0 : animation [animationName].length / 2;
				}
				// - Set the speed of the animation based on how fast UCPlayer is actually moving
				animation ["Run"].speed = (moveSpeedGoal / moveSpeedMax) * (1 + (1 - GetGroundFriction ()) * 0.6f);
				animation ["Walk"].speed = animation ["Run"].speed * 2;
				// - Keep track of which foot should be lifted first when a new animation begins
				stepWithRightFootFirst = (animation [animationName].time > animation [animationName].length / 2);
			}
		}
	}

}
