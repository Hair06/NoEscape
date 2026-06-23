using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRange = 2f;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Camera interactCamera;
    [SerializeField] private LayerMask itemInfoMask = ~0;

    private IInteractable currentInteractable;
    private IInteractable currentLookInteractable;
    private ItemInfoData currentLookInfo;
    private bool infoPinnedByInteract;

    void Update()
    {
        UpdateLookItemInfo();

        IInteractable interactable = currentLookInteractable ?? currentInteractable;
        if (interactable != null)
        {
            if (GameInputBridge.GetKeyDown(KeyCode.E))
            {
                TryShowItemInfo(GetItemInfoFromInteractable(interactable), false);
                interactable.Interact();
                // KHONG goi ClearInteractable() o day nua.
                // Binh xang se tu roi vung -> OnTriggerExit xoa.
                // May phat khong bi huy -> bam E nhieu lan van do duoc.
            }
        }

        if (infoPinnedByInteract && !ItemInfoUI.IsVisible)
        {
            infoPinnedByInteract = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[PlayerInteraction] Trigger entered '{other.name}' on layer '{LayerMask.LayerToName(other.gameObject.layer)}'.");

        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
            Debug.Log($"[PlayerInteraction] Current interactable set to '{other.name}'.");

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
            Debug.Log($"[PlayerInteraction] Trigger exited current interactable '{other.name}'.");
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

    private void UpdateLookItemInfo()
    {
        Camera cameraToUse = interactCamera != null ? interactCamera : Camera.main;
        if (cameraToUse == null)
        {
            return;
        }

        Ray ray = new Ray(cameraToUse.transform.position, cameraToUse.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, itemInfoMask, QueryTriggerInteraction.Collide))
        {
            IInteractable lookInteractable = GetInteractableFromCollider(hit.collider);
            if (currentLookInteractable != lookInteractable)
            {
                currentLookInteractable = lookInteractable;
                if (currentLookInteractable != null)
                {
                    Debug.Log($"[PlayerInteraction] Raycast target interactable '{((Component)currentLookInteractable).name}' via collider '{hit.collider.name}'.");
                }
            }

            ItemInfoData itemInfo = GetItemInfoFromCollider(hit.collider);
            if (itemInfo != null)
            {
                if (currentLookInfo != itemInfo)
                {
                    Debug.Log($"[PlayerInteraction] Raycast hit item info '{itemInfo.name}' via collider '{hit.collider.name}'. Distance={hit.distance:0.00}.");
                    currentLookInfo = itemInfo;
                    TryShowItemInfo(itemInfo, false);
                }

                return;
            }

            if (currentLookInfo != null)
            {
                Debug.Log($"[PlayerInteraction] Raycast hit '{hit.collider.name}', but it has no ItemInfoData.");
            }
        }
        else if (currentLookInfo != null)
        {
            Debug.Log("[PlayerInteraction] Raycast no longer hits an item with ItemInfoData.");
        }

        currentLookInteractable = null;
        currentLookInfo = null;
        if (!infoPinnedByInteract && ItemInfoUI.IsVisible)
        {
            ItemInfoUI.Instance.HideInfo();
        }
    }

    private void TryShowItemInfo(ItemInfoData itemInfo, bool pinPanel)
    {
        if (itemInfo == null)
        {
            Debug.LogWarning("[PlayerInteraction] Cannot show item info because ItemInfoData is missing on the target item.");
            return;
        }

        if (ItemInfoUI.Instance == null)
        {
            Debug.LogError("[PlayerInteraction] Cannot show item info because ItemInfoUI.Instance is null.");
            return;
        }

        if (string.IsNullOrWhiteSpace(itemInfo.itemName))
        {
            Debug.LogWarning($"[PlayerInteraction] ItemInfoData on '{itemInfo.name}' has empty itemName.");
        }

        if (string.IsNullOrWhiteSpace(itemInfo.description))
        {
            Debug.LogWarning($"[PlayerInteraction] ItemInfoData on '{itemInfo.name}' has empty description.");
        }

        if (string.IsNullOrWhiteSpace(itemInfo.useDescription))
        {
            Debug.LogWarning($"[PlayerInteraction] ItemInfoData on '{itemInfo.name}' has empty useDescription.");
        }

        infoPinnedByInteract = pinPanel;
        Debug.Log($"[PlayerInteraction] Calling ItemInfoUI.ShowInfo for '{itemInfo.name}'. PinnedByInteract={pinPanel}.");
        ItemInfoUI.Instance.ShowInfo(itemInfo);
    }

    private static ItemInfoData GetItemInfoFromInteractable(IInteractable interactable)
    {
        if (interactable is Component component)
        {
            ItemInfoData itemInfo = GetItemInfoFromComponent(component);
            if (itemInfo == null)
            {
                Debug.LogWarning($"[PlayerInteraction] Interactable '{component.name}' has no ItemInfoData on itself, parent, or children.");
            }

            return itemInfo;
        }

        return null;
    }

    private static ItemInfoData GetItemInfoFromCollider(Collider collider)
    {
        return collider != null ? GetItemInfoFromComponent(collider) : null;
    }

    private static IInteractable GetInteractableFromCollider(Collider collider)
    {
        if (collider == null) return null;

        IInteractable interactable = collider.GetComponent<IInteractable>();
        if (interactable != null) return interactable;

        interactable = collider.GetComponentInParent<IInteractable>();
        if (interactable != null) return interactable;

        return collider.GetComponentInChildren<IInteractable>();
    }

    private static ItemInfoData GetItemInfoFromComponent(Component component)
    {
        ItemInfoData itemInfo = component.GetComponent<ItemInfoData>();
        if (itemInfo != null) return itemInfo;

        itemInfo = component.GetComponentInParent<ItemInfoData>();
        if (itemInfo != null) return itemInfo;

        return component.GetComponentInChildren<ItemInfoData>();
    }
}
