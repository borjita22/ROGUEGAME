using UnityEngine;

/// <summary>
/// Controlador principal del jugador
/// Se encarga de gestionar aspectos como la salud del jugador o la invulnerabilidad
/// </summary>
public class PlayerController : MonoBehaviour
{
    private PlayerInputHandler playerInputHandler;
    private PlayerMovementController movementController;

    private Vector3 movementDirection;

	private void Awake()
	{
        playerInputHandler = GetComponent<PlayerInputHandler>();
        movementController = GetComponent<PlayerMovementController>();

        if(GetComponent<SkillCoroutineRunner>() == null)
		{
            this.gameObject.AddComponent<SkillCoroutineRunner>();
		}
	}
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        movementDirection = movementController.GetCurrentVelocity();
    }

    public PlayerInputHandler GetPlayerInputHandler()
	{
        return playerInputHandler;
	}

    public Vector3 GetMovementDirection()
	{
        return movementDirection;
	}
}
