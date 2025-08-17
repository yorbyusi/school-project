using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneControlWithScore : MonoBehaviour
{
    [Header("Button References")]
    public Button restartButton;
    public Button nextSceneButton;

    [Header("Scene Settings")]
    public string nextSceneName;

    [Header("Game Manager Reference")]
    public GameFlowManager gameFlowManager;

    void Start()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartScene);

        if (nextSceneButton != null)
            nextSceneButton.onClick.AddListener(GoToNextScene);
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void GoToNextScene()
    {
        if (gameFlowManager != null && !string.IsNullOrEmpty(nextSceneName))
        {
            int score = gameFlowManager.totalScore;
            int timeBonus = Mathf.FloorToInt(gameFlowManager.addedTime); // ✅ FIX: Convert float to int safely
            int average = Mathf.FloorToInt(score);

            PlayerPrefs.SetInt("Level2_Score", average);
            PlayerPrefs.Save();
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
