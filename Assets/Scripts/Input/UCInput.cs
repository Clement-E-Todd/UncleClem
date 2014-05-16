using UnityEngine;
using System.Collections;

public abstract class UCInput : MonoBehaviour {

	public abstract Vector2	GetMovement ();
	public abstract Vector2	GetCamera ();
	public abstract bool	GetRun (bool tap);
	public abstract bool	GetJump (bool tap);
	public abstract bool	GetAttack (bool tap);
	public abstract bool	GetGrab (bool tap);
	public abstract bool	GetDuck (bool tap);
	public abstract bool	GetLockOn (bool tap);
}
