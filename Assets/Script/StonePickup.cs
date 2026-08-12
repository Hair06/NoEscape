using UnityEngine;
using TMPro; // Thêm thư viện TextMeshPro
using UnityEngine.InputSystem;

public enum StoneType { Blue, Red }

public class StonePickup : MonoBehaviour
{
    [Header("LOẠI ĐÁ")]
    public StoneType stoneType;

    [Header("GIAO DIỆN UI GỢI Ý")]
    [SerializeField] private TextMeshProUGUI promptText; // Kéo Text UI vào đây

    [Header("ÂM THANH NHẶT")]
    [SerializeField] private AudioSource pickupSound;

    public static bool HasBlueStone { get; set; } = false;
    public static bool HasRedStone { get; set; } = false;

    private bool isPlayerNearby = false;

    private void Start()
    {
        // Ẩn UI prompt khi bắt đầu game
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            
            // Hiển thị dòng chữ gợi ý trên UI
            if (promptText != null)
            {
                string stoneName = (stoneType == StoneType.Blue) ? "Đá Xanh" : "Đá Đỏ";
                promptText.text = $"Nhấn [E] để nhặt {stoneName}";
                promptText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;

            // Ẩn UI prompt khi người chơi đi ra xa
            if (promptText != null)
            {
                promptText.gameObject.SetActive(false);
            }
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

        // Ẩn dòng chữ UI ngay khi nhặt xong
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }

        gameObject.SetActive(false);
    }
}