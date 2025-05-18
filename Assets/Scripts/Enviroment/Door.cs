using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Clase que controla una puerta que puede ser abierta por un interruptor
public class Door : MonoBehaviour
{

	//necesito metodos para abrir y cerrar la puerta

	private void OnEnable()
	{
		DoorTrigger._OnTriggerStatus += CheckTriggerStatus;
	}

	private void OnDisable()
	{
		DoorTrigger._OnTriggerStatus -= CheckTriggerStatus;
	}

	private void CheckTriggerStatus(bool status, Door door)
	{
		if(door == this)
		{
			if(status == true)
			{
				Open();
			}
			else
			{
				Close();
			}
		}
	}

	private void Open()
	{
		this.GetComponent<Renderer>().enabled = false;
		Debug.Log("Opening door");
	}

	private void Close()
	{
		this.GetComponent<Renderer>().enabled = true;
		Debug.Log("Closing door");
	}
}
