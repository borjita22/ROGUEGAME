using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Clase controladora de las cargas de las escenas
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader SceneLoaderInstance;

    [Header("Scene loading configuration")]
    [SerializeField] private float fadeTime = 1.0f;

    private SceneTransitionController sceneTransitionController;


    private Coroutine loadSceneCoroutine;

    private void Awake()
    {
        if(SceneLoaderInstance == null)
        {
            SceneLoaderInstance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);

        sceneTransitionController = FindFirstObjectByType<SceneTransitionController>();
    }

    //Necesito una referencia al SceneTransition

    public void LoadScene(string sceneName)
    {
        if(loadSceneCoroutine == null)
        {
            loadSceneCoroutine = StartCoroutine(LoadSceneAsync(sceneName));
        }
        else
        {
            StopAllCoroutines();
            loadSceneCoroutine = null;
        }
        
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        if(sceneTransitionController)
        {
            yield return sceneTransitionController.FadeOut(fadeTime);
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while(!asyncLoad.isDone)
        {
            yield return null;
        }

        if(sceneTransitionController)
        {
            yield return sceneTransitionController.FadeIn(fadeTime * 2f);
        }
    }
}
