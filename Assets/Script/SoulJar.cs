using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class SoulJar : MonoBehaviour
{
    [Header("Cấu hình Bệ Tế")]
    public RitualAltar targetAltar; // Kéo Object Bệ Tế vào đây (nếu không kéo code sẽ tự tìm)

    [Header("Giao diện UI")]
    public TextMeshProUGUI promptText; 
    public string promptMessage = "Bấm [E] để nhặt Bình";

    private bool isNearPlayer = false;

    private void Start()
    {
        // Tự động tìm RitualAltar trong Scene nếu quên kéo vào Inspector
        if (targetAltar == null)
        {
            targetAltar = FindObjectOfType<RitualAltar>();
        }
    }

    private void Update()
    {
        bool eKeyPressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        if (isNearPlayer && eKeyPressed)
        {
            if (targetAltar != null)
            {
                targetAltar.OnPickUpJar(); // Kích hoạt sự kiện nhặt bình trên Bệ Tế
            }
            else
            {
                Debug.LogError("[SoulJar] Không tìm thấy RitualAltar trong Scene!");
            }

            if (promptText != null) promptText.gameObject.SetActive(false);

            // Tắt object chiếc bình dưới đất
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNearPlayer = true;
            if (promptText != null)
            {
                promptText.text = promptMessage;
                promptText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNearPlayer = false;
            if (promptText != null) promptText.gameObject.SetActive(false);
        }
    }
}