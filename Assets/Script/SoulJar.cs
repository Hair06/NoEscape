using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; // Thêm thư viện Input System mới

public class SoulJar : MonoBehaviour
{
    public RitualAltar targetAltar; // Kéo Object Bệ Tế vào đây

    [Header("Giao diện UI")]
    public TextMeshProUGUI promptText; // Kéo UI Text vào đây
    public string promptMessage = "Bấm [E] để nhặt Bình";

    private bool isNearPlayer = false;

    private void Update()
    {
        // Kiểm tra phím E bằng Input System mới
        bool eKeyPressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        if (isNearPlayer && eKeyPressed)
        {
            if (targetAltar != null)
            {
                targetAltar.OnPickUpJar();
            }

            if (promptText != null) promptText.gameObject.SetActive(false);

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