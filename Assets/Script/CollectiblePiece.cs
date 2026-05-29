using UnityEngine;
// Bắt buộc phải thêm thư viện này để đọc phím E theo chuẩn mới
using UnityEngine.InputSystem; 

public class CollectiblePiece : MonoBehaviour
{
    [Header("Cấu hình UI Text")]
    [SerializeField] private GameObject promptCanvasObject; // UI hướng dẫn (Nhấn [E] để nhặt)

    [Header("Âm thanh khi nhặt")]
    [SerializeField] private AudioClip collectSound;

    private bool isPlayerInside = false;

    private void Update()
    {
        // SỬA TẠI ĐÂY: Thay thế hoàn toàn Input.GetKeyDown bằng Keyboard.current
        if (isPlayerInside && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            CollectThisPiece();
        }
    }

    private void CollectThisPiece()
    {
        // Cộng 1 mảnh vào tiến trình nhiệm vụ của Chapter1Manager
        if (Chapter1Manager.Instance != null)
        {
            Chapter1Manager.Instance.collectedPieces++;
            Debug.Log($"Đã nhặt được mảnh ảnh! Tiến độ hiện tại: {Chapter1Manager.Instance.collectedPieces}/{Chapter1Manager.Instance.totalPiecesRequired}");
        }

        // Phát âm thanh sột soạt giấy khi nhặt
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        // Ẩn chữ hướng dẫn và xóa mảnh ảnh này khỏi Map 3D
        if (promptCanvasObject != null) promptCanvasObject.SetActive(false);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            if (promptCanvasObject != null) promptCanvasObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (promptCanvasObject != null) promptCanvasObject.SetActive(false);
        }
    }
}