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

    [Header("Raycast Settings")]
    [SerializeField] private float pickupRange = 3f; // Khoảng cách nhìn thấy

    private Camera playerCamera;
    private bool isLookingAt = false;

    private void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);

        // Tìm camera player
        playerCamera = Camera.main;
    }

    private void Update()
    {
        CheckLookAt();

        if (isLookingAt && GameInputBridge.GetKeyDown(KeyCode.E))
            Collect();
    }

    private void CheckLookAt()
    {
        if (playerCamera == null) return;

        // Bắn ray từ giữa màn hình
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            if (hit.collider.gameObject == gameObject)
            {
                // Đang nhìn vào crowbar
                if (!isLookingAt)
                {
                    isLookingAt = true;
                    if (promptText != null)
                    {
                        promptText.text = interactMessage;
                        promptText.gameObject.SetActive(true);
                    }
                }
            }
            else
            {
                StopLookingAt();
            }
        }
        else
        {
            StopLookingAt();
        }
    }

    private void StopLookingAt()
    {
        if (!isLookingAt) return;
        isLookingAt = false;
        if (promptText != null) promptText.gameObject.SetActive(false);
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
}