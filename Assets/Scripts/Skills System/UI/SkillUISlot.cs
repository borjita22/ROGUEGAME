using UnityEngine;
using UnityEngine.UI;

//Esta clase representa un slot de la UI
public class SkillUISlot : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private Slider cooldownSlider;
    [SerializeField] private Outline selectionOutline;

    private float maxSkillCooldown;

    [SerializeField] private SkillDefinition attachedSkill;
    
    public void Initialize(SkillDefinition skill)
	{
        selectionOutline = GetComponentInChildren<Outline>();
        cooldownSlider = GetComponentInChildren<Slider>();

        if (skill != null)
		{
            skillIcon.sprite = skill.skillIcon;
            skillIcon.enabled = true;
            maxSkillCooldown = skill.cooldown;
            cooldownSlider.maxValue = maxSkillCooldown;
            cooldownSlider.value = 0f;

            attachedSkill = skill; //Esto unicamente es con propositos de testing, para probar si se vincula correctamente
		}

        
	}

    public void UpdateCooldown(float remainingCooldown)
	{
        //maxSkillCooldown = totalCooldown;

        float fillAmount = maxSkillCooldown - remainingCooldown;

        cooldownSlider.value = fillAmount;
	}

    public void SetSelected(bool selected)
	{
        if(selectionOutline != null)
		{
            selectionOutline.enabled = selected;
		}
	}
}
