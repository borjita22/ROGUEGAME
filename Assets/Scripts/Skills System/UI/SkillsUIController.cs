using UnityEngine;
using DG.Tweening;

public class SkillsUIController : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private CanvasGroup skillsPanelCanvasGroup;
    [SerializeField] private RectTransform skillsPanelRectTransform;

	[SerializeField] private SkillUISlot[] skillSlots = new SkillUISlot[4];

    [Header("Gamepad configuration")]
    [SerializeField] private float panelInactiveAlpha;
    [SerializeField] private float panelActiveAlpha;
    [SerializeField] private float panelInactiveScale;

    [Header("Animation configuration")]
    [SerializeField] private float animationDuration;
    [SerializeField] private Ease animationEase;

    private Vector3 originalPanelScale;
    private bool isUsingGamepad = false;
    private bool isWheelActive = false;


    private Tween alphaTween;
    private Tween scaleTween;

	private PlayerSkillsController skillsController;
	private int currentSelectedIndex = 0;

	private void Awake()
	{
		if(skillsPanelRectTransform != null)
		{
            originalPanelScale = skillsPanelRectTransform.localScale;
		}

		skillsController = FindFirstObjectByType<PlayerSkillsController>();
	}

	private void OnEnable()
	{
		if(InputDeviceManager.Instance != null)
		{
			InputDeviceManager.Instance.OnInputDeviceChanged += OnInputDeviceChanged;
		}

		PlayerInputHandler playerInputHandler = FindFirstObjectByType<PlayerInputHandler>();

		if(playerInputHandler)
		{
			playerInputHandler._OnOpenSkillWheel += OnSkillWheelStatusChanged;
		}

		if(skillsController != null)
		{
			skillsController.OnSkillEquipped += OnSkillEquipped;
			skillsController.OnSkillSelected += OnSkillSelected;
		}



		UpdatePanelVisuals();
		
	}

	private void Start()
	{
		InitializeSkillSlots();
	}

	private void OnDisable()
	{
		if (InputDeviceManager.Instance != null)
		{
			InputDeviceManager.Instance.OnInputDeviceChanged -= OnInputDeviceChanged;
		}

		PlayerInputHandler playerInputHandler = FindFirstObjectByType<PlayerInputHandler>();

		if (playerInputHandler)
		{
			playerInputHandler._OnOpenSkillWheel -= OnSkillWheelStatusChanged;
		}

		if (skillsController != null)
		{
			skillsController.OnSkillEquipped -= OnSkillEquipped;
			skillsController.OnSkillSelected -= OnSkillSelected;
		}

		KillTweens();
	}

	private void Update()
	{
		if(skillsController != null)
		{
			for(int i = 0; i < skillSlots.Length; i++)
			{
				if(skillSlots[i] != null)
				{
					var skill = skillsController.GetSkillAtSlot(i);

					if(skill != null && skill.IsOnCooldown)
					{
						skillSlots[i].UpdateCooldown(skill.RemainingCooldown);
					}
				}
			}
		}
	}

	private void UpdatePanelVisuals()
	{
		if (skillsPanelCanvasGroup == null || skillsPanelRectTransform == null) return;

		KillTweens();

		float targetAlpha = 1f;

		Vector3 targetScale = originalPanelScale;


		if(isUsingGamepad)
		{
			targetAlpha = isWheelActive ? panelActiveAlpha : panelInactiveAlpha;
			targetScale = originalPanelScale * (isWheelActive ? 1.0f : panelInactiveScale);
		}

		//Animar alpha
		alphaTween = DOTween.To(
				() => skillsPanelCanvasGroup.alpha,
				x => skillsPanelCanvasGroup.alpha = x,
				targetAlpha,
				animationDuration
			).SetEase(animationEase);

		// Animar escala
		scaleTween = skillsPanelRectTransform.DOScale(
			targetScale,
			animationDuration
		).SetEase(animationEase);
	}

	private void OnInputDeviceChanged(InputDeviceType deviceType)
	{
		isUsingGamepad = deviceType == InputDeviceType.Gamepad;

		Debug.Log("Input device changed: " + deviceType);

		if (!isUsingGamepad)
		{
			isWheelActive = true;
		}

		UpdatePanelVisuals();
		SetOutlineSelectionForUISlots(deviceType);
	}

	private void OnSkillWheelStatusChanged(bool active)
	{
		isWheelActive = active;

		UpdatePanelVisuals();

		Debug.Log("Skill wheel status changed: " + isWheelActive);
	}

	private void KillTweens()
	{
		if (alphaTween != null && alphaTween.IsActive())
			alphaTween.Kill();

		if (scaleTween != null && scaleTween.IsActive())
			scaleTween.Kill();
	}

	private void InitializeSkillSlots()
	{
		if (skillsController == null) return;

		for(int i=0; i<skillSlots.Length; i++)
		{
			if(skillSlots[i] != null)
			{
				var skill = skillsController.GetSkillAtSlot(i);

				if(skill != null)
				{
					skillSlots[i].Initialize(skill.Definition);
				}
				else
				{
					skillSlots[i].Initialize(null);
				}

				//para activar el outline, hay que ver si estamos usando teclado o mando
				SetOutlineSelectionForUISlots(InputDeviceManager.Instance.CurrentDeviceType);
				
			}
		}
	}

	private void OnSkillEquipped(int index, ISkill skill)
	{
		if (index >= 0 && index < skillSlots.Length && skillSlots[index] != null)
		{
			skillSlots[index].Initialize(skill?.Definition);
		}
	}

	private void OnSkillSelected(int index)
	{
		currentSelectedIndex = index;

		// Actualizar selección visual
		for (int i = 0; i < skillSlots.Length; i++)
		{
			if (skillSlots[i] != null)
			{
				skillSlots[i].SetSelected(i == index);
			}
		}
	}

	private void SetOutlineSelectionForUISlots(InputDeviceType deviceType)
	{
		for(int i = 0; i < skillSlots.Length; i++)
		{
			if(skillSlots[i] != null)
			{
				if (deviceType == InputDeviceType.Keyboard)
				{
					skillSlots[i].SetSelected(i == currentSelectedIndex);
				}
				else
				{
					skillSlots[i].SetSelected(false);
				}
			}
			
		}
		
	}
}
