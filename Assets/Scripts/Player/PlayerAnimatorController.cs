using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    private Animator animator;
    private PlayerMovementController movementController;
	private PlayerInteractionsController interactionsController;
	private PlayerInputHandler inputHandler;

	private void Awake()
	{
        movementController = GetComponent<PlayerMovementController>();
		interactionsController = GetComponent<PlayerInteractionsController>();
		inputHandler = GetComponent<PlayerInputHandler>();
		animator = GetComponent<Animator>();
	}

	// Start is called before the first frame update
	void Start()
	{
		if (movementController)
		{
			movementController.OnVelocityChanged += HandleVelocityChange;
			movementController.OnJump += SetJumpingStatus;
			movementController.OnDash += SetDashingStatus;
		}

		if (inputHandler)
		{
			inputHandler.OnHealthRecover += DisplayHealthRecoveryAnimation;
			inputHandler.OnManaRecover += DisplayManaRecoveryAnimation;
			
		}

		if(interactionsController)
		{
			interactionsController.OnHeavyCarrying += SetHeavyCarryingStatus;
			interactionsController.OnLightCarrying += SetLightCarryingStatus;
		}
	}

	private void OnDisable()
	{
		if (movementController)
		{
			movementController.OnVelocityChanged -= HandleVelocityChange;
			movementController.OnJump -= SetJumpingStatus;
			movementController.OnDash -= SetDashingStatus;
		}

		if (inputHandler)
		{
			inputHandler.OnHealthRecover -= DisplayHealthRecoveryAnimation;
			inputHandler.OnManaRecover -= DisplayManaRecoveryAnimation;
		}

		if (interactionsController)
		{
			interactionsController.OnHeavyCarrying -= SetHeavyCarryingStatus;
			interactionsController.OnLightCarrying -= SetLightCarryingStatus;
		}
	}

	private void Update()
	{
		//if(movementController)
		//{
		//	if(movementController.GetGroundedState() == false && animator.GetBool("IsGrounded") == true) //Si el jugador no esta en el suelo y no estamos ya en la animacion de jump, la ejecutamos
		//	{
		//		animator.SetBool("IsGrounded", false);
		//	}
		//	else if(movementController.GetGroundedState() == true && animator.GetBool("IsGrounded") == false) //Si el jugador esta en el suelo y estamos en la animacion de jump, dejamos de estarlo
		//	{
		//		animator.SetBool("IsGrounded", true);
		//	}
		//}
	}

	private void HandleVelocityChange(Vector3 currentVelocity)
	{
		float speed = new Vector2(currentVelocity.x, currentVelocity.z).magnitude / movementController.GetMovementSpeed();
		//Debug.Log("Speed = " + speed);
		if(animator)
		{
			animator.SetFloat("Velocity", speed);

			//Debug.Log("Animator velocity: " + animator.GetFloat("Velocity"));
		}
	}

	private void DisplayHealthRecoveryAnimation()
	{
		if(animator)
		{
			animator.SetTrigger("HealthRecovery");
		}
	}

	private void DisplayManaRecoveryAnimation()
	{
		if (animator)
		{
			animator.SetTrigger("ManaRecovery");
		}
	}

	private void SetHeavyCarryingStatus(bool status)
	{
		if(animator)
		{
			animator.SetBool("Carry_Heavy", status);
		}
	}

	private void SetLightCarryingStatus(bool status)
	{
		if(animator)
		{
			animator.SetBool("Carry_Light", status);
		}
	}

	private void SetJumpingStatus(bool status)
	{
		if(animator)
		{
			Debug.Log("Changing jumping status");
			animator.SetBool("IsGrounded", !status);
		}
	}

	private void SetDashingStatus(bool status)
	{
		if(animator)
		{
			animator.SetBool("Dashing", status);
		}
	}
}
