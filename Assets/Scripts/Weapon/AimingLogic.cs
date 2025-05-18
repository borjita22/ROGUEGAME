using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class AimingLogic : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform weaponSprite;
    [SerializeField] private Camera mainCamera;

    [Header("Orbit configuration")]
    [SerializeField] private float orbitDistance;
    [SerializeField] private float weaponHeight;

    [Header("Movement Smoothing")]
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 targetOrbitPosition;
    [SerializeField] private float stationarySmoothTime = 0.1f;
    [SerializeField] private float movingSmoothTime = 0.03f; // Más rápido cuando se mueve
    [SerializeField] private float velocityThreshold = 0.5f; // Umbral para considerar que el jugador está en movimiento
    private Vector3 lastPlayerPosition;
    private Vector3 playerVelocity;


    [Header("Visual configuration")]
    [SerializeField] private Vector3 rotationOffset;
    [SerializeField] private bool flipSpriteOnLeftSide = true;

    private PlayerInputHandler inputHandler;


    private Vector2 aimDirection = Vector2.right; // Dirección de apuntado actual
    private SpriteRenderer weaponSpriteRenderer;
    private Vector3 mouseWorldPosition;
    private bool usingMouse = true;

    [SerializeField] private float frontViewAngleThreshold;
    [SerializeField] private float rearViewAngleThreshold;

    public static event Action<bool> OnFrontViewVisibilityChanged;


    [Header("Vertical Aiming")]
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 30f;
    [SerializeField] private float verticalRotationSpeed = 60f; // Grados por segundo
    [Header("Mouse")]
    [SerializeField] private float scrollSensitivity = 0.1f;
    private float currentVerticalAngle = 0f;

    [Header("Aiming line references")]
    [SerializeField] private Transform weaponMuzzle;
    private LineRenderer aimingLine;

    private int projectileLayer;
    int aimingLineLayerMask;


    private void Awake()
	{
        inputHandler = FindFirstObjectByType<PlayerInputHandler>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (weaponSprite != null)
            weaponSpriteRenderer = weaponSprite.GetComponent<SpriteRenderer>();

        aimingLine = GetComponentInChildren<LineRenderer>();
    }

	private void OnEnable()
	{
        inputHandler._ResetVRotation += ResetVerticalRotation;
	}

	private void OnDisable()
	{
        inputHandler._ResetVRotation -= ResetVerticalRotation;
    }


	private void Start()
	{
        if (inputHandler == null)
        {
            Debug.LogError("WeaponAimingController: No se ha encontrado un componente que implemente IPlayerInput");
            enabled = false;
            return;
        }

        // Verificar que tenemos todas las referencias necesarias
        if (playerTransform == null)
        {
            Debug.LogError("WeaponAimingController: playerTransform no asignado");
            enabled = false;
            return;
        }

        projectileLayer = LayerMask.NameToLayer("Projectile");
        aimingLineLayerMask = ~0;
        aimingLineLayerMask &= ~(1 << projectileLayer);
    }

	private void Update()
	{
        DetectInputDevice();

        // Procesar el input de apuntado y actualizar la posición/rotación del arma
        Vector3 displacement = playerTransform.position - lastPlayerPosition;
        playerVelocity = displacement / Time.fixedDeltaTime;
        lastPlayerPosition = playerTransform.position;

        ProcessAiming();
        ProcessVerticalAiming();

        // Actualizar la orientación del sprite
        //UpdateSpriteOrientation();
        UpdateSpriteVisibility();
        UpdateAimingLine();

    }

	private void FixedUpdate()
	{
        
    }

	//Esto vamos a poder centralizarlo en otra clase, y tanto el movimiento como el apuntado pueden acudir a esa clase a comprobar
	private void DetectInputDevice()
    {
        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.1f)
        {
            usingMouse = true;
        }
        // O si el stick derecho del mando se ha movido
        else if (Gamepad.current != null && Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.1f)
        {
            //Debug.Log("Usando mando");
            //Debug.Log(Gamepad.current.rightStick.ReadValue());
            usingMouse = false;
        }
    }

    //ESTA DANDO PROBLEMAS CON EL MOVIMIENTO UTILIZANDO EL MANDO, YA QUE PARECE QUE NO RECONOCE MUY BIEN EL INPUT
    private void ProcessAiming()
	{
        Vector2 aimInput = inputHandler.AimingInput;
        //Debug.Log(aimInput);

        if (usingMouse)
        {
            // La posición del ratón viene en coordenadas de pantalla, necesitamos convertirla
            mouseWorldPosition = GetWorldPositionFromScreenPoint(aimInput);

            // Calcular la dirección de apuntado (ignorando Y para mantenerlo en el plano horizontal)
            Vector3 dirToMouse = mouseWorldPosition - playerTransform.position;
            dirToMouse.y = 0;

            // Solo actualizar si hay una dirección significativa
            if (dirToMouse.sqrMagnitude > 0.005f)
            {
                // Normalizar y convertir a Vector2 (x, z)
                aimDirection = new Vector2(dirToMouse.x, dirToMouse.z).normalized;
            }
        }
        else
        {
            Vector2 stickInput = Gamepad.current.rightStick.ReadValue();

            // Solo actualizar si hay una entrada significativa
            if (stickInput.sqrMagnitude > 0.1f)
            {
                //Debug.Log("Stick input: " + stickInput);

                // Actualizar la dirección de apuntado
                aimDirection = stickInput.normalized;

                // Actualizar la posición en el mundo para consistencia
                Vector3 stickDirection = new Vector3(stickInput.x, 0, stickInput.y);
                mouseWorldPosition = playerTransform.position + stickDirection * 10f;
            }
        }

        Debug.DrawRay(this.transform.position, this.transform.forward * 5f, Color.yellow);
        PositionWeaponInOrbit();
    }

    private void ProcessVerticalAiming()
	{
        float verticalInput = 0f;

        if(usingMouse)
		{
            verticalInput = Mouse.current.scroll.ReadValue().y * scrollSensitivity;
		}
        else if(Gamepad.current != null)
		{
            if(Gamepad.current.leftShoulder.isPressed)
			{
                verticalInput -= 1f;
			}
            if(Gamepad.current.rightShoulder.isPressed)
			{
                verticalInput += 1f;
			}
		}

        if(Mathf.Abs(verticalInput) > 0.001f)
		{
            currentVerticalAngle += verticalInput * verticalRotationSpeed * Time.deltaTime;

            currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
		}

        //Debug.Log("Current vertical angle " + currentVerticalAngle);
	}

    //Settea la rotacion vertical del arma a 0 para centrarla
    private void ResetVerticalRotation()
	{
        currentVerticalAngle = 0f;
	}


    private Vector3 GetWorldPositionFromScreenPoint(Vector2 screenPoint)
    {
        // Crear un rayo desde la cámara hacia la posición del ratón
        Ray ray = mainCamera.ScreenPointToRay(screenPoint);

        // Definir un plano horizontal a la altura del jugador
        Plane horizontalPlane = new Plane(Vector3.up, playerTransform.position.y);

        // Lanzar el rayo contra el plano
        if (horizontalPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        // Si el raycast falla, usar una distancia predeterminada
        return ray.GetPoint(10f);
    }


    private void PositionWeaponInOrbit()
    {
        float currentSmoothTime = (playerVelocity.magnitude > velocityThreshold)
       ? movingSmoothTime
       : stationarySmoothTime;

        // Predicción de posición - añadir un poco de la velocidad del jugador
        Vector3 predictedPosition = playerTransform.position + (playerVelocity * 0.025f);

        // Calcular la posición orbital basada en la posición predictiva
        targetOrbitPosition = predictedPosition + new Vector3(aimDirection.x, weaponHeight, aimDirection.y) * orbitDistance;

        // Suavizado con el tiempo adecuado
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetOrbitPosition,
            ref currentVelocity,
            currentSmoothTime
        );

        Vector3 targetPoint;
        if (usingMouse)
        {
            Vector3 mouseToPlayer = mouseWorldPosition - playerTransform.position;
            mouseToPlayer.y = 0;

            if (mouseToPlayer.magnitude < orbitDistance)
            {
                targetPoint = playerTransform.position + new Vector3(aimDirection.x, 0, aimDirection.y) * 10f;
            }
            else
            {
                targetPoint = mouseWorldPosition;
            }
        }
        else
        {
            targetPoint = playerTransform.position + new Vector3(aimDirection.x, 0, aimDirection.y) * 10f;
        }

        Vector3 directionToTarget = targetPoint - transform.position;
        directionToTarget.y = 0;

        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            // Suavizar también la rotación puede ayudar
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 15f);

            // Aplicar rotación vertical
            Quaternion verticalRotation = Quaternion.Euler(currentVerticalAngle, 0, 0);
            transform.rotation = transform.rotation * verticalRotation;
        }


    }

    private void UpdateSpriteVisibility()
    {
        // Convertir la rotación absoluta a un ángulo relativo entre -180 y 180
        float yRotation = transform.eulerAngles.y;
        if (yRotation > 180) yRotation -= 360;

        // Tomar el valor absoluto para simplificar los cálculos
        float absAngle = Mathf.Abs(yRotation);

        // Mostrar/ocultar vista frontal (cuando está mirando aproximadamente hacia/desde la cámara)
        bool showFrontView = absAngle >= frontViewAngleThreshold || absAngle <= rearViewAngleThreshold;

        OnFrontViewVisibilityChanged?.Invoke(showFrontView);

    }

    private void UpdateAimingLine()
	{
        if (aimingLine == null) return;

        /*
        aimingLine.SetPosition(0, weaponMuzzle.transform.position);
        aimingLine.SetPosition(1, weaponMuzzle.transform.position + (weaponMuzzle.transform.forward * 10f)); //esto luego sera dependiente del alcance del arma
        */
        //El laser de apuntado tiene que ignorar las propias balas del jugador
        bool _hit = Physics.Raycast(weaponMuzzle.transform.position, weaponMuzzle.transform.forward, out RaycastHit hit, 10f, aimingLineLayerMask);

        if(_hit)
		{
            DrawAimingLine(weaponMuzzle.transform.position, hit.point);
        }
        else
		{
            DrawAimingLine(weaponMuzzle.transform.position, weaponMuzzle.transform.position + (weaponMuzzle.transform.forward * 10f));
		}
    }

    private void DrawAimingLine(Vector3 startPosition, Vector3 endPosition)
	{
        aimingLine.SetPosition(0, startPosition);
        aimingLine.SetPosition(1, endPosition);
	}

    /// <summary>
    /// Obtiene la dirección actual de apuntado (útil para otros sistemas como el disparo) OJO, quizas nos interesa obtener el vector forward del muzzle del cañon, ya que esto indica en la direccion en la que esta apuntando el arma
    /// </summary>
    public Vector2 GetAimDirection()
    {
        return aimDirection;
    }

    public Transform GetWeaponMuzzle()
	{
        return weaponMuzzle;
	}

}
