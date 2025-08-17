using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class JobSelectionUI : MonoBehaviour
{
    [Header("UI Panel Pemilihan Job")]
    public GameObject uiPanel;
    public Button menulisButton;
    public Button menggambarButton;
    public Button mencariInfoButton;
    public TMP_Text jobText; // teks status di panel (TMP)

    [Header("Reset & Submit")]
    public Button resetButton;
    public Button submitButton;

    [System.Serializable]
    public struct AssignedJobData
    {
        public string npcName;
        public string job;

        public AssignedJobData(string npcName, string job)
        {
            this.npcName = npcName;
            this.job = job;
        }
    }

    [Header("Prefabs untuk Job")]
    public GameObject prefabMenulis;
    public GameObject prefabMenggambar;
    public GameObject prefabMencariInfo;

    // Mapping NPC -> Job
    private readonly Dictionary<JobSelectorTriggerTMP, string> assignedJobs = new();
    // Snapshot final setelah submit
    public List<AssignedJobData> finalAssignedJobs = new();

    [Header("Toggle Objects Saat Submit")]
    public GameObject PlayerObject;  // off setelah submit
    public GameObject PlayerAvatar;  // on setelah submit

    [Header("Animator Player (Trigger: Talking)")]
    public Animator playerAnimator;
    public Transform headOverride; // tidak dipakai di versi Canvas-only, tetap disimpan jika butuh nanti

    [Header("UI Diskusi (muncul setelah submit)")]
    public DiscussionPromptUI discussionUI;      // drag panel UI diskusi
    public string finalDiscussionLine = "";      // hasil pilihan dialog

    [Header("Speech Bubble UI (Canvas TMP)")]
    public GameObject speechBubbleUI;            // Panel/objek TMP di Canvas (setActive(false) di awal)
    public TMP_Text speechBubbleText;            // Komponen TMP untuk menampilkan kalimat
    public bool autoHideSpeech = true;
    public float speechHideDelay = 3f;

    private JobSelectorTriggerTMP currentTarget;
    private readonly HashSet<string> usedJobs = new(); // job global yang sudah dipakai (unik)

    private void Start()
    {
        if (menulisButton != null)       menulisButton.onClick.AddListener(() => OnJobSelected("Menulis", prefabMenulis));
        if (menggambarButton != null)    menggambarButton.onClick.AddListener(() => OnJobSelected("Menggambar", prefabMenggambar));
        if (mencariInfoButton != null)   mencariInfoButton.onClick.AddListener(() => OnJobSelected("Mencari Informasi", prefabMencariInfo));
        if (submitButton != null)        submitButton.onClick.AddListener(SubmitAssignedJobs);
        if (resetButton != null)         resetButton.onClick.AddListener(ResetAllJobs);

        if (uiPanel != null) uiPanel.SetActive(false);
        if (speechBubbleUI != null) speechBubbleUI.SetActive(false);

        // Dengarkan hasil submit diskusi
        if (discussionUI != null)
        {
            discussionUI.gameObject.SetActive(false); // pastikan tersembunyi dulu
            discussionUI.onSubmit.AddListener(HandleDiscussionSubmitted);
        }
    }

    public void SetCurrentTarget(JobSelectorTriggerTMP target)
    {
        currentTarget = target;
        if (uiPanel != null) uiPanel.SetActive(true);

        string targetJob = assignedJobs.ContainsKey(target) ? assignedJobs[target] : null;

        if (jobText != null)
            jobText.text = string.IsNullOrEmpty(targetJob)
                ? "Pilihlah pekerjaan untuk temanmu ini."
                : ("Job: " + targetJob);

        if (menulisButton != null)
            menulisButton.interactable = !usedJobs.Contains("Menulis") || targetJob == "Menulis";
        if (menggambarButton != null)
            menggambarButton.interactable = !usedJobs.Contains("Menggambar") || targetJob == "Menggambar";
        if (mencariInfoButton != null)
            mencariInfoButton.interactable = !usedJobs.Contains("Mencari Informasi") || targetJob == "Mencari Informasi";
    }

    public void ClearTarget()
    {
        currentTarget = null;
        if (uiPanel != null) uiPanel.SetActive(false);
    }

    private void OnJobSelected(string jobName, GameObject prefab)
    {
        if (jobText != null) jobText.text = "Job: " + jobName;

        if (currentTarget != null)
        {
            // Bebaskan job lama jika NPC ini sudah punya
            if (assignedJobs.TryGetValue(currentTarget, out var previousJob))
                usedJobs.Remove(previousJob);

            // Assign prefab & simpan job baru
            currentTarget.AssignPrefab(prefab, jobName);
            usedJobs.Add(jobName);
            assignedJobs[currentTarget] = jobName;
        }

        if (uiPanel != null) uiPanel.SetActive(false);
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    private void ResetAllJobs()
    {
        // Hapus prefab dari tiap NPC
        foreach (var kvp in assignedJobs)
        {
            var target = kvp.Key;
            var jobName = kvp.Value;

            var existing = target.transform.Find(jobName + "_Instance");
            if (existing != null) Destroy(existing.gameObject);
        }

        assignedJobs.Clear();
        usedJobs.Clear();

        if (currentTarget != null)
            SetCurrentTarget(currentTarget);

        if (jobText != null)
            jobText.text = "Job: -";

        finalDiscussionLine = "";

        // Sembunyikan UI Diskusi & Speech
        if (discussionUI != null) discussionUI.gameObject.SetActive(false);
        if (speechBubbleUI != null) speechBubbleUI.SetActive(false);

        Debug.Log("✅ Semua job telah di-reset.");
    }

    private void SubmitAssignedJobs()
    {
        // Pastikan 3 job terisi (unik)
        if (assignedJobs.Count < 3)
        {
            Debug.LogWarning("🚫 Belum semua teman diberi pekerjaan.");
            return;
        }

        // Snapshot hasil
        finalAssignedJobs.Clear();
        foreach (var kvp in assignedJobs)
        {
            var target = kvp.Key;
            var job = kvp.Value;
            finalAssignedJobs.Add(new AssignedJobData(target.name, job));
            Debug.Log($"📝 Submit: {target.name} = {job}");
        }

        // Kunci pemilihan
        if (menulisButton)     menulisButton.interactable = false;
        if (menggambarButton)  menggambarButton.interactable = false;
        if (mencariInfoButton) mencariInfoButton.interactable = false;
        if (resetButton)       resetButton.interactable = false;
        if (submitButton)      submitButton.interactable = false;

        if (uiPanel != null) uiPanel.SetActive(false);
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        OnSubmitJobsComplete();
    }

    private void OnSubmitJobsComplete()
    {
        Debug.Log("🎉 Semua pekerjaan telah dikirim. Lanjutkan ke proses selanjutnya.");

        // Toggle objek scene
        if (PlayerObject != null) PlayerObject.SetActive(false);
        if (PlayerAvatar != null) PlayerAvatar.SetActive(true);

        // Munculkan UI diskusi
        if (discussionUI != null)
            discussionUI.gameObject.SetActive(true);
    }

    // Dipanggil saat tombol "Kirim Pesan" di DiscussionPromptUI ditekan
    private void HandleDiscussionSubmitted(string chosenLine)
    {
        finalDiscussionLine = chosenLine;
        Debug.Log($"📣 Discussion Submitted: {finalDiscussionLine}");

        // 1) Trigger anim Talking
        if (playerAnimator != null)
            playerAnimator.SetTrigger("Talking");

        // 2) Tampilkan TMP di Canvas
        if (speechBubbleUI != null && speechBubbleText != null)
        {
            speechBubbleUI.SetActive(true);
            speechBubbleText.text = finalDiscussionLine;

            if (autoHideSpeech)
                StartCoroutine(HideSpeechBubbleAfterDelay(speechHideDelay));
        }
    }

    private IEnumerator HideSpeechBubbleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (speechBubbleUI != null)
            speechBubbleUI.SetActive(false);
    }
}
