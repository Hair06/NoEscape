using UnityEngine;

public class CrowbarCollectible : MonoBehaviour, IInteractable
{
    [Header("Tên item")]
    public string itemName = "Crowbar";

    [Header("Raycast Settings")]
    [SerializeField] private float pickupRange = 3f;

    [Header("Âm thanh")]
    [SerializeField] private AudioClip collectSound;

    private Camera playerCamera;
    private bool isLookingAt = false;

    private void Start()
    {
        playerCamera = Camera.main;
    }

    private void Update()
    {
        CheckLookAt();
    }

    private void CheckLookAt()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            if (hit.collider.gameObject == gameObject)
                isLookingAt = true;
            else
                isLookingAt = false;
        }
        else
        {
            isLookingAt = false;
        }
    }

    // ← IInteractable: PlayerInteraction tự hiện prompt này
    public string GetInteractPrompt()
    {
        return isLookingAt ? "Nhấn [E] để nhặt xà beng" : "";
    }

    public void Interact()
    {
        PlayerInventory.Add(itemName);
        Debug.Log("Đã nhặt Crowbar!");

        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        Destroy(gameObject);
    }
}