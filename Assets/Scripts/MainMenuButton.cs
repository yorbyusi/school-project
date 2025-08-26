using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuButton : MonoBehaviour
{
    public Button button;
    public TMP_Text lastScoreText;

    public void SetLastScore(int score, int maxScore)
    {
        lastScoreText.text = $"Skor terkahir: {score}/{maxScore}";
    }
}