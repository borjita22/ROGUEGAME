using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform spriteTransform;

    [Header("Movement configuration")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float decceleration;


    private Rigidbody rb;
    private PlayerInputHandler playerInput;
    private Vector3 currentVelocity;


	private void Awake()
	{
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInputHandler>();

        if(rb)
		{
            rb.constraints = RigidbodyConstraints.FreezeRotation;
		}
	}

	private void FixedUpdate()
	{
        ProcessMovement();
	}

    private void ProcessMovement()
	{
        Vector2 input = playerInput.MovementInput;
        Vector3 movementDirection = new Vector3(input.x, 0, input.y).normalized;

        Vector3 targetVelocity = movementDirection * moveSpeed;

        float smoothFactor = movementDirection.magnitude > 0.1f ? acceleration : decceleration;

        currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, targetVelocity.x, smoothFactor * Time.fixedDeltaTime);
        currentVelocity.z = Mathf.MoveTowards(currentVelocity.z, targetVelocity.z, smoothFactor * Time.fixedDeltaTime);

        currentVelocity.y = rb.velocity.y;

        rb.velocity = currentVelocity;
	}

    public Vector3 GetMovementDirection()
	{
        return rb.velocity;
	}
}
