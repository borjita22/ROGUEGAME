using System.Collections;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform spriteTransform;

    [Header("Movement Configuration")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float maxMoveSpeed = 16f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float deceleration = 35f;
    [SerializeField] private float directionChangeMultiplier = 1.5f;
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.Linear(0, 0, 1, 1); // Para mapear el input a la velocidad

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
    [SerializeField] private bool isJumping = false;
    private float jumpTimer = 0f;

    [Space]
    [Header("Dash configuration")]
    [SerializeField] private bool dashEnabled = true;
    [SerializeField] private float dashDistance;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashCooldown;
    private float remainingCooldown;
    private bool isDashOnCooldown = false;
    private bool isExecutingDash = false;
    private Coroutine dashCoroutine;
    [SerializeField] private GameObject dashTrail;


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

    [SerializeField] private PlayerStats playerStats;

    private HookController hookController;

    public bool isGrappling = false;
    private bool grapplingStarted = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInputHandler>();

        if (rb)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
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
        
        if(!isGrounded && isJumping)
		{
			UpdateJumpTime();
		}
        else if(isGrounded)
		{
            ResetJumpState();
            if(isGrappling && grapplingStarted)
            {
                isGrappling = false;
                grapplingStarted = false;
            }
            
		}

        UpdateDashCooldown();
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
        ManageCharacterOrientation();
        
    }

    private void ProcessMovement()
    {
        if (isGrappling) return;

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
        Vector3 targetVelocity = movementDirection * moveSpeed * speedFactor;

        // Calcular factor de suavizado
        float smoothFactor;

        if (inputMagnitude > 0.1f)
        {
            // Comprobar si hay un cambio significativo de direcci?n
            if (Vector3.Dot(movementDirection, MovementDirection) < 0.5f && IsMoving)
            {
                // Cambio brusco de direcci?n - aplicar factor multiplicador
                smoothFactor = acceleration * directionChangeMultiplier;
            }
            else
            {
                // Aceleraci?n normal
                smoothFactor = acceleration;
            }
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
        if(!isGrounded && isJumping)
		{
            currentVelocity.x /= horizontalMoveDecelerationFactor;
            currentVelocity.z /= horizontalMoveDecelerationFactor;
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

    private void ProcessJump()
    {
        if(isGrounded)
        {
            jumpIndex = 0;
            PerformJump();
        }
        else if(canDoubleJump && jumpIndex == 0)
        {
            PerformJump();
            jumpIndex++;
        }

    }

    private void PerformJump()
    {
        OnJump?.Invoke(true);
        isJumpFinished = false;
        initialJumpVelocity = Mathf.Sqrt(jumpForce * Mathf.Abs(Physics.gravity.y));

        Vector3 velocity = rb.velocity;

        velocity.y = initialJumpVelocity;

        velocity.y = Mathf.Lerp(initialJumpVelocity, 0f, jumpTimer);

        rb.velocity = velocity;

        isJumping = true;
        jumpTimer = 0f;
    }

    private void InitializeDash()
	{
        if (!dashEnabled || isDashOnCooldown) return;

        OnDash?.Invoke(true);
        Vector3 direction = new Vector3(playerInput.MovementInput.x, 0f, playerInput.MovementInput.y);
        dashCoroutine = SkillCoroutineRunner.Instance.RunCoroutine(
            PerformDashMovement(direction));

    }



    private IEnumerator PerformDashMovement(Vector3 direction)
    {
        //IsExecuting = true;
        isExecutingDash = true;

        remainingCooldown = dashCooldown;

        PlayDashFeedback();

        // Normalizar direcci?n
        direction = direction.normalized;

        // Variables para tracking
        float distanceTraveled = 0f;

        while (distanceTraveled < dashDistance)
        {
            float moveStep = dashSpeed * Time.deltaTime;

            if (distanceTraveled + moveStep > dashDistance)
            {
                moveStep = dashDistance - distanceTraveled;
            }


            // Mover al jugador
            this.transform.position += (direction * moveStep);
            distanceTraveled += moveStep;

            // Detectar interacciones durante el dash
            //DetectDashInteractions(direction);

            yield return null;
        }

        // Iniciar cooldown
        CancelDash();
        //StartCooldown();

    }

    public void CancelDash()
    {
        if(isExecutingDash && dashCoroutine != null)
		{
            SkillCoroutineRunner.Instance.StopRoutine(dashCoroutine);
            isExecutingDash = false;
            dashCoroutine = null;

            if(dashTrail)
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
		}

        Debug.Log("Remaining dash cooldown " + remainingCooldown);
	}

    //protected override void PlayFeedback()
    //{
    //    //Muestra un halo (line renderer o algo asi) en la posicion del jugador
    //    if (Definition.skillPrefab == null) return;

    //    if (dashPrefab == null)
    //    {
    //        dashPrefab = GameObject.Instantiate(Definition.skillPrefab, owner.transform);
    //    }
    //    else
    //    {
    //        dashPrefab.SetActive(true);
    //    }

    //    dashPrefab.transform.position = owner.transform.position;
    //    dashPrefab.transform.rotation = owner.transform.rotation;
    //}

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
        grapplingStarted = true;
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