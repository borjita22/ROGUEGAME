using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Scene Collection", menuName = "Game/Scene Collection")]
public class SceneCollection : ScriptableObject
{
    [SerializeField] private SceneOption[] ScenesToLoad;

    public string GetRandomSceneName()
    {
        if(ScenesToLoad.Length == 0) return null;

        int totalWeight = 0;

        foreach(SceneOption option in ScenesToLoad)
        {
            totalWeight += option.weight;
        }

        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;


        foreach (SceneOption option in ScenesToLoad)
        {
            currentWeight += option.weight;
            if (randomValue < currentWeight)
                return option.SceneName;
        }

        return ScenesToLoad[0].SceneName;
    }
}

[System.Serializable]
public class SceneOption
{
    public string SceneName;
    [SerializeField] [Range(0,100)] public int weight;

}
