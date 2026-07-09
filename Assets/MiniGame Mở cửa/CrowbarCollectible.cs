using UnityEngine;
using TMPro;

public class CrowbarCollectible : MonoBehaviour
{
    [Header("Tên item")]
    public string itemName = "Crowbar";

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string interactMessage = "Nhấn [E] để nhặt xà beng";

    [Header("Âm thanh")]
    [SerializeField] private AudioClip collectSound;

    private bool isPlayerInside = false;

    private void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerInside && GameInputBridge.GetKeyDown(KeyCode.E))
            Collect();
    }

    private void Collect()
    {
        PlayerInventory.Add(itemName);
        Debug.Log("Đã nhặt Crowbar!");

        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        if (promptText != null) promptText.gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInside = true;
        if (promptText != null)
        {
            promptText.text = interactMessage;
            promptText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInside = false;
        if (promptText != null) promptText.gameObject.SetActive(false);
    }
}