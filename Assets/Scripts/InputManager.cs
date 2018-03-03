using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#region Keyaboard Structures

public enum KeyboardAxisCode
{
	Horizontal,
	Vertical
}

#endregion

#region Controller Structures

public enum ControllerCode
{
	None,
	LeftBumper,
	RightBumper,
	Back,
	Start,
	XboxButton,
	Y,
	X,
	A,
	B,
	DPadUp,
	DPadDown,
	DPadLeft,
	DPadRight,
	LeftStickClick,
	RightStickClick,
	LeftStickHorizontalAxis,
	LeftStickVerticalAxis,
	RightStickHorizontalAxis,
	RightStickVerticalAxis,
	LeftTriggerAxis,
	RightTriggerAxis,
	DPadHorizontalAxis,
	DPadVerticalAxis
}

[System.Serializable]
public class ControllerMappedInput
{
	public bool IsAxis;
	public KeyCode Key;
	public string Axis;

	public ControllerMappedInput(KeyCode key)
	{
		IsAxis = false;
		Key = key;
		Axis = string.Empty;
	}

	public ControllerMappedInput(string axis)
	{
		IsAxis = true;
		Key = KeyCode.None;
		Axis = axis;
	}
}

#endregion

#region Comp Dev Input Structures

public enum CompDevInput
{
	DirectionHorizontal,
	DirectionVertical
}

[System.Serializable]
public class CompDevMappedInput
{
	public CompDevInput MappedCompDevInput;
	public bool IsKeyboardAxis;
	public KeyboardAxisCode KeyboardAxis;
	public KeyCode[] KeyboardKeys;
	public bool IsControllerAxis;
	public ControllerCode ControllerInput;
}

public enum ButtonStateType
{
	Down,
	Up,
	Constant
}

#endregion

public enum InputMode
{
	None,
	Keyboard,
	Controller
}

public enum OS
{
	Mac,
	Windows,
	Linux
}

public class InputManager : MonoBehaviour
{
	public static InputManager Instance;

	public InputMode PlayerInputMode;
	public int MaxPlayerCount;

	private OS currentOS;

	private bool LeftTriggerPressed;
	private bool RightTriggerPressed;

	public EventSystem InputEventSystem;
	public StandaloneInputModule InputModule;

	private string EventSystemControllerHorizontalAxis;
	private string EventSystemControllerVerticalAxis;
	private string EventSystemControllerSubmit;
	private string EventSystemControllerCancel;

	#region Input Maps

	private Dictionary<CompDevInput, CompDevMappedInput> CompDevInputMap;

	private Dictionary<string, ControllerMappedInput> ControllerMap;

	private Dictionary<string, string> KeyboardAxisMap;

	#endregion

	#region Comp Dev Inputs

	public CompDevMappedInput DirectionHorizontal;

	public CompDevMappedInput DirectionVertical;

	#endregion

	#region Keyboard Axes

	private static readonly string KeyboardHorizontalAxis = "Keyboard_Horizontal";
	private static readonly string KeyboardVerticalAxis = "Keyboard_Vertical";

	private static readonly string KeyboardSubmit = "Keyboard_Submit";
	private static readonly string KeyboardCancel = "Keyboard_Cancel";

	#endregion

	#region Controller Axes

	private static readonly string XboxControllerLeftStickHorizontalAxis = "Controller_Horizontal";
	private static readonly string XboxControllerLeftStickVerticalAxis = "Controller_Vertical";
	private static readonly string XboxControllerRightStickHorizontalAxis = "Controller_Alt_Horizontal";
	private static readonly string XboxControllerRightStickVerticalAxis = "Controller_Alt_Vertical";
	private static readonly string XboxControllerRightStickHorizontalAxisOSX = "Controller_Alt_Horizontal_OSX";
	private static readonly string XboxControllerRightStickVerticalAxisOSX = "Controller_Alt_Vertical_OSX";
	private static readonly string XboxControllerLeftTriggerAxisOSX = "Controller_Left_Trigger_OSX";
	private static readonly string XboxControllerRightTriggerAxisOSX = "Controller_Right_Trigger_OSX";
	private static readonly string XboxControllerLeftTriggerAxisWindows = "Controller_Left_Trigger_Windows";
	private static readonly string XboxControllerRightTriggerAxisWindows = "Controller_Right_Trigger_Windows";
	private static readonly string XboxControllerLeftTriggerAxisLinux = "Controller_Left_Trigger_Linux";
	private static readonly string XboxControllerRightTriggerAxisLinux = "Controller_Right_Trigger_Linux";
	private static readonly string XboxControllerDPadHorizontalWindows = "Controller_DPad_Horizontal_Windows";
	private static readonly string XboxControllerDPadVerticalWindows = "Controller_DPad_Vertical_Windows";
	private static readonly string XboxControllerDPadHorizontalLinux = "Controller_DPad_Horizontal_Linux";
	private static readonly string XboxControllerDPadVerticalLinux = "Controller_DPad_Vertical_Linux";

	private static readonly string XboxControllerSubmitOSX = "Controller_Submit_OSX";
	private static readonly string XboxControllerSubmitWindows = "Controller_Submit_Windows";
	private static readonly string XboxControllerSubmitLinux = "Controller_Submit_Linux";
	private static readonly string XboxControllerCancelOSX = "Controller_Cancel_OSX";
	private static readonly string XboxControllerCancelWindows = "Controller_Cancel_Windows";
	private static readonly string XboxControllerCancelLinux = "Controller_Cancel_Linux";

	#endregion

	#region Unity Callbacks

	void Awake()
	{
		Cursor.visible = false;

		InputEventSystem = gameObject.GetComponent<EventSystem>();
		InputModule = gameObject.GetComponent<StandaloneInputModule>();

		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else
		{
			Destroy(gameObject);
		}

		ControllerMap = new Dictionary<string, ControllerMappedInput>();

		KeyboardAxisMap = new Dictionary<string, string>();

		CompDevInputMap = new Dictionary<CompDevInput, CompDevMappedInput>();

		switch (Application.platform)
		{
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXPlayer:
				SetupOSXMap(ref ControllerMap, MaxPlayerCount);
				break;
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.WindowsPlayer:
				SetupWindowsMap(ref ControllerMap, MaxPlayerCount);
				break;
			case RuntimePlatform.LinuxPlayer:
				SetupLinuxMap(ref ControllerMap, MaxPlayerCount);
				break;
		}

		SetupKeyboardAxisMap(ref KeyboardAxisMap, MaxPlayerCount);

		SetupCompDevInputMap(ref CompDevInputMap);
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}

		if (PlayerInputMode == InputMode.Keyboard)
		{
			if (InputModule.horizontalAxis != KeyboardHorizontalAxis)
			{
				InputModule.horizontalAxis = KeyboardHorizontalAxis;
			}

			if (InputModule.verticalAxis != KeyboardVerticalAxis)
			{
				InputModule.verticalAxis = KeyboardVerticalAxis;
			}

			if (InputModule.submitButton != KeyboardSubmit)
			{
				InputModule.submitButton = KeyboardSubmit;
			}

			if (InputModule.cancelButton != KeyboardCancel)
			{
				InputModule.cancelButton = KeyboardCancel;
			}
		}
		else if (PlayerInputMode == InputMode.Controller)
		{
			if (InputModule.horizontalAxis != EventSystemControllerHorizontalAxis)
			{
				InputModule.horizontalAxis = EventSystemControllerHorizontalAxis;
			}

			if (InputModule.verticalAxis != EventSystemControllerVerticalAxis)
			{
				InputModule.verticalAxis = EventSystemControllerVerticalAxis;
			}

			if (InputModule.submitButton != EventSystemControllerSubmit)
			{
				InputModule.submitButton = EventSystemControllerSubmit;
			}

			if (InputModule.cancelButton != EventSystemControllerCancel)
			{
				InputModule.cancelButton = EventSystemControllerCancel;
			}
		}
	}

	#endregion

	#region Comp Dev Input Getters

	public float GetCompDevInputAxis(CompDevInput compDev, int playerIndex = 0)
	{
		if (CompDevInputMap.ContainsKey(compDev))
		{
			CompDevMappedInput mappedInput = CompDevInputMap[compDev];

			if (PlayerInputMode == InputMode.Keyboard)
			{
				if (mappedInput.IsKeyboardAxis)
				{
					return GetKeyboardAxis(mappedInput.KeyboardAxis, playerIndex);
				}
			}
			else if (PlayerInputMode == InputMode.Controller)
			{
				if (mappedInput.IsControllerAxis)
				{
					return GetControllerAxis(mappedInput.ControllerInput, playerIndex);
				}
			}
		}

		return 0f;
	}

	public bool GetCompDevInputButton(CompDevInput compDevInputButton, int playerIndex = 0, ButtonStateType buttonState = ButtonStateType.Down)
	{
		if (CompDevInputMap.ContainsKey(compDevInputButton))
		{
			CompDevMappedInput mappedInput = CompDevInputMap[compDevInputButton];
			if (PlayerInputMode == InputMode.Keyboard)
			{
				if (!mappedInput.IsKeyboardAxis)
				{
					return GetKeyboardButton(mappedInput.KeyboardKeys[playerIndex], buttonState);
				}
			}
			else if (PlayerInputMode == InputMode.Controller)
			{
				if (!mappedInput.IsControllerAxis)
				{
					return GetControllerButton(mappedInput.ControllerInput, playerIndex, buttonState);
				}
			}
		}

		return false;
	}

	#endregion

	#region Comp Dev Map Setup

	private void SetupCompDevInputMap(ref Dictionary<CompDevInput, CompDevMappedInput> map)
	{
		map.Add(CompDevInput.DirectionHorizontal, DirectionHorizontal);
		map.Add(CompDevInput.DirectionVertical, DirectionVertical);
	}

	#endregion

	#region Keyboard Getters

	public float GetKeyboardAxis(KeyboardAxisCode keyboardAxisCode, int playerIndex)
	{
		string keyboardAxisCodeName = GetKeyboardAxisCodeName(keyboardAxisCode, playerIndex);
		if (KeyboardAxisMap.ContainsKey(keyboardAxisCodeName))
		{
			string mappedAxisString = KeyboardAxisMap[keyboardAxisCodeName];

			if (!string.IsNullOrEmpty(mappedAxisString))
			{
				return Input.GetAxisRaw(mappedAxisString);
			}
		}

		return 0f;
	}


	public bool GetKeyboardButton(KeyCode buttonCode, ButtonStateType buttonState = ButtonStateType.Down)
	{
		if (buttonCode != KeyCode.None)
		{
			switch (buttonState)
			{
				case ButtonStateType.Down:
					return Input.GetKeyDown(buttonCode);
				case ButtonStateType.Up:
					return Input.GetKeyUp(buttonCode);
				case ButtonStateType.Constant:
					return Input.GetKey(buttonCode);
			}
		}

		return false;
	}

	#endregion

	#region Keyboard Map Setup

	private void SetupKeyboardAxisMap(ref Dictionary<string, string> map, int maxPlayers)
	{
		for (int playerIndex = 0; playerIndex < maxPlayers; playerIndex++)
		{
			map.Add(GetKeyboardAxisCodeName(KeyboardAxisCode.Horizontal, playerIndex), GetKeyboardAxisName(KeyboardHorizontalAxis, playerIndex));
			map.Add(GetKeyboardAxisCodeName(KeyboardAxisCode.Vertical, playerIndex), GetKeyboardAxisName(KeyboardVerticalAxis, playerIndex));
		}
	}

	private string GetKeyboardAxisCodeName(KeyboardAxisCode genericAxisCode, int playerIndex)
	{
		return string.Format("{0}_{1}", genericAxisCode, playerIndex + 1);
	}

	private string GetKeyboardAxisName(string genericAxisName, int playerIndex)
	{
		return string.Format("{0}_{1}", genericAxisName, playerIndex + 1);
	}

	#endregion

	#region Controller Getters

	public float GetControllerAxis(ControllerCode axisCode, int gamePadIndex)
	{
		string axisName = GetGamePadControllerCodeName(axisCode, gamePadIndex);

		if (ControllerMap.ContainsKey(axisName))
		{
			ControllerMappedInput mappedInput = ControllerMap[axisName];
			if (mappedInput.IsAxis)
			{
				if (!string.IsNullOrEmpty(mappedInput.Axis))
				{
					if (currentOS == OS.Mac)
					{
						if (axisCode == ControllerCode.LeftTriggerAxis)
						{
							float leftTriggerAxisValue = Input.GetAxis(mappedInput.Axis);

							if (LeftTriggerPressed)
							{
								leftTriggerAxisValue = MapMacTriggerAxisToPercent(leftTriggerAxisValue);
							}
							else
							{
								if (leftTriggerAxisValue != 0)
								{
									LeftTriggerPressed = true;
									leftTriggerAxisValue = MapMacTriggerAxisToPercent(leftTriggerAxisValue);
								}
							}

							return leftTriggerAxisValue;
						}
						else if (axisCode == ControllerCode.RightTriggerAxis)
						{
							float rightTriggerAxisValue = Input.GetAxis(mappedInput.Axis);

							if (RightTriggerPressed)
							{
								rightTriggerAxisValue = MapMacTriggerAxisToPercent(rightTriggerAxisValue);
							}
							else
							{
								if (rightTriggerAxisValue != 0)
								{
									RightTriggerPressed = true;
									rightTriggerAxisValue = MapMacTriggerAxisToPercent(rightTriggerAxisValue);
								}
							}

							return rightTriggerAxisValue;
						}
						else
						{
							return Input.GetAxis(mappedInput.Axis);
						}
					}
					else
					{
						return Input.GetAxis(mappedInput.Axis);
					}
				}
			}
		}

		return 0f;
	}

	public bool GetControllerButton(ControllerCode buttonCode, int gamePadIndex, ButtonStateType buttonState = ButtonStateType.Down)
	{
		string buttonName = GetGamePadControllerCodeName(buttonCode, gamePadIndex);

		if (ControllerMap.ContainsKey(buttonName))
		{
			ControllerMappedInput mappedInput = ControllerMap[buttonName];
			if (!mappedInput.IsAxis)
			{
				if (mappedInput.Key != KeyCode.None)
				{
					switch (buttonState)
					{
						case ButtonStateType.Down:
							return Input.GetKeyDown(mappedInput.Key);
						case ButtonStateType.Up:
							return Input.GetKeyUp(mappedInput.Key);
						case ButtonStateType.Constant:
							return Input.GetKey(mappedInput.Key);
					}
				}
			}
		}

		return false;
	}

	#endregion

	#region Controller Map Setups

	private void SetupOSXMap(ref Dictionary<string, ControllerMappedInput> map, int maxPlayers)
	{
		currentOS = OS.Mac;

		map.Add(ControllerCode.None.ToString(), new ControllerMappedInput(KeyCode.None));

		for (int gamePadIndex = 0; gamePadIndex < maxPlayers; gamePadIndex++)
		{
			map.Add(GetGamePadControllerCodeName(ControllerCode.LeftBumper, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton13, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.RightBumper, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton14, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.Back, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton10, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.Start, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton9, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.XboxButton, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton15, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.Y, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton19, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.X, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton18, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.A, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton16, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.B, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton17, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.DPadUp, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton5, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.DPadDown, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton6, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.DPadLeft, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton7, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.DPadRight, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton8, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.LeftStickClick, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton11, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.RightStickClick, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton1, gamePadIndex)));

			map.Add(GetGamePadControllerCodeName(ControllerCode.LeftStickHorizontalAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerLeftStickHorizontalAxis, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.LeftStickVerticalAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerLeftStickVerticalAxis, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.RightStickHorizontalAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerRightStickHorizontalAxisOSX, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.RightStickVerticalAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerRightStickVerticalAxisOSX, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.LeftTriggerAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerLeftTriggerAxisOSX, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.RightTriggerAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerRightTriggerAxisOSX, gamePadIndex)));
		}

		map.Add(ControllerCode.DPadHorizontalAxis.ToString(), new ControllerMappedInput(string.Empty));
		map.Add(ControllerCode.DPadVerticalAxis.ToString(), new ControllerMappedInput(string.Empty));

		EventSystemControllerHorizontalAxis = XboxControllerLeftStickHorizontalAxis;
		EventSystemControllerVerticalAxis = XboxControllerLeftStickVerticalAxis;
		EventSystemControllerSubmit = XboxControllerSubmitOSX;
		EventSystemControllerCancel = XboxControllerCancelOSX;
	}

	private void SetupWindowsMap(ref Dictionary<string, ControllerMappedInput> map, int maxPlayers)
	{
		currentOS = OS.Windows;

		map.Add(ControllerCode.None.ToString(), new ControllerMappedInput(KeyCode.None));
		map.Add(ControllerCode.XboxButton.ToString(), new ControllerMappedInput(KeyCode.None));
		map.Add(ControllerCode.DPadUp.ToString(), new ControllerMappedInput(KeyCode.None));
		map.Add(ControllerCode.DPadDown.ToString(), new ControllerMappedInput(KeyCode.None));
		map.Add(ControllerCode.DPadLeft.ToString(), new ControllerMappedInput(KeyCode.None));
		map.Add(ControllerCode.DPadRight.ToString(), new ControllerMappedInput(KeyCode.None));

		for (int gamePadIndex = 0; gamePadIndex < maxPlayers; gamePadIndex++)
		{
			map.Add(GetGamePadControllerCodeName(ControllerCode.LeftBumper, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton4, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.RightBumper, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton5, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.Back, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton6, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.Start, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton7, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.Y, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton3, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.X, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton2, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.A, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton0, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.B, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton1, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.LeftStickClick, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton8, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.RightStickClick, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton9, gamePadIndex)));

			map.Add(GetGamePadControllerCodeName(ControllerCode.LeftStickHorizontalAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerLeftStickHorizontalAxis, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.LeftStickVerticalAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerLeftStickVerticalAxis, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.RightStickHorizontalAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerRightStickHorizontalAxis, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.RightStickVerticalAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerRightStickVerticalAxis, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.LeftTriggerAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerLeftTriggerAxisWindows, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.RightTriggerAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerRightTriggerAxisWindows, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.DPadHorizontalAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerDPadHorizontalWindows, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.DPadVerticalAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerDPadVerticalWindows, gamePadIndex)));
		}

		EventSystemControllerHorizontalAxis = XboxControllerLeftStickHorizontalAxis;
		EventSystemControllerVerticalAxis = XboxControllerLeftStickVerticalAxis;
		EventSystemControllerSubmit = XboxControllerSubmitWindows;
		EventSystemControllerCancel = XboxControllerCancelWindows;
	}

	private void SetupLinuxMap(ref Dictionary<string, ControllerMappedInput> map, int maxPlayers)
	{
		currentOS = OS.Linux;

		map.Add(ControllerCode.None.ToString(), new ControllerMappedInput(KeyCode.None));
		map.Add(ControllerCode.XboxButton.ToString(), new ControllerMappedInput(KeyCode.None));

		for (int gamePadIndex = 0; gamePadIndex < maxPlayers; gamePadIndex++)
		{
			map.Add(GetGamePadControllerCodeName(ControllerCode.LeftBumper, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton4, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.RightBumper, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton5, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.Back, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton6, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.Start, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton7, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.Y, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton3, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.X, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton2, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.A, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton0, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.B, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton1, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.DPadUp, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton13, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.DPadDown, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton14, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.DPadLeft, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton11, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.DPadRight, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton12, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.LeftStickClick, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton9, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.RightStickClick, gamePadIndex), new ControllerMappedInput(GetGamePadKeyCode(KeyCode.JoystickButton10, gamePadIndex)));

			map.Add(GetGamePadControllerCodeName(ControllerCode.LeftStickHorizontalAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerLeftStickHorizontalAxis, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.LeftStickVerticalAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerLeftStickVerticalAxis, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.RightStickHorizontalAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerRightStickHorizontalAxis, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.RightStickVerticalAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerRightStickVerticalAxis, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.LeftTriggerAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerLeftTriggerAxisLinux, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.RightTriggerAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerRightTriggerAxisLinux, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.DPadHorizontalAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerDPadHorizontalLinux, gamePadIndex)));
			map.Add(GetGamePadControllerCodeName(ControllerCode.DPadVerticalAxis, gamePadIndex), new ControllerMappedInput(GetGamePadAxisName(XboxControllerDPadVerticalLinux, gamePadIndex)));
		}

		EventSystemControllerHorizontalAxis = XboxControllerLeftStickHorizontalAxis;
		EventSystemControllerVerticalAxis = XboxControllerLeftStickVerticalAxis;
		EventSystemControllerSubmit = XboxControllerSubmitLinux;
		EventSystemControllerCancel = XboxControllerCancelLinux;
	}

	private KeyCode GetGamePadKeyCode(KeyCode genericKeyCode, int gamePadIndex)
	{
		string genericKeyCodeString = genericKeyCode.ToString();
		if (genericKeyCodeString.Contains("Joystick"))
		{
			string withoutJoystick = genericKeyCodeString.Replace("Joystick", "");
			string withGamePadNumber = string.Format("Joystick{0}{1}", gamePadIndex + 1, withoutJoystick);

			return (KeyCode)System.Enum.Parse(typeof(KeyCode), withGamePadNumber);
		}

		return genericKeyCode;
	}

	private string GetGamePadAxisName(string genericAxisName, int gamePadIndex)
	{
		return string.Format("{0}_{1}", genericAxisName, gamePadIndex + 1);
	}

	private string GetGamePadControllerCodeName(ControllerCode genericControllerCode, int gamePadIndex)
	{
		return string.Format("{0}_{1}", genericControllerCode, gamePadIndex + 1);
	}

	#endregion

	private float MapMacTriggerAxisToPercent(float macTriggerAxisValue)
	{
		float macTriggerAxisValueAsPercent = macTriggerAxisValue + 1f;

		macTriggerAxisValueAsPercent /= 2f;

		return macTriggerAxisValueAsPercent;
	}
}
