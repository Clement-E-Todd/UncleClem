using UnityEngine;
using System.Collections;

// An input scheme for a wired Xbox 360 controller plugged into a Mac.

public class UCInputTesting : UCInput {

	public override Vector2	GetMovement () {
		return new Vector2 (Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
	}

	public override Vector2	GetCamera () {
		return new Vector2 (0,0);
	}

	public override bool	GetRun (bool tap) {
		return tap ? Input.GetKeyDown(KeyCode.JoystickButton18) : Input.GetKey(KeyCode.JoystickButton18);
	}

	public override bool	GetJump (bool tap) {
		return tap ? Input.GetKeyDown(KeyCode.JoystickButton16) : Input.GetKey(KeyCode.JoystickButton16);
	}

	public override bool	GetAttack (bool tap) {
		return tap ? Input.GetKeyDown(KeyCode.JoystickButton17) : Input.GetKey(KeyCode.JoystickButton17);
	}

	public override bool	GetGrab (bool tap) {
		return tap ? Input.GetKeyDown(KeyCode.JoystickButton19) : Input.GetKey(KeyCode.JoystickButton19);
	}

	public override bool	GetDuck (bool tap) {
		return tap ? Input.GetKeyDown(KeyCode.JoystickButton13) : Input.GetKey(KeyCode.JoystickButton13);
	}

	public override bool	GetLockOn (bool tap) {
		return tap ? Input.GetKeyDown(KeyCode.JoystickButton14) : Input.GetKey(KeyCode.JoystickButton14);
	}

}
