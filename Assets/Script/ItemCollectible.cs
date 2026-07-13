using UnityEngine;

public class ItemCollectible : MonoBehaviour
{
    [Header("Nhận diện mảnh ảnh nhiệm vụ")]
    [SerializeField] private string pieceId;

    [Header("Cấu hình mẫu xem 3D")]
    [Tooltip("Kéo bản sao Prefab 3D của riêng mảnh này (mảnh ảnh có đủ kết cấu 2 mặt) vào đây")]
    [SerializeField] private GameObject itemVisualPrefab; 

    // Hàm này sẽ được gọi từ sự kiện tương tác Interactable cũ của bro ngoài môi trường
    public void TriggerInspectMode()
    {
        if (ItemViewer.Instance != null)
        {
            // Bật giao diện soi 3D lên, truyền chính nó qua để xử lý nhặt sau
            ItemViewer.Instance.StartInspect(itemVisualPrefab, this);
            
            // Tạm thời ẩn mảnh ghép ngoài môi trường đi để tạo cảm giác người chơi đã cầm lên tay
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Chưa tìm thấy Object ItemViewer (Inspect Station) trong Scene!");
            ConfirmCollect(); // Cứu nguy: nếu quên tạo trạm soi thì tự nhặt luôn vào túi
        }
    }

    // Hàm này được gọi khi người chơi xem xong và nhấn nút "Xác nhận nhặt" (Phím E)
    public void ConfirmCollect()
    {
        if (Chapter1Manager.Instance != null)
        {
            string resolvedPieceId = string.IsNullOrWhiteSpace(pieceId)
                ? gameObject.name + "_" + GetInstanceID()
                : pieceId;

            Chapter1Manager.Instance.RegisterCollectedPiece(
                resolvedPieceId
            );
        }

        // Hủy vĩnh viễn mảnh này ngoài môi trường vì đã bỏ vào túi thành công
        Destroy(gameObject);
    }

    // Hàm cứu nguy nếu người chơi xem xong không muốn nhặt mà nhấn hủy bỏ (Phím F/ESC)
    public void CancelInspect()
    {
        // Hiện lại mảnh ghép ngoài môi trường tại vị trí cũ để có thể nhặt lại sau
        gameObject.SetActive(true);
    }
}