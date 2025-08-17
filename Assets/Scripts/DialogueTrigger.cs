using UnityEngine;
using StarterAssets;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueUI dialogueUI;
    public ThirdPersonController playerMovement;
    public GameObject uiButtonToHide;

    private bool playerInside = false;
    private bool dialogueStarted = false;
    private float originalMoveSpeed;

    private void Update()
    {
        if (playerInside && !dialogueStarted && Input.GetKeyDown(KeyCode.E))
        {
            if (dialogueUI != null && !dialogueUI.hasFinished)
            {
                dialogueStarted = true;
                dialogueUI.StartDialogue(OnDialogueComplete);

                if (playerMovement != null)
                {
                    originalMoveSpeed = playerMovement.MoveSpeed;
                    playerMovement.MoveSpeed = 0f;
                }

                if (uiButtonToHide != null)
                {
                    uiButtonToHide.SetActive(false);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }

    private void OnDialogueComplete()
    {
        dialogueStarted = false;

        if (playerMovement != null)
        {
            playerMovement.MoveSpeed = originalMoveSpeed;
        }
    }
}
