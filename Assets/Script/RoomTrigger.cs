using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    [SerializeField] private HorrorRoomManager roomManager;

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra nếu Object bước qua vùng này có Tag là "Player"
        if (other.CompareTag("Player"))
        {
            if (roomManager != null)
            {
                roomManager.PlayerEnteredRoom(); // Kích hoạt đóng cửa
                gameObject.SetActive(false); // Ẩn vùng Trigger đi để không bị kích hoạt lại liên tục
            }
        }
    }
}