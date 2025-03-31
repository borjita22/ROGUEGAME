using UnityEngine;

[ExecuteInEditMode]
public class SpriteOutline : MonoBehaviour
{
    public Color color = Color.white;

    [Range(0, 16)]
    public int outlineSize = 1;

    private SpriteRenderer spriteRenderer;

    [SerializeField] private bool outlineEnabled = false;

    void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        //UpdateOutline(true);
        SetOutlineStatus(true);
    }

    void OnDisable()
    {
        //UpdateOutline(false);
    }

    void Update()
    {
        //UpdateOutline(true);
        if(outlineEnabled == false)
		{
            SetOutlineStatus(false);
		}
        else
		{
            SetOutlineStatus(true);
		}

    }

    public void SetOutlineStatus(bool status)
	{
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_EnableOutline", status ? 1f : 0);

        mpb.SetFloat("_Outline", status ? 1f : 0);
        mpb.SetColor("_OutlineColor", color);
        mpb.SetFloat("_OutlineSize", outlineSize);
        spriteRenderer.SetPropertyBlock(mpb);
    }

   
}
