using UnityEngine;

/// <summary>
/// Clase que representa un proyectil instanciado como parte de la ejecucion de una habilidad
/// </summary>
public class SkillProjectile : MonoBehaviour
{
    [Header("Effect settings")]
    public EffectType[] producedEffectTypes;

    public float damage;

    private Vector3 projectileDirection;

    private Rigidbody rb;

	public bool IsConsumedOnInteraction => throw new System.NotImplementedException();

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	public void Initialize(ProjectileSkillDefinition skillDefinition)
	{
        this.producedEffectTypes = skillDefinition.producedEffectTypes;
        this.damage = skillDefinition.projectileDamage;
        
        //ademas de todo esto, se estableceran diferentes patrones o trayectorias de movimiento dependiendo del tipo que sea este proyectil en concreto
        //esto podria definirlo tambien el projectileSkillDefinition
	}

    public void SetProjectileDirection(Vector3 direction)
	{
        this.projectileDirection = direction;
    }

	private void Update()
	{
		//mueve el proyectil en la direccion especificada en su inicializacion
	}

	private void FixedUpdate()
	{
		if(rb != null)
		{
			Vector3 nextPosition = rb.position + projectileDirection * 10f * Time.fixedDeltaTime; //esto esta hardcodeado, luego va a recibir la velocidad tambien de la definicion del proyectil
			rb.MovePosition(nextPosition);
		}
	}

	//Al entrar en el trigger, comprueban si un objeto es interactuable
	private void OnTriggerEnter(Collider other)
	{
		IEffectable effectable = other.GetComponent<IEffectable>();

		if(effectable != null)
		{
			foreach(EffectType effectType in producedEffectTypes)
			{
				effectable.ApplyEffect(effectType); //Quizas la duracion sea super innecesaria, ya que se saldria de los diferentes efectos de estado mediante interaccion y aplicacion de otros efectos de estado diferentes. Por ejemplo, el aplicar hielo a un objeto quemado hace que deje de estar quemado
			}
		}
	}


}
