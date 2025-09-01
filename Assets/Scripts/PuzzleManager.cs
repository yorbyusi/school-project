using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class PuzzleManager : MonoBehaviour
{
    [System.Serializable]
    public class PuzzlePieceData
    {
        public Sprite sprite;
        public int index; // index for correct slot
    }

    [Header("Puzzle Setup")]
    public PuzzlePieceData[] pieces;   // assign 12 sprites in inspector
    public PuzzlePiece piecePrefab;    // prefab with PuzzlePiece component
    public Transform pieceParent;      // left panel
    public PuzzleSlot slotPrefab;      // prefab with PuzzleSlot component
    public Transform slotParent;       // right panel (4x3 grid)

    [Header("UI")]
    public TMP_Text timerText;
    public float timeLimit = 60f;
    public UnityEvent<bool, int> onPuzzleFinished;

    [Header("Popup Settings")]
    public GameObject popupPanel;
    public TextMeshProUGUI popupText;
    [Multiline(3)]
    public string winMessage = "You Win!";
    [Multiline(3)]
    public string loseMessage = "Time’s Up!";


    private float timer;
    private int correctPlaced;
    private bool isFinished = false;

    public int MaxScore => 40;

    //private void Start()
    //{
    //    InitPuzzle();
    //}

    private void Update()
    {
        if (timer > 0 && isFinished == false)
        {
            timer -= Time.deltaTime;
            timerText.text = $"Waktu sisa: {Mathf.CeilToInt(timer).ToString()}";

            if (timer <= 0)
                EndPuzzle(false);
        }
    }

    public void InitPuzzle()
    {
        // reset
        foreach (Transform t in slotParent) Destroy(t.gameObject);
        foreach (Transform t in pieceParent) Destroy(t.gameObject);

        timer = timeLimit;

        // 1) Build slots in the CORRECT order 0..N-1
        for (int i = 0; i < pieces.Length; i++)
        {
            var slot = Instantiate(slotPrefab, slotParent);
            slot.slotIndex = i;
            slot.manager = this;
        }

        // 2) Build a shuffled list of indices for VISUAL order on the left
        List<int> indices = new List<int>(pieces.Length);
        for (int i = 0; i < pieces.Length; i++) indices.Add(i);
        Shuffle(indices);

        // 3) Spawn pieces in shuffled order, but assign their TRUE index from array
        foreach (int idx in indices)
        {
            var piece = Instantiate(piecePrefab, pieceParent);
            piece.pieceIndex = idx; // CORRECT: piece belongs to slot 'idx'
            var img = piece.GetComponent<Image>();
            if (img) img.sprite = pieces[idx].sprite;
        }
    }

    public void CheckAllSlots()
    {
        int correct = 0;

        for (int i = 0; i < slotParent.childCount; i++)
        {
            var slot = slotParent.GetChild(i).GetComponent<PuzzleSlot>();
            if (slot == null || slot.transform.childCount == 0) continue;

            var piece = slot.transform.GetChild(0).GetComponent<PuzzlePiece>();
            if (piece != null && piece.pieceIndex == slot.slotIndex)
                correct++;
        }

        if (correct >= pieces.Length)
            EndPuzzle(true);
    }


    public void CheckSlot(PuzzleSlot slot, PuzzlePiece piece)
    {
        if (slot.slotIndex == piece.pieceIndex)
        {
            correctPlaced++;
            if (correctPlaced >= pieces.Length)
                EndPuzzle(true);
        }
    }

    void EndPuzzle(bool success)
    {
        isFinished = true;
        int reward = 0;
        if (success)
        {
            reward = MaxScore;
        }
        else
        {
            // count partial points
            int correct = 0;
            for (int i = 0; i < slotParent.childCount; i++)
            {
                var slot = slotParent.GetChild(i).GetComponent<PuzzleSlot>();
                if (slot != null && slot.transform.childCount > 0)
                {
                    var piece = slot.transform.GetChild(0).GetComponent<PuzzlePiece>();
                    if (piece != null && piece.pieceIndex == slot.slotIndex)
                        correct += (MaxScore / pieces.Length);
                }
            }

            reward = correct;
            // fail if reward < 10 * pieces.Length
            //success = (reward >= pieces.Length * 10);
        }

        popupPanel.SetActive(true);
        popupText.text = success ? winMessage : loseMessage;

        StartCoroutine(WaitForTap(() =>
        {
            popupPanel.SetActive(false);
            onPuzzleFinished.Invoke(success, reward);
        }));
    }

    private IEnumerator WaitForTap(System.Action onClose)
    {
        while (!Input.GetMouseButtonDown(0) && Input.touchCount == 0)
            yield return null;
        onClose?.Invoke();
    }


    void Shuffle<T>(IList<T> list)
    {
        var rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}
