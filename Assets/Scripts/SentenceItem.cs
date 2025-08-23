using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SentenceItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Item UI")]
    [SerializeField] private Image background;               // optional, for styling
    [SerializeField] private TextMeshProUGUI sentenceText;   // required

    private SentenceMiniGame game;
    private RectTransform rect;
    private Transform originalParent;
    private RectTransform placeholder;
    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;

    private int originalIndex; // the correct position in final order (0..N-1)
    public int OriginalIndex => originalIndex;

    // --- Hysteresis config ---
    [SerializeField] private float moveThresholdFraction = 0.35f; // must move 35% of item height since last commit
    [SerializeField] private float neighborGateFraction = 0.15f;  // must cross 15% past neighbor center in the move direction

    // --- Hysteresis state ---
    private int lastCommittedIndex;
    private float lastCommitPointerY;
    private float itemHalfHeight;


    public void Setup(SentenceMiniGame game, int index, string sentence)
    {
        this.game = game;
        this.originalIndex = index;

        if (!rect) rect = GetComponent<RectTransform>();
        if (!canvasGroup)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (sentenceText) sentenceText.text = sentence;
        else Debug.LogWarning("[SentenceItem] Missing TextMeshProUGUI reference.");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!rect) rect = GetComponent<RectTransform>();
        originalParent = transform.parent;

        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null)
        {
            Debug.LogError("[SentenceItem] No Canvas found in parents.");
            return;
        }

        // Create placeholder with same size so the layout keeps space
        var placeholderGO = new GameObject("Placeholder", typeof(RectTransform), typeof(LayoutElement));
        placeholder = placeholderGO.GetComponent<RectTransform>();
        placeholder.SetParent(originalParent);

        // Copy size from this item
        var le = placeholderGO.GetComponent<LayoutElement>();
        var size = rect.rect.size;
        le.preferredWidth = size.x;
        le.preferredHeight = size.y;
        le.flexibleWidth = 0;
        le.flexibleHeight = 0;

        placeholder.SetSiblingIndex(transform.GetSiblingIndex());

        itemHalfHeight = GetComponent<RectTransform>().rect.height * 0.5f;
        lastCommittedIndex = placeholder.GetSiblingIndex();
        lastCommitPointerY = GetComponent<RectTransform>().position.y;

        // Move dragged item to top-level canvas space so it can float
        transform.SetParent(rootCanvas.transform, true);
        canvasGroup.blocksRaycasts = false; // so raycasts pass “through” while dragging

        //game?.SetLayoutEnabled(false); // freeze layout
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rect == null || placeholder == null || originalParent == null) return;

        // Follow pointer
        rect.position = eventData.position;
        float pointerY = rect.position.y;

        // 1) Movement hysteresis: don’t even consider reordering until we’ve moved enough since last commit
        float moveThreshold = itemHalfHeight * moveThresholdFraction;
        if (Mathf.Abs(pointerY - lastCommitPointerY) < moveThreshold)
            return;

        int count = originalParent.childCount;
        int targetIndex = lastCommittedIndex;
        bool movingUp = pointerY > lastCommitPointerY;

        // 2) Directional gate (Schmitt trigger): only cross into a neighbor once you’re well into its zone
        float gateOffset = itemHalfHeight * neighborGateFraction;

        if (movingUp)
        {
            // Walk upwards from current committed slot
            for (int i = lastCommittedIndex - 1; i >= 0; i--)
            {
                var child = originalParent.GetChild(i) as RectTransform;
                if (child == null) continue;

                float neighborCenterY = child.position.y;
                // require pointer to be ABOVE neighbor center + small offset
                if (pointerY > neighborCenterY + gateOffset)
                {
                    targetIndex = i;
                }
                else break; // didn’t clear this gate; stop climbing
            }
        }
        else // moving down
        {
            // Walk downwards from current committed slot
            for (int i = lastCommittedIndex + 1; i < count; i++)
            {
                var child = originalParent.GetChild(i) as RectTransform;
                if (child == null) continue;

                float neighborCenterY = child.position.y;
                // require pointer to be BELOW neighbor center - small offset
                if (pointerY < neighborCenterY - gateOffset)
                {
                    targetIndex = i;
                }
                else break; // didn’t clear this gate; stop descending
            }
        }

        // 3) Commit only when index actually changes; then reset our hysteresis anchor
        if (targetIndex != lastCommittedIndex)
        {
            placeholder.SetSiblingIndex(targetIndex);
            lastCommittedIndex = targetIndex;
            lastCommitPointerY = pointerY; // reset the “distance since last commit”
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (originalParent == null)
        {
            // Safety net if something weird happened
            Destroy(placeholder ? placeholder.gameObject : null);
            return;
        }

        // Put item back into layout at placeholder position
        int index = placeholder != null ? placeholder.GetSiblingIndex() : originalParent.childCount;
        transform.SetParent(originalParent, true);
        transform.SetSiblingIndex(index);

        // Clean up
        if (placeholder != null) Destroy(placeholder.gameObject);
        placeholder = null;

        canvasGroup.blocksRaycasts = true;

        // Let the game check correctness based on current child order
        game?.OnSentenceDropped();
        //game?.SetLayoutEnabled(true); // re-enable layout
    }
}
