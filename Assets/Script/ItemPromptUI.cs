using TMPro;
using UnityEngine;

public class ItemPromptUI : MonoBehaviour
{
    [Header("Prompt Root")]
    [SerializeField] private GameObject promptPanel;

    [Header("Rows")]
    [SerializeField] private GameObject infoRow;
    [SerializeField] private GameObject interactRow;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI interactText;

    private const string InfoPrompt = "[I] Xem th\u00f4ng tin";

    private void Awake()
    {
        Hide();
    }

    public void Show(bool hasInfo, string interactPrompt)
    {
        bool hasInteractPrompt = interactPrompt != null;

        if (infoText != null)
        {
            infoText.text = InfoPrompt;
        }

        if (interactText != null)
        {
            interactText.text = interactPrompt;
        }

        SetActive(infoRow, hasInfo);
        SetActive(interactRow, hasInteractPrompt);
        SetActive(promptPanel != null ? promptPanel : gameObject, hasInfo || hasInteractPrompt);
    }

    public void Hide()
    {
        SetActive(infoRow, false);
        SetActive(interactRow, false);
        SetActive(promptPanel != null ? promptPanel : gameObject, false);
    }

    private static void SetActive(GameObject target, bool isActive)
    {
        if (target != null && target.activeSelf != isActive)
        {
            target.SetActive(isActive);
        }
    }
}
