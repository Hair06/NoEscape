using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ItemInfoUI : MonoBehaviour
{
    public static ItemInfoUI Instance;
    public static bool IsVisible => Instance != null && Instance.infoPanel != null && Instance.infoPanel.activeSelf;

    [Header("Panel")]
    public GameObject infoPanel;

    [Header("Text")]
    public TextMeshProUGUI showInfoText;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI useText;

    [Header("Button")]
    public Button closeButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[ItemInfoUI] Duplicate ItemInfoUI found on '{name}'. Replacing previous instance from '{Instance.name}'.");
        }

        Instance = this;
        Debug.Log($"[ItemInfoUI] Instance assigned from '{name}'.");
        ValidateReferences();

        if (infoPanel != null)
        {
            Debug.Log($"[ItemInfoUI] Closing panel on Awake: '{infoPanel.name}'.");
            infoPanel.SetActive(false);
        }
    }

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideInfo);
        }
    }

    private void Update()
    {
        if (infoPanel != null && infoPanel.activeSelf)
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                HideInfo();
            }
        }
    }

    public void ShowInfo(ItemInfoData itemInfo)
    {
        Debug.Log($"[ItemInfoUI] ShowInfo called. ItemInfoData={(itemInfo != null ? itemInfo.name : "NULL")}.");

        if (itemInfo == null)
        {
            Debug.LogWarning("[ItemInfoUI] ShowInfo stopped because ItemInfoData is null.");
            return;
        }

        ValidateReferences();

        if (infoPanel != null)
        {
            Debug.Log($"[ItemInfoUI] Opening panel '{infoPanel.name}' for item '{itemInfo.itemName}'.");
            infoPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("[ItemInfoUI] Cannot open ItemInfoPanel because infoPanel is not assigned in the Inspector.");
        }

        if (itemNameText != null)
        {
            itemNameText.text = itemInfo.itemName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = itemInfo.description;
        }

        if (useText != null)
        {
            useText.text = itemInfo.useDescription;
        }

        if (showInfoText != null)
        {
            showInfoText.text =
                $"{itemInfo.itemName}\n\n{itemInfo.description}\n\n{itemInfo.useDescription}";
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void HideInfo()
    {
        Debug.Log("[ItemInfoUI] HideInfo called.");

        if (infoPanel != null)
        {
            Debug.Log($"[ItemInfoUI] Closing panel '{infoPanel.name}'.");
            infoPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[ItemInfoUI] HideInfo could not close panel because infoPanel is null.");
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void ValidateReferences()
    {
        Canvas parentCanvas = GetComponentInParent<Canvas>(true);
        if (parentCanvas == null)
        {
            Debug.LogWarning("[ItemInfoUI] No Canvas found in parents. ItemInfoPanel may not render.");
        }
        else if (!parentCanvas.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[ItemInfoUI] Parent Canvas '{parentCanvas.name}' is inactive in hierarchy.");
        }

        if (EventSystem.current == null)
        {
            Debug.LogWarning("[ItemInfoUI] No EventSystem found. Close button clicks may not work.");
        }

        if (infoPanel == null)
        {
            Debug.LogError("[ItemInfoUI] infoPanel is not assigned in the Inspector.");
        }
        else if (!infoPanel.activeInHierarchy && infoPanel.activeSelf)
        {
            Debug.LogWarning($"[ItemInfoUI] infoPanel '{infoPanel.name}' is activeSelf but inactive in hierarchy. Check parent Canvas.");
        }
        else if (showInfoText == null)
        {
            showInfoText = FindTextByName(infoPanel.transform, "showinfo");
        }

        if (showInfoText == null) Debug.LogWarning("[ItemInfoUI] showInfoText is not assigned. This is OK if using itemNameText/descriptionText/useText.");
        if (itemNameText == null) Debug.LogWarning("[ItemInfoUI] itemNameText is not assigned.");
        if (descriptionText == null) Debug.LogWarning("[ItemInfoUI] descriptionText is not assigned.");
        if (useText == null) Debug.LogWarning("[ItemInfoUI] useText is not assigned.");
    }

    private static TextMeshProUGUI FindTextByName(Transform root, string targetName)
    {
        foreach (TextMeshProUGUI text in root.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (string.Equals(text.name, targetName, System.StringComparison.OrdinalIgnoreCase))
            {
                return text;
            }
        }

        return null;
    }
}
