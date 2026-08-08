using UnityEngine;
using UnityEngine.InputSystem; // Bắt buộc cho New Input System

public enum StoneType { Blue, Red }

public class StonePickup : MonoBehaviour
{
    [Header("LOẠI ĐÁ")]
    public StoneType stoneType;

    [Header("ÂM THANH NHẶT")]
    [SerializeField] private AudioSource pickupSound;

    public static bool HasBlueStone { get; set; } = false;
    public static bool HasRedStone { get; set; } = false;

    private bool isPlayerNearby = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            string stoneName = (stoneType == StoneType.Blue) ? "Đá Xanh" : "Đá Đỏ";
            Debug.Log($"[GỢI Ý] Nhấn [E] để nhặt {stoneName}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

    private void Update()
    {
        // Bắt phím E bằng New Input System
        if (isPlayerNearby && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            InteractPickup();
        }
    }

    public void InteractPickup()
    {
        if (stoneType == StoneType.Blue) HasBlueStone = true;
        else if (stoneType == StoneType.Red) HasRedStone = true;

        if (pickupSound != null) pickupSound.Play();

        gameObject.SetActive(false);
    }
}