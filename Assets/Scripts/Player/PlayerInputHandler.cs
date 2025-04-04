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

	private InputAction healthRecoverAction;
	private InputAction manaRecoverAction;


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
	public event Action OnAttackReleased;

	//Action events: Keyboard
	public event Action OnSwitchSkill;
	public event Action OnUseSkill;

	//Action events: Gamepad
	public delegate void OnOpenSkillWheel(bool status);
	public event OnOpenSkillWheel _OnOpenSkillWheel;


	public delegate void OnUseGamepadSkill(int index);
	public event OnUseGamepadSkill _OnUseGamepadSkill;

	public event Action OnInteract;

	//Luego estos inputs van a necesitar un parametro que determine la cantidad de salud/mana que van a permitir recuperar, y que se le pasara al controlador principal del jugador, que sera
	//quien tenga acceso a la cantidad de salud y mana del jugador
	public event Action OnHealthRecover; 
	public event Action OnManaRecover;
	


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
		healthRecoverAction = playerInput.FindActionMap("Interaction").FindAction("HealthRecover");
		manaRecoverAction = playerInput.FindActionMap("Interaction").FindAction("ManaRecover");

		attackAction.performed += OnAttackPerformed;
		attackAction.canceled += OnAttackCanceled;
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
		healthRecoverAction.performed += ctx => OnHealthRecover?.Invoke();
		manaRecoverAction.performed += ctx => OnManaRecover?.Invoke();


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

	//Luego esto va a haber que dividirlo en diferentes tipos de input, por si solamente se quisiesen habilitar o deshabilitar determinados tipos
	//Por ejemplo, mientras estas cargando con algun objeto como una caja, no vas a poder disparar o curarte salud ni mana
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
		healthRecoverAction.Enable();
		manaRecoverAction.Enable();
	}

	private void DisableInputActions()
	{

		attackAction.performed -= OnAttackPerformed;
		attackAction.canceled -= OnAttackCanceled;
		resetVRotationAction.performed -= OnResetVRotationPerformed;
		weaponStatusAction.performed -= SetWeaponStatus;

		switchSkillAction.performed -= ctx => SwitchSkillAction();
		useSkillAction.performed -= ctx => UseSkillKeyboard();

		openSkillWheelAction.performed -= ctx => OnSkillWheelStatusChange(true);
		openSkillWheelAction.canceled -= ctx => OnSkillWheelStatusChange(false);

		useSkillA_Action.performed -= ctx => OnUseSkillOnGamepad(0);
		useSkillB_Action.performed -= ctx => OnUseSkillOnGamepad(1);
		useSkillX_Action.performed -= ctx => OnUseSkillOnGamepad(2);
		useSkillY_Action.performed -= ctx => OnUseSkillOnGamepad(3);

		interactAction.performed -= ctx => OnInteract?.Invoke();
		healthRecoverAction.performed -= ctx => OnHealthRecover?.Invoke();
		manaRecoverAction.performed -= ctx => OnManaRecover?.Invoke();

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
		healthRecoverAction.Disable();
		manaRecoverAction.Disable();
	}

	private void OnInputDeviceChanged(InputDeviceType deviceType)
	{
		Debug.Log($"PlayerInputHandler: Dispositivo cambiado a {deviceType}");
	}

	private void OnAttackPerformed(InputAction.CallbackContext context)
	{
		isAttacking = true; //esta variable de aqui de momento no nos esta sirviendo para nada
		OnAttack?.Invoke();
	}

	private void OnAttackCanceled(InputAction.CallbackContext context)
	{
		isAttacking = false;
		OnAttackReleased?.Invoke();
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
