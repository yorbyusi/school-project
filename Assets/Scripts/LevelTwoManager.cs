using System.Collections;
using UnityEngine;

public class LevelTwoManager : MonoBehaviour
{
    public SimplePopupText popupText;
    public EndingPopup endingPopup;

    [Multiline(5)]
    public string firstMessage;
    [Multiline(5)]
    public string secondMessage;
}