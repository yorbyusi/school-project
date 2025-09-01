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


    [Header("Starters")]
    public SimplePopupText popupText;

    [Multiline(5)]
    public string firstMessage;
    [Multiline(5)]
    public string secondMessage;

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
        StartGame();
        GenerateButtons();
        quizMiniGame.gameObject.SetActive(false);

        quizMiniGame.onQuizFinished.AddListener(OnMiniGameFinished);
        sentenceMiniGame.onMiniGameFinished.AddListener(OnMiniGameFinished);
        puzzleManager.onPuzzleFinished.AddListener(OnMiniGameFinished);

    }

    private void StartGame()
    {
        buttonParent.gameObject.SetActive(false);
        popupText.ShowMessage(firstMessage);
        popupText.onHide.AddListener(ShowSecondDialog);
    }

    private void ShowSecondDialog()
    {
        popupText.onHide.RemoveListener(ShowSecondDialog);

        popupText.shouldHide = false;
        popupText.ShowMessage(secondMessage);
        popupText.onComplete.AddListener(ShowChoiceButtons);
        //popupText.onHide.AddListener(ShowChoiceButtons);
    }

    private void ShowChoiceButtons()
    {
        popupText.onHide.RemoveListener(ShowChoiceButtons);
        //popupText.HideMessage();
        buttonParent.gameObject.SetActive(true);
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
        popupText.gameObject.SetActive(false);

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
        popupText.gameObject.SetActive(true);

        choiceButtons[activeMiniGameIndex].interactable = false;
        choicesPicked++;
        currentPoints += reward;
        Debug.Log($"Reward from mini game: {reward} -- {currentPoints}");

        if (success)
        {
            //currentPoints += reward;
            //choiceButtons[activeMiniGameIndex].interactable = false;
            //choicesPicked++;
            Debug.Log($"Mini game success! Total Points = {currentPoints}");
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
            endingPopup?.Show(currentPoints, 100);
        }
    }

}
