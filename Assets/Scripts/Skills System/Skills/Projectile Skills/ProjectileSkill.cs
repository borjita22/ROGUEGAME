using UnityEngine;

public class ProjectileSkill : BaseSkill
{
	private ProjectileSkillDefinition projectileDefinition;

	private GameObject projectilePrefab;
	private SkillProjectile projectile;

	public ProjectileSkill(SkillDefinition definition, PlayerController owner) : base(definition, owner)
	{
		if (definition is ProjectileSkillDefinition projectileDef)
		{
			projectileDefinition = projectileDef;
		}
		else if (definition is ProjectileSkillDefinition staticDef)
		{
			projectileDefinition = ScriptableObject.CreateInstance<ProjectileSkillDefinition>();
			projectileDefinition.skillName = definition.skillName;
			projectileDefinition.cooldown = definition.cooldown;

		}
	}


	//Esto no puede recibir la direccion de movimiento, sino la direccion de apuntado
	public override void Use(Vector3 direction)
	{
        if (!CanUse()) return;
        IsExecuting = true;
        Vector3 spawnPosition = owner.transform.position;
        if (projectileDefinition.spawnOffset != Vector3.zero) //igual esto no es necesario
        {
            spawnPosition += direction.normalized * projectileDefinition.spawnOffset.magnitude;
        }

        Quaternion rotation = Quaternion.LookRotation(direction);

        if (projectilePrefab == null)
        {
            projectilePrefab = GameObject.Instantiate(Definition.skillPrefab);
            //Al instanciarlo la primera vez, hay que settearle las propiedades basicas. El prefab del proyectil tambien va a tener un effectType (o un array de estos) que se le pasara en el metodo 
            //Initialize
            projectile = projectilePrefab.GetComponent<SkillProjectile>();
            if (projectile)
            {
                projectile.Initialize(projectileDefinition);
            }
        }
        else
        {
            projectilePrefab.SetActive(true);
        }
        projectilePrefab.transform.position = spawnPosition;
        if (projectile)
        {
            projectile.SetProjectileDirection(direction);
        }
        //aqui se podria inicializar el proyectil en cuestion
        Cancel();
    }

	public override void Cancel()
	{
		IsExecuting = false;
		StartCooldown();
	}

	
}
