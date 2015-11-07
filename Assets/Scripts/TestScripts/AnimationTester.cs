using UnityEngine;
using System.Collections;

public class AnimationTester : MonoBehaviour
{

	public float animationSpeed = 1.0f;
	float previousSpeed = 0.0f;

	// Use this for initialization
	void Start ()
	{
		GetComponent<Animation>().CrossFade ("Stand", 2.0f, PlayMode.StopAll);
		GetComponent<Animation>().wrapMode = WrapMode.Loop;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Space)) {
			if (GetComponent<Animation>().IsPlaying ("Stand")) {
				GetComponent<Animation>().CrossFade ("Walk", 0.5f, PlayMode.StopAll);
			} else if (GetComponent<Animation>().IsPlaying ("Walk")) {
				GetComponent<Animation>().CrossFade ("Run", 0.5f, PlayMode.StopAll);
				GetComponent<Animation>()["Run"].time = GetComponent<Animation>()["Walk"].time;
			} else if (GetComponent<Animation>().IsPlaying ("Run")) {
				GetComponent<Animation>().CrossFade ("Stand", 0.5f, PlayMode.StopAll);
			}
		}

		if (Input.GetKeyDown (KeyCode.S)) {
			GetComponent<Animation>().CrossFade ("Stand", 0.5f, PlayMode.StopAll);
		}

		if (Input.GetKeyDown (KeyCode.W)) {
			if (GetComponent<Animation>().IsPlaying ("Run")) {
				GetComponent<Animation>().CrossFade ("Walk", 0.5f, PlayMode.StopAll);
			} else if (GetComponent<Animation>().IsPlaying ("Walk")) {
				GetComponent<Animation>().CrossFade ("Run", 0.5f, PlayMode.StopAll);
			} else {
				GetComponent<Animation>().CrossFade ("Walk", 0.5f, PlayMode.StopAll);
			}
		}

		if (Input.GetKeyDown (KeyCode.T)) {
			GetComponent<Animation>().CrossFade ("Test", 0.5f, PlayMode.StopAll);
		}

		if (animationSpeed != previousSpeed) {
			foreach (AnimationState state in GetComponent<Animation>()) {
				state.speed = animationSpeed;
			}
			previousSpeed = animationSpeed;
		}
	}
}
