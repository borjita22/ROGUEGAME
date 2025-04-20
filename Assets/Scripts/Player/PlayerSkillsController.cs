using System;
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

	public event Action<int, ISkill> OnSkillEquipped;
	public event Action<int> OnSkillSelected;


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

		//playerInput._OnOpenSkillWheel += SetSkillWheelStatus;
		//playerInput._OnUseGamepadSkill += PerformSkillGamepad;
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

		OnSkillSelected?.Invoke(selectedSkillIndex);
	}

	private void PerformSkill()
	{
		//Debug.Log("Performing skill");
		UseSkillAtIndex(selectedSkillIndex);
	}

	
	private void UseSkillAtIndex(int index)
	{
		if (index < 0 || index >= MAX_SKILL_SLOTS) return;

		if(equippedSkills[index] != null && equippedSkills[index].CanUse())
		{
			if(!(equippedSkills[index].Definition is ProjectileSkillDefinition))
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
			Debug.Log("Skill cannot be used while it?s on cooldown");
		}
	}

	public ISkill GetSkillAtSlot(int index)
	{
		if(index >= 0 && index < MAX_SKILL_SLOTS)
		{
			return equippedSkills[index];
		}
		return null;
	}

	public void EquipSkill(SkillDefinition skillDef, int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= MAX_SKILL_SLOTS) return;

		// Desequipar habilidad actual si existe
		if (equippedSkills[slotIndex] != null)
		{
			//equippedSkills[slotIndex].OnUnequip(); // Asumiendo que implementaste este m?todo
		}

		// Crear nueva instancia
		ISkill newSkill = skillDef.CreateInstance(playerController);

		// Equipar
		skillSlots[slotIndex].skillDefinition = skillDef;
		skillSlots[slotIndex].runtimeSkill = newSkill;
		equippedSkills[slotIndex] = newSkill;

		// Notificar
		OnSkillEquipped?.Invoke(slotIndex, newSkill);

		// Si es el slot seleccionado, actualizar referencia
		if (slotIndex == selectedSkillIndex)
		{
			currentSelectedSkill = skillDef;
		}
	}

	
}


[System.Serializable]
public class SkillSlot
{
	public SkillDefinition skillDefinition;
	public ISkill runtimeSkill;
}
