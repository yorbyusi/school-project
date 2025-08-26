using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button level1Btn;
    public Button level2Btn;
    public Button level3Btn;
    public Button level4Btn;
    public Button level5Btn;
    public MainMenuButton[] buttons;

    private void Start()
    {
        string prefix = "SekolahLevel";
        int level = 1;
        foreach (var btn in buttons)
        {
            int cacheLevel = level;
            btn.button.onClick.AddListener(() => OpenScene(prefix + cacheLevel));
            int score = GameState.Instance.GetScore(cacheLevel);
            int maxScore = GameState.Instance.GetMaxScore(cacheLevel);
            btn.SetLastScore(score, maxScore);
            level++;
        }

    }

    private void OpenScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
