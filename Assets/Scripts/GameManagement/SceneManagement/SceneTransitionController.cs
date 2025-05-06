using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SceneTransitionController : MonoBehaviour
{
    [Header("UI configuration")]
    [SerializeField] private CanvasGroup fadeCG;
    [SerializeField] private AnimationCurve fadeAnimationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public IEnumerator FadeOut(float duration)
    {
        ActivateFader();
        fadeCG.blocksRaycasts = true;
        float timeElapsed = 0f;

        while(timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;

            float normalizedTime = timeElapsed / duration;

            fadeCG.alpha = fadeAnimationCurve.Evaluate(normalizedTime);

            yield return null;
        }

        fadeCG.alpha = 1f;
    }

    public IEnumerator FadeIn(float duration)
    {
        ActivateFader();
        float timeElapsed = 0f;

        while(timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;

            float normalizedTime = timeElapsed / duration;

            fadeCG.alpha = 1f - fadeAnimationCurve.Evaluate(normalizedTime);

            yield return null;
        }

        fadeCG.alpha = 0f;
        fadeCG.blocksRaycasts = false;
    }

    private void ActivateFader()
    {
        if(fadeCG == null)
        {
            fadeCG = GameObject.Find("FaderCanvas").GetComponent<CanvasGroup>();
        }

        if(fadeCG.enabled == false || fadeCG.gameObject.activeSelf == false)
        {
            fadeCG.gameObject.SetActive(true);
            fadeCG.enabled = true;
        }
    }
}
