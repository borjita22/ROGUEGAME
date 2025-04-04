using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Box : EffectableObject, IPickable
{
	[SerializeField] public PickableWeight weight  { get; protected set; }

	protected override void Awake()
	{
		base.Awake();
		weight = PickableWeight.Heavy;
	}
	public void PickUp(Transform parent)
	{
		//Colocar este objeto como hijo del transform pasado como parametro
		this.transform.SetParent(parent);
		this.transform.position = parent.position;
	}


	public void Drop()
	{
		this.transform.SetParent(null);
	}


	protected override void OnEffectApplied(EffectType effectType)
	{
		base.OnEffectApplied(effectType);

	}

}
