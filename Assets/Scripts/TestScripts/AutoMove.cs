using UnityEngine;
using System.Collections;

public class AutoMove : MonoBehaviour {

	Vector3 startPosition;
	public Vector3 destination = new Vector3(0,0,0);
	public float duration = 5.0f;
	float time = 0;

	void Start () {
		startPosition = transform.position;
	}

	void Update () {
		float progress = 0.5f + Mathf.Sin(time)/2;
		transform.position = Vector3.Lerp(startPosition, destination, progress);
		time += Time.deltaTime * Mathf.PI / duration;
	}
}
