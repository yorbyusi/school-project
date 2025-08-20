using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public void Start()
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
    }

    private void SetVisibleButtonChoice(bool isVisible)
    {
        buttonChoice.SetActive(isVisible);
    }

    private void OnButtonClicked(int buttonIndex)
    {
        SetVisibleButtonChoice(false);
        indexChosen = buttonIndex;

        if (indexChosen >= 0 && indexChosen < teleportPos.Length)
        {
            var playerRb = playerTransform.GetComponent<Rigidbody>();
            //playerRb.velocity = Vector3.zero; // Reset velocity before teleporting
            //playerRb.MovePosition(teleportPos[indexChosen].transform.position); // Move player to the teleport position
            //playerRb.MoveRotation(teleportPos[indexChosen].transform.rotation); // Rotate player to the teleport position

            playerTransform.position = teleportPos[indexChosen].transform.position;
            playerTransform.rotation = teleportPos[indexChosen].transform.rotation;
        }

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
        yield return new WaitForSeconds(1f);
        buttonE.SetActive(true);
    }

    private void StartGame()
    {
        SetVisibleButtonChoice(false);
        popupText.ShowMessage(firstMessage);
        popupText.onHide.AddListener(ShowSecondDialog);
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
