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

    [Header("Rải ngẫu nhiên mỗi lần mở")]
    [Tooltip("Bán kính (pixel) vùng rải quanh tâm Panel")]
    [SerializeField] private float scatterRadius = 180f;
    [Tooltip("Góc nghiêng ngẫu nhiên tối đa (độ)")]
    [SerializeField] private float maxRandomTilt = 35f;

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
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    // Manager gọi khi mở mini game: rải miếng băng ra vị trí + góc ngẫu nhiên
    public void ScatterRandom()
    {
        if (isPeeled) return;

        // Vị trí ngẫu nhiên trong vùng tròn quanh tâm Panel
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float dist = Random.Range(0f, scatterRadius);
        Vector2 pos = new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);
        rectTransform.anchoredPosition = pos;

        // Góc nghiêng ngẫu nhiên cho tự nhiên
        float tilt = Random.Range(-maxRandomTilt, maxRandomTilt);
        rectTransform.localRotation = Quaternion.Euler(0, 0, tilt);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (PauseMenu.IsPaused) return;
        if (isPeeled) return;

        isDragging = true;
        startMousePos = eventData.position;
        startAnchoredPos = rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (PauseMenu.IsPaused) return;
        if (isPeeled || !isDragging) return;

        Vector2 dragDelta = eventData.position - startMousePos;

        // Miếng băng dịch theo chuột một phần để trông như đang bị bóc
        rectTransform.anchoredPosition = startAnchoredPos + dragDelta * followAmount;

        // Mờ dần theo quãng kéo
        float progress = Mathf.Clamp01(dragDelta.magnitude / peelDistance);
        canvasGroup.alpha = 1f - progress * 0.5f;

        // Đủ quãng -> bong ra
        if (dragDelta.magnitude >= peelDistance)
        {
            Peel();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (PauseMenu.IsPaused)
        {
            isDragging = false;
            return;
        }

        if (isPeeled) return;

        isDragging = false;

        // Chưa kéo đủ -> đàn hồi về chỗ cũ (giữ nguyên góc nghiêng random)
        rectTransform.anchoredPosition = startAnchoredPos;
        canvasGroup.alpha = 1f;
    }

    private void Peel()
    {
        isPeeled = true;
        isDragging = false;

        if (peelAudio != null) peelAudio.Play();

        if (manager != null) manager.OnPiecePeeled();

        gameObject.SetActive(false);

        Debug.Log($"{gameObject.name} đã được lột ra.");
    }
}
