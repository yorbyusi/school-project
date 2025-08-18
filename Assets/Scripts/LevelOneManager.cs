using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelOneManager : MonoBehaviour
{
    public Button[] choiceButton;
    public GameObject[] teleportPos;
    public Transform playerTransform;
    public DialogueUI[] dialogeUIs;

    private int indexChosen = 0;

    public SimplePopupText popupText;

    [Multiline(5)]
    public string firstMessage;
    [Multiline(5)]
    public string secondMessage;

    public GameObject buttonChoice;

    public void Start()
    {
        for (int i = 0; i < choiceButton.Length; i++)
        {
            int index = i; // important! capture local copy
            choiceButton[i].onClick.AddListener(() => OnButtonClicked(index));
        }

        foreach(var dialogeUI in dialogeUIs)
        {
            dialogeUI.onStartedCallback += () => SetVisibleButtonChoice(false);
            dialogeUI.onFinishCallback += () => SetVisibleButtonChoice(true);
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

        choiceButton[indexChosen].interactable = false;
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

}
