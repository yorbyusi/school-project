using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

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

    [Multiline(3)]
    [SerializeField] private string[] questionQuizzes;
    [SerializeField] private EmojiSpawner emojiSpawner;

    private int currentQuizIndex = 0;

    [Header("Starters")]
    public SimplePopupText popupText;

    [Multiline(5)]
    public string firstMessage;
    [Multiline(5)]
    public string secondMessage;
    public GameObject yesNoPanel;
    public Button yesBtn;
    public Button noBtn;

    [Multiline(5)]
    public string thirdMessage;
    [Multiline(5)]
    public string fourthMessage;
    [Multiline(5)]
    public string fifthMessage;

    [Header("Visuals")]
    public GameObject[] characters;

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

    public void SetVisibleCharacters(bool visible)
    {
        if(visible)
        {
            StartCoroutine(ShowCharactersRoutine());
        }
        else
        {
            foreach (var character in characters)
            {
                character.SetActive(visible);
            }
        }
    }

    private IEnumerator ShowCharactersRoutine()
    {
        foreach (var character in characters)
        {
            character.SetActive(true);
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void ShowSecondDialog()
    {
        popupText.onHide.RemoveListener(ShowSecondDialog);

        popupText.shouldHide = false;
        popupText.ShowMessage(secondMessage);
        popupText.onComplete.AddListener(ShowSecondOption);
        //popupText.onComplete.AddListener(ShowChoiceButtons);
        //popupText.onHide.AddListener(ShowChoiceButtons);
    }

    private void ShowSecondOption()
    {
        SetVisibleCharacters(true);
        popupText.onComplete.RemoveListener(ShowSecondOption);

        yesNoPanel.gameObject.SetActive(true);
        yesBtn.onClick.AddListener(ShowThirdDialog);
        noBtn.onClick.AddListener(LoadToMenu);

        void LoadToMenu()
        {
            AudioManager.Instance.StopBGM();
            AudioManager.Instance.StopAmbience();
            SceneManager.LoadScene("MainMenu");
        }
    }

    private void ShowThirdDialog()
    {
        popupText.shouldHide = true;
        SetVisibleCharacters(false);
        yesNoPanel.gameObject.SetActive(false);

        popupText.onHide.RemoveListener(ShowThirdDialog);

        popupText.ShowMessage(thirdMessage);
        popupText.onHide.AddListener(ShowFourthDialog);
    }

    private void ShowFourthDialog()
    {
        popupText.onHide.RemoveListener(ShowFourthDialog);

        popupText.ShowMessage(fourthMessage);
        popupText.onHide.AddListener(ShowFifthDialog);
    }

    private void ShowFifthDialog()
    {
        popupText.onHide.RemoveListener(ShowFifthDialog);

        popupText.shouldHide = false;
        popupText.ShowMessage(fifthMessage);
        popupText.onComplete.AddListener(ShowChoiceButtons);
    }

    private void ShowChoiceButtons()
    {
        SetVisibleCharacters(true);
        popupText.onHide.RemoveListener(ShowChoiceButtons);

        popupText.canvas.interactable = false;
        popupText.canvas.blocksRaycasts = false;
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

    private void SetTextBasedOnQuizIndex(int index)
    {
        popupText.ShowMessage(questionQuizzes[index]);
    }

    private void OnChoiceSelected(int index)
    {
        if (!choiceButtons[index].interactable)
            return;

        if(index > currentQuizIndex)
        {
            emojiSpawner.SpawnEmoji("emoji_sad");
            AudioManager.Instance.PlaySFX("Sad");
            return;
        }

        activeMiniGameIndex = index;
        buttonParent.gameObject.SetActive(false);
        popupText.gameObject.SetActive(false);

        SetVisibleCharacters(false);

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
        Debug.Log($"Mini game finished. Success: {success}, Reward: {reward}");
        if (activeMiniGameIndex == -1) return;

        buttonParent.gameObject.SetActive(true);
        popupText.gameObject.SetActive(true);

        currentQuizIndex++;

        // index 1 can be repeated until success
        if (activeMiniGameIndex != 1 || success)
            choiceButtons[activeMiniGameIndex].interactable = false;

        choicesPicked++;
        currentPoints += reward;
        Debug.Log($"Reward from mini game: {reward} -- {currentPoints}");

        SetVisibleCharacters(true);

        if (success)
        {
            if (currentQuizIndex < questionQuizzes.Length)
                SetTextBasedOnQuizIndex(currentQuizIndex);
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
