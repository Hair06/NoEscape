using UnityEngine;

public class PickUp : MonoBehaviour
{
    [Header("Cấu hình nhặt")]
    [SerializeField] private GameObject itemInHand;
    [SerializeField] private float pickupDistance = 3.0f;

    [Header("Tên item trên hotbar")]
    [Tooltip("Tên phải khớp với AltarSeal và Icon Library, ví dụ: ConMat")]
    [SerializeField] private string itemName = "ConMat";

    private Transform playerTransform;
    private bool isPlayerNearby = false;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= pickupDistance)
        {
            if (!isPlayerNearby)
            {
                isPlayerNearby = true;
                Debug.Log("👉 Đến gần vật phẩm. Nhấn E để nhặt!");
            }

            if (CheckKeyE())
            {
                PickUpItem();
            }
        }
        else
        {
            if (isPlayerNearby)
            {
                isPlayerNearby = false;
            }
        }
    }

    private bool CheckKeyE()
    {
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            return UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame;
        }
        return false;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    void PickUpItem()
    {
        if (itemInHand != null)
        {
            itemInHand.SetActive(true);
        }

        // Them vao hotbar
        PlayerInventory.Add(itemName);

        Debug.Log("🎒 Đã nhặt vật phẩm thành công: " + itemName);
        Destroy(gameObject);
    }
}