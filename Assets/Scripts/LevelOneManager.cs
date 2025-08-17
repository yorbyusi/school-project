using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelOneManager : MonoBehaviour
{
    public Button[] choiceButton;
    public GameObject[] teleportPos;
    public Transform playerTransform;

    private int indexChosen = 0;

    public void Start()
    {
        for (int i = 0; i < choiceButton.Length; i++)
        {
            int index = i; // important! capture local copy
            choiceButton[i].onClick.AddListener(() => OnButtonClicked(index));
        }
    }

    private void OnButtonClicked(int buttonIndex)
    {
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

}
