using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalController : MonoBehaviour, IInteractable
{
    public bool IsConsumedOnInteraction => false;

    [SerializeField] private SceneCollection sceneCollection;

    [SerializeField] private string defaultSceneName;

    [SerializeField] private bool useDefaultScene = false;


    private void Awake()
    {
        
    }

    public bool CanInteractWith(IInteractable other)
    {
        throw new System.NotImplementedException();
    }

    public InteractionResult Interact(PlayerInteractionsController controller = null)
    {
        ActivatePortal(); //Luego habra que comprobar diferentes cosas

        return InteractionResult.InteractionApplied;
    }

    public InteractionResult InteractWith(IInteractable other)
    {
        throw new System.NotImplementedException();
    }

    public void ReceiveInteraction(IInteractable from)
    {
        throw new System.NotImplementedException();
    }

    private void ActivatePortal()
    {
        string sceneToLoad = defaultSceneName;

        if(useDefaultScene)
        {
            sceneToLoad = defaultSceneName;
        }
        else if(sceneCollection != null)
        {
            sceneToLoad = sceneCollection.GetRandomSceneName();
        }
        else
        {
            Debug.LogError("ERROR: No se ha encontrado una escena que cargar");
        }

        if(SceneLoader.SceneLoaderInstance && !string.IsNullOrEmpty(sceneToLoad))
        {
            SceneLoader.SceneLoaderInstance.LoadScene(sceneToLoad);
        }
    }
}
