using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PuzzlePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int pieceIndex; // correct slot index
    [HideInInspector] public Transform originalParent;

    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private bool droppedOnSlot = false;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(originalParent.root); // move to top canvas
        droppedOnSlot = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (!droppedOnSlot) // if no slot accepted it
        {
            transform.SetParent(originalParent);
            rect.anchoredPosition = Vector2.zero;
        }
    }

    public void SnapToSlot(Transform slot)
    {
        droppedOnSlot = true;
        transform.SetParent(slot);

        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
    }

}
