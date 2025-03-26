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

    [Header("Visual configuration")]
    [SerializeField] private Vector3 rotationOffset;
    [SerializeField] private bool flipSpriteOnLeftSide = true;

    private PlayerInputHandler inputHandler;


    private Vector2 aimDirection = Vector2.right; // Direcci�n de apuntado actual
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
    private float currentVerticalAngle = 0f;

    [Header("Aiming line references")]
    [SerializeField] private Transform weaponMuzzle;
    private LineRenderer aimingLine;


    private void Awake()
	{
        inputHandler = GetComponentInParent<PlayerInputHandler>();

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
    }

	private void Update()
	{
        DetectInputDevice();

        // Actualizar la orientaci�n del sprite
        //UpdateSpriteOrientation();
        UpdateSpriteVisibility();
        UpdateAimingLine();
    }

	private void FixedUpdate()
	{
        // Procesar el input de apuntado y actualizar la posici�n/rotaci�n del arma
        ProcessAiming();
        ProcessVerticalAiming();
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
            // La posici�n del rat�n viene en coordenadas de pantalla, necesitamos convertirla
            mouseWorldPosition = GetWorldPositionFromScreenPoint(aimInput);

            // Calcular la direcci�n de apuntado (ignorando Y para mantenerlo en el plano horizontal)
            Vector3 dirToMouse = mouseWorldPosition - playerTransform.position;
            dirToMouse.y = 0;

            // Solo actualizar si hay una direcci�n significativa
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
                Debug.Log("Stick input: " + stickInput);

                // Actualizar la direcci�n de apuntado
                aimDirection = stickInput.normalized;

                // Actualizar la posici�n en el mundo para consistencia
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
            verticalInput = Mouse.current.scroll.ReadValue().y;
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

        Debug.Log("Current vertical angle " + currentVerticalAngle);
	}

    //Settea la rotacion vertical del arma a 0 para centrarla
    private void ResetVerticalRotation()
	{
        currentVerticalAngle = 0f;
	}


    private Vector3 GetWorldPositionFromScreenPoint(Vector2 screenPoint)
    {
        // Crear un rayo desde la c�mara hacia la posici�n del rat�n
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
        // 1. Posicionar el arma en �rbita alrededor del jugador
        Vector3 orbitPosition = playerTransform.position + new Vector3(aimDirection.x, weaponHeight, aimDirection.y) * orbitDistance;

        transform.position = orbitPosition;

        // 2. Hacer que el vector forward del arma apunte hacia donde est� el rat�n/stick
        Vector3 targetPoint;

        if (usingMouse)
        {
            // Calcular la distancia entre el rat�n y el jugador en el plano XZ
            Vector3 mouseToPlayer = mouseWorldPosition - playerTransform.position;
            mouseToPlayer.y = 0;

            // Si el rat�n est� muy cerca del jugador, usar la direcci�n orbital en lugar de la posici�n exacta
            if (mouseToPlayer.magnitude < orbitDistance)
            {
                // Usar la direcci�n de la �rbita como direcci�n de apuntado
                targetPoint = playerTransform.position + new Vector3(aimDirection.x, 0, aimDirection.y) * 10f;
            }
            else
            {
                // Usar la posici�n real del rat�n
                targetPoint = mouseWorldPosition;
            }
        }
        else
        {
            // Para mando, usar la direcci�n del stick
            targetPoint = playerTransform.position + new Vector3(aimDirection.x, 0, aimDirection.y) * 10f;
        }

        Vector3 directionToTarget = targetPoint - transform.position;
        directionToTarget.y = 0; // Mantener el arma nivelada

        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            transform.forward = directionToTarget.normalized;

            Quaternion verticalRotation = Quaternion.Euler(currentVerticalAngle, 0, 0);
            transform.rotation = transform.rotation * verticalRotation;
        }


    }

    private void UpdateSpriteVisibility()
    {
        // Convertir la rotaci�n absoluta a un �ngulo relativo entre -180 y 180
        float yRotation = transform.eulerAngles.y;
        if (yRotation > 180) yRotation -= 360;

        // Tomar el valor absoluto para simplificar los c�lculos
        float absAngle = Mathf.Abs(yRotation);

        // Mostrar/ocultar vista frontal (cuando est� mirando aproximadamente hacia/desde la c�mara)
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
        bool _hit = Physics.Raycast(weaponMuzzle.transform.position, transform.forward, out RaycastHit hit, 10f);

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
    /// Obtiene la direcci�n actual de apuntado (�til para otros sistemas como el disparo)
    /// </summary>
    public Vector2 GetAimDirection()
    {
        return aimDirection;
    }

}
