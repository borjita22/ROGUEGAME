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
	private InputAction weaponEffectAction;

	//Jump action
	private InputAction jumpAction;

	//Dash action
	private InputAction dashAction;

	//Hook action
	private InputAction hookAction;

	//Skill related actions: Keyboard
	private InputAction switchSkillAction;
	private InputAction useSkillAction;

	//Interaction related actions
	private InputAction interactAction;

	private InputAction healthRecoverAction;
	private InputAction manaRecoverAction;


	public Vector2 MovementInput => movementAction.ReadValue<Vector2>();

	public Action _ResetVRotation;

	private Weapon weapon;
	private bool weaponEnabled = true;

	private bool skillsWheelEnabled = false;


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
				// Para el rat?n, devolver la posici?n tal cual
				return rawInput;
			}
		}
	}

	public bool IsUsingGamepad => InputDeviceManager.Instance.IsUsingGamepad;


	public bool isAttacking { get; private set; }

	public event Action OnJump;
	private bool jumpButtonHeld;

	public event Action OnDash;

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


	public event Action OnSwitchWeaponEffect;

	public event Action OnHookEnabled;


	//TODO: Refactorizar para mejor division entre movimiento, ataque y skills

	private void Awake()
	{
		weapon = FindFirstObjectByType<Weapon>();

		movementAction = playerInput.FindActionMap("Movement").FindAction("Move");
		aimingAction = playerInput.FindActionMap("Combat").FindAction("Aim");
		attackAction = playerInput.FindActionMap("Combat").FindAction("Attack");
		resetVRotationAction = playerInput.FindActionMap("Combat").FindAction("ResetVerticalRotation");
		weaponStatusAction = playerInput.FindActionMap("Combat").FindAction("SetWeaponStatus");
		weaponEffectAction = playerInput.FindActionMap("Combat").FindAction("SetWeaponEffect");

		jumpAction = playerInput.FindActionMap("Movement").FindAction("Jump");
		dashAction = playerInput.FindActionMap("Movement").FindAction("Dash");
		hookAction = playerInput.FindActionMap("Movement").FindAction("Hook");

		switchSkillAction = playerInput.FindActionMap("Skills").FindAction("SwitchSkill");
		useSkillAction = playerInput.FindActionMap("Skills").FindAction("UseSkill");

		interactAction = playerInput.FindActionMap("Interaction").FindAction("Interact");
		healthRecoverAction = playerInput.FindActionMap("Interaction").FindAction("HealthRecover");
		manaRecoverAction = playerInput.FindActionMap("Interaction").FindAction("ManaRecover");

		attackAction.performed += OnAttackPerformed;
		attackAction.canceled += OnAttackCanceled;
		resetVRotationAction.performed += OnResetVRotationPerformed;
		weaponStatusAction.performed += SetWeaponStatus;
		weaponEffectAction.performed += SwitchWeaponEffect;

		jumpAction.performed += OnJumpPerformed;
		jumpAction.canceled += OnJumpCancelled;

		dashAction.performed += ctx => OnDashPerformed();

		hookAction.performed += OnUseHook;

		switchSkillAction.performed += ctx => SwitchSkillAction();
		useSkillAction.performed += ctx => UseSkillKeyboard();

		/*
		openSkillWheelAction.performed += ctx => OnSkillWheelStatusChange(true);
		openSkillWheelAction.canceled += ctx => OnSkillWheelStatusChange(false);

		useSkillA_Action.performed += ctx => OnUseSkillOnGamepad(0);
		useSkillB_Action.performed += ctx => OnUseSkillOnGamepad(1);
		useSkillX_Action.performed += ctx => OnUseSkillOnGamepad(2);
		useSkillY_Action.performed += ctx => OnUseSkillOnGamepad(3);
		*/
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
		weaponEffectAction.Enable();

		jumpAction.Enable();
		dashAction.Enable();
		hookAction.Enable();


		switchSkillAction.Enable();
		useSkillAction.Enable();

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
		weaponEffectAction.performed -= SwitchWeaponEffect;

		jumpAction.performed -= OnJumpPerformed;
		jumpAction.canceled -= OnJumpCancelled; //Esto ya no me interesa

		dashAction.performed -= ctx => OnDashPerformed();

		hookAction.performed -= OnUseHook;

		switchSkillAction.performed -= ctx => SwitchSkillAction();
		useSkillAction.performed -= ctx => UseSkillKeyboard();

		/*
		openSkillWheelAction.performed -= ctx => OnSkillWheelStatusChange(true);
		openSkillWheelAction.canceled -= ctx => OnSkillWheelStatusChange(false);

		useSkillA_Action.performed -= ctx => OnUseSkillOnGamepad(0);
		useSkillB_Action.performed -= ctx => OnUseSkillOnGamepad(1);
		useSkillX_Action.performed -= ctx => OnUseSkillOnGamepad(2);
		useSkillY_Action.performed -= ctx => OnUseSkillOnGamepad(3);
		*/

		interactAction.performed -= ctx => OnInteract?.Invoke();
		healthRecoverAction.performed -= ctx => OnHealthRecover?.Invoke();
		manaRecoverAction.performed -= ctx => OnManaRecover?.Invoke();

		movementAction?.Disable();
		aimingAction?.Disable();
		attackAction.Disable();
		resetVRotationAction.Disable();
		weaponStatusAction.Disable();
		weaponEffectAction.Disable();

		jumpAction.Disable();
		dashAction.Disable();
		hookAction.Disable();


		switchSkillAction.Disable();
		useSkillAction.Disable();


		interactAction.Disable();
		healthRecoverAction.Disable();
		manaRecoverAction.Disable();
	}

	private void OnInputDeviceChanged(InputDeviceType deviceType)
	{
		Debug.Log($"PlayerInputHandler: Dispositivo cambiado a {deviceType}");
	}

	//No se puede invocar el evento si tenemos la rueda de habilidades activa
	private void OnJumpPerformed(InputAction.CallbackContext context)
	{
		if (skillsWheelEnabled == true) return;

		jumpButtonHeld = true;
		OnJump?.Invoke();
	}

	private void OnJumpCancelled(InputAction.CallbackContext context)
	{
		jumpButtonHeld = false;
	}

	private void OnDashPerformed()
	{
		OnDash?.Invoke();
	}

	private void OnUseHook(InputAction.CallbackContext context)
	{
		OnHookEnabled?.Invoke();
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

	private void SwitchWeaponEffect(InputAction.CallbackContext context)
    {
		OnSwitchWeaponEffect?.Invoke();
    }

	public void DisableWeapon()
	{
		weaponEnabled = false;
		weapon.gameObject.SetActive(false);
		weaponStatusAction.Disable();
	}

	public void EnableWeapon()
	{
		weaponEnabled = true;
		weapon.gameObject.SetActive(true);
		weaponStatusAction.Enable();
	}

	private void SwitchSkillAction()
	{
		OnSwitchSkill?.Invoke();
	}

	private void UseSkillKeyboard()
	{
		OnUseSkill?.Invoke();
	}

	

	public bool IsJumpHeld()
	{
		return jumpButtonHeld;
	}

}
