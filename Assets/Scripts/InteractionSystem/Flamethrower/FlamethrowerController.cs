using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlamethrowerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem mainFlamePS;
    [SerializeField] private BoxCollider boxCollider;

    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    private float lastCollisionTime;
    private float currentColliderLength;
    private bool isResetting = false;


    public float lengthFactor = 1.0f;
    public float minColliderLength = 1.0f;
    public float defaultColliderLength = 8.75f;

    [Header("Reset Settings")]
    public float resetDelay = 0.25f; // Tiempo sin colisiones antes de resetear
    public float resetSpeed = 5.0f; // Velocidad a la que regresa a su tamaño original
    // Start is called before the first frame update
    void Start()
    {
        currentColliderLength = defaultColliderLength;
        lastCollisionTime = Time.time;

        var collisionModule = mainFlamePS.collision;

        if(!collisionModule.enabled)
		{
            Debug.LogWarning("modulo de colision desactivado");
		}
        AddForwarderToParticleSystem();

        UpdateColliderLength(defaultColliderLength);
    }

    private void AddForwarderToParticleSystem()
	{
        ParticleCollisionForwarder forwarder = mainFlamePS.gameObject.GetComponent<ParticleCollisionForwarder>();

        if(forwarder == null)
		{
            forwarder = mainFlamePS.gameObject.AddComponent<ParticleCollisionForwarder>();
            forwarder.controller = this;
		}
	}

    public void OnParticleCollision(GameObject other)
	{
        int numCollisions = mainFlamePS.GetCollisionEvents(other, collisionEvents);

        lastCollisionTime = Time.time;
        isResetting = false;

        if (numCollisions > 0)
		{
            for(int i = 0; i< numCollisions; i++)
			{
                Vector3 collisionPoint = collisionEvents[i].intersection;

                Vector3 localCollisionPoint = transform.InverseTransformPoint(collisionPoint);

                float distanceZ = localCollisionPoint.z;

                UpdateColliderLength(distanceZ);
            }
		}
	}

    private void UpdateColliderLength(float distance)
    {
        if (boxCollider != null)
        {
            // Asegurar una longitud mínima
            float adjustedLength = Mathf.Max(distance * lengthFactor, minColliderLength);

            // Obtener tamaño actual
            Vector3 size = boxCollider.size;

            // Actualizar solo el eje Z (longitud)
            size.z = adjustedLength;
            boxCollider.size = size;

            // Centrar el collider (mover centro a la mitad de la longitud)
            Vector3 center = boxCollider.center;
            center.z = adjustedLength / 2;
            boxCollider.center = center;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateColliderLength(10f);
        if (Time.time - lastCollisionTime > resetDelay && !isResetting)
        {
            isResetting = true;
        }

        if (isResetting)
        {
            // Interpolar suavemente hacia el tamaño original
            currentColliderLength = Mathf.Lerp(
                currentColliderLength,
                defaultColliderLength,
                resetSpeed * Time.deltaTime
            );

            // Actualizar el collider con el nuevo tamaño
            UpdateColliderLength(currentColliderLength);

            // Si ya estamos muy cerca del tamaño original, finalizar el reset
            if (Mathf.Abs(currentColliderLength - defaultColliderLength) < 0.1f)
            {
                currentColliderLength = defaultColliderLength;
                UpdateColliderLength(currentColliderLength);
            }
        }

    }
}
