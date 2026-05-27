using UnityEngine;
using TMPro; // Bắt buộc phải có nếu bạn dùng TextMeshPro làm UI

public class EmptyFrameInteract : MonoBehaviour
{
    [Header("Cấu hình UI Text")]
    [SerializeField] private GameObject promptCanvasObject; // Object chứa Text (hoặc cả cụm UI Prompt)
    [SerializeField] private TMP_Text promptText;            // Thành phần TextMeshPro để đổi chữ
    [SerializeField] private string messageToShow = "Nhấn [E] để tiến hành phong ấn";

    private bool isPlayerInside = false;

    private void Awake()
    {
        // Ban đầu ẩn dòng chữ hướng dẫn đi
        if (promptCanvasObject != null) promptCanvasObject.SetActive(false);
    }

    private void Update()
    {
        // Nếu Player đang đứng trong vùng và nhấn phím E
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E))
        {
            // Ẩn Text Prompt ngay khi mở bảng UI Mini-game để đỡ vướng màn hình
            if (promptCanvasObject != null) promptCanvasObject.SetActive(false);

            // Gọi Bộ Quản Lý kích hoạt Mini-game và đóng băng Player
            if (Chapter1Manager.Instance != null)
            {
                Chapter1Manager.Instance.TryTriggerPuzzle();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem có đúng là Player bước vào không dựa vào Tag
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;

            // Đổi nội dung chữ và hiển thị Text UI lên màn hình
            if (promptText != null) promptText.text = messageToShow;
            if (promptCanvasObject != null) promptCanvasObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;

            // Khi Player đi ra xa khỏi khung tranh thì ẩn chữ đi
            if (promptCanvasObject != null) promptCanvasObject.SetActive(false);
        }
    }
}