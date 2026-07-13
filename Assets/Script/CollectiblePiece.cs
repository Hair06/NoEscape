using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CollectiblePiece : MonoBehaviour
{
    [Header("Ten item tren hotbar (moi manh dat rieng)")]
    [SerializeField] private string itemName = "ManhGiay1";   // ManhGiay1..4

    [Header("Cấu hình UI Text bằng TextMesh Pro")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string interactMessage = "Nhấn [E] để nhặt mảnh ảnh";

    [Header("Âm thanh khi nhặt")]
    [SerializeField] private AudioClip collectSound;

    [Header("Bán kính quét phát hiện Player đứng sẵn")]
    [Tooltip("Khoảng cách (mét) xung quanh mảnh giấy để tự động phát hiện người chơi")]
    [SerializeField] private float detectionRadius = 1.2f;

    private bool isPlayerInside = false;
    private bool promptSuppressedUntilExit = false;
    private bool isCollected = false;

    private void Start()
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);
        CheckIfPlayerIsAlreadyInside();
    }

    private void OnEnable()
    {
        CheckIfPlayerIsAlreadyInside();
    }

    private void Update()
    {
        if (isPlayerInside && GameInputBridge.GetKeyDown(KeyCode.E))
        {
            CollectThisPiece();
        }
    }

    public void SuppressPromptUntilExit()
    {
        promptSuppressedUntilExit = true;
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void CollectThisPiece()
    {
        if (isCollected) return;
        isCollected = true;

        ProximityLightGlow proximityLight = GetComponent<ProximityLightGlow>();
        if (proximityLight != null)
        {
            proximityLight.TurnOffLightOnPickup();
        }

        if (Chapter1Manager.Instance != null)
        {
            Chapter1Manager.Instance.RegisterCollectedPiece(itemName);
        }

        PlayerInventory.Add(itemName);   // them vao hotbar

        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        if (promptText != null) promptText.gameObject.SetActive(false);

        if (ItemInfoUI.IsVisible) ItemInfoUI.Instance.HideInfo();
        Destroy(gameObject);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            promptSuppressedUntilExit = false;
            if (promptText != null)
            {
                promptText.text = interactMessage;
                promptText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            if (!promptSuppressedUntilExit && promptText != null && !promptText.gameObject.activeSelf)
            {
                promptText.text = interactMessage;
                promptText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            promptSuppressedUntilExit = false;
            if (promptText != null) promptText.gameObject.SetActive(false);
        }
    }

    private void CheckIfPlayerIsAlreadyInside()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                isPlayerInside = true;
                promptSuppressedUntilExit = false;
                if (promptText != null)
                {
                    promptText.text = interactMessage;
                    promptText.gameObject.SetActive(true);
                }
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}