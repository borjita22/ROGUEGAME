using System.Collections;
using UnityEngine;

public class SpriteBlender : MonoBehaviour
{
    [SerializeField] private SpriteRenderer topViewSprite;
    [SerializeField] private SpriteRenderer frontViewSprite;

	[SerializeField] private float fadeSpeed = 5f;       // Velocidad de la transición
	[SerializeField] private float maxAlpha = 1f;        // Alfa máximo
	[SerializeField] private float minAlpha = 0f;        // Alfa mínimo (generalmente 0)

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
        // Asegurarnos de que el sprite esté activo para que se pueda ver la transición
        sprite.enabled = true;

        float targetAlpha = fadeIn ? maxAlpha : minAlpha;
        Color currentColor = sprite.color;
        float currentAlpha = currentColor.a;

        // Continuar la transición hasta alcanzar el valor objetivo
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
