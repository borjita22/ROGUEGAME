using System.Collections;
using UnityEngine;

public class DashSkill : BaseSkill
{
	private Coroutine activeCoroutine;
	private DashSkillDefinition dashDefinition;

    private GameObject dashPrefab;


    public DashSkill(SkillDefinition definition, PlayerController owner) : base(definition, owner)
    {
        // Verificar y castear a la definición específica
        if (definition is DashSkillDefinition dashDef)
        {
            dashDefinition = dashDef;
        }
        else if (definition is StaticSkillDefinition staticDef && staticDef.staticBehaviour == StaticSkillBehaviour.Dash)
        {
            // Caso de compatibilidad hacia atrás (usando StaticSkillDefinition)
            dashDefinition = ScriptableObject.CreateInstance<DashSkillDefinition>();
            dashDefinition.skillName = definition.skillName;
            dashDefinition.cooldown = definition.cooldown;
            // Otros valores por defecto
        }
        else
        {
            Debug.LogError($"Intentando crear DashSkill con definición incorrecta: {definition.GetType()}");
        }
    }

    

	public override void Use(Vector3 direction)
	{
        if (!CanUse()) return;

        Debug.Log($"Iniciando dash: {Definition.skillName}");

        // Iniciar la corrutina
        activeCoroutine = SkillCoroutineRunner.Instance.RunCoroutine(
            PerformDashMovement(direction));
    }

    private IEnumerator PerformDashMovement(Vector3 direction)
    {
        IsExecuting = true;

        // Normalizar dirección
        direction = direction.normalized;

        // Valores de la definición específica
        float dashDistance = dashDefinition.dashDistance;
        float dashSpeed = dashDefinition.dashSpeed;
        bool providesInvulnerability = dashDefinition.providesInvulnerability;

        // Variables para tracking
        float distanceTraveled = 0f;

        // Activar invulnerabilidad si es necesario
        if (providesInvulnerability)
        {
            //owner.SetInvulnerable(true);
        }

        // Efectos visuales y sonoros
        PlayFeedback();

        
        // Desactivar invulnerabilidad
        if (providesInvulnerability)
        {
            //owner.SetInvulnerable(false);
        }

        while (distanceTraveled < dashDistance)
        {
            float moveStep = dashSpeed * Time.deltaTime;

            // Evitar sobrepasarse
            if (distanceTraveled + moveStep > dashDistance)
            {
                moveStep = dashDistance - distanceTraveled;
            }


            // Mover al jugador
            owner.transform.position += (direction * moveStep);
            distanceTraveled += moveStep;

            // Detectar interacciones durante el dash
            //DetectDashInteractions(direction);

            yield return null;
        }

        // Iniciar cooldown
        Cancel();
        StartCooldown();

    }


    public override void Cancel()
    {
        if (IsExecuting && activeCoroutine != null)
        {
            // Detener corrutina
            SkillCoroutineRunner.Instance.StopRoutine(activeCoroutine);

            // Desactivar invulnerabilidad si estaba activa
            if (dashDefinition.providesInvulnerability)
            {
                //owner.SetInvulnerable(false);
            }

            if(dashPrefab) //Hay que replantearse la opcion de instanciar y destruir objetos, sobre todo en las skills que van a tener poco cooldown
			{
                dashPrefab.SetActive(false);
			}

            // Limpiar estado
            IsExecuting = false;
            activeCoroutine = null;
        }
    }

	protected override void PlayFeedback()
	{
        //Muestra un halo (line renderer o algo asi) en la posicion del jugador
        if (Definition.skillPrefab == null) return;

        if(dashPrefab == null)
		{
            dashPrefab = GameObject.Instantiate(Definition.skillPrefab, owner.transform);
        }
        else
		{
            dashPrefab.SetActive(true);
		}

        dashPrefab.transform.position = owner.transform.position;
        dashPrefab.transform.rotation = owner.transform.rotation;
	}

	public override void OnEquipSkill()
	{
		base.OnEquipSkill();
	}

    //Cuando se desequipa la skill, se ejecutaran las acciones que toquen. En este caso, se destruye el prefab
    //que estaba vinculado al jugador
	public override void OnUnequipSkill()
	{
		if(dashPrefab)
		{
            GameObject.Destroy(dashPrefab);
		}
	}
}
