using UnityEngine;
using TMPro;

public class EmptyFrameInteract : MonoBehaviour
{
    [Header("Cấu hình UI Text")]
    [SerializeField] private GameObject promptCanvasObject; // Object chứa Text (hoặc Canvas UI Prompt)
    [SerializeField] private TMP_Text promptText;            // Thành phần TextMeshPro để đổi chữ
    [SerializeField] private string messageToShow = "Nhấn [E] để tiến hành phong ấn";

    private bool isPlayerInside = false;

    private void Awake()
    {
        if (promptCanvasObject != null) promptCanvasObject.SetActive(false);
    }

    private void Update()
    {
        // Nếu Chapter 1 chưa active -> Tắt UI và dừng xử lý
        if (!MiniGameFlowManager.IsChapterActive(1))
        {
            if (promptCanvasObject != null && promptCanvasObject.activeSelf)
                promptCanvasObject.SetActive(false);
            return;
        }

        // Bắt phím E khi Player đang ở trong vùng
        if (isPlayerInside && GameInputBridge.GetKeyDown(KeyCode.E))
        {
            if (promptCanvasObject != null)
                promptCanvasObject.SetActive(false);

            bool puzzleOpened = false;

            if (Chapter1Manager.Instance != null)
            {
                puzzleOpened = Chapter1Manager.Instance.TryTriggerPuzzle();
            }

            if (puzzleOpened && QuestManager.Instance != null)
            {
                QuestManager.Instance.CompleteSubQuestForChapter(1, 1);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            ShowPromptIfActive();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Giúp hiển thị UI ngay nếu Chapter 1 được kích hoạt khi Player đã đứng sẵn ở đây từ trước
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            ShowPromptIfActive();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;

            if (promptCanvasObject != null) 
                promptCanvasObject.SetActive(false);
        }
    }

    // Hàm phụ trách hiển thị dòng chữ E gợi ý
    private void ShowPromptIfActive()
    {
        if (MiniGameFlowManager.IsChapterActive(1))
        {
            if (promptText != null) promptText.text = messageToShow;
            if (promptCanvasObject != null && !promptCanvasObject.activeSelf) 
                promptCanvasObject.SetActive(true);
        }
    }
}