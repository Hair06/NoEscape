using UnityEngine;

public class PedestalTrigger : MonoBehaviour, IInteractable
{
    public enum PedestalType
    {
        BluePedestal,
        RedPedestal,
    }

    [Header("Loại bệ đá")]
    [SerializeField] private PedestalType pedestalType;

    [Header("Tham chiếu câu đố")]
    [SerializeField] private StoneDoorPuzzle puzzleManager;

    public string GetInteractPrompt()
    {
        if (puzzleManager == null ||
            !puzzleManager.IsPlacementStepActive())
        {
            return "";
        }

        if (pedestalType == PedestalType.BluePedestal)
        {
            if (puzzleManager.IsBluePlaced)
            {
                return "";
            }

            return StonePickup.HasBlueStone
                ? "Nhấn [E] để đặt Đá Xanh"
                : "Cần Đá Xanh cho bệ này";
        }

        if (puzzleManager.IsRedPlaced)
        {
            return "";
        }

        return StonePickup.HasRedStone
            ? "Nhấn [E] để đặt Đá Đỏ"
            : "Cần Đá Đỏ cho bệ này";
    }

    public void Interact()
    {
        if (puzzleManager == null ||
            !puzzleManager.IsPlacementStepActive())
        {
            return;
        }

        if (pedestalType == PedestalType.BluePedestal)
        {
            puzzleManager.TryPlaceBlueStone();
        }
        else
        {
            puzzleManager.TryPlaceRedStone();
        }
    }
}
