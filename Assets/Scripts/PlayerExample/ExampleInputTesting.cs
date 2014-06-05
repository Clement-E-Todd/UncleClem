using UnityEngine;
using System.Collections;

// An input scheme for a wired Xbox 360 controller plugged into a Mac.

public class ExampleInputTesting : ExampleInput
{

	public int playerNumber = 1;
	public int configuration = 0; // 0: GameCube Controller, Other: Xbox 360 Controller

	public override Vector2	GetMovement ()
	{
		Vector2 leftStick = new Vector2 (Input.GetAxisRaw ("GamePad" + playerNumber + "_Axis1"), -Input.GetAxisRaw ("GamePad" + playerNumber + "_Axis2"));
		if (configuration == 0)
			leftStick *= 1.25f;
		if (leftStick.magnitude > 1)
			leftStick = leftStick.normalized;
		return leftStick;
	}

	public override Vector2	GetCamera ()
	{
		if (configuration == 0) // GameCube
			return new Vector2 (Input.GetAxisRaw ("GamePad" + playerNumber + "_Axis4"), Input.GetAxisRaw ("GamePad" + playerNumber + "_Axis3"));
		else // Xbox 360
			return new Vector2 (Input.GetAxisRaw ("GamePad" + playerNumber + "_Axis5"), Input.GetAxisRaw ("GamePad" + playerNumber + "_Axis4"));
	}

	public override bool	GetRun (bool tap)
	{
		if (configuration == 0) // GameCube
			return Input.GetAxisRaw ("GamePad" + playerNumber + "_Axis6") > 0.25f;
		else // Xbox 360
			return Input.GetAxisRaw ("GamePad" + playerNumber + "_Axis3") < -0.25f;
	}

	public override bool	GetJump (bool tap)
	{
		int button;

		if (configuration == 0) button = 1; // GameCube
		else {	// Xbox 360
			if (IsOnMacOS ()) button = 16;
			else if (IsOnWindows ()) button = 0;
			else return false;
		}

		return GetButton(button, tap);
	}

	public override bool	GetAttack (bool tap)
	{
		int button;
		
		if (configuration == 0) button = 2; // GameCube
		else {	// Xbox 360
			if (IsOnMacOS ()) button = 17;
			else if (IsOnWindows ()) button = 1;
			else return false;
		}
		
		return GetButton(button, tap);
	}

	public override bool	GetGrab (bool tap)
	{
		int button;
		
		if (configuration == 0) button = 3; // GameCube
		else {	// Xbox 360
			if (IsOnMacOS ()) button = 19;
			else if (IsOnWindows ()) button = 3;
			else return false;
		}
		
		return GetButton(button, tap);
	}

	public override bool GetDuck (bool tap)
	{
		if (configuration == 0) // GameCube
			return Input.GetAxisRaw ("GamePad" + playerNumber + "_Axis3") < -0.25f;
		else { // Xbox 360
			Debug.LogError("No Duck button set for Xbox controller!");
			return false;
		}
	}

	public override bool GetLockOn (bool tap)
	{
		return false;
	}


	bool GetButton(int button, bool tap) {
		if (tap)
			return Input.GetKeyDown (KeyCode.JoystickButton0 + button + playerNumber*20);
		else
			return Input.GetKey (KeyCode.JoystickButton0 + button + playerNumber*20);
	}

	bool IsOnMacOS ()
	{
		return Application.platform == RuntimePlatform.OSXDashboardPlayer ||
			Application.platform == RuntimePlatform.OSXEditor ||
			Application.platform == RuntimePlatform.OSXPlayer ||
			Application.platform == RuntimePlatform.OSXWebPlayer;
	}

	bool IsOnWindows ()
	{
		return Application.platform == RuntimePlatform.WindowsEditor ||
			Application.platform == RuntimePlatform.WindowsPlayer ||
			Application.platform == RuntimePlatform.WindowsWebPlayer;
	}

}
