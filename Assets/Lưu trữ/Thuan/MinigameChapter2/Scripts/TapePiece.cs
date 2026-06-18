using UnityEngine;
using UnityEngine.EventSystems;

// Gắn script này vào từng miếng băng keo (Image trong Canvas).
// Người chơi nhấn giữ chuột rồi kéo một đoạn để lột miếng băng ra.
public class TapePiece : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Cấu hình lột băng")]
    [Tooltip("Tổng quãng kéo (pixel) cần đạt để miếng băng bong ra")]
    [SerializeField] private float peelDistance = 200f;

    [Tooltip("Miếng băng dịch theo chuột bao nhiêu phần (0 = đứng yên, 1 = bám sát chuột)")]
    [SerializeField] private float followAmount = 0.4f;

    [Header("Âm thanh (có thể để trống)")]
    [SerializeField] private AudioSource peelAudio;   // tiếng xé băng keo

    // Quản lý tổng sẽ gán giá trị này khi khởi tạo
    [HideInInspector] public TapePeelPuzzle manager;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector2 startMousePos;     // vị trí chuột khi bắt đầu kéo
    private Vector2 startAnchoredPos;  // vị trí ban đầu của miếng băng
    private bool isPeeled = false;
    private bool isDragging = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // Cần CanvasGroup để làm mờ dần khi bong
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isPeeled) return;

        isDragging = true;
        startMousePos = eventData.position;
        startAnchoredPos = rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPeeled || !isDragging) return;

        // Quãng kéo từ điểm bắt đầu
        Vector2 dragDelta = eventData.position - startMousePos;

        // Miếng băng dịch theo chuột một phần để trông như đang bị bóc
        rectTransform.anchoredPosition = startAnchoredPos + dragDelta * followAmount;

        // Hơi nghiêng miếng băng theo hướng kéo cho sinh động
        float tilt = Mathf.Clamp(dragDelta.x * 0.05f, -20f, 20f);
        rectTransform.localRotation = Quaternion.Euler(0, 0, -tilt);

        // Mờ dần theo quãng kéo
        float progress = Mathf.Clamp01(dragDelta.magnitude / peelDistance);
        canvasGroup.alpha = 1f - progress * 0.5f; // mờ tối đa còn 0.5 khi gần bong

        // Đủ quãng -> bong ra
        if (dragDelta.magnitude >= peelDistance)
        {
            Peel();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isPeeled) return;

        isDragging = false;

        // Nếu chưa kéo đủ thì miếng băng đàn hồi về chỗ cũ
        rectTransform.anchoredPosition = startAnchoredPos;
        rectTransform.localRotation = Quaternion.identity;
        canvasGroup.alpha = 1f;
    }

    private void Peel()
    {
        isPeeled = true;
        isDragging = false;

        if (peelAudio != null) peelAudio.Play();

        // Báo cho quản lý tổng biết một miếng đã được gỡ
        if (manager != null) manager.OnPiecePeeled();

        // Tắt miếng băng (có thể đổi thành animation bay ra nếu muốn)
        gameObject.SetActive(false);

        Debug.Log($"{gameObject.name} đã được lột ra.");
    }
}