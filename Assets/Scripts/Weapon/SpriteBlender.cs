using System.Collections;
using UnityEngine;

public class SpriteBlender : MonoBehaviour
{
    [SerializeField] private SpriteRenderer topViewSprite;
    [SerializeField] private SpriteRenderer frontViewSprite;

	[SerializeField] private float fadeSpeed = 5f;       // Velocidad de la transici�n
	[SerializeField] private float maxAlpha = 1f;        // Alfa m�ximo
	[SerializeField] private float minAlpha = 0f;        // Alfa m�nimo (generalmente 0)

    private Coroutine topFadeCoroutine;
    private Coroutine frontFadeCoroutine;

	private void OnEnable()
	{
		AimingLogic.OnFrontViewVisibilityChanged += HandleSpritesVisibility;
	}

	private void OnDisable()
	{
		AimingLogic.OnFrontViewVisibilityChanged -= HandleSpritesVisibility;
	}

    private void HandleSpritesVisibility(bool shouldShow)
    {
        if (topFadeCoroutine != null) StopCoroutine(topFadeCoroutine);
        if (frontFadeCoroutine != null) StopCoroutine(frontFadeCoroutine);

        // Iniciar nuevas transiciones
        topFadeCoroutine = StartCoroutine(FadeSprite(topViewSprite, shouldShow));
        frontFadeCoroutine = StartCoroutine(FadeSprite(frontViewSprite, shouldShow));
        //frontViewFadeCoroutine = StartCoroutine(FadeSprite(frontViewSprite, shouldShow));
    }

    private IEnumerator FadeSprite(SpriteRenderer sprite, bool fadeIn)
    {
        // Asegurarnos de que el sprite est� activo para que se pueda ver la transici�n
        sprite.enabled = true;

        float targetAlpha = fadeIn ? maxAlpha : minAlpha;
        Color currentColor = sprite.color;
        float currentAlpha = currentColor.a;

        // Continuar la transici�n hasta alcanzar el valor objetivo
        while (Mathf.Abs(currentAlpha - targetAlpha) > 0.01f)
        {
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

            currentColor.a = currentAlpha;
            sprite.color = currentColor;

            yield return null;
        }

        // Asegurar que se alcanza exactamente el valor objetivo
        currentColor.a = targetAlpha;
        sprite.color = currentColor;

        // Si el alfa final es 0, podemos desactivar el sprite completamente para optimizar
        if (targetAlpha == 0)
        {
            sprite.enabled = false;
        }
    }
}
