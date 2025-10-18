using System.Collections;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    
    private string ScoreKey(int level) => $"SekolahLevel{level}";
    private string MaxScoreKey(int level) => $"SekolahLevel{level}_Max";

    public int GetScore(int level) => PlayerPrefs.GetInt(ScoreKey(level), 0);
    public int GetMaxScore(int level) => PlayerPrefs.GetInt(MaxScoreKey(level), 100);

    public void SetScore(int level, int latestScore, int maxScore)
    {
        Debug.Log($"[GameState] Setting score for level {level}: {latestScore}/{maxScore}");
        int currentMax = GetScore(level);
        PlayerPrefs.SetInt(ScoreKey(level), latestScore);
        PlayerPrefs.SetInt(MaxScoreKey(level), maxScore);
        PlayerPrefs.Save();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

    }
}