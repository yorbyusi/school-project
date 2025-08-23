using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;

public class MiniGameManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform buttonParent;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private GameObject miniGameOverlay;
    [SerializeField] private GameObject sentenceOverlay;

    [Space(10)]
    [SerializeField] private QuizMiniGame quizMiniGame; // reference to quiz component
    [SerializeField] private SentenceMiniGame sentenceMiniGame;


    [Header("Settings")]
    [SerializeField] private int totalChoices = 3;
    [SerializeField] private int rewardPoints = 10;
    public string[] buttonText;

    private List<Button> choiceButtons = new();
    private int activeMiniGameIndex = -1;
    private int currentPoints = 0;

    private void Start()
    {
        GenerateButtons();
        miniGameOverlay.SetActive(false);

        quizMiniGame.onQuizFinished.AddListener(OnMiniGameFinished);
        sentenceMiniGame.onMiniGameFinished.AddListener(OnSentenceGameFinished);

    }

    private void GenerateButtons()
    {
        for (int i = 0; i < totalChoices; i++)
        {
            var button = Instantiate(buttonPrefab, buttonParent);
            int index = i; // capture for delegate
            button.onClick.AddListener(() => OnChoiceSelected(index));
            button.GetComponentInChildren<TextMeshProUGUI>().text = buttonText[i];
            choiceButtons.Add(button);
        }
    }

    private void OnChoiceSelected(int index)
    {
        if (!choiceButtons[index].interactable)
            return;

        activeMiniGameIndex = index;

        if (index == 0)
        {
            miniGameOverlay.SetActive(true);
            quizMiniGame.StartQuiz();
        }
        else if (index == 1)
        {
            sentenceOverlay.SetActive(true);
            sentenceMiniGame.StartGame();
        }

    }

    private void OnMiniGameFinished(bool success, int reward)
    {
        if (activeMiniGameIndex == -1) return;

        if (success)
        {
            currentPoints += reward;
            Debug.Log($"Mini game success! Total Points = {currentPoints}");
        }
        else
        {
            Debug.Log("Mini game failed.");
        }

        choiceButtons[activeMiniGameIndex].interactable = false;
        miniGameOverlay.SetActive(false);
        activeMiniGameIndex = -1;
    }

    private void OnSentenceGameFinished(bool success)
    {
        Debug.Log(success ? "Sentence game success!" : "Sentence game failed.");
        OnMiniGameFinished(success, success ? 10 : 0); // reuse manager’s flow
        sentenceOverlay.SetActive(false);
    }

}
