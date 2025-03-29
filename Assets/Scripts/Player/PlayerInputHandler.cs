using System;
using UnityEngine;
using UnityEngine.InputSystem;

//Esta clase se va a encargar de gestionar todos los eventos de input del jugador
public class PlayerInputHandler : MonoBehaviour, IPlayerInput
{
	[SerializeField] private InputActionAsset playerInput;

	//Movement, aiming and attack actions
	private InputAction movementAction;
	private InputAction aimingAction;
	private InputAction attackAction;
	private InputAction resetVRotationAction;
	private InputAction weaponStatusAction;

	//Skill related actions: Keyboard
	private InputAction switchSkillAction;
	private InputAction useSkillAction;

	//Skill related actions: Gamepad
	private InputAction openSkillWheelAction;
	private InputAction useSkillA_Action;
	private InputAction useSkillB_Action;
	private InputAction useSkillX_Action;
	private InputAction useSkillY_Action;

	//Interaction related actions
	private InputAction interactAction;


	public Vector2 MovementInput => movementAction.ReadValue<Vector2>();

	public Action _ResetVRotation;

	private Weapon weapon;
	private bool weaponEnabled = true;


	public Vector2 AimingInput
	{
		get
		{
			Vector2 rawInput = aimingAction.ReadValue<Vector2>();

			// Verificar si estamos usando gamepad
			if (InputDeviceManager.Instance.IsUsingGamepad)
			{
				// Para el gamepad, aplicar una zona muerta para evitar drift
				if (rawInput.sqrMagnitude < 0.1f)
					return Vector2.zero;

				// Devolver el input normalizado del stick
				return rawInput.normalized;
			}
			else
			{
				// Para el ratón, devolver la posición tal cual
				return rawInput;
			}
		}
	}

	public bool IsUsingGamepad => InputDeviceManager.Instance.IsUsingGamepad;


	public bool isAttacking { get; private set; }

	public event Action OnAttack;

	//Action events: Keyboard
	public event Action OnSwitchSkill;
	public event Action OnUseSkill;

	//Action events: Gamepad
	public delegate void OnOpenSkillWheel(bool status);
	public event OnOpenSkillWheel _OnOpenSkillWheel;


	public delegate void OnUseGamepadSkill(int index);
	public event OnUseGamepadSkill _OnUseGamepadSkill;

	public event Action OnInteract;

	


	//TODO: Refactorizar para mejor division entre movimiento, ataque y skills

	private void Awake()
	{
		weapon = GetComponentInChildren<Weapon>();

		movementAction = playerInput.FindActionMap("Movement").FindAction("Move");
		aimingAction = playerInput.FindActionMap("Combat").FindAction("Aim");
		attackAction = playerInput.FindActionMap("Combat").FindAction("Attack");
		resetVRotationAction = playerInput.FindActionMap("Combat").FindAction("ResetVerticalRotation");
		weaponStatusAction = playerInput.FindActionMap("Combat").FindAction("SetWeaponStatus");

		switchSkillAction = playerInput.FindActionMap("Skills").FindAction("SwitchSkill");
		useSkillAction = playerInput.FindActionMap("Skills").FindAction("UseSkill");

		openSkillWheelAction = playerInput.FindActionMap("Skills").FindAction("OpenSkillWheel");
		useSkillA_Action = playerInput.FindActionMap("Skills").FindAction("UseSkillA");
		useSkillB_Action = playerInput.FindActionMap("Skills").FindAction("UseSkillB");
		useSkillX_Action = playerInput.FindActionMap("Skills").FindAction("UseSkillX");
		useSkillY_Action = playerInput.FindActionMap("Skills").FindAction("UseSkillY");

		interactAction = playerInput.FindActionMap("Interaction").FindAction("Interact");

		attackAction.performed += OnAttackPerformed;
		resetVRotationAction.performed += OnResetVRotationPerformed;
		weaponStatusAction.performed += SetWeaponStatus;

		switchSkillAction.performed += ctx => SwitchSkillAction();
		useSkillAction.performed += ctx => UseSkillKeyboard();

		openSkillWheelAction.performed += ctx => OnSkillWheelStatusChange(true);
		openSkillWheelAction.canceled += ctx => OnSkillWheelStatusChange(false);

		useSkillA_Action.performed += ctx => OnUseSkillOnGamepad(0);
		useSkillB_Action.performed += ctx => OnUseSkillOnGamepad(1);
		useSkillX_Action.performed += ctx => OnUseSkillOnGamepad(2);
		useSkillY_Action.performed += ctx => OnUseSkillOnGamepad(3);

		interactAction.performed += ctx => OnInteract?.Invoke();


		EnableInputActions();
	}

	private void OnEnable()
	{
		if(InputDeviceManager.Instance != null)
		{
			InputDeviceManager.Instance.OnInputDeviceChanged += OnInputDeviceChanged;
		}
	}

	private void OnDisable()
	{
		if (InputDeviceManager.Instance != null)
		{
			InputDeviceManager.Instance.OnInputDeviceChanged += OnInputDeviceChanged;
		}

		DisableInputActions();
	}

	private void EnableInputActions()
	{
		movementAction.Enable();
		aimingAction.Enable();
		attackAction.Enable();
		resetVRotationAction.Enable();
		weaponStatusAction.Enable();


		switchSkillAction.Enable();
		useSkillAction.Enable();
		openSkillWheelAction.Enable();

		useSkillA_Action.Enable();
		useSkillB_Action.Enable();
		useSkillX_Action.Enable();
		useSkillY_Action.Enable();

		interactAction.Enable();
	}

	private void DisableInputActions()
	{
		movementAction?.Disable();
		aimingAction?.Disable();
		attackAction.Disable();
		resetVRotationAction.Disable();


		switchSkillAction.Disable();
		useSkillAction.Disable();
		openSkillWheelAction.Disable();

		useSkillA_Action.Disable();
		useSkillB_Action.Disable();
		useSkillX_Action.Disable();
		useSkillY_Action.Disable();

		interactAction.Disable();
	}

	private void OnInputDeviceChanged(InputDeviceType deviceType)
	{
		Debug.Log($"PlayerInputHandler: Dispositivo cambiado a {deviceType}");
	}

	private void OnAttackPerformed(InputAction.CallbackContext context)
	{
		isAttacking = true;
		OnAttack?.Invoke();
	}

	private void OnResetVRotationPerformed(InputAction.CallbackContext context)
	{
		_ResetVRotation?.Invoke();
	}

	//Este metodo quizas podria decirle al controlador del jugador que desactive el arma en lugar de desactivarla el mismo
	//Deberiamos tener un evento de activacion/desactivacion del arma. ESTA POR VER
	private void SetWeaponStatus(InputAction.CallbackContext context)
	{
		weaponEnabled = !weaponEnabled;

		weapon.gameObject.SetActive(weaponEnabled);

	}

	private void SwitchSkillAction()
	{
		OnSwitchSkill?.Invoke();
	}

	private void UseSkillKeyboard()
	{
		OnUseSkill?.Invoke();
	}


	private void OnSkillWheelStatusChange(bool status)
	{
		_OnOpenSkillWheel?.Invoke(status);
	}

	private void OnUseSkillOnGamepad(int index)
	{
		_OnUseGamepadSkill?.Invoke(index);
	}

}
