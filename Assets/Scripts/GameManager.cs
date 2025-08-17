using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using StarterAssets;
using UnityEngine.SceneManagement; // <- Tambahkan ini

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Result UI")]
    public GameObject resultPanel;
    public TMP_Text resultText;
    public int totalRequiredNPC = 5;
    public Button retryButton;
    public Button nextLevelButton;

    [Header("Scene Settings")]
public string nextSceneName; // ← diisi di Inspector, contoh: "Level2"


    [Header("Player Reference")]
    public GameObject player;
    private ThirdPersonController playerController;

    private int completedNPCCount = 0;
    private int totalPoints = 0;
    private bool isShowingResult = false;

    void Awake()
    {
        Instance = this;
        resultPanel.SetActive(false);
    }

    void Start()
    {
        playerController = player.GetComponent<ThirdPersonController>();

        resultPanel.SetActive(false);
        retryButton.onClick.AddListener(RestartGame);
        nextLevelButton.onClick.AddListener(ProceedNextLevel);
    }

    void Update()
    {
        if (isShowingResult)
        {
            //Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playerController != null && playerController.enabled)
                playerController.enabled = false;
        }
    }

    public void AddPoint(int value)
    {
        totalPoints += value;
        completedNPCCount++;

        if (completedNPCCount >= totalRequiredNPC)
        {
            StartCoroutine(ShowResultWithDelay());
        }
    }

    private IEnumerator ShowResultWithDelay()
    {
        yield return new WaitForSeconds(3f);

        isShowingResult = true;
        resultPanel.SetActive(true);
        resultText.text = $"<b>Kesan Pertama Selesai!</b>\nKamu mendapatkan <b>{totalPoints} Poin Kesopanan</b>!\n\n" +
                          $"Siap lanjut ke Level 2: \"Manajemen Diri Saat Ujian\"?";
    }

    private void RestartGame()
{
    // Hapus skor
    PlayerPrefs.DeleteKey("Level1_Score");

    // Restart scene
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}


    private void ProceedNextLevel()
{
    Debug.Log("Lanjut ke Level 2...");

    // Simpan skor ke PlayerPrefs
    PlayerPrefs.SetInt("Level1_Score", totalPoints);
    PlayerPrefs.Save();

    // Pindah ke scene baru (dari Inspector)
    if (!string.IsNullOrEmpty(nextSceneName))
        SceneManager.LoadScene(nextSceneName);
    else
        Debug.LogWarning("Nama scene belum diatur di Inspector!");

    // (opsional) reset cursor state
    //Cursor.lockState = CursorLockMode.Locked;
    //Cursor.visible = false;
}

}
