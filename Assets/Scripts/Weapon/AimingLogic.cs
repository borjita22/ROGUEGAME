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


    private Vector2 aimDirection = Vector2.right; // Dirección de apuntado actual
    private SpriteRenderer weaponSpriteRenderer;
    private Vector3 mouseWorldPosition;
    private bool usingMouse = true;

    [SerializeField] private float frontViewAngleThreshold;
    [SerializeField] private float rearViewAngleThreshold;

    public static event Action<bool> OnFrontViewVisibilityChanged;


    private void Awake()
	{
        inputHandler = GetComponentInParent<PlayerInputHandler>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (weaponSprite != null)
            weaponSpriteRenderer = weaponSprite.GetComponent<SpriteRenderer>();
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

        // Actualizar la orientación del sprite
        //UpdateSpriteOrientation();
        UpdateSpriteVisibility();
    }

	private void FixedUpdate()
	{
        // Procesar el input de apuntado y actualizar la posición/rotación del arma
        ProcessAiming();
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
            Debug.Log(Gamepad.current.rightStick.ReadValue());
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
                Debug.Log("Stick input: " + stickInput);

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
        // 1. Posicionar el arma en órbita alrededor del jugador
        Vector3 orbitPosition = playerTransform.position + new Vector3(aimDirection.x, weaponHeight, aimDirection.y) * orbitDistance;

        transform.position = orbitPosition;

        // 2. Hacer que el vector forward del arma apunte hacia donde está el ratón/stick
        Vector3 targetPoint;

        if (usingMouse)
        {
            // Calcular la distancia entre el ratón y el jugador en el plano XZ
            Vector3 mouseToPlayer = mouseWorldPosition - playerTransform.position;
            mouseToPlayer.y = 0;

            // Si el ratón está muy cerca del jugador, usar la dirección orbital en lugar de la posición exacta
            if (mouseToPlayer.magnitude < orbitDistance)
            {
                // Usar la dirección de la órbita como dirección de apuntado
                targetPoint = playerTransform.position + new Vector3(aimDirection.x, 0, aimDirection.y) * 10f;
            }
            else
            {
                // Usar la posición real del ratón
                targetPoint = mouseWorldPosition;
            }
        }
        else
        {
            // Para mando, usar la dirección del stick
            targetPoint = playerTransform.position + new Vector3(aimDirection.x, 0, aimDirection.y) * 10f;
        }

        Vector3 directionToTarget = targetPoint - transform.position;
        directionToTarget.y = 0; // Mantener el arma nivelada

        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            transform.forward = directionToTarget.normalized;
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

    /// <summary>
    /// Obtiene la dirección actual de apuntado (útil para otros sistemas como el disparo)
    /// </summary>
    public Vector2 GetAimDirection()
    {
        return aimDirection;
    }

}
