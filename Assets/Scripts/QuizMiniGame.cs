using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class QuizMiniGame : MonoBehaviour
{
    [System.Serializable]
    public class QuizData
    {
        public string question;
        public List<string> answers;
        public int correctIndex;
    }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Transform answersParent; // must have GridLayoutGroup
    [SerializeField] private Button answerPrefab;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Quiz Settings")]
    [SerializeField] private float timeLimit = 10f;
    [SerializeField] private int pointsPerCorrect = 5;

    [Header("Quiz Database")]
    public QuizData[] quizzes;

    // success always true now (game ends after last Q), int = total points earned
    public UnityEvent<bool, int> onQuizFinished = new();

    private int currentQuizIndex = 0;
    private int currentPoints = 0;
    private Coroutine quizRoutine;

    public void StartQuiz()
    {
        if (quizzes == null || quizzes.Length == 0)
        {
            Debug.LogWarning("No quizzes assigned!");
            onQuizFinished.Invoke(false, 0);
            return;
        }

        currentPoints = 0;
        currentQuizIndex = 0;
        ShowQuiz(currentQuizIndex);
    }

    private void ShowQuiz(int index)
    {
        var quiz = quizzes[index];

        questionText.text = quiz.question;

        // clear old answers
        foreach (Transform child in answersParent)
            Destroy(child.gameObject);

        for (int i = 0; i < quiz.answers.Count; i++)
        {
            int choiceIndex = i;
            var btn = Instantiate(answerPrefab, answersParent);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = quiz.answers[i];
            btn.onClick.AddListener(() => OnAnswerSelected(choiceIndex));
        }

        // restart timer
        if (quizRoutine != null) StopCoroutine(quizRoutine);
        quizRoutine = StartCoroutine(QuizTimer());
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
            timerText.text = $"Time: {Mathf.CeilToInt(timer)}";
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

        // success always true, since we always finish all Qs
        onQuizFinished.Invoke(true, currentPoints);
    }
}
