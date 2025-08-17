using UnityEngine;
using System.Collections;

public class BidirectionalTeleportWithSwitch : MonoBehaviour
{
    [Header("Player")]
    public GameObject player;
    private CharacterController playerController;

    [Header("Teleport Settings")]
    public Transform linkedPortalPosition;
    public GameObject linkedPortalObject;     // GameObject portal tujuan
    public float teleportDelay = 3f;
    public KeyCode teleportKey = KeyCode.E;

    [Header("UI")]
    public GameObject teleportPromptUI;
    public GameObject loadingScreen;

    [Header("Camera")]
    public GameObject currentCamera;
    public GameObject targetCamera;

    private bool isTeleporting = false;
    private bool playerInTrigger = false;

    private void Start()
    {
        if (player != null)
        {
            playerController = player.GetComponent<CharacterController>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            playerInTrigger = true;
            teleportPromptUI?.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            playerInTrigger = false;
            teleportPromptUI?.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInTrigger && !isTeleporting && Input.GetKeyDown(teleportKey))
        {
            if (playerController != null)
                playerController.enabled = false;

            StartCoroutine(Teleport());
        }
    }

    private IEnumerator Teleport()
    {
        isTeleporting = true;
        teleportPromptUI?.SetActive(false);
        loadingScreen?.SetActive(true);
        // Kamera
        currentCamera?.SetActive(false);
        targetCamera?.SetActive(true);

        yield return new WaitForSeconds(teleportDelay);

        // Pindahkan posisi player
        if (linkedPortalPosition != null && player != null)
        {
            player.transform.position = linkedPortalPosition.position + Vector3.up * 0.5f;
            playerController.enabled = true;

            // Aktifkan portal tujuan
            if (linkedPortalObject != null)
                linkedPortalObject.SetActive(true);

            // Nonaktifkan portal ini
            gameObject.SetActive(false);

            Debug.Log("Teleported to: " + linkedPortalPosition.name);
        }
        else
        {
            Debug.LogWarning("Teleport gagal – linkedPortalPosition atau player belum diassign.");
        }
        // yield return new WaitForSeconds(teleportDelay);
        loadingScreen?.SetActive(false);
        isTeleporting = false;
    }
}
