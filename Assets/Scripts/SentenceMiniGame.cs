using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SentenceMiniGame : MonoBehaviour
{
    [System.Serializable]
    public class SentenceData
    {
        [TextArea] public string sentence;
    }

    [Header("UI References")]
    [SerializeField] private RectTransform sentenceParent; // Panel with VerticalLayoutGroup
    [SerializeField] private SentenceItem sentencePrefab;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Settings")]
    [SerializeField] private float timeLimit = 30f;

    [Header("Database")]
    public List<SentenceData> sentences = new();

    [Header("Popup Settings")]
    public GameObject popupPanel;
    public TextMeshProUGUI popupText;
    public string winMessage = "You Win!";
    public string loseMessage = "Time’s Up!";

    public UnityEvent<bool> onMiniGameFinished = new(); // success on correct, fail on timeout

    private Coroutine timerRoutine;
    private VerticalLayoutGroup layoutGroup;
    private bool gameEnded = false;

    private void Awake()
    {
        layoutGroup = sentenceParent.GetComponent<VerticalLayoutGroup>();
    }

    public void SetLayoutEnabled(bool enabled)
    {
        if (layoutGroup != null)
            layoutGroup.enabled = enabled;
    }

    public void StartGame()
    {
        ClearOld();
        if (sentences == null || sentences.Count == 0)
        {
            Debug.LogWarning("[SentenceMiniGame] No sentences set.");
            onMiniGameFinished.Invoke(false);
            return;
        }

        SpawnSentencesShuffled();

        if (timerRoutine != null) StopCoroutine(timerRoutine);
        timerRoutine = StartCoroutine(TimerRoutine());
    }

    private void ClearOld()
    {
        if (sentenceParent == null) return;
        for (int i = sentenceParent.childCount - 1; i >= 0; i--)
            Destroy(sentenceParent.GetChild(i).gameObject);
    }

    private void SpawnSentencesShuffled()
    {
        // Build shuffled indices
        var indices = new List<int>(sentences.Count);
        for (int i = 0; i < sentences.Count; i++) indices.Add(i);
        indices.Shuffle();

        foreach (int originalIndex in indices)
        {
            var item = Instantiate(sentencePrefab, sentenceParent);
            item.Setup(this, originalIndex, sentences[originalIndex].sentence);
        }
    }

    public void OnSentenceDropped()
    {
        // run order check after 1 frame
        StartCoroutine(CheckOrderNextFrame());
    }

    private IEnumerator CheckOrderNextFrame()
    {
        yield return null; // wait 1 frame

        bool correct = true;

        for (int i = 0; i < sentenceParent.childCount; i++)
        {
            var item = sentenceParent.GetChild(i).GetComponent<SentenceItem>();
            if (item == null)
            {
                Debug.Log($" Slot {i} is null (probably placeholder just destroyed)");
                correct = false;
                break;
            }

            Debug.Log($"Slot {i} contains OriginalIndex={item.OriginalIndex}");

            if (item.OriginalIndex != i)
            {
                correct = false;
            }
        }

        if (correct)
        {
            Debug.Log(" Correct order achieved!");
            EndGame(true);
        }
        else
        {
            Debug.Log(" Order not correct yet.");
        }
    }



    private IEnumerator TimerRoutine()
    {
        float t = timeLimit;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            if (timerText) timerText.text = $"Time: {Mathf.CeilToInt(t)}";
            yield return null;
        }
        EndGame(false); // timeout
    }

    private void EndGame(bool success)
    {
        if (gameEnded) return;
        gameEnded = true;

        if (timerRoutine != null)
            StopCoroutine(timerRoutine);

        // Show popup
        popupPanel.SetActive(true);
        popupText.text = success ? winMessage : loseMessage;

        // Add listener for click anywhere
        StartCoroutine(WaitForTap(() =>
        {
            popupPanel.SetActive(false);
            onMiniGameFinished?.Invoke(success);
        }));
    }

    private IEnumerator WaitForTap(System.Action onClose)
    {
        bool tapped = false;
        while (!tapped)
        {
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            {
                tapped = true;
            }
            yield return null;
        }
        onClose?.Invoke();
    }
}

public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
