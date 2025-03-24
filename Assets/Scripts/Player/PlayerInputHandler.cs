using System;
using UnityEngine;
using UnityEngine.InputSystem;

//Esta clase se va a encargar de gestionar todos los eventos de input del jugador
public class PlayerInputHandler : MonoBehaviour, IPlayerInput
{
	[SerializeField] private InputActionAsset playerInput;

	private InputAction movementAction;
	private InputAction aimingAction;
	private InputAction attackAction;

	private bool usingGamepad = false;

	public Vector2 MovementInput => movementAction.ReadValue<Vector2>();

	public Vector2 AimingInput
	{
		get
		{
			Vector2 rawInput = aimingAction.ReadValue<Vector2>();

			// Verificar si estamos usando gamepad
			if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
			{
				usingGamepad = true;
				// Para el gamepad, aplicar una zona muerta para evitar drift
				if (rawInput.sqrMagnitude < 0.1f)
					return Vector2.zero;

				// Devolver el input normalizado del stick
				return rawInput.normalized;
			}
			else if (Mouse.current != null && Mouse.current.wasUpdatedThisFrame)
			{
				usingGamepad = false;
				// Para el ratón, devolver la posición tal cual
				return rawInput;
			}

			// Si no podemos determinar, devolver la entrada tal cual
			return rawInput;
		}
	}

	public bool IsUsingGamepad => usingGamepad;


	public bool isAttacking { get; private set; }

	public event Action OnAttack;



	private void Awake()
	{
		movementAction = playerInput.FindActionMap("Movement").FindAction("Move");
		aimingAction = playerInput.FindActionMap("Combat").FindAction("Aim");
		attackAction = playerInput.FindActionMap("Combat").FindAction("Attack");

		attackAction.performed += OnAttackPerformed;

		movementAction.Enable();
		aimingAction.Enable();
		attackAction.Enable();
	}

	private void OnDisable()
	{
		movementAction?.Disable();
		aimingAction?.Disable();
	}

	private void OnAttackPerformed(InputAction.CallbackContext context)
	{
		isAttacking = true;
		OnAttack?.Invoke();
	}
}
