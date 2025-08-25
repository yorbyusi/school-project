using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameFlowManager : MonoBehaviour
{
    [Header("Panel Selesai")]
    public GameObject endPanel;
    public TMP_Text finalScoreText;

    [Header("VFX Jawaban")]
    public GameObject vfxCorrect;
    public GameObject vfxWrong;


    [Header("Animator")]
    public Animator resultAnimator;

    private bool timeIsOut = false; // true jika waktu habis alami

    [Header("Materi awal")]
    public Button startAfterRead;
    public TMP_Text readText;
    public float typewriterSpeed = 0.05f;

    [Header("Panels")]
    public GameObject startPanel;
    public GameObject[] questionPanels;
    public GameObject emotionChoicePanel;

    [Header("UI")]
    public Button startButton;
    public TMP_InputField[] essayInputs;
    public Button nextFromEssayButton;

    [Header("Pilihan Ganda")]
    public Button[][] multipleChoiceButtons;
    // public int[][] multipleChoicePoints;

    [Header("Skor & Emosi")]
    public int totalScore = 0;
    public int maxScore;
    public TMP_Text scoreText;

    [Header("Emosi System")]
    public Image emotionBar;
    private float currentEmotion = 1f;

    [Header("Produktifitas & Timer")]
    public float addedTime = 0f; // total waktu tambahan karena pilihan emosi
    public TMP_Text timerText;
    private float remainingTime = 40f; // 5 menit default
    private bool timerRunning = false;

    [Header("Tombol Emosi")]
    public Button btnBernafas;
    public Button btnIstirahat;
    public Button btnLanjut;

    private int currentQuestion = 0;
    private int essayMinChars = 100; // jumlah huruf minimal

    [Header("Kunci Jawaban")]
    public int[] correctAnswers = new int[4]; // index 0–3 = soal 1–4

    public EndingPopup endingPopup;

    void Awake()
    {
        // Inisialisasi array tombol pilihan ganda
        multipleChoiceButtons = new Button[4][];
        correctAnswers = new int[4]; // index A=0, B=1, C=2, D=3

        // Set kunci jawaban (contoh)
        correctAnswers[0] = 1; // Soal 1 → A
        correctAnswers[1] = 0; // Soal 2 → C
        correctAnswers[2] = 0; // Soal 3 → B
        correctAnswers[3] = 2; // Soal 4 → D

        for (int i = 0; i < 4; i++)
        {
            Transform panel = questionPanels[i].transform;
            multipleChoiceButtons[i] = new Button[4];

            for (int j = 0; j < 4; j++)
            {
                multipleChoiceButtons[i][j] = panel.Find($"Button{j}").GetComponent<Button>();

                int capturedI = i;
                int capturedJ = j;

                multipleChoiceButtons[i][j].onClick.AddListener(() =>
                {
                    bool isCorrect = IsCorrectAnswer(capturedI, capturedJ);

                    if (isCorrect)
                    {
                        Debug.Log($"✅ Jawaban BENAR untuk Soal {capturedI + 1}");
                        if (resultAnimator != null)
                            resultAnimator.SetTrigger("Win");

                        PlayVFX(vfxCorrect);
                    }
                    else
                    {
                        Debug.Log($"❌ Jawaban SALAH untuk Soal {capturedI + 1}");
                        if (resultAnimator != null)
                            resultAnimator.SetTrigger("Lose");

                        PlayVFX(vfxWrong);
                    }



                    AddPoints(capturedI, capturedJ);
                    ShowEmotionChoice();
                });
            }
        }

        // Tombol emosi
        btnBernafas.onClick.AddListener(HandleBernafas);
        btnIstirahat.onClick.AddListener(HandleIstirahat);
        btnLanjut.onClick.AddListener(HandleLanjut);


    }

    private IEnumerator Typewriter(string message)
    {
        startAfterRead.interactable = false;
        readText.text = "";

        for (int i = 0; i < message.Length; i++)
        {
            readText.text += message[i];

            var playSfxThreshold = 2;
            if (i % playSfxThreshold == 0 && !char.IsWhiteSpace(message[i]))
            {
                AudioManager.Instance?.PlaySFX("beep-1", 0.9f, 1.2f);
            }

            yield return new WaitForSeconds(typewriterSpeed);
        }

        startAfterRead.interactable = true;
    }

    void Start()
    {
        nextFromEssayButton.gameObject.SetActive(false); // sembunyikan tombol saat awal
        // startPanel.SetActive(true);
        HideAllQuestionPanels();
        emotionChoicePanel.SetActive(false);

        startButton.onClick.AddListener(() =>
        {
            startPanel.SetActive(false);
            ShowCurrentQuestion();
            timerRunning = true;
        });

        // Tambahkan listener ke semua essay input
        foreach (var input in essayInputs)
        {
            input.onValueChanged.AddListener(delegate { CheckEssayInput(); });
        }

        nextFromEssayButton.onClick.AddListener(() => ShowEmotionChoice());

        nextFromEssayButton.interactable = false; // awalnya tidak aktif
        currentEmotion = 1f;
        emotionBar.fillAmount = currentEmotion;
        UpdateScoreUI();

        StartCoroutine(Typewriter(readText.text));
    }


    void Update()
    {
        if (timerRunning)
        {
            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0 && !timeIsOut)
            {
                remainingTime = 0;
                timerRunning = false;
                timeIsOut = true;
                Debug.Log("Waktu Habis!");
                EndExam();
            }



            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    void PlayVFX(GameObject vfx)
{
    if (vfx != null)
    {
        StartCoroutine(PlayAndDeactivate(vfx, 1f)); // 1 detik
    }
}

    IEnumerator PlayAndDeactivate(GameObject vfx, float duration)
    {
        vfx.SetActive(false); // reset
        vfx.SetActive(true);  // mainkan
        yield return new WaitForSeconds(duration);
        vfx.SetActive(false); // matikan lagi setelah selesai
    }


    void CheckEssayInput()
    {
        foreach (var input in essayInputs)
        {
            if (input.gameObject.activeSelf && input.text.Length >= essayMinChars)
            {
                nextFromEssayButton.interactable = true;
                return;
            }
        }
        nextFromEssayButton.interactable = false;
    }


    void AddPoints(int questionIndex, int answerIndex)
    {
        maxScore += 10;
        if (IsCorrectAnswer(questionIndex, answerIndex))
        {
            totalScore += 10; // Hanya jawaban benar yang mendapat 10 poin
            Debug.Log("Point +10 ditambahkan");
        }
        else
        {
            Debug.Log("Jawaban salah, tidak ada poin.");
        }

        UpdateScoreUI();
    }


    void ShowEmotionChoice()
    {
        if (currentQuestion < questionPanels.Length)
            questionPanels[currentQuestion].SetActive(false);

        emotionChoicePanel.SetActive(true);
        nextFromEssayButton.gameObject.SetActive(false);
    }

    void ContinueToNextQuestion()
{
    emotionChoicePanel.SetActive(false);
    currentQuestion++;

    ReduceEmotion(currentQuestion - 1);

    // ❗ Sembunyikan tombol essay saat berpindah soal
    nextFromEssayButton.gameObject.SetActive(false);

    // ❗ Stop timer ketika PG ke-4 selesai dijawab
    if (currentQuestion == 4)
    {
        timerRunning = false;
        Debug.Log("⏱️ Waktu dihentikan setelah 4 soal PG selesai.");
    }

    if (currentQuestion < questionPanels.Length)
    {
        ShowCurrentQuestion();
    }
    else
    {
        EndExam(); // langsung ke akhir
    }
}

    void EndExam()
    {
        timerRunning = false; // Stop timer

        HideAllQuestionPanels();
        emotionChoicePanel.SetActive(false);

        // 💡 Hitung skor dari jawaban essay
        CalculateEssayScore();

        if (endPanel != null) endPanel.SetActive(true);
        if (finalScoreText != null)
    {
        float totalTime = 40f + addedTime; // jika default awalnya 240 detik
        finalScoreText.text = $"Skor Akhir: {totalScore}\nWaktu Total: {totalTime:F0} detik (+{addedTime:F0}s)";
    }

}



    void ShowCurrentQuestion()
{
    if (currentQuestion < questionPanels.Length)
    {
        questionPanels[currentQuestion].SetActive(true);

        // Jika ini adalah soal Essay (index 4 atau 5)
        if (currentQuestion == 4 || currentQuestion == 5)
        {
            nextFromEssayButton.gameObject.SetActive(true); // tampilkan tombol
            nextFromEssayButton.interactable = false;        // tetap terkunci sampai 100 huruf
        }
        else
        {
            nextFromEssayButton.gameObject.SetActive(false); // sembunyikan di soal PG
        }
    }
}


    void ReduceEmotion(int questionIndex)
    {
        float amount = 0f;

        if (questionIndex <= 3) amount = 0.10f;
        else if (questionIndex == 4 || questionIndex == 5) amount = 0.30f;

        currentEmotion = Mathf.Clamp01(currentEmotion - amount);
        emotionBar.fillAmount = currentEmotion;
    }

    void UpdateScoreUI()
{
    if (scoreText != null)
    {
        float totalTime = 40f + addedTime; // 240 = 4 menit default
        scoreText.text = $"Skor: {totalScore}";
    }
}



    void HideAllQuestionPanels()
    {
        foreach (var panel in questionPanels)
            panel.SetActive(false);
    }

    // === HANDLER EMOSI ===
    void HandleBernafas()
{
    if (timeIsOut) return; // hanya blokir jika waktu habis alami
    currentEmotion = Mathf.Clamp01(currentEmotion + 0.10f);
    emotionBar.fillAmount = currentEmotion;
    UpdateScoreUI();
    ContinueToNextQuestion();
}



    void HandleIstirahat()
{
    if (timeIsOut) return;
    float added = 10f;
    remainingTime += added;
    addedTime += added;

    UpdateScoreUI();
    ContinueToNextQuestion();
}

    void HandleLanjut()
{
    if (timeIsOut) return;
    float added = 10f;
    remainingTime += added;
    addedTime += added;
    currentEmotion = Mathf.Clamp01(currentEmotion - 0.20f);
    emotionBar.fillAmount = currentEmotion;

    UpdateScoreUI();
    ContinueToNextQuestion();
}


    bool IsCorrectAnswer(int questionIndex, int selectedAnswerIndex)
    {
        return correctAnswers[questionIndex] == selectedAnswerIndex;
    }
    void CalculateEssayScore()
    {
        int totalChars = 0;

        foreach (var input in essayInputs)
        {
            totalChars += input.text.Length;
        }

        float rawPoints = totalChars * 0.1f;
        int essayPoints = Mathf.FloorToInt(rawPoints);

        totalScore += essayPoints;
        maxScore += essayPoints;
        Debug.Log($"📝 Total huruf: {totalChars} | Essay Point: {essayPoints}");

        UpdateScoreUI();
        endingPopup.Show(totalScore, maxScore);
    }


}
