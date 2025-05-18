using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private PlayerStats playerStats;

    [Header("Movement Configuration")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float maxMoveSpeed = 16f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float deceleration = 35f;
    [SerializeField] private float directionChangeMultiplier = 1.5f;
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.Linear(0, 0, 1, 1); // Para mapear el input a la velocidad

    [Space]
    [Header("Ground state configuration")]
    [SerializeField] private PhysicMaterial playerPhysicsMaterial;
    [SerializeField] private float defaultDrag = 0.5f;
    [SerializeField] private float slopeDrag = 5f;

    [SerializeField] private float slopeAngleThreshold = 20f;
    [SerializeField] private float slopeRaycastDistance = 1.5f;
    [SerializeField] private float movementThreshold = 0.1f;
    [SerializeField] private bool isOnSlope = false;
    [SerializeField] private float currentSurfaceAngle = 0f;

    [Space]
    [Header("Jump configuration")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float fallMultiplier = 2f;
    [SerializeField] private float horizontalMoveDecelerationFactor = 1.2f;
    [SerializeField] private float jumpCooldown = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private Transform groundCheckpoint;

    [SerializeField] private bool isGrounded;
    private int jumpIndex = 0;
    [SerializeField] private bool canDoubleJump;
    private float initialJumpVelocity;
    [SerializeField] public bool isJumping = false;
    private float jumpTimer = 0f;

    [Space]
    [Header("Jump buffer input config")]
    private Queue<float> jumpInputBuffer = new Queue<float>();
    [SerializeField] private float jumpBufferedTime = 0.2f;

    [Space]
    [Header("Dash configuration")]
    [SerializeField] private bool dashEnabled = true;
    [SerializeField] private float dashDistance;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashCooldown;
    private float remainingCooldown;
    private bool isDashOnCooldown = false;
    [SerializeField] private bool isExecutingDash = false;
    private Coroutine dashCoroutine;
    [SerializeField] private GameObject dashTrail;
    private bool dashRaycastCollide = false;


    // Variables internas
    private Rigidbody rb;
    private PlayerInputHandler playerInput;
    private Vector3 currentVelocity;

    // Propiedades para el controlador de animaci?n
    public float NormalizedSpeed => currentVelocity.magnitude / moveSpeed;
    public Vector3 MovementDirection => new Vector3(currentVelocity.x, 0, currentVelocity.z).normalized;
    public bool IsMoving => currentVelocity.magnitude > 0.1f;

    // Evento para notificar cambios de velocidad
    public event System.Action<Vector3> OnVelocityChanged;

    public event System.Action<bool> OnJump;
    private bool isJumpFinished = false;

    public event System.Action<bool> OnDash;


    private bool isFacingRight = true;

    

    private HookController hookController;

    public bool isGrappling = false;
    [SerializeField] private bool grapplingStarted = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInputHandler>();

        if (rb)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            playerPhysicsMaterial = GetComponent<Collider>().material;
        }

        hookController = GetComponentInChildren<HookController>();
    }

    private void OnEnable()
    {
        if (playerInput)
        {
            playerInput.OnJump += ProcessJump;
            playerInput.OnDash += InitializeDash;
        }

        if(playerStats)
        {
            playerStats.OnStatChange += OnPlayerStatChanged;
        }

        if(hookController)
        {
            hookController.OnGrapplingStarted += StartHook;
        }

    }

    private void OnDisable()
    {
        if (playerInput)
        {
            playerInput.OnJump -= ProcessJump;
            playerInput.OnDash -= InitializeDash;
        }

        if (playerStats)
        {
            playerStats.OnStatChange -= OnPlayerStatChanged;
        }

        if (hookController)
        {
            hookController.OnGrapplingStarted -= StartHook;
        }

    }

	private void Start()
	{
		if(dashTrail)
		{
            dashTrail.SetActive(false);
		}
	}

	private void Update()
	{
        CheckGrounded();
        CheckSurfaceAngle();
        
        if(!isGrounded && isJumping)
		{
			UpdateJumpTime();
		}
        else if(isGrounded)
		{
            if(rb.velocity.y < 0.1f)
			{
                ResetJumpState(); //esto deberia resetearse solamente si tocas el suelo estando en estado jumping
            }
            if (grapplingStarted)
            {
                isGrappling = false;
                grapplingStarted = false;
            }

            ProcessBufferedJump();
            
		}

        DashRaycastDetection();
        UpdateDashCooldown();

        CleanJumpBuffer();
	}

    private void ProcessBufferedJump()
	{
        if(jumpInputBuffer.Count > 0 && isGrounded)
		{
            float oldest = jumpInputBuffer.Peek();

            if(Time.time - oldest <= jumpBufferedTime)
			{
                jumpIndex = 0;
                //PerformJump();
                Invoke(nameof(PerformJump), 0.05f); //Introducir pequeño delay al salto en buffer
			}

            jumpInputBuffer.Clear();
		}
	}

    private void CleanJumpBuffer()
    {
        // Eliminar entradas que excedan el tiempo del buffer
        while (jumpInputBuffer.Count > 0 && Time.time - jumpInputBuffer.Peek() > jumpBufferedTime)
        {
            jumpInputBuffer.Dequeue();
        }
    }

    private void UpdateJumpTime()
	{
		if (rb.velocity.y > 0)
		{
			jumpTimer += Time.deltaTime * 2f;
			jumpTimer = Mathf.Clamp01(jumpTimer);
		}
	}

    private void ResetJumpState()
	{
        jumpTimer = 0f;
        isJumping = false;

        if(isJumpFinished == false && rb.velocity.y <= 0f)
		{
            OnJump?.Invoke(false);
            isJumpFinished = true;
		}
	}

	private void FixedUpdate()
    {
        ApplyGravityModifiers();
        ProcessMovement();
        AdjustDragForSlope();
        ManageCharacterOrientation();
        
    }

    private void ProcessMovement()
    {
        if (isGrappling || isExecutingDash) return;

        // Obtener input
        Vector2 input = playerInput.MovementInput;

        // Calcular direcci?n y magnitud
        Vector3 movementDirection = new Vector3(input.x, 0, input.y);
        float inputMagnitude = Mathf.Min(movementDirection.magnitude, 1f);

        // Normalizar la direcci?n (si hay input)
        if (inputMagnitude > 0.1f)
        {
            movementDirection.Normalize();
        }

        // Aplicar curva de respuesta a la magnitud del input
        float speedFactor = speedCurve.Evaluate(inputMagnitude);

        // Calcular velocidad objetivo basada en la magnitud del input
        Vector3 targetVelocity = movementDirection * (moveSpeed + (0.25f * playerStats.Speed)) * speedFactor;

        // Calcular factor de suavizado
        float smoothFactor;

        if (inputMagnitude > 0.1f)
        {
            // Comprobar si hay un cambio significativo de direcci?n
            if (Vector3.Dot(movementDirection, MovementDirection) < 0.5f)
            {
                // Cambio brusco de direcci?n - aplicar factor multiplicador
                smoothFactor = acceleration * directionChangeMultiplier;
            }
            else
            {
                // Aceleraci?n normal
                smoothFactor = acceleration;
            }

            Debug.Log("Movement speed " + (moveSpeed + (0.25f * playerStats.Speed)));
        }
        else
        {
            // Desaceleraci?n al detenerse
            smoothFactor = deceleration;
        }

        // Aplicar movimiento con suavizado
        currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, targetVelocity.x, smoothFactor * Time.fixedDeltaTime);
        currentVelocity.z = Mathf.MoveTowards(currentVelocity.z, targetVelocity.z, smoothFactor * Time.fixedDeltaTime);

        currentVelocity.y = rb.velocity.y;


        //reducir velocidad de movimiento x y z si el jugador esta saltando
        if (isJumping)
        {
            // Durante un salto intencional, mantener la velocidad vertical del Rigidbody
            currentVelocity.y = rb.velocity.y;

            // Reducir velocidad de movimiento x y z si el jugador está saltando
            currentVelocity.x /= horizontalMoveDecelerationFactor;
            currentVelocity.z /= horizontalMoveDecelerationFactor;
        }
        else if (!isGrounded && rb.velocity.y > 0)
        {
            // Si no estamos en un salto intencional pero estamos moviéndonos hacia arriba 
            // (probablemente por un "rebote" al subir un escalón)
            // Limitar la velocidad vertical para evitar saltos no deseados
            float maxUnintendedYVelocity = 1f; // Ajustar este valor según necesites
            currentVelocity.y = Mathf.Min(rb.velocity.y, maxUnintendedYVelocity);
        }
        else
        {
            // En otros casos (en el suelo o cayendo) mantener la velocidad vertical
            currentVelocity.y = rb.velocity.y;
        }

        // Aplicar velocidad al rigidbody
        rb.velocity = currentVelocity;

        // Notificar cambios de velocidad
        OnVelocityChanged?.Invoke(currentVelocity);
    }

    private void CheckGrounded()
	{
        isGrounded = Physics.CheckSphere(groundCheckpoint.position, groundCheckRadius, groundLayer);
        
	}

    private void ApplyGravityModifiers()
	{
        if (rb)
        {
            if (rb.velocity.y < 0f)
            {
                rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;
            }
        }
    }

    // M?todo p?blico para debugging
    public Vector3 GetCurrentVelocity()
    {
        return currentVelocity;
    }

    public float GetMovementSpeed()
	{
        return moveSpeed;
	}

    public bool GetGroundedState()
	{
        return isGrounded;
	}

    private void ManageCharacterOrientation()
	{
        if(currentVelocity.x < -0.15f && isFacingRight)
		{
            this.transform.localScale = new Vector3(-1f, 1f, 1f);
            isFacingRight = false;
		}
        else if(currentVelocity.x > 0.15f && !isFacingRight)
		{
            this.transform.localScale = new Vector3(1f, 1f, 1f);
            isFacingRight = true;
        }
	}

    private void CheckSurfaceAngle()
	{
        if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, slopeRaycastDistance, groundLayer))
		{
            currentSurfaceAngle = Vector3.Angle(hitInfo.normal, Vector3.up);
            isOnSlope = currentSurfaceAngle > slopeAngleThreshold;
		}
        else
		{
            isOnSlope = false;
            currentSurfaceAngle = 0f;
		}
	}

    private void AdjustDragForSlope()
	{
        //bool isMoving = rb.velocity != Vector3.zero;

        if(isOnSlope)
		{
            if(playerPhysicsMaterial)
			{
                playerPhysicsMaterial.staticFriction = slopeDrag;
			}
		}
        else
		{
            if (playerPhysicsMaterial)
            {
                playerPhysicsMaterial.staticFriction = defaultDrag;
            }
        }
	}

    private void ProcessJump()
    {
        if(isGrounded)
        {
            jumpIndex = 0;
            PerformJump();

            jumpInputBuffer.Clear();
        }
        else if(canDoubleJump && jumpIndex == 0)
        {
            PerformJump();
            jumpIndex++;

            jumpInputBuffer.Clear();
        }
        else
		{
            jumpInputBuffer.Enqueue(Time.time);
		}
        //comprobar si no esta en el suelo y si el inputBuffer tiene algun elemento que extraer

    }

    private void PerformJump()
    {
        isJumpFinished = false;
        initialJumpVelocity = Mathf.Sqrt(jumpForce * Mathf.Abs(Physics.gravity.y));

        Vector3 velocity = rb.velocity;

        velocity.y = initialJumpVelocity;

        velocity.y = Mathf.Lerp(initialJumpVelocity, 0f, jumpTimer);

        rb.velocity = velocity;

        isJumping = true;
        jumpTimer = 0f;

        OnJump?.Invoke(true);
    }

    //Aqui tambien, para poder realizar el dash, hay que tener en cuenta que no podemos estar en colision con una pared lateral o, mas bien, hay que detener el dash cuando colisionemos con una 
    private void InitializeDash()
	{
        if (!dashEnabled || isDashOnCooldown) return;

        OnDash?.Invoke(true);
        Vector3 direction = GetMovementDirection();

        dashCoroutine = SkillCoroutineRunner.Instance.RunCoroutine(
            PerformDashMovement(direction));

        dashEnabled = false;
    }

    private Vector3 GetMovementDirection()
	{
        Vector3 movementDirection = new Vector3(playerInput.MovementInput.x, 0f, playerInput.MovementInput.y);

        if(movementDirection == Vector3.zero)
		{
            movementDirection = isFacingRight ? Vector3.right : Vector3.left;
        }

        return movementDirection;
	}

    private IEnumerator PerformDashMovement(Vector3 direction)
    {
        isExecutingDash = true;
        remainingCooldown = dashCooldown;
        PlayDashFeedback();

        // Guardar la velocidad original
        Vector3 originalVelocity = rb.velocity;

        // Deshabilitar la gravedad temporalmente
        rb.useGravity = false;

        // Duración total del dash
        float dashDuration = dashDistance / dashSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            // Usar una curva de velocidad para suavizar inicio y fin
            float t = elapsedTime / dashDuration;
            float speedFactor = Mathf.Sin(t * Mathf.PI); // Forma de campana suave

            // Aplicar velocidad directamente al rigidbody
            rb.velocity = direction * dashSpeed * speedFactor;

            elapsedTime += Time.deltaTime;

            // Si hay colisión, salir del bucle
            if (dashRaycastCollide)
                break;

            yield return null;
        }

        

        CancelDash(direction, originalVelocity);
        StartDashCooldown();

    }

    private void DashRaycastDetection()
	{
        //lanza un rayo hacia adelante desde la posicion del jugador. si el rayo colisiona con un objeto de la layer ground, se detiene el dash
        RaycastHit hit;

        Vector3 facingDirection = isFacingRight ? Vector3.right : Vector3.left;

        Debug.DrawRay(transform.position, MovementDirection + facingDirection, Color.yellow);

        if (Physics.Raycast(transform.position, MovementDirection + facingDirection, out hit, 1f, groundLayer))
		{
            //Debug.Log("Dash raycast detection hit something");
            dashRaycastCollide = true;
            dashEnabled = false;
        }
        else
		{
            dashRaycastCollide = false;
		}
	}

    public void CancelDash(Vector3 direction, Vector3 originalVelocity)
    {
        if(isExecutingDash && dashCoroutine != null)
		{
            SkillCoroutineRunner.Instance.StopRoutine(dashCoroutine);
            isExecutingDash = false;
            dashCoroutine = null;

            // Restaurar la gravedad y permitir una transición suave
            rb.useGravity = true;
            rb.velocity = new Vector3(direction.x * moveSpeed * 0.5f, originalVelocity.y, direction.z * moveSpeed * 0.5f);

            if (dashTrail)
			{
                dashTrail.SetActive(false);
			}

            OnDash?.Invoke(false);
        }
    }

    protected void StartDashCooldown()
    {
        //Debug.Log("Starting skill cooldown");
        remainingCooldown = dashCooldown;
    }

    private void UpdateDashCooldown()
	{
        if(remainingCooldown > 0f)
		{
			if(isDashOnCooldown == false)
			{
                isDashOnCooldown = true;
			}
            remainingCooldown -= Time.deltaTime;
		}
		else
		{
            isDashOnCooldown = false;
            //comprobar que no estoy chocando con nada que inhabilite el dash
            if (dashRaycastCollide == true) return;

            dashEnabled = true;
		}

        //Debug.Log("Remaining dash cooldown " + remainingCooldown);
	}

    private void PlayDashFeedback()
	{
        if(dashTrail)
		{
            dashTrail.SetActive(true);
            dashTrail.transform.SetPositionAndRotation(this.transform.position, this.transform.rotation);
		}
	}

    //Esto hay que clampearlo despues
    private void OnPlayerStatChanged(string statName, int newValue)
    {
        if (statName == "Speed")
        {
            moveSpeed += (moveSpeed * (0.1f * newValue)); //Esto esta hardcodeado, hay que poner un ratio de crecimiento para cada controlador

            moveSpeed = Mathf.Clamp(moveSpeed, 0f, maxMoveSpeed);
        }
    }

    private void StartHook()
    {
        //grapplingStarted = true;
    }

    //ESTO TAMBIEN TENDRIA QUE AFECTAR A LOS ELEMENTOS DEL SUELO, QUIZAS
    //HAY QUE TIRAR UN RAYCAST HACIA ADELANTE Y, SI EL RAYCAST COLISIONA CON UN ELEMENTO DE LA LAYER GROUND, POR EJEMPLO, DETENER EL DASH IGUAL
    //QUE SE HACE AQUI
	private void OnCollisionStay(Collision collision)
	{
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            if (isExecutingDash)
            {
                Vector3 direction = GetMovementDirection();
                Vector3 velocity = rb.velocity;

                CancelDash(direction, velocity);
            }
        }
    }

	private void OnDrawGizmos()
	{
        if (groundCheckpoint != null)
        {
            Gizmos.color = isJumping ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckpoint.position, groundCheckRadius);
        }
    }


}