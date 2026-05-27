using UnityEngine;
using TMPro; // Sử dụng nếu bạn có TextMeshPro để hiện chữ UI

public class PlayerInteraction : MonoBehaviour
{
    public float interactRange = 2f;
    [SerializeField] private TextMeshProUGUI promptText; // Kéo Text UI vào đây (nếu có)

    private IInteractable currentInteractable;

    void Update()
    {
        if (currentInteractable != null)
        {
            // Bấm E để tương tác
            if (GameInputBridge.GetKeyDown(KeyCode.E))
            {
                currentInteractable.Interact();
                
                // Sau khi nhặt (bị hủy), xóa prompt text luôn
                if (currentInteractable == null || currentInteractable.Equals(null))
                {
                    ClearInteractable();
                }
            }
        }
    }

    // Phát hiện khi đi vào vùng của vật phẩm
    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
            if (promptText != null)
            {
                promptText.text = currentInteractable.GetInteractPrompt();
                promptText.gameObject.SetActive(true);
            }
        }
    }

    // Phát hiện khi đi ra khỏi vùng của vật phẩm
    private void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null && interactable == currentInteractable)
        {
            ClearInteractable();
        }
    }

    private void ClearInteractable()
    {
        currentInteractable = null;
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }
}
