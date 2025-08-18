using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class EndingPopup : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup canvas;
    public TextMeshProUGUI finalScoreText;
    public Button restartBtn;
    public Button nextLevelBtn;

    [Header("Timing")]
    public float startDelay = 0.15f;     // delay before the whole show starts
    public float fadeDuration = 0.35f;   // canvas fade to 1
    public float charDelay = 0.03f;      // seconds per character

    [Header("Punch Settings (buttons only)")]
    public float punchScale = 0.25f;
    public float punchDuration = 0.45f;
    public int punchVibrato = 10;
    [Range(0f, 1f)] public float punchElasticity = 1f;

    private Coroutine _routine;
    private Tween _fadeTween, _punchA, _punchB;

    private void Awake()
    {
        // Start hidden and non-interactable
        if (canvas != null)
        {
            canvas.alpha = 0f;
            canvas.interactable = false;
            canvas.blocksRaycasts = false;
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "";
            finalScoreText.enableWordWrapping = true; // wrap, don't overflow near edges
        }

        restartBtn.onClick.AddListener(() => OpenScene("SekolahLevel1"));
        nextLevelBtn.onClick.AddListener(() => OpenScene("SekolahLevel2"));
    }
    
    private void OpenScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ShowEnding(int score, int maxScore)
    {
        ShowEnding($"Skor akhir kamu: {score}/{maxScore}");
    }

    public void ShowEnding(string message)
    {
        if (_routine != null) StopCoroutine(_routine);
        KillTweens();

        _routine = StartCoroutine(ShowEndingRoutine(message));
    }

    private IEnumerator ShowEndingRoutine(string message)
    {
        gameObject.SetActive(true);

        // Reset canvas & text
        canvas.alpha = 0f;
        canvas.interactable = false;
        canvas.blocksRaycasts = false;
        finalScoreText.text = "";

        // Optional start delay
        yield return new WaitForSeconds(startDelay);

        // 1) Fade canvas to 1 (no punch on popup)
        _fadeTween = canvas.DOFade(1f, fadeDuration);
        yield return _fadeTween.WaitForCompletion();

        canvas.interactable = true;
        canvas.blocksRaycasts = true;

        // 2) Typewriter text (char by char)
        for (int i = 0; i < message.Length; i++)
        {
            finalScoreText.text += message[i];
            yield return new WaitForSeconds(charDelay);
        }

        // 3) Punch first button (restart)
        if (restartBtn != null)
        {
            _punchA = restartBtn.transform
                .DOPunchScale(Vector3.one * punchScale, punchDuration, punchVibrato, punchElasticity);
            yield return _punchA.WaitForCompletion();
        }

        // 4) Punch second button (next level)
        if (nextLevelBtn != null)
        {
            _punchB = nextLevelBtn.transform
                .DOPunchScale(Vector3.one * punchScale, punchDuration, punchVibrato, punchElasticity);
            yield return _punchB.WaitForCompletion();
        }

        _routine = null;
    }

    public void Hide()
    {
        if (_routine != null) StopCoroutine(_routine);
        KillTweens();
        StartCoroutine(HideRoutine());
    }

    private IEnumerator HideRoutine()
    {
        canvas.interactable = false;
        canvas.blocksRaycasts = false;
        _fadeTween = canvas.DOFade(0f, fadeDuration);
        yield return _fadeTween.WaitForCompletion();
        gameObject.SetActive(false);
    }

    private void KillTweens()
    {
        _fadeTween?.Kill();
        _punchA?.Kill();
        _punchB?.Kill();
    }

    private void OnDisable()
    {
        if (_routine != null) StopCoroutine(_routine);
        KillTweens();
    }
}
