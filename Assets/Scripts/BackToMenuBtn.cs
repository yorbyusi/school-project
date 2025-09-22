using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenuBtn : MonoBehaviour
{
    void Start()
    {
        var btn = GetComponent<UnityEngine.UI.Button>();

        btn.onClick.AddListener(LoadToMenu);

        void LoadToMenu()
        {
            AudioManager.Instance.StopBGM();
            AudioManager.Instance.StopAmbience();
            SceneManager.LoadScene("MainMenu");
        }
    }
}
