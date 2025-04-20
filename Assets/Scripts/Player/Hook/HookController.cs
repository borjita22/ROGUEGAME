using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//Controla el gancho del jugador
public class HookController : MonoBehaviour
{
    private PlayerInputHandler inputHandler;

    [SerializeField] public GrappleRope grappleRope;

    private Vector3 grappleDirection;

    [Header("Transform References")]
    public Transform player;
    public Transform gunPivot;
    public Transform firePoint;

    [Header("Physics")]
    public Rigidbody playerRigidbody;

    [Header("Grappling Settings")]
    [SerializeField] private float maxGrappleDistance;
    [SerializeField] private float grappleSpeed; // Aumentado considerablemente
    [SerializeField] private float positionDamping;
    [SerializeField] private float hookThreshold; // Distancia a la que se considera "llegado" al punto

    [Header("Hook Anchor")]
    [SerializeField] private GameObject hookAnchorPrefab; // Prefab del objeto ancla
    private GameObject hookAnchor; // Instancia actual del ancla
    private Rigidbody anchorRigidbody;
    private ConfigurableJoint joint;

    [Header("Movement Preservation")]
    [SerializeField] private float momentumMultiplier = 0.8f; // Qué tanto momento mantener al llegar

    [Header("Hookable elements")]
    [SerializeField] private float hookablesDetectionRadius;
    [SerializeField] private List<IHookable> hookablesInRange = new List<IHookable>();
    [SerializeField] public IHookable closestHookable;

    [Header("Testing")]
    [SerializeField] private Transform testHookPoint;

    // Variables de estado
    [SerializeField] private bool isGrappling = false;
    public Vector3 grapplePoint;
    [HideInInspector] public Vector3 grappleDistanceVector;
    private Vector3 originalVelocity;

    private PlayerMovementController movementController;

    public event Action OnGrapplingStarted;

    private void Awake()
    {
        movementController = FindFirstObjectByType<PlayerMovementController>();
        inputHandler = FindFirstObjectByType<PlayerInputHandler>();
    }

    private void Start()
    {
        if (grappleRope != null)
        {
            //grappleRope.enabled = false;
            grappleRope.gameObject.SetActive(false);
        }

        if (playerRigidbody == null && player != null)
        {
            playerRigidbody = player.GetComponent<Rigidbody>();
        }

        if (hookAnchor == null)
        {
            hookAnchor = Instantiate(hookAnchorPrefab, player.position, Quaternion.identity);
        }

        anchorRigidbody = hookAnchor.GetComponent<Rigidbody>();
        if (anchorRigidbody == null)
        {
            anchorRigidbody = hookAnchor.AddComponent<Rigidbody>();
            anchorRigidbody.isKinematic = true;
            anchorRigidbody.useGravity = false;
        }

        joint = hookAnchor.GetComponent<ConfigurableJoint>();
        if (joint == null)
        {
            joint = hookAnchor.AddComponent<ConfigurableJoint>();
        }

        // Asegurarse de que esté desactivado al inicio
        hookAnchor.SetActive(false);
    }

    private void OnEnable()
    {
        if(inputHandler)
        {
            inputHandler.OnHookEnabled += PerformHookAction;
        }
    }

    private void OnDisable()
    {
        if (inputHandler)
        {
            inputHandler.OnHookEnabled -= PerformHookAction;
        }
    }

    private void Update()
    {
        // Si estamos enganchados, comprobamos si hemos llegado al destino
        if (isGrappling)
        {
            float distanceToTarget = Vector3.Distance(player.position, grapplePoint);

            if (distanceToTarget <= hookThreshold)
            {
                // Hemos llegado al objetivo, mantenemos el momentum
                StopGrapple(true);
            }
        }

        DetectHookablesInRange();
    }

    private void PerformHookAction()
    {
        if (closestHookable != null)
        {
            SetGrapplePoint((closestHookable as MonoBehaviour).transform.position);
            EnableRopeGrappling();
        }
    }

    //En este metodo, ya que hemos setteado el grappling point anteriormente, tal vez no haria falta settearlo de nuevo
    public void ShootGrapple()
    {
        // Evitamos múltiples enganches simultáneos
        if (isGrappling) return;

        if (movementController)
        {
            movementController.isGrappling = true;
        }

        // Guardamos la velocidad original para preservar el momento
        if (playerRigidbody != null)
        {
            originalVelocity = playerRigidbody.velocity;
        }

        if (testHookPoint != null)
        {
            SetGrapplePoint(testHookPoint.position);
            ConfigureAndStartGrapple();
            
        }
        else
        {
            if(closestHookable != null)
            {
                SetGrapplePoint((closestHookable as MonoBehaviour).transform.position);
                ConfigureAndStartGrapple();
            }
            
        }

        
    }

    public void SetGrapplePoint(Vector3 point)
    {
        grapplePoint = point;
        grappleDistanceVector = grapplePoint - firePoint.position;

        grappleDirection = (grapplePoint - player.position).normalized;

        if (grappleRope != null)
        {
            //grappleRope.enabled = true;
            grappleRope.gameObject.SetActive(true);
        }
    }

    private void ConfigureAndStartGrapple()
    {

        // Activar y posicionar el ancla
        hookAnchor.SetActive(true);
        hookAnchor.transform.position = grapplePoint;

        // Reiniciar la configuración del joint
        ResetJointConfiguration();

        // Configuración crucial del joint para el comportamiento de gancho
        joint.connectedBody = playerRigidbody;

        // Esta es la clave: el connectedAnchor debe ser local al jugador, no un punto global
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = Vector3.zero; // Centro del jugador, no grapplePoint

        // Configuración de movimiento - IMPORTANTE para el comportamiento correcto
        //joint.xMotion = ConfigurableJointMotion.Limited;
        //joint.yMotion = ConfigurableJointMotion.Limited;
        //joint.zMotion = ConfigurableJointMotion.Limited;

        // Límites de movimiento muy estrictos
        SoftJointLimit limit = new SoftJointLimit();
        limit.limit = 0.25f; // Casi no hay movimiento = tirón muy directo
        joint.linearLimit = limit;

        // Drives para el tirón fuerte
        JointDrive driveSetting = new JointDrive
        {
            positionSpring = grappleSpeed,  // Valor alto = tirón fuerte
            positionDamper = positionDamping, // Poco damping = menos rebotes
            maximumForce = Mathf.Infinity   // Sin límite de fuerza
        };

        joint.xDrive = driveSetting;
        joint.yDrive = driveSetting;
        joint.zDrive = driveSetting;

        // Asegurar que la proyección esté activa para evitar problemas de física
        joint.projectionMode = JointProjectionMode.PositionAndRotation;
        joint.projectionDistance = 0.1f;

        isGrappling = true;

        //StartCoroutine(DisableRopeAfterDelay(0.5f));

    }
        /*
        // Calcular dirección y distancia
        Vector3 directionToTarget = (grapplePoint - player.position).normalized;
        float distanceToTarget = Vector3.Distance(player.position, grapplePoint);

        // Calcular fuerza del impulso (más distancia = más fuerza)
        float forceMagnitude = Mathf.Min(distanceToTarget * grappleSpeed, 1000f);

        // Aplicar un único impulso fuerte
        Debug.Log("Force magnitude: " + forceMagnitude);
        playerRigidbody.AddForce(directionToTarget * forceMagnitude, ForceMode.Impulse);

        // Activar la visualización de la cuerda
        if (grappleRope != null)
        {
            grappleRope.gameObject.SetActive(true);
        }

        // Iniciar una corrutina para desactivar la cuerda después de un tiempo
        StartCoroutine(DisableRopeAfterDelay(0.5f));

        isGrappling = true;
        
    }
        */

    private IEnumerator DisableRopeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StopGrapple(true);
    }

    private void ResetJointConfiguration()
    {
        // Resetear el joint a un estado neutro
        joint.xMotion = ConfigurableJointMotion.Free;
        joint.yMotion = ConfigurableJointMotion.Free;
        joint.zMotion = ConfigurableJointMotion.Free;

        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;

        JointDrive zeroDrive = new JointDrive
        {
            positionSpring = 0,
            positionDamper = 0,
            maximumForce = 0
        };

        joint.xDrive = zeroDrive;
        joint.yDrive = zeroDrive;
        joint.zDrive = zeroDrive;
    }

    public void StopGrapple(bool preserveMomentum = false)
    {
        if (!isGrappling) return;

        // Si queremos preservar el momento

        
        if (preserveMomentum && playerRigidbody != null)
        {
            // Calculamos la dirección actual hacia el objetivo
            Vector3 directionToTarget = grappleDirection;

            // Obtenemos la velocidad actual
            float currentSpeed = playerRigidbody.velocity.magnitude;

            

            // Aplicamos un nuevo impulso en la dirección correcta
            Vector3 newVelocity = directionToTarget * currentSpeed * momentumMultiplier;

            Debug.Log(newVelocity);

            playerRigidbody.AddForce(newVelocity, ForceMode.Impulse);
        }
        

        

        if (grappleRope != null)
        {
            grappleRope.gameObject.SetActive(false);
        }

        hookAnchor.SetActive(false);


        isGrappling = false;
        //ResetJointConfiguration();

        Invoke(nameof(StopGrappling), 0.5f); //Esto va a ser el cooldown para poder volver a usar el gancho
    }

    private void StopGrappling()
    {
        /*
        if(movementController)
        {
            movementController.isGrappling = false;
        }
        */
        OnGrapplingStarted?.Invoke();
        
    }

    public void EnableRopeGrappling()
    {
        
        if (grappleRope != null)
        {
            //grappleRope.enabled = false;
            grappleRope.gameObject.SetActive(true);
            //grappleRope.DrawRope();
        }
    }


    private void DetectHookablesInRange()
    {
        List<IHookable> currentHookables = new List<IHookable>();

        Vector3 direction = firePoint.forward;

        RaycastHit[] hits = Physics.SphereCastAll(player.position, hookablesDetectionRadius, direction, 1000f);

        foreach(var hit in hits)
        {
            IHookable hookable = hit.collider.GetComponent<IHookable>();

            if(hookable != null) //Para que no se meta mas veces en la lista
            {
                currentHookables.Add(hookable);
            }
        }

        hookablesInRange.RemoveAll(h => !currentHookables.Contains(h));


        foreach (var hookable in currentHookables)
        {
            if (!hookablesInRange.Contains(hookable))
            {
                hookablesInRange.Add(hookable);
            }
        }

        closestHookable = GetClosestHookable();
    }

    private IHookable GetClosestHookable()
    {
        if (hookablesInRange == null || hookablesInRange.Count == 0)
            return null;

        IHookable closest = null;
        float closestDistance = float.MaxValue;

        foreach (IHookable hookable in hookablesInRange)
        {
            // Necesitamos la posición del objeto, así que obtenemos su Transform
            // Asumimos que IHookable es implementado por MonoBehaviours
            MonoBehaviour mb = hookable as MonoBehaviour;
            if (mb == null)
                continue;

            // Calcular la distancia
            float distance = Vector3.Distance(firePoint.position, mb.transform.position);

            // Actualizar el más cercano si corresponde
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = hookable;
            }
        }

        return closest;
    }

}

[CustomEditor(typeof(HookController))]
public class HookControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HookController hookController = (HookController)target;

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Testing Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Test Grapple"))
        {
            //hookController.ShootGrapple();
            //hookController.grappleRope.DrawRope();
            if(hookController.closestHookable != null)
            {
                hookController.SetGrapplePoint((hookController.closestHookable as MonoBehaviour).transform.position);
                hookController.EnableRopeGrappling();
            }
            
        }

        if (GUILayout.Button("Stop Grapple"))
        {
            hookController.StopGrapple(false);
        }
    }
}