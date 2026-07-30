using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PuzzlePiece : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Target Settings")]
    [SerializeField] private RectTransform targetSlot; // Kéo ô Slot đích tương ứng vào đây
    [SerializeField] private float snapDistance = 50f;  // Khoảng cách để tự hút vào ô

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private bool isSnapped = false;
    private int correctRotation = 0; // Góc xoay đúng là 0 độ

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        originalPosition = rectTransform.anchoredPosition;

        // Tự động ngẫu nhiên góc xoay ban đầu của mảnh ghép (90, 180, 270 độ) để làm khó người chơi
        int[] randomAngles = { 0, 90, 180, 270 };
        int randomChoice = randomAngles[Random.Range(1, randomAngles.Length)];
        rectTransform.eulerAngles = new Vector3(0, 0, randomChoice);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (PauseMenu.IsPaused) return;
        if (isSnapped) return; // Nếu đã ghép đúng vị trí thì không cho kéo nữa
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f; // Làm mờ mảnh khi đang kéo
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (PauseMenu.IsPaused) return;
        if (isSnapped) return;
        // Di chuyển mảnh ghép theo tọa độ chuột
        rectTransform.anchoredPosition += eventData.delta / transform.root.GetComponent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (PauseMenu.IsPaused)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
            return;
        }

        if (isSnapped) return;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // Tính khoảng cách giữa mảnh ghép hiện tại và ô Slot đích
        float distance = Vector2.Distance(rectTransform.position, targetSlot.position);

        // Điều kiện thắng mảnh này: Gần sát ô đích VÀ góc xoay phải về chuẩn 0 độ
        if (distance < snapDistance && Mathf.DeltaAngle(rectTransform.eulerAngles.z, correctRotation) == 0)
        {
            rectTransform.position = targetSlot.position; // Hút chặt vào ô
            isSnapped = true;
            Debug.Log($"{gameObject.name} đã vào đúng vị trí!");

            // Gọi quản lý tổng kiểm tra xem thắng toàn bộ chưa
            FindFirstObjectByType<PuzzleManager>().CheckWinCondition();
        }
        else
        {
            // Trả về vị trí cũ nếu sai
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    // Click Chuột phải để xoay mảnh ghép 90 độ
    public void OnPointerClick(PointerEventData eventData)
    {
        if (PauseMenu.IsPaused) return;
        if (isSnapped) return;

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            rectTransform.Rotate(0, 0, 90f);
        }
    }

    public bool IsSnapped() => isSnapped;
}
