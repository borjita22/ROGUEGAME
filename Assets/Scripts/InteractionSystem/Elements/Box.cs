using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Box : EffectableObject, IPickable
{
    [SerializeField] public PickableWeight weight = PickableWeight.Heavy;

	PickableWeight IPickable.weight => weight;

    protected bool isBurning = false;

    [Header("Special properties")]
    [SerializeField] protected float timeAlive;
    protected float elapsedTimeToKill;

	protected void Start()
	{
        elapsedTimeToKill = 0f;
	}

	protected override void Update()
	{
		CheckBurningState();
	}

	protected virtual void CheckBurningState()
	{
		if (isBurning)
		{
			elapsedTimeToKill += Time.deltaTime;

			if (elapsedTimeToKill >= timeAlive)
			{
                RemoveEffect(EffectType.Fire);
				this.gameObject.SetActive(false);
			}
		}
	}

	public override InteractionResult Interact(PlayerInteractionsController controller)
    {
        // Si puede ser recogida, la recogemos
        PickUp(weight == PickableWeight.Heavy ?
               controller.heavyPickableElementSlot :
               controller.lightPickableElementSlot);

        return InteractionResult.ItemPickedUp;
    }

    public void PickUp(Transform parent)
    {
        transform.SetParent(parent);
        transform.position = parent.position;

        if (rb)
        {
            rb.isKinematic = true;
        }
        if (groundCollider)
        {
            groundCollider.enabled = false;
        }
    }

    public void Drop()
    {
        transform.SetParent(null);

        if (groundCollider)
        {
            groundCollider.enabled = true;
        }
        if (rb)
        {
            rb.isKinematic = false;
        }
    }

    protected override void OnEffectApplied(EffectType effectType)
	{
		base.OnEffectApplied(effectType);

        if(activeEffects.ContainsKey(EffectType.Fire))
		{
            isBurning = true;
		}
        else
		{
            isBurning = false;
            //elapsedTimeToKill = 0f;
		}

        if(effectType == EffectType.Ice)
		{
            isBurning = false;
            elapsedTimeToKill = 0f;
		}

	}

}
