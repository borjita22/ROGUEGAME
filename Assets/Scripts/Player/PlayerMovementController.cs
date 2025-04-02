using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform spriteTransform;

    [Header("Movement Configuration")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float deceleration = 35f;
    [SerializeField] private float directionChangeMultiplier = 1.5f;
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.Linear(0, 0, 1, 1); // Para mapear el input a la velocidad

    // Variables internas
    private Rigidbody rb;
    private PlayerInputHandler playerInput;
    private Vector3 currentVelocity;

    // Propiedades para el controlador de animación
    public float NormalizedSpeed => currentVelocity.magnitude / moveSpeed;
    public Vector3 MovementDirection => new Vector3(currentVelocity.x, 0, currentVelocity.z).normalized;
    public bool IsMoving => currentVelocity.magnitude > 0.1f;

    // Evento para notificar cambios de velocidad
    public event System.Action<Vector3> OnVelocityChanged;

    private bool isFacingRight = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInputHandler>();

        if (rb)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    private void FixedUpdate()
    {
        ProcessMovement();
        ManageCharacterOrientation();
    }

    private void ProcessMovement()
    {
        // Obtener input
        Vector2 input = playerInput.MovementInput;

        // Calcular dirección y magnitud
        Vector3 movementDirection = new Vector3(input.x, 0, input.y);
        float inputMagnitude = Mathf.Min(movementDirection.magnitude, 1f);

        // Normalizar la dirección (si hay input)
        if (inputMagnitude > 0.1f)
        {
            movementDirection.Normalize();
        }

        // Aplicar curva de respuesta a la magnitud del input
        float speedFactor = speedCurve.Evaluate(inputMagnitude);

        // Calcular velocidad objetivo basada en la magnitud del input
        Vector3 targetVelocity = movementDirection * moveSpeed * speedFactor;

        // Calcular factor de suavizado
        float smoothFactor;

        if (inputMagnitude > 0.1f)
        {
            // Comprobar si hay un cambio significativo de dirección
            if (Vector3.Dot(movementDirection, MovementDirection) < 0.5f && IsMoving)
            {
                // Cambio brusco de dirección - aplicar factor multiplicador
                smoothFactor = acceleration * directionChangeMultiplier;
            }
            else
            {
                // Aceleración normal
                smoothFactor = acceleration;
            }
        }
        else
        {
            // Desaceleración al detenerse
            smoothFactor = deceleration;
        }

        // Aplicar movimiento con suavizado
        currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, targetVelocity.x, smoothFactor * Time.fixedDeltaTime);
        currentVelocity.z = Mathf.MoveTowards(currentVelocity.z, targetVelocity.z, smoothFactor * Time.fixedDeltaTime);
        currentVelocity.y = rb.velocity.y;

        // Aplicar velocidad al rigidbody
        rb.velocity = currentVelocity;

        // Notificar cambios de velocidad
        OnVelocityChanged?.Invoke(currentVelocity);
    }

    // Método público para debugging
    public Vector3 GetCurrentVelocity()
    {
        return currentVelocity;
    }

    public float GetMovementSpeed()
	{
        return moveSpeed;
	}

    private void ManageCharacterOrientation()
	{
        if(currentVelocity.x < -0.1f && isFacingRight)
		{
            this.transform.localScale = new Vector3(-1f, 1f, 1f);
            isFacingRight = false;
		}
        else if(currentVelocity.x > 0.1f && !isFacingRight)
		{
            this.transform.localScale = new Vector3(1f, 1f, 1f);
            isFacingRight = true;
        }
	}

}