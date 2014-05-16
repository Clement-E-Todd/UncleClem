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
		if (IsOnMacOS())
			return tap ? Input.GetKeyDown(KeyCode.JoystickButton18) : Input.GetKey(KeyCode.JoystickButton18);
		if (IsOnWindows())
			return tap ? Input.GetKeyDown (KeyCode.JoystickButton2) : Input.GetKey (KeyCode.JoystickButton2);
		else
			return false;
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
		throw new System.NotImplementedException ();
	}

	public override bool GetLockOn (bool tap) {
		throw new System.NotImplementedException ();
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
