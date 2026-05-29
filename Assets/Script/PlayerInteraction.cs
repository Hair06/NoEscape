using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRange = 2f;
    [SerializeField] private TextMeshProUGUI promptText;

    private IInteractable currentInteractable;

    void Update()
    {
        if (currentInteractable != null)
        {
            if (GameInputBridge.GetKeyDown(KeyCode.E))
            {
                currentInteractable.Interact();
                // KHONG goi ClearInteractable() o day nua.
                // Binh xang se tu roi vung -> OnTriggerExit xoa.
                // May phat khong bi huy -> bam E nhieu lan van do duoc.
            }
        }
    }

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