using System.Collections;
using UnityEngine;

public static class AppStartup
{

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitBeforeScene()
    {
        SpawnAudioManager();
    }

    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    //private static void InitAfterScene()
    //{

    //}

    private static void SpawnAudioManager()
    {
        var prefab = Resources.Load<GameObject>("AudioManager");
        Debug.Log($"AudioManager prefab: {prefab}");
        if (prefab != null && GameObject.FindObjectOfType<AudioManager>() == null)
        {
            GameObject.Instantiate(prefab);
        }
    }
}