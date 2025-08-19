using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text pointText;
    public Button[] optionButtons;
    public TMP_Text[] optionTexts;
    public TMP_Text npcNameText; // Assign this via Inspector
    public string npcName;       // Set per NPC via Inspector
    [Header("Expression VFX")]
    public GameObject angryVFX;
    public GameObject smileVFX;
    public GameObject shyVFX;


    [Header("Dialogue Options")]
    public string NPCDialog;
    public string[] options = new string[4];
    public string[] answers = new string[4];
    public string[] expressions = new string[4];
    public int[] points = new int[4];

    public BlendShapeController faceController;

    public bool hasFinished = false;

    public System.Action onFinishCallback;
    public System.Action onStartedCallback;
    public System.Action<int> onPointAdded;

    private void Start()
    {
        foreach (var btn in optionButtons)
        {
            btn.gameObject.SetActive(false);
        }
    }

    public void StartDialogue(System.Action callback)
    {
        if (hasFinished) return;
        npcNameText.text = npcName;
        onFinishCallback += callback;
        onStartedCallback?.Invoke();
        dialoguePanel.SetActive(true);
        dialogueText.text = NPCDialog;

        for (int i = 0; i < options.Length; i++)
        {
            optionTexts[i].text = options[i];
            int index = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => OnOptionSelected(index));
            optionButtons[i].gameObject.SetActive(true);
        }

        //Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnOptionSelected(int index)
    {
        int point = points[index];
        string response = answers[index];
        string expression = expressions[index];

        dialogueText.text = response;

        foreach (var btn in optionButtons)
            btn.gameObject.SetActive(false);

        PlayExpression(expression);
        hasFinished = true;

        GameManager.Instance.AddPoint(point);
        StartCoroutine(ShowPointAndClose(point));
    }

    private IEnumerator ShowPointAndClose(int point)
    {
        onPointAdded?.Invoke(point);
        pointText.text = $"Anda mendapatkan {(point >= 0 ? "+" : "")}{point} Poin Kesopanan!";
        yield return new WaitForSeconds(2f);

        dialoguePanel.SetActive(false);
        pointText.text = "";

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        onFinishCallback?.Invoke();
    }

    private void PlayExpression(string expression)
{
    if (faceController == null) return;

    switch (expression.ToLower())
    {
        case "smile":
            faceController.Smile();
            if (smileVFX != null) StartCoroutine(PlayVFX(smileVFX));
            break;
        case "angry":
            faceController.Angry();
            if (angryVFX != null) StartCoroutine(PlayVFX(angryVFX));
            break;
        case "shy":
            faceController.Shy();
            if (shyVFX != null) StartCoroutine(PlayVFX(shyVFX));
            break;
        case "clear":
            faceController.ClearAll();
            break;
    }
}
private IEnumerator PlayVFX(GameObject vfx)
{
    vfx.SetActive(true);
    yield return new WaitForSeconds(1f);
    vfx.SetActive(false);
}


}
