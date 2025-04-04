using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpritesZWrite : MonoBehaviour
{
    // Un valor mayor = m�s sensibilidad a peque�os cambios en Z
    [SerializeField] private float zDepthResolution = 100f;

    // Actualiza en tiempo real durante el modo Editor y en ejecuci�n
    private void Update()
    {
        UpdateSortingOrder();
    }

    // Actualiza el sorting order basado en la posici�n Z
    private void UpdateSortingOrder()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Convierte la posici�n Z en un sorting order (valores Z m�s peque�os = objetos m�s cercanos = mayor sorting order)
            int sortingOrderFromZ = Mathf.RoundToInt(-transform.position.z * zDepthResolution);

            // Aplica el nuevo sorting order
            spriteRenderer.sortingOrder = sortingOrderFromZ;
        }
    }
}
