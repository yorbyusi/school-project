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

    public Image[] starImages;
    private int starCount = 0;   // how many stars to spawn this time

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

        var currentScene = SceneManager.GetActiveScene().name;
        restartBtn.onClick.AddListener(() => OpenScene(currentScene));

        //var nextLevelName = currentScene switch
        //{
        //    "SekolahLevel1" => "SekolahLevel2",
        //    "SekolahLevel2" => "SekolahLevel3",
        //    "SekolahLevel3" => "SekolahLevel4",
        //    "SekolahLevel4" => "SekolahLevel5",
        //    _ => null
        //};

        //if(currentScene == "SekolahLevel5")
        //{
        //    nextLevelBtn.onClick.AddListener(() => OpenScene("MainMenu"));
        //    return;
        //}

        nextLevelBtn.onClick.AddListener(() => OpenScene("MainMenu"));
    }

    private void Update()
    {
        if (Application.isEditor && Input.GetKeyDown(KeyCode.P))
        {
            ShowEnding("Debug Ending: Pressed P", 15, 15);
        }
    }

    private void OpenScene(string sceneName)
    {
        AudioManager.Instance.StopBGM();
        AudioManager.Instance.StopAmbience();
        SceneManager.LoadScene(sceneName);
    }

    public void Show(int score, int maxScore)
    {
        ShowEnding($"Skor akhir kamu: {score}/{maxScore}", score, maxScore);

        var level = SceneManager.GetActiveScene().name switch
        {
            "SekolahLevel1" => 1,
            "SekolahLevel2" => 2,
            "SekolahLevel3" => 3,
            "SekolahLevel4" => 4,
            "SekolahLevel5" => 5,
            _ => 0
        };

        GameState.Instance.SetScore(level, score, maxScore);
    }

    public void ShowEnding(string message, int score, int maxScore)
    {
        // calculate stars
        if (score <= 0)
            starCount = 1;
        else if (score < maxScore)
            starCount = 2;
        else
            starCount = 3;

        // disable next level if not max stars
        //if (nextLevelBtn != null)
        //    nextLevelBtn.interactable = (starCount == 3);

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShowEndingRoutine(message));
    }


    private IEnumerator ShowEndingRoutine(string message)
    {
        gameObject.SetActive(true);

        // Reset canvas, text, stars, buttons
        canvas.alpha = 0f;
        canvas.interactable = false;
        canvas.blocksRaycasts = false;

        finalScoreText.text = "";
        if (starImages != null)
        {
            foreach (var star in starImages)
            {
                if (star != null) star.gameObject.SetActive(false);
            }
        }
        if (restartBtn) restartBtn.gameObject.SetActive(false);
        if (nextLevelBtn) nextLevelBtn.gameObject.SetActive(false);

        // Optional start delay
        yield return new WaitForSeconds(startDelay);

        // 1) Fade in canvas
        _fadeTween = canvas.DOFade(1f, fadeDuration);
        yield return _fadeTween.WaitForCompletion();
        AudioManager.Instance.PlaySFX("popup-show");

        yield return new WaitForSeconds(0.3f);
        // 2) Animate stars one by one, only show starCount
        if (starImages != null)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                var star = starImages[i];
                if (star == null) continue;

                if (i < starCount) // show only up to starCount
                {
                    star.gameObject.SetActive(true);
                    star.transform.localScale = Vector3.zero;

                    AudioManager.Instance.PlaySFX("Star-1");
                    star.transform
                        .DOScale(Vector3.one, 0.4f)
                        .SetEase(Ease.OutBack)
                        .WaitForCompletion();

                    yield return new WaitForSeconds(0.7f);
                }
                else
                {
                    star.gameObject.SetActive(false); // hide unused stars
                }
            }
        }


        // 3) Typewriter text
        for (int i = 0; i < message.Length; i++)
        {
            finalScoreText.text += message[i];

            var playSfxThreshold = 2;
            if (i % playSfxThreshold == 0 && !char.IsWhiteSpace(message[i]))
            {
                AudioManager.Instance?.PlaySFX("beep-1", 0.9f, 1.2f);
            }

            yield return new WaitForSeconds(charDelay);
        }

        // 4) Show & punch restart button
        if (restartBtn != null)
        {
            restartBtn.gameObject.SetActive(true);
            restartBtn.transform.localScale = Vector3.one;
            _punchA = restartBtn.transform
                .DOPunchScale(Vector3.one * punchScale, punchDuration, punchVibrato, punchElasticity);
            yield return _punchA.WaitForCompletion();
        }

        // 5) Show & punch next level button
        if (nextLevelBtn != null)
        {
            nextLevelBtn.gameObject.SetActive(true);
            nextLevelBtn.transform.localScale = Vector3.one;
            _punchB = nextLevelBtn.transform
                .DOPunchScale(Vector3.one * punchScale, punchDuration, punchVibrato, punchElasticity);
            yield return _punchB.WaitForCompletion();
        }

        // enable interaction AFTER everything
        canvas.interactable = true;
        canvas.blocksRaycasts = true;

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
