using UnityEngine;
using System.Collections;

public class AnimationTester : MonoBehaviour
{

	public float animationSpeed = 1.0f;
	float previousSpeed = 0.0f;

	// Use this for initialization
	void Start ()
	{
		animation.CrossFade ("Stand", 2.0f, PlayMode.StopAll);
		animation.wrapMode = WrapMode.Loop;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Space)) {
			if (animation.IsPlaying ("Stand")) {
				animation.CrossFade ("Walk", 0.5f, PlayMode.StopAll);
			} else if (animation.IsPlaying ("Walk")) {
				animation.CrossFade ("Run", 0.5f, PlayMode.StopAll);
				animation["Run"].time = animation["Walk"].time;
			} else if (animation.IsPlaying ("Run")) {
				animation.CrossFade ("Stand", 0.5f, PlayMode.StopAll);
			}
		}

		if (Input.GetKeyDown (KeyCode.S)) {
			animation.CrossFade ("Stand", 0.5f, PlayMode.StopAll);
		}

		if (Input.GetKeyDown (KeyCode.W)) {
			if (animation.IsPlaying ("Run")) {
				animation.CrossFade ("Walk", 0.5f, PlayMode.StopAll);
			} else if (animation.IsPlaying ("Walk")) {
				animation.CrossFade ("Run", 0.5f, PlayMode.StopAll);
			} else {
				animation.CrossFade ("Walk", 0.5f, PlayMode.StopAll);
			}
		}

		if (Input.GetKeyDown (KeyCode.T)) {
			animation.CrossFade ("Test", 0.5f, PlayMode.StopAll);
		}

		if (animationSpeed != previousSpeed) {
			foreach (AnimationState state in animation) {
				state.speed = animationSpeed;
			}
			previousSpeed = animationSpeed;
		}
	}
}
