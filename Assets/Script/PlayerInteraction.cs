using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI promptText;

    private IInteractable currentInteractable;
    private ItemInfoData currentItemInfo;
    private Transform currentItemRoot;
    private bool promptHiddenForCurrentTarget;
    private readonly HashSet<Collider> nearbyColliders = new HashSet<Collider>();

    void Update()
    {
        RemoveInvalidNearbyColliders();

        if (!IsCurrentTargetValid())
        {
            RefreshCurrentTarget();
        }

        if (currentItemInfo != null && GameInputBridge.GetKeyDown(KeyCode.I))
        {
            Debug.Log($"[PlayerInteraction] I pressed for item info '{currentItemInfo.name}'.");
            promptHiddenForCurrentTarget = true;
            HidePrompt();
            SuppressCurrentTargetPromptsUntilExit();
            ShowCurrentItemInfo();
        }

        if (currentInteractable != null && GameInputBridge.GetKeyDown(KeyCode.E))
        {
            promptHiddenForCurrentTarget = true;
            HidePrompt();
            SuppressCurrentTargetPromptsUntilExit();
            if (ItemInfoUI.IsVisible)
            {
                ItemInfoUI.Instance.HideInfo();
            }

            currentInteractable.Interact();
            // KHONG goi ClearInteractable() o day nua.
            // Binh xang se tu roi vung -> OnTriggerExit xoa.
            // May phat khong bi huy -> bam E nhieu lan van do duoc.
            if (!IsCurrentTargetValid())
            {
                RefreshCurrentTarget();
            }
            else
            {
                UpdatePrompt();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsUsableTrigger(other)) return;

        nearbyColliders.Add(other);
        TryUseNearbyCollider(other, true);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsUsableTrigger(other)) return;

        nearbyColliders.Add(other);
        if (!IsCurrentTargetValid())
        {
            TryUseNearbyCollider(other, false);
        }
        else
        {
            UpdatePrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other != null)
        {
            nearbyColliders.Remove(other);
        }

        Transform exitedRoot = GetItemRoot(other);
        if (currentItemRoot != null && exitedRoot == currentItemRoot && !HasNearbyColliderForRoot(currentItemRoot))
        {
            Debug.Log($"[PlayerInteraction] Trigger exited current item '{currentItemRoot.name}'.");
            ClearCurrentTarget(true);
            RefreshCurrentTarget();
        }
    }

    private void ShowCurrentItemInfo()
    {
        if (currentItemInfo == null)
        {
            Debug.LogWarning("[PlayerInteraction] Cannot show item info because ItemInfoData is missing on the target item.");
            return;
        }

        if (ItemInfoUI.Instance == null)
        {
            Debug.LogError("[PlayerInteraction] Cannot show item info because ItemInfoUI.Instance is null.");
            return;
        }

        ItemInfoUI.Instance.ShowInfo(currentItemInfo);
    }

    private void TryUseNearbyCollider(Collider collider, bool logEnter)
    {
        NearbyItemTarget target = GetTargetFromCollider(collider);
        if (!target.HasAnyTarget || (currentItemRoot != null && target.Root != currentItemRoot))
        {
            return;
        }

        if (currentItemRoot != target.Root)
        {
            promptHiddenForCurrentTarget = false;
        }

        currentItemRoot = target.Root;
        currentInteractable = target.Interactable;
        currentItemInfo = target.ItemInfo;

        if (logEnter)
        {
            Debug.Log($"[PlayerInteraction] Trigger entered item '{currentItemRoot.name}' via collider '{collider.name}'.");
        }

        UpdatePrompt();
    }

    private void RefreshCurrentTarget()
    {
        RemoveInvalidNearbyColliders();

        foreach (Collider collider in nearbyColliders)
        {
            NearbyItemTarget target = GetTargetFromCollider(collider);
            if (!target.HasAnyTarget) continue;

            if (currentItemRoot != target.Root)
            {
                promptHiddenForCurrentTarget = false;
            }

            currentItemRoot = target.Root;
            currentInteractable = target.Interactable;
            currentItemInfo = target.ItemInfo;
            UpdatePrompt();
            return;
        }

        ClearCurrentTarget(false);
    }

    private void ClearCurrentTarget(bool closeInfo)
    {
        currentItemRoot = null;
        currentInteractable = null;
        currentItemInfo = null;
        promptHiddenForCurrentTarget = false;

        HidePrompt();

        if (closeInfo && ItemInfoUI.IsVisible)
        {
            ItemInfoUI.Instance.HideInfo();
        }
    }

    private void HidePrompt()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    public void SuppressCurrentPromptUntilExit()
    {
        promptHiddenForCurrentTarget = true;
        HidePrompt();
        SuppressCurrentTargetPromptsUntilExit();
    }

    private void SuppressCurrentTargetPromptsUntilExit()
    {
        if (currentItemRoot == null) return;

        foreach (CollectiblePiece collectiblePiece in currentItemRoot.GetComponentsInChildren<CollectiblePiece>(true))
        {
            collectiblePiece.SuppressPromptUntilExit();
        }

        CollectiblePiece parentCollectiblePiece = currentItemRoot.GetComponentInParent<CollectiblePiece>();
        if (parentCollectiblePiece != null)
        {
            parentCollectiblePiece.SuppressPromptUntilExit();
        }
    }

    private void UpdatePrompt()
    {
        if (promptText == null) return;
        if (promptHiddenForCurrentTarget)
        {
            HidePrompt();
            return;
        }

        StringBuilder builder = new StringBuilder();
        if (currentItemInfo != null)
        {
            builder.Append("I - Xem th\u00f4ng tin v\u1eadt ph\u1ea9m");
        }

        if (currentInteractable != null)
        {
            string interactPrompt = currentInteractable.GetInteractPrompt();
            if (!string.IsNullOrWhiteSpace(interactPrompt))
            {
                if (builder.Length > 0) builder.AppendLine();
                builder.Append(interactPrompt);
            }
        }

        if (builder.Length > 0)
        {
            promptText.text = builder.ToString();
            promptText.gameObject.SetActive(true);
        }
        else
        {
            promptText.gameObject.SetActive(false);
        }
    }

    private bool IsCurrentTargetValid()
    {
        bool hasInteractable = currentInteractable != null;
        bool hasItemInfo = currentItemInfo != null;
        bool hasRoot = currentItemRoot != null;
        return hasRoot && (hasInteractable || hasItemInfo) && HasNearbyColliderForRoot(currentItemRoot);
    }

    private void RemoveInvalidNearbyColliders()
    {
        nearbyColliders.RemoveWhere(collider => collider == null || !collider.gameObject.activeInHierarchy);
    }

    private bool HasNearbyColliderForRoot(Transform root)
    {
        if (root == null) return false;

        foreach (Collider collider in nearbyColliders)
        {
            if (collider == null) continue;
            Transform colliderRoot = GetItemRoot(collider);
            if (colliderRoot == root) return true;
        }

        return false;
    }

    private bool IsUsableTrigger(Collider other)
    {
        if (other == null || other.transform == transform || other.transform.IsChildOf(transform))
        {
            return false;
        }

        return GetTargetFromCollider(other).HasAnyTarget;
    }

    private static NearbyItemTarget GetTargetFromCollider(Collider collider)
    {
        if (collider == null) return default;

        IInteractable interactable = GetInteractableFromCollider(collider);
        ItemInfoData itemInfo = GetItemInfoFromComponent(collider);
        Transform root = GetRootFromTargets(interactable, itemInfo, collider);

        return new NearbyItemTarget(root, interactable, itemInfo);
    }

    private static Transform GetItemRoot(Collider collider)
    {
        NearbyItemTarget target = GetTargetFromCollider(collider);
        return target.Root;
    }

    private static Transform GetRootFromTargets(IInteractable interactable, ItemInfoData itemInfo, Collider fallbackCollider)
    {
        if (interactable is Component interactableComponent)
        {
            return interactableComponent.transform;
        }

        if (itemInfo != null)
        {
            return itemInfo.transform;
        }

        return fallbackCollider != null ? fallbackCollider.transform : null;
    }

    private static IInteractable GetInteractableFromCollider(Collider collider)
    {
        if (collider == null) return null;

        IInteractable interactable = collider.GetComponent<IInteractable>();
        if (interactable != null) return interactable;

        return collider.GetComponentInParent<IInteractable>();
    }

    private static ItemInfoData GetItemInfoFromComponent(Component component)
    {
        ItemInfoData itemInfo = component.GetComponent<ItemInfoData>();
        if (itemInfo != null) return itemInfo;

        return component.GetComponentInParent<ItemInfoData>();
    }

    private readonly struct NearbyItemTarget
    {
        public readonly Transform Root;
        public readonly IInteractable Interactable;
        public readonly ItemInfoData ItemInfo;
        public bool HasAnyTarget => Root != null && (Interactable != null || ItemInfo != null);

        public NearbyItemTarget(Transform root, IInteractable interactable, ItemInfoData itemInfo)
        {
            Root = root;
            Interactable = interactable;
            ItemInfo = itemInfo;
        }
    }
}
