using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleRope : MonoBehaviour
{
    private HookController grapplingGun;


    [Header("References")]
    [SerializeField] private LineRenderer lineRenderer;

    [Header("General settings")]
    [SerializeField] private int precision; //determina el numero de puntos del line renderer
    [Range(0, 50)] [SerializeField] private float straightenLineSpeed = 2f;

    [Header("Rope Animation Settings:")]
    public AnimationCurve ropeAnimationCurve;
    [Range(0.01f, 4)] [SerializeField] private float StartWaveSize = 2;
    [SerializeField] float waveSize = 0;

    [Header("Rope Progression:")]
    public AnimationCurve ropeProgressionCurve;
    [SerializeField] [Range(1, 50)] private float ropeProgressionSpeed = 1;


    float moveTime = 0;

    [HideInInspector] public bool isGrappling = true;


    [SerializeField] bool straightLine = true;



    private void Awake()
    {
        grapplingGun = GetComponentInParent<HookController>();
    }



    private void OnEnable()
    {
        moveTime = 0f;
        lineRenderer.positionCount = precision;
        waveSize = StartWaveSize;
        straightLine = false;
        //grapplingStarted = false;

        //DisplayLinePointsToFirePoint();

        lineRenderer.enabled = true;

        
    }

    private void OnDisable()
    {
        lineRenderer.enabled = false;
        isGrappling = false;

        
    }

    private void Update()
    {
        moveTime += Time.deltaTime;

        DrawRope();

    }


    private void DisplayLinePointsToFirePoint()
    {
        for(int i=0; i < precision; i++)
        {
            lineRenderer.SetPosition(i, grapplingGun.firePoint.position); //el controlador principal debera tener un transform, que es el que se dispara
        }
    }

    public void DrawRope()
    {
        if(!straightLine)
        {
            if(Vector3.Distance(lineRenderer.GetPosition(precision -1), grapplingGun.grapplePoint) <= 0.1f)
            {
                straightLine = true;
            }
            else
            {
                DrawWavingRope();
            }
            
        }
        else
        {
            if(!isGrappling)
            {
                
                isGrappling = true;
            }

            if(waveSize > 0)
            {
                waveSize -= Time.deltaTime * straightenLineSpeed;
            }
            else
            {
                waveSize = 0;

                if(lineRenderer.positionCount != 2)
                {
                    lineRenderer.positionCount = 2;
                }

                DrawStraightRope();
                grapplingGun.ShootGrapple();
            }
        }
    }

    private void DrawWavingRope()
    {
        //lineRenderer.SetPosition(0, grapplingGun.player.position);
        for (int i = 0; i < precision; i++)
        {
            float delta = (float)i / ((float)precision - 1f);

            // Calcular dirección del gancho
            Vector3 grappleDir = (grapplingGun.grapplePoint - grapplingGun.firePoint.position).normalized;

            // Calcular un vector perpendicular adecuado para 3D
            Vector3 perpendicular;
            if (Mathf.Abs(Vector3.Dot(grappleDir, Vector3.up)) > 0.9f)
                perpendicular = Vector3.Cross(grappleDir, Vector3.right).normalized;
            else
                perpendicular = Vector3.Cross(grappleDir, Vector3.up).normalized;

            // Calcular el offset basado en la curva de animación
            Vector3 offset = perpendicular * ropeAnimationCurve.Evaluate(delta) * waveSize;

            // Calcular la posición objetivo
            Vector3 targetPosition = Vector3.Lerp(grapplingGun.firePoint.position, grapplingGun.grapplePoint, delta) + offset;

            // Animar la progresión
            Vector3 currentPosition = Vector3.Lerp(grapplingGun.firePoint.position, targetPosition, ropeProgressionCurve.Evaluate(moveTime) * ropeProgressionSpeed);

            lineRenderer.SetPosition(i, currentPosition);
        }

        //lineRenderer.SetPosition(precision - 1, grapplingGun.grapplePoint);
    }

    private void DrawStraightRope()
    {
        //lineRenderer.posi
        lineRenderer.SetPosition(0, grapplingGun.firePoint.position);
        lineRenderer.SetPosition(1, grapplingGun.grapplePoint);
    }



}
