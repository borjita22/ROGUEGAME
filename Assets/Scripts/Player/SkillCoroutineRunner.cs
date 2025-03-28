using System.Collections;
using UnityEngine;

public class SkillCoroutineRunner : MonoBehaviour
{
    public static SkillCoroutineRunner Instance { get; private set; }

	private void Awake()
	{
		if(Instance == null)
		{
            Instance = this;
		}
        else
		{
            Destroy(this);
		}
	}
	
	public Coroutine RunCoroutine(IEnumerator routine)
	{
		return StartCoroutine(routine);
	}

	public void StopRoutine(Coroutine coroutine)
	{
		if(coroutine != null)
		{
			StopCoroutine(coroutine);
		}
	}
}
