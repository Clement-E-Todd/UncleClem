using UnityEngine;
using System.Collections;

public class UCFriction : MonoBehaviour {
	/*	Sets the friction of an object, generally a platform of some kind.
		Any object without this behaviour is assumed to have the default friction
		of 1.0f, so this would usually only be used on something made of ice or
		mud (with recommended frictions of 0.1f and 2.0f respectively).
	*/
	public float friction = 1.0f;
}
