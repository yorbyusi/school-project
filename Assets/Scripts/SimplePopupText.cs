using System;
using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class SimplePopupText : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    public CanvasGroup canvas;
    public TextMeshProUGUI messageText;

    [Header("Settings")]
    public float fadeDuration = 0.5f;
    public float typewriterSpeed = 0.03f; // seconds per char

    public UnityEvent onShow;
    public UnityEvent onHide;

    private Coroutine typewriterRoutine;
    private bool isComplete = false;
    private string cacheFullText = "";

    private void Awake()
    {
        canvas.alpha = 0;
        canvas.interactable = false;
        canvas.blocksRaycasts = false;
    }

    public void ShowMessage(string message)
    {
        StopAllCoroutines();
        canvas.DOFade(1f, fadeDuration)
            .OnComplete(() =>
            {
                canvas.interactable = true;
                canvas.blocksRaycasts = true;
                onShow?.Invoke();
            });

        cacheFullText = message;
        typewriterRoutine = StartCoroutine(Typewriter(message));
    }

    private IEnumerator Typewriter(string message)
    {
        messageText.text = "";
        yield return new WaitForSeconds(fadeDuration);

        isComplete = false;

        for (int i = 0; i < message.Length; i++)
        {
            messageText.text += message[i];

            var playSfxThreshold = 2;
            if (i % playSfxThreshold == 0 && !char.IsWhiteSpace(message[i]))
            {
                AudioManager.Instance?.PlaySFX("beep-1", 0.9f, 1.2f);
            }

            yield return new WaitForSeconds(typewriterSpeed);
        }

        isComplete = true;
    }



    public void HideMessage()
    {
        StopAllCoroutines();
        canvas.interactable = false;
        canvas.blocksRaycasts = false;

        canvas.DOFade(0f, fadeDuration)
            .OnComplete(() =>
            {
                onHide?.Invoke();
            });
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isComplete)
        {
            // Skip typing, instantly show full text
            StopCoroutine(typewriterRoutine);
            messageText.text = cacheFullText;
            isComplete = true;
        }
        else
        {
            HideMessage();
        }
    }
}
