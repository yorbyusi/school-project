using BrunoMikoski.AnimationsSequencer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmojiSpawner : MonoBehaviour
{
    public EmojiData[] emoji;

    public Image[] emojiImage;
    public AnimationSequence[] showSequence;

    public void SpawnEmoji(string name)
    {
        // stop all before playing new
        foreach (var seq in showSequence)
        {
            seq.Kill(true);
        }

        Debug.Log("Trying to spawn emoji: " + name);
        for (int i = 0; i < emoji.Length; i++)
        {
            Debug.Log("Checking emoji: " + emoji[i].emojiName);
            if (emoji[i].emojiName == name)
            {
                for (int j = 0; j < emojiImage.Length; j++)
                {
                    Debug.Log("Checking image slot " + j);

                    emojiImage[j].sprite = emoji[i].emojiSprite;
                    Debug.Log("Spawned Emoji: " + name);
                    showSequence[j].Play();
                }
            }
        }

    }
}

[System.Serializable]
public class EmojiData
{
    public string emojiName;
    public Sprite emojiSprite;
}