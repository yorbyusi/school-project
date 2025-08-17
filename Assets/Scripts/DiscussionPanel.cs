using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class DiscussionPromptUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text headerText;     // "Pimpin diskusi..."
    [SerializeField] private Button option1Button;
    [SerializeField] private Button option2Button;
    [SerializeField] private Button option3Button;
    [SerializeField] private Button submitButton;

    [Header("Option Labels (TMP) - optional; auto-find if left null")]
    [SerializeField] private TMP_Text option1Label;
    [SerializeField] private TMP_Text option2Label;
    [SerializeField] private TMP_Text option3Label;

    [Header("Content")]
    [TextArea] public string header = "Pimpin diskusi dengan memilih kata-kata yang tepat!";
    [TextArea] public string option1 = "Ayo, kita diskusikan dulu ide-ide kita!";
    [TextArea] public string option2 = "Siapa yang sudah selesai dengan tugasnya?";
    [TextArea] public string option3 = "Ada ide lain yang perlu dibicarakan?";
    public string submitText = "Kirim Pesan";

    [Header("Behavior")]
    public bool unlockCursorOnShow = true;
    public bool lockCursorOnSubmit = true;
    public bool hideAfterSubmit = true;

    [Header("Events")]
    public UnityEvent<string> onSubmit; // mengirim kalimat terpilih

    private int selectedIndex = -1;

    private void Awake()
    {
        if (option1Label == null && option1Button != null)
            option1Label = option1Button.GetComponentInChildren<TMP_Text>(true);
        if (option2Label == null && option2Button != null)
            option2Label = option2Button.GetComponentInChildren<TMP_Text>(true);
        if (option3Label == null && option3Button != null)
            option3Label = option3Button.GetComponentInChildren<TMP_Text>(true);

        if (option1Button) option1Button.onClick.AddListener(() => SelectOption(0));
        if (option2Button) option2Button.onClick.AddListener(() => SelectOption(1));
        if (option3Button) option3Button.onClick.AddListener(() => SelectOption(2));
        if (submitButton)  submitButton.onClick.AddListener(SubmitSelected);
    }

    private void OnEnable()
    {
        if (headerText) headerText.text = header;
        if (option1Label) option1Label.text = option1;
        if (option2Label) option2Label.text = option2;
        if (option3Label) option3Label.text = option3;

        if (submitButton)
        {
            var submitLbl = submitButton.GetComponentInChildren<TMP_Text>(true);
            if (submitLbl != null) submitLbl.text = submitText;
            submitButton.interactable = false;
        }

        selectedIndex = -1;
        SetButtonVisual(option1Button, false);
        SetButtonVisual(option2Button, false);
        SetButtonVisual(option3Button, false);

        if (unlockCursorOnShow)
        {
            //Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void SelectOption(int idx)
    {
        selectedIndex = idx;
        SetButtonVisual(option1Button, idx == 0);
        SetButtonVisual(option2Button, idx == 1);
        SetButtonVisual(option3Button, idx == 2);

        if (submitButton) submitButton.interactable = true;
    }

    private void SetButtonVisual(Button btn, bool selected)
    {
        if (btn == null) return;
        // Simple “selected” feel: matikan interaksi pada yang dipilih
        btn.interactable = !selected;
    }

    private void SubmitSelected()
    {
        if (selectedIndex < 0) return;

        string chosen = selectedIndex switch
        {
            0 => option1,
            1 => option2,
            2 => option3,
            _ => string.Empty
        };

        onSubmit?.Invoke(chosen);
        Debug.Log($"[DiscussionPromptUI] Submitted: {chosen}");

        if (hideAfterSubmit) gameObject.SetActive(false);

        if (lockCursorOnSubmit)
        {
            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;
        }
    }
}
