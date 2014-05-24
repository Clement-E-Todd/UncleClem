using UnityEngine;
using System.Collections;

// An input scheme for a wired Xbox 360 controller plugged into a Mac.

public class ExampleInputTesting : ExampleInput {

	public override Vector2	GetMovement () {
		return new Vector2 (Input.GetAxisRaw("GamePad1_Axis1"), -Input.GetAxisRaw("GamePad1_Axis2"));
	}

	public override Vector2	GetCamera () {
		return new Vector2 (Input.GetAxisRaw("GamePad1_Axis4"), Input.GetAxisRaw("GamePad1_Axis5"));
	}

	public override bool	GetRun (bool tap) {
		return Input.GetAxisRaw("GamePad1_Axis3") < -0.25f;
	}

	public override bool	GetJump (bool tap) {
		if (IsOnMacOS())
			return tap ? Input.GetKeyDown(KeyCode.JoystickButton16) : Input.GetKey(KeyCode.JoystickButton16);
		if (IsOnWindows())
			return tap ? Input.GetKeyDown (KeyCode.JoystickButton0) : Input.GetKey (KeyCode.JoystickButton0);
		else
			return false;
	}

	public override bool	GetAttack (bool tap) {
		if (IsOnMacOS())
			return tap ? Input.GetKeyDown(KeyCode.JoystickButton17) : Input.GetKey(KeyCode.JoystickButton17);
		if (IsOnWindows())
			return tap ? Input.GetKeyDown (KeyCode.JoystickButton1) : Input.GetKey (KeyCode.JoystickButton1);
		else
			return false;
	}

	public override bool	GetGrab (bool tap) {
		if (IsOnMacOS())
			return tap ? Input.GetKeyDown(KeyCode.JoystickButton19) : Input.GetKey(KeyCode.JoystickButton19);
		if (IsOnWindows())
			return tap ? Input.GetKeyDown (KeyCode.JoystickButton3) : Input.GetKey (KeyCode.JoystickButton3);
		else
			return false;
	}

	public override bool GetDuck (bool tap) {
		return false;
	}

	public override bool GetLockOn (bool tap) {
		return false;
	}

	bool IsOnMacOS() {
		return Application.platform == RuntimePlatform.OSXDashboardPlayer ||
				Application.platform == RuntimePlatform.OSXEditor ||
				Application.platform == RuntimePlatform.OSXPlayer ||
				Application.platform == RuntimePlatform.OSXWebPlayer;
	}

	bool IsOnWindows() {
		return Application.platform == RuntimePlatform.WindowsEditor ||
				Application.platform == RuntimePlatform.WindowsPlayer ||
				Application.platform == RuntimePlatform.WindowsWebPlayer;
	}

}
