using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LevelOneManager : MonoBehaviour
{
    public bool isSequential = false;
    public Button[] choiceButton;
    public GameObject[] teleportPos;
    public Transform playerTransform;
    public DialogueUI[] dialogeUIs;

    private int indexChosen = 0;

    public SimplePopupText popupText;
    public EndingPopup endingPopup;

    [Multiline(5)]
    public string firstMessage;
    [Multiline(5)]
    public string secondMessage;

    public int maxScore = 40;
    public int currentScore = 0;

    public int needAnswered = 4;
    public int currentAnswered = 0;

    public GameObject buttonChoice;
    public GameObject buttonE;
    public CanvasGroup blackOverlay;

    public string bgmName = "bgm-playful";

    public IEnumerator Start()
    {

        currentScore = 0;
        currentAnswered = 0;

        if (isSequential)
        {
            for (int i = 0; i < choiceButton.Length; i++)
            {
                choiceButton[i].interactable = false;
            }
            choiceButton[0].interactable = true;
        }
        else
        {
            for (int i = 0; i < choiceButton.Length; i++)
            {
                choiceButton[i].interactable = true;
            }
        }

            for (int i = 0; i < choiceButton.Length; i++)
        {
            int index = i; // important! capture local copy
            choiceButton[i].onClick.AddListener(() => OnButtonClicked(index));
        }

        foreach(var dialogeUI in dialogeUIs)
        {
            dialogeUI.onStartedCallback += () => SetVisibleButtonChoice(false);
            dialogeUI.onFinishCallback += () => SetVisibleButtonChoice(true);
            dialogeUI.onPointAdded += AddAnswered;
        }

        StartGame();

        yield return new WaitForSeconds(0.8f);
        AudioManager.Instance.PlayBGM(bgmName);
    }

    private void SetVisibleButtonChoice(bool isVisible)
    {
        buttonChoice.SetActive(isVisible);
        if(isVisible)
            popupText.Show();
        else
            popupText.HideMessage();
    }

    private void OnButtonClicked(int buttonIndex)
    {
        SetVisibleButtonChoice(false);
        indexChosen = buttonIndex;

        if (indexChosen >= 0 && indexChosen < teleportPos.Length)
        {
        }

        blackOverlay.DOFade(1f, 0.3f).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                var playerRb = playerTransform.GetComponent<Rigidbody>();
                playerTransform.position = teleportPos[indexChosen].transform.position;
                playerTransform.rotation = teleportPos[indexChosen].transform.rotation;
            });

        StartCoroutine(DelaySpawnPressE());
        choiceButton[indexChosen].interactable = false;

        if(isSequential )
        {
            // make next index button interactable
            if (indexChosen + 1 < choiceButton.Length)
            {
                if(choiceButton[indexChosen + 1] != null)
                {
                    choiceButton[indexChosen + 1].interactable = true;
                }
            }
        }
    }

    private IEnumerator DelaySpawnPressE()
    {
        yield return new WaitForSeconds(1.8f);
        buttonE.SetActive(true);
        blackOverlay.DOFade(0f, 0.7f).SetEase(Ease.OutCirc);
    }

    private void StartGame()
    {
        SetVisibleButtonChoice(false);
        popupText.ShowMessage(firstMessage);
        //popupText.onHide.AddListener(ShowSecondDialog);
        popupText.onComplete.AddListener(OnComplete);
    }

    private void OnComplete()
    {
        SetVisibleButtonChoice(true);
    }

    private void ShowSecondDialog()
    {
        popupText.onHide.RemoveListener(ShowSecondDialog);
        popupText.ShowMessage(secondMessage);
        popupText.onHide.AddListener(ShowChoiceButtons);
    }

    private void ShowChoiceButtons()
    {
        popupText.onHide.RemoveListener(ShowChoiceButtons);
        SetVisibleButtonChoice(true);
        popupText.HideMessage();
    }

    public void AddAnswered(int point)
    {
        currentScore += point;
        currentAnswered++;

        if(currentAnswered >= needAnswered)
        {
            endingPopup.Show(currentScore, maxScore);
        }
    }
}
