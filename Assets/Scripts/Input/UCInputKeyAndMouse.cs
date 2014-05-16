using UnityEngine;
using System.Collections;

public class UCInputKeyAndMouse : UCInput {
	
	public override Vector2	GetMovement () {
		return new Vector2 ((Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0),
		                    (Input.GetKey(KeyCode.S) ? -1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0));
	}
	
	public override Vector2	GetCamera () {
		return new Vector2 (0,0);
	}
	
	public override bool	GetRun (bool tap) {
		return tap ? Input.GetKeyDown(KeyCode.LeftShift) : Input.GetKey(KeyCode.LeftShift);
	}
	
	public override bool	GetJump (bool tap) {
		return tap ? Input.GetKeyDown(KeyCode.Space) : Input.GetKey(KeyCode.Space);
	}
	
	public override bool	GetAttack (bool tap) {
		return false;
	}
	
	public override bool	GetGrab (bool tap) {
		return false;
	}
	
	public override bool	GetDuck (bool tap) {
		return false;
	}
	
	public override bool	GetLockOn (bool tap) {
		return false;
	}
	
}
