using UnityEngine;


/// <summary>
/// Clase que va a gestionar las skills del jugador
/// </summary>
public class PlayerSkillsController : MonoBehaviour
{
	[SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerInputHandler playerInput; //por el momento, tendra que ser asi. la idea seria no tener una referencia al inputHandler y otra al controlador principal, pero bueno
    [SerializeField] private AimingLogic weaponAimingLogic; //necesitamos la direccion de apuntado en el caso de que se quiera ejecutar una habilidad que requiere apuntado, para pasarle la direccion como parametro

    private const int MAX_SKILL_SLOTS = 4;

	[SerializeField] private SkillSlot[] skillSlots = new SkillSlot[MAX_SKILL_SLOTS];

    [SerializeField] private ISkill[] equippedSkills = new ISkill[MAX_SKILL_SLOTS];

    //Para modo teclado y raton -> Indice de habilidad seleccionada
    private int selectedSkillIndex = 0;

    //Modo mando -> Control de rueda de habilidades
    [SerializeField] private bool skillWheelActive = false;

	[SerializeField] private SkillDefinition currentSelectedSkill;

	private void OnEnable()
	{
		
	}

	private void Awake()
	{
		
        
		
		playerController = GetComponent<PlayerController>();
		playerInput = GetComponent<PlayerInputHandler>();

		if (weaponAimingLogic == null)
		{
			if (playerController)
			{
				weaponAimingLogic = playerController.GetComponentInChildren<AimingLogic>();
			}
		}

		InitializeSkills();

		RegisterInputEvents();
	}

	//Realmente este metodo no se va a llamar en el juego, ya que el jugador empezara siempre la partida sin habilidades asociadas
	//Esto solo se esta haciendo para realizar pruebas de funcionamiento de las skills
	private void InitializeSkills()
	{
		for(int i = 0; i < MAX_SKILL_SLOTS; i++)
		{
			if(skillSlots[i] != null && skillSlots[i].skillDefinition != null)
			{
				skillSlots[i].runtimeSkill = skillSlots[i].skillDefinition.CreateInstance(playerController);

				equippedSkills[i] = skillSlots[i].runtimeSkill;
			}
			else
			{
				equippedSkills[i] = null;
			}
		}

		currentSelectedSkill = skillSlots[0].skillDefinition;
	}

	private void RegisterInputEvents()
	{
		playerInput.OnSwitchSkill += SwitchSkill;
		playerInput.OnUseSkill += PerformSkill;

		playerInput._OnOpenSkillWheel += SetSkillWheelStatus;
		playerInput._OnUseGamepadSkill += PerformSkillGamepad;
	}

	private void Update()
	{
		//aqui hay que hacer que todas las skills actualicen su cooldown. Como solamente lo van a actualizar si es necesario, no importaria hacerlo con todas ellas
		foreach(var skill in equippedSkills)
		{
			skill.UpdateCooldown(Time.deltaTime);
		}
	}


	private void SwitchSkill()
	{
		//aqui tambien cambiariamos el skill Index entiendo yo, aunque quizas no haria falta. Hay que investigar esta logica para teclado
		Debug.Log("Skill switched: Keyboard");

		selectedSkillIndex = (selectedSkillIndex + 1) % MAX_SKILL_SLOTS;

		Debug.Log($"Skill switched to {selectedSkillIndex}");

		currentSelectedSkill = skillSlots[selectedSkillIndex].skillDefinition;
	}

	private void PerformSkill()
	{
		//Debug.Log("Performing skill");
		UseSkillAtIndex(selectedSkillIndex);
	}

	private void SetSkillWheelStatus(bool status)
	{
		Debug.Log("Wheel is enabled: " + status);
		skillWheelActive = status;
	}

	//OJO, hay que ver si esta es la mejor manera para las skills del gamepad o habria que crear otro tipo de evento para que se reciba un index
	//lo suyo seria que cada slot de skill tuviese un indice fijo, y se ejecutase la skill asociada a ese slot fijo
	//Por ejemplo, la skill del boton A SIEMPRE va a tener el indice 0, lo unico que cambia es la potencial skill asignada a ese slot con ese indice, y asi con todas
	private void PerformSkillGamepad(int skillIndex)
	{
		if(skillWheelActive)
		{
			Debug.Log("Performing skill on index " + skillIndex);
			UseSkillAtIndex(skillIndex);
		}
	}


	private void UseSkillAtIndex(int index)
	{
		if (index < 0 || index >= MAX_SKILL_SLOTS) return;

		if(equippedSkills[index] != null && equippedSkills[index].CanUse())
		{
			if(! (equippedSkills[index].Definition is ProjectileSkillDefinition))
			{
				equippedSkills[index].Use(playerController.GetMovementDirection()); //Esto seria momentaneo, solamente para ver si se ejecuta la skill que corresponda
			}
			else
			{
				//Si es de proyectil, tiene que recibir si o si la direccion de apuntado, no la de movimiento
				equippedSkills[index].Use(weaponAimingLogic.GetWeaponMuzzle().forward);
			}
		}
			
		else
		{
			Debug.Log("Skill cannot be used while it´s on cooldown");
		}
	}
}


[System.Serializable]
public class SkillSlot
{
	public SkillDefinition skillDefinition;
	public ISkill runtimeSkill;
}
