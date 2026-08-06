using UnityEngine;
using UnityEngine.InputSystem; // Bắt buộc cho New Input System

public class PedestalTrigger : MonoBehaviour
{
    public enum PedestalType { BluePedestal, RedPedestal }

    [Header("LOẠI BỆ ĐÁ")]
    public PedestalType pedestalType;

    [Header("THAM CHIẾU PUZZLE MANAGER")]
    [SerializeField] private StoneDoorPuzzle puzzleManager;

    private bool isPlayerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;

            if (pedestalType == PedestalType.BluePedestal)
            {
                if (StonePickup.HasBlueStone) Debug.Log("[GỢI Ý] Nhấn [E] để đặt Đá Xanh");
                else Debug.Log("[GỢI Ý] Cần tìm Đá Xanh...");
            }
            else if (pedestalType == PedestalType.RedPedestal)
            {
                if (StonePickup.HasRedStone) Debug.Log("[GỢI Ý] Nhấn [E] để đặt Đá Đỏ");
                else Debug.Log("[GỢI Ý] Cần tìm Đá Đỏ...");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }

    private void Update()
    {
        // Bắt phím E bằng New Input System
        if (isPlayerInside && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (pedestalType == PedestalType.BluePedestal)
            {
                puzzleManager.TryPlaceBlueStone();
            }
            else if (pedestalType == PedestalType.RedPedestal)
            {
                puzzleManager.TryPlaceRedStone();
            }
        }
    }
}