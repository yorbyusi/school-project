using UnityEngine;
using UnityEngine.EventSystems;

public class PuzzleSlot : MonoBehaviour, IDropHandler
{
    public int slotIndex;
    public PuzzleManager manager;

    public void OnDrop(PointerEventData eventData)
    {
        var newPiece = eventData.pointerDrag?.GetComponent<PuzzlePiece>();
        if (newPiece == null) return;

        // If this slot already has a piece, swap instead of sending to pool
        if (transform.childCount > 0)
        {
            var oldPiece = transform.GetChild(0).GetComponent<PuzzlePiece>();
            if (oldPiece != null && oldPiece != newPiece)
            {
                // send old piece to where newPiece came from
                Transform oldParent = newPiece.originalParent; // where new piece was before
                oldPiece.SnapToSlot(oldParent);
            }
        }

        // finally snap the new piece into this slot
        newPiece.SnapToSlot(transform);

        Debug.Log($"Dropped piece {newPiece.pieceIndex} into slot {slotIndex}");
        manager.CheckAllSlots();
    }
}
