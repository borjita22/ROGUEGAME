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

    private void HandleVelocityChange(Vector3 currentVelocity)
	{
		float speed = new Vector2(currentVelocity.x, currentVelocity.z).magnitude / movementController.GetMovementSpeed();
		//Debug.Log("Speed = " + speed);
		if(animator)
		{
			animator.SetFloat("Velocity", speed);

			Debug.Log("Animator velocity: " + animator.GetFloat("Velocity"));
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
}
