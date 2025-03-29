using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputDeviceManager : MonoBehaviour
{
	private static InputDeviceManager _instance;

	public static InputDeviceManager Instance
	{
		get
		{
			if(_instance == null)
			{
				GameObject managerObject = new GameObject("InputDeviceManager");

				_instance = managerObject.AddComponent<InputDeviceManager>();
			}

			return _instance;
		}
	}

	public event Action<InputDeviceType> OnInputDeviceChanged;

	[SerializeField] private InputDeviceType _currentDeviceType = InputDeviceType.Keyboard;

	public InputDeviceType CurrentDeviceType => _currentDeviceType;

	public bool IsUsingGamepad => _currentDeviceType == InputDeviceType.Gamepad;

	private float lastKeyboardActivityTime;
	private float lastGamepadActivityTime;


	private void Awake()
	{
		if(_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
			return;
		}

		_instance = this;
		DontDestroyOnLoad(this.gameObject);

		lastKeyboardActivityTime = Time.unscaledTime;
		lastGamepadActivityTime = 0f;
	}

	private void Update()
	{
		DetectInputDevice();
	}

	private void DetectInputDevice()
	{
		if (Keyboard.current != null && Keyboard.current.wasUpdatedThisFrame)
		{
			lastKeyboardActivityTime = Time.unscaledTime;
		}

		bool anyButtonPressed, significantStickMovement;

		DetectGamepadUsage(out anyButtonPressed, out significantStickMovement);

		if (anyButtonPressed || significantStickMovement)
		{
			lastGamepadActivityTime = Time.unscaledTime;

		}

		//Debug.Log($"Last keyboard activity time: {lastKeyboardActivityTime}");
		//Debug.Log($"Last gamepad activity time: {lastGamepadActivityTime}");

		// CAMBIO PRINCIPAL: Simplificar la lógica de cambio
		bool deviceChanged = false;

		// Si el último dispositivo usado fue el gamepad y actualmente estamos en modo teclado
		if (_currentDeviceType == InputDeviceType.Keyboard && lastGamepadActivityTime > lastKeyboardActivityTime)
		{
			_currentDeviceType = InputDeviceType.Gamepad;
			deviceChanged = true;
			Debug.Log("Change device to gamepad");
		}
		// Si el último dispositivo usado fue el teclado/ratón y actualmente estamos en modo gamepad
		else if (_currentDeviceType == InputDeviceType.Gamepad && lastKeyboardActivityTime > lastGamepadActivityTime)
		{
			_currentDeviceType = InputDeviceType.Keyboard;
			deviceChanged = true;
			Debug.Log("Change device to keyboard");
		}

		if (deviceChanged)
		{
			Debug.Log($"Input device changed to {_currentDeviceType}. Keyboard time: {lastKeyboardActivityTime}, Gamepad time: {lastGamepadActivityTime}");
			OnInputDeviceChanged?.Invoke(_currentDeviceType);
		}
	}

	private void DetectGamepadUsage(out bool anyButtonPressed, out bool significantStickMovement)
	{
		if (Gamepad.current == null)
		{
			anyButtonPressed = false;
			significantStickMovement = false;
			return;
		}
		

		anyButtonPressed = Gamepad.current.buttonSouth.wasPressedThisFrame ||
				Gamepad.current.buttonNorth.wasPressedThisFrame ||
				Gamepad.current.buttonEast.wasPressedThisFrame ||
				Gamepad.current.buttonWest.wasPressedThisFrame ||
				Gamepad.current.leftShoulder.wasPressedThisFrame ||
				Gamepad.current.rightShoulder.wasPressedThisFrame ||
				Gamepad.current.leftTrigger.wasPressedThisFrame ||
				Gamepad.current.rightTrigger.wasPressedThisFrame ||
				Gamepad.current.startButton.wasPressedThisFrame ||
				Gamepad.current.selectButton.wasPressedThisFrame;

		// También puedes verificar movimientos significativos de los sticks
		//ESTO ME LA PELA
		significantStickMovement = Gamepad.current.leftStick.ReadValue().magnitude > 0.3f ||
			Gamepad.current.rightStick.ReadValue().magnitude > 0.3f;
	}

	public void ForceInputDeviceType(InputDeviceType deviceType)
	{
		if(_currentDeviceType != deviceType)
		{
			_currentDeviceType = deviceType;
			OnInputDeviceChanged?.Invoke(_currentDeviceType);   
		}
	}
}

public enum InputDeviceType
{
	Keyboard,
	Gamepad
}
