using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class QuizMiniGame : MonoBehaviour
{
    [System.Serializable]
    public class QuizData
    {
        public string question;
        public List<string> answers;
        public int correctIndex;
    }

    [Header("Intro Message")]
    [SerializeField] private CanvasGroup introCanvas;
    [SerializeField] private TextMeshProUGUI introText;
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private float introDelay = 0.5f;
    [Multiline(3)]
    [SerializeField] private string introMessage = "Jawablah pertanyaan berikut dengan benar!";


    [Header("UI References")]
    [SerializeField] private CanvasGroup quizCanvas;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Transform answersParent; // must have GridLayoutGroup
    [SerializeField] private Button answerPrefab;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Quiz Settings")]
    [SerializeField] private float timeLimit = 10f;
    [SerializeField] private int pointsPerCorrect = 5;

    [Header("Quiz Database")]
    public QuizData[] quizzes;

    [Header("Popup Settings")]
    public GameObject popupPanel;
    public TextMeshProUGUI popupText;
    [Multiline(3)]
    public string doneMessage = "You Win!";


    // success always true now (game ends after last Q), int = total points earned
    public UnityEvent<bool, int> onQuizFinished = new();

    private int currentQuizIndex = 0;
    private int currentPoints = 0;
    private Coroutine quizRoutine;

    public int MaxPoints => quizzes.Length * pointsPerCorrect;

    public void StartQuiz()
    {
        if (quizzes == null || quizzes.Length == 0)
        {
            Debug.LogWarning("No quizzes assigned!");
            onQuizFinished.Invoke(false, 0);
            return;
        }

        quizCanvas.alpha = 0f;
        currentPoints = 0;
        currentQuizIndex = 0;

        // Hide question/answers until intro is done
        questionText.text = "";
        foreach (Transform child in answersParent)
            Destroy(child.gameObject);

        introCanvas.DOFade(1f, 0.25f);
        introText.gameObject.SetActive(true);
        StartCoroutine(TypewriterIntro(introMessage, () =>
        {
            StartCoroutine(WaitForTap(() =>
            {
                introText.gameObject.SetActive(false);
                introCanvas.DOFade(0f, 0.25f);
                ShowQuiz(currentQuizIndex);
                quizCanvas.DOFade(1f, 0.5f);
                quizCanvas.interactable = true;
                quizCanvas.blocksRaycasts = true;
            }));
        }));
    }

    private IEnumerator TypewriterIntro(string message, System.Action onComplete)
    {
        introText.text = "";
        yield return new WaitForSeconds(introDelay);

        for (int i = 0; i < message.Length; i++)
        {
            introText.text += message[i];

            // phonetic effect every 2 chars
            if (i % 2 == 0 && !char.IsWhiteSpace(message[i]))
                AudioManager.Instance?.PlaySFX("beep-1", 0.9f, 1.2f);

            yield return new WaitForSeconds(typewriterSpeed);
        }

        onComplete?.Invoke();
    }


    private void ShowQuiz(int index)
    {
        var quiz = quizzes[index];
        questionText.text = quiz.question;

        // clear old answers
        foreach (Transform child in answersParent)
            Destroy(child.gameObject);

        // spawn hidden first
        List<Button> spawned = new List<Button>();
        for (int i = 0; i < quiz.answers.Count; i++)
        {
            int choiceIndex = i;
            var btn = Instantiate(answerPrefab, answersParent);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = quiz.answers[i];
            btn.onClick.AddListener(() => OnAnswerSelected(choiceIndex));

            btn.transform.localScale = Vector3.zero; // start invisible
            spawned.Add(btn);
        }

        // animate buttons one by one
        StartCoroutine(AnimateAnswers(spawned));

        // restart timer
        if (quizRoutine != null) StopCoroutine(quizRoutine);
        quizRoutine = StartCoroutine(QuizTimer());
    }

    private IEnumerator AnimateAnswers(List<Button> buttons)
    {
        yield return new WaitForSeconds(1f);

        foreach (var btn in buttons)
        {
            btn.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
            AudioManager.Instance?.PlaySFX("popup-show", 0.9f, 1.2f);
            yield return new WaitForSeconds(0.2f);
        }
    }


    private void OnAnswerSelected(int index)
    {
        var quiz = quizzes[currentQuizIndex];
        bool correct = index == quiz.correctIndex;

        if (quizRoutine != null)
            StopCoroutine(quizRoutine);

        if (correct)
        {
            currentPoints += pointsPerCorrect;
        }

        // go next quiz or end
        StartCoroutine(DelayShowQuiz());
    }

    private IEnumerator DelayShowQuiz()
    {
        yield return new WaitForSeconds(0.3f);
        currentQuizIndex++;
        if (currentQuizIndex < quizzes.Length)
        {
            ShowQuiz(currentQuizIndex);
        }
        else
        {
            EndQuiz();
        }
    }

    private IEnumerator QuizTimer()
    {
        float timer = timeLimit;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            timerText.text = $"Waktu sisa: {Mathf.CeilToInt(timer)}";
            yield return null;
        }

        // time out counts as wrong, but still go next
        currentQuizIndex++;
        if (currentQuizIndex < quizzes.Length)
        {
            ShowQuiz(currentQuizIndex);
        }
        else
        {
            EndQuiz();
        }
    }

    private void EndQuiz()
    {
        if (quizRoutine != null)
            StopCoroutine(quizRoutine);

        bool success = true;
        int reward = success ? currentPoints : 0;

        popupPanel.SetActive(true);
        var endScoreText = $"\n<color=#FFEE40> Kamu mendapatkan {currentPoints} poin!</color>";
        popupText.text = doneMessage + endScoreText;

        StartCoroutine(WaitForTap(() =>
        {
            popupPanel.SetActive(false);
            onQuizFinished.Invoke(success, reward);
        }));
    }

    private IEnumerator WaitForTap(System.Action onClose)
    {
        while (!Input.GetMouseButtonDown(0) && Input.touchCount == 0)
            yield return null;
        onClose?.Invoke();
    }

}
