using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;

public class MiniGameManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform buttonParent;
    //[SerializeField] private Button buttonPrefab;
    public Button[] buttonChoices;

    [Space(10)]
    [SerializeField] private QuizMiniGame quizMiniGame;
    [SerializeField] private SentenceMiniGame sentenceMiniGame;
    [SerializeField] private PuzzleManager puzzleManager;


    [Header("Settings")]
    [SerializeField] private int totalChoices = 3;
    [SerializeField] private int rewardPoints = 10;
    public string[] buttonText;

    private List<Button> choiceButtons = new();
    private int activeMiniGameIndex = -1;
    private int currentPoints = 0;

    public int choicesPicked = 0;
    public EndingPopup endingPopup;

    private void Start()
    {
        GenerateButtons();
        quizMiniGame.gameObject.SetActive(false);

        quizMiniGame.onQuizFinished.AddListener(OnMiniGameFinished);
        sentenceMiniGame.onMiniGameFinished.AddListener(OnMiniGameFinished);
        puzzleManager.onPuzzleFinished.AddListener(OnMiniGameFinished);

    }

    private void GenerateButtons()
    {
        for (int i = 0; i < buttonChoices.Length; i++)
        {
            var button = buttonChoices[i];
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
        buttonParent.gameObject.SetActive(false);

        if (index == 0)
        {
            quizMiniGame.gameObject.SetActive(true);
            quizMiniGame.StartQuiz();
        }
        else if (index == 1)
        {
            sentenceMiniGame.gameObject.SetActive(true);
            sentenceMiniGame.StartGame();
        }
        else if (index == 2)
        {
            puzzleManager.gameObject.SetActive(true);
            puzzleManager.InitPuzzle();
        }

    }

    private void OnMiniGameFinished(bool success, int reward)
    {
        if (activeMiniGameIndex == -1) return;
        buttonParent.gameObject.SetActive(true);

        if (success)
        {
            currentPoints += reward;
            Debug.Log($"Mini game success! Total Points = {currentPoints}");
            choiceButtons[activeMiniGameIndex].interactable = false;
            choicesPicked++;
        }
        else
        {
            Debug.Log("Mini game failed. Button stays active for retry.");
        }

        activeMiniGameIndex = -1;

        // disable all mini games
        quizMiniGame.gameObject.SetActive(false);
        sentenceMiniGame.gameObject.SetActive(false);
        puzzleManager.gameObject.SetActive(false);

        if (choicesPicked >= totalChoices)
        {
            endingPopup?.ShowEnding($"Kamu telah menyelesaikan level ini.\nTotal Points: {currentPoints}", currentPoints, 50);
        }
    }

}
