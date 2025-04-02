using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    private Animator animator;
    private PlayerMovementController movementController;

	private void Awake()
	{
        movementController = GetComponent<PlayerMovementController>();
		animator = GetComponent<Animator>();
	}

	private void OnDisable()
	{
		if (movementController)
		{
			movementController.OnVelocityChanged -= HandleVelocityChange;
		}
	}

	// Start is called before the first frame update
	void Start()
    {
        if(movementController)
		{
			movementController.OnVelocityChanged += HandleVelocityChange;
		}
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
