using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Một mảnh vỡ của đĩa nhạc. Kéo-thả vào đúng ô đích + (tùy chọn) xoay đúng góc.
public class DiscPiece : MonoBehaviour,
    IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Ô đích tương ứng")]
    [Tooltip("Kéo RectTransform ô đích của mảnh này vào đây")]
    [SerializeField] private RectTransform targetSlot;
    [SerializeField] private float snapDistance = 50f;

    [Header("Có cần xoay đúng góc không?")]
    [Tooltip("Bật = phải xoay mảnh về 0 độ mới ghép được. Tắt = chỉ cần đúng vị trí.")]
    [SerializeField] private bool requireRotation = false;

    [Header("Vùng rải ngẫu nhiên")]
    [Tooltip("Bán kính (pixel) vùng rải quanh tâm Panel")]
    [SerializeField] private float scatterRadius = 320f;
    [Tooltip("Khoảng cách tối thiểu phải cách tâm để không trúng đích sẵn")]
    [SerializeField] private float minDistanceFromCenter = 140f;

    // Manager gán giá trị này khi khởi tạo
    [HideInInspector] public DiscPuzzleManager manager;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private bool isSnapped = false;
    private int correctRotation = 0;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        originalPosition = rectTransform.anchoredPosition;
    }

    // Manager gọi hàm này khi mở mini game: rải mảnh ra vị trí ngẫu nhiên
    public void ScatterRandom()
    {
        if (isSnapped) return;

        // Chọn 1 điểm ngẫu nhiên trong vành khăn (tránh vùng giữa = đích)
        Vector2 pos;
        int safety = 0;
        do
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(minDistanceFromCenter, scatterRadius);
            pos = new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);
            safety++;
        }
        while (pos.magnitude < minDistanceFromCenter && safety < 20);

        rectTransform.anchoredPosition = pos;
        originalPosition = pos; // trả về đây nếu kéo sai

        // Xoay ngẫu nhiên nếu bật yêu cầu xoay
        if (requireRotation)
        {
            int[] angles = { 90, 180, 270 };
            rectTransform.eulerAngles = new Vector3(0, 0, angles[Random.Range(0, angles.Length)]);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (PauseMenu.IsPaused) return;
        if (isSnapped) return;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (PauseMenu.IsPaused) return;
        if (isSnapped) return;
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

        float distance = Vector2.Distance(rectTransform.position, targetSlot.position);

        bool rotationOK = !requireRotation
            || Mathf.DeltaAngle(rectTransform.eulerAngles.z, correctRotation) == 0;

        if (distance < snapDistance && rotationOK)
        {
            rectTransform.position = targetSlot.position;
            if (requireRotation) rectTransform.eulerAngles = Vector3.zero;
            isSnapped = true;
            canvasGroup.blocksRaycasts = false;
            Debug.Log($"{gameObject.name} đã vào đúng vị trí!");

            if (manager != null) manager.CheckComplete();
        }
        else
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (PauseMenu.IsPaused) return;
        if (isSnapped || !requireRotation) return;
        if (eventData.button == PointerEventData.InputButton.Right)
            rectTransform.Rotate(0, 0, 90f);
    }

    public bool IsSnapped() => isSnapped;
}
