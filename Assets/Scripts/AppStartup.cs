using System.Collections;
using UnityEngine;

public static class AppStartup
{

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitBeforeScene()
    {
        SpawnAudioManager();
        SpawnGameState();
    }

    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    //private static void InitAfterScene()
    //{

    //}

    private static void SpawnAudioManager()
    {
        var prefab = Resources.Load<GameObject>("AudioManager");
        Debug.Log($"AudioManager prefab: {prefab}");
        if (prefab != null)
        {
            GameObject.Instantiate(prefab);
        }
    }

    private static void SpawnGameState()
    {
        var prefab = Resources.Load<GameObject>("GameState");
        Debug.Log($"GameState prefab: {prefab}");
        if (prefab != null)
        {
            GameObject.Instantiate(prefab);
        }
    }
}