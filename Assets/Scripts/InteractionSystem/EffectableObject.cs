using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public abstract class EffectableObject : InteractableEntity, IEffectable
{
	//Cada objeto debe determinar si se le puede o no aplicar cada determinado efecto
	[SerializeField] protected List<EffectType> aplicableEffects = new List<EffectType>();
	[SerializeField] protected EffectConfig effectConfig;
	protected Dictionary<EffectType, EffectInstance> activeEffects = new Dictionary<EffectType, EffectInstance>();
	
	protected Collider attachedCollider;
	protected Collider groundCollider;

	protected Rigidbody rb;

	protected GameObject effectObject;

	protected virtual void Awake()
	{
		attachedCollider = GetComponent<Collider>();
		rb = GetComponent<Rigidbody>();
		groundCollider = transform.Find("Collider").GetComponent<Collider>();
	}
	public virtual void ApplyEffect(EffectType effectType)
	{
		if (!aplicableEffects.Contains(effectType)) return;

		if(!activeEffects.ContainsKey(effectType))
		{
			activeEffects[effectType] = new EffectInstance(effectType, 0f); //La duracion seguramente no sea necesaria
		}
		OnEffectApplied(effectType);
		HandleEffectInteractions(effectType);
	}

	public bool HasEffect(EffectType effectType)
	{
		return activeEffects.ContainsKey(effectType);
	}

	public void RemoveEffect(EffectType effectType)
	{
		OnEffectRemoved(effectType);
		if (activeEffects.ContainsKey(effectType))
		{
			activeEffects.Remove(effectType);
		}
	}

	protected virtual void HandleEffectInteractions(EffectType newEffect)
	{
		// Reglas b�sicas de interacci�n (customizables en subclases)
		if (newEffect == EffectType.Ice && HasEffect(EffectType.Fire))
		{
			RemoveEffect(EffectType.Fire);
		}
		else if (newEffect == EffectType.Fire && HasEffect(EffectType.Ice))
		{
			RemoveEffect(EffectType.Ice);
		}
		//resto de interacciones entre efectos
	}

	protected virtual void OnEffectApplied(EffectType effectType)
	{
		// Obtener el efecto visual del pool
		effectObject = effectConfig.GetEffectPool(effectType).GetObject();
		if (effectObject != null)
		{
			VisualEffect vfx = effectObject.GetComponent<VisualEffect>();
			if (vfx != null)
			{
				ConfigureVisualEffect(vfx);
			}
			

			if(effectObject)
			{
				effectObject.transform.SetParent(this.transform);
				effectObject.transform.position = attachedCollider.transform.position;
				// Guardar referencia al objeto visual
				activeEffects[effectType].VisualObject = effectObject;
			}
			
		}
	}

	//aqui podriamos encargarnos de devolver el objeto a su correspondiente pool
	protected virtual void OnEffectRemoved(EffectType effectType)
	{
		// Devolver el objeto visual al pool
		if (activeEffects[effectType].VisualObject != null)
		{
			activeEffects[effectType].VisualObject.transform.SetParent(null);

			//Si tuviese la escala modificada, volver a poner el objeto en su escala real
			if(activeEffects[effectType].VisualObject.transform.localScale != Vector3.one)
			{
				activeEffects[effectType].VisualObject.transform.localScale = Vector3.one;
			}
			//activeEffects[effectType].VisualObject.SetActive(false);
			PoolableObject poolableObject = activeEffects[effectType].VisualObject.GetComponent<PoolableObject>();

			if(poolableObject)
			{
				poolableObject.ReturnToPool();
			}

			if(effectObject)
			{
				effectObject = null;
			}
		}
	}

	protected void ConfigureVisualEffect(VisualEffect vfx)
	{
		if (groundCollider is BoxCollider boxCollider)
		{
			vfx.SetBool("IsSphereCollider", false);
			Vector3 size = Vector3.Scale(boxCollider.size, transform.lossyScale);
			vfx.SetVector3("BoxSize", size);
		}
		else if (groundCollider is SphereCollider sphereCollider)
		{
			vfx.SetBool("IsSphereCollider", true);
			float radius = sphereCollider.radius * Mathf.Max(transform.lossyScale.x,
				transform.lossyScale.y, transform.lossyScale.z);
			vfx.SetFloat("SphereRadius", radius);
		}

		//VisualEffect secondaryEffect = vfx.GetComponentInChildren<VisualEffect>();


	}

	protected virtual void Update()
	{

	}

	public override void ReceiveInteraction(IInteractable from)
	{
		base.ReceiveInteraction(from);

		if(from is EffectableObject effectable)
		{
			foreach(var effect in effectable.activeEffects)
			{
				if(effect.Value.IsActive) //Este IsActive de aqui actualmente no creo que me sirva de mucho, ya que en el momento en que deja de estar activo simplemente no tiene ningun efecto
				{
					
				}
				ApplyEffect(effect.Key);
			}
		}
	}

	//Esto hay que replantearselo por dise�o, si nos compensa aplicar efectos por contacto o solamente por interaccion
	protected void OnTriggerStay(Collider other)
	{
		//aplicar efecto sobre el otro objeto si detectamos que es Effectable
		if(other.TryGetComponent<IEffectable>(out IEffectable effectableObj))
		{
			
			foreach(var effect in activeEffects)
			{
				if(!(((EffectableObject)effectableObj).GetActiveEffects().ContainsKey(effect.Key)))
				{
					effectableObj.ApplyEffect(effect.Key);
				}
				
			}
		}
	}

	public Dictionary<EffectType, EffectInstance> GetActiveEffects()
	{
		return activeEffects;
	}
}
