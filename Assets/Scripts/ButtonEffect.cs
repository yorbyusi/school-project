using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening; // if you use DOTween, otherwise swap with Lerp

public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Settings")]
    private string onHoverSfx = "button-hover";
    private string onClickSfx = "button-click";
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float scaleDuration = 0.15f;

    private AudioManager audioManager;
    private Button button;
    private Vector3 originalScale;
    private Tween scaleTween;

    private void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;
    }

    private void Start()
    {
        audioManager = AudioManager.Instance;
        if (audioManager == null)
            Debug.LogWarning("No AudioManager found in the scene.");

        if (button != null)
            button.onClick.AddListener(PlayClickSound);
    }

    private void OnDisable()
    {
        if (scaleTween != null && scaleTween.IsActive())
            scaleTween.Kill();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && !button.interactable)
            return;
        PlayHoverSound();
        AnimateScale(originalScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(button != null && !button.interactable)
            return;
        AnimateScale(originalScale);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AnimateScale(originalScale);

        if (button != null && !button.interactable)
            return;

        PlayClickSound();
    }

    private void AnimateScale(Vector3 targetScale)
    {
        if (scaleTween != null && scaleTween.IsActive())
            scaleTween.Kill();

        scaleTween = transform.DOScale(targetScale, scaleDuration).SetEase(Ease.OutQuad);
    }

    private void PlayHoverSound()
    {
        if (audioManager != null)
            audioManager.PlaySFX(onHoverSfx);
    }

    private void PlayClickSound()
    {
        if (audioManager != null)
            audioManager.PlaySFX(onClickSfx);
    }
}
