using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpritesZWrite : MonoBehaviour
{
    // Un valor mayor = más sensibilidad a pequeños cambios en Z
    [SerializeField] private float zDepthResolution = 100f;

    // Actualiza en tiempo real durante el modo Editor y en ejecución
    private void Update()
    {
        UpdateSortingOrder();
    }

    // Actualiza el sorting order basado en la posición Z
    private void UpdateSortingOrder()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Convierte la posición Z en un sorting order (valores Z más pequeños = objetos más cercanos = mayor sorting order)
            int sortingOrderFromZ = Mathf.RoundToInt(-transform.position.z * zDepthResolution);

            // Aplica el nuevo sorting order
            spriteRenderer.sortingOrder = sortingOrderFromZ;
        }
    }
}
