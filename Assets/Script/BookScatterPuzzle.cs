using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class BookScatterPuzzle : MonoBehaviour
{
    [System.Serializable]
    public class BookMoveInfo
    {
        public Transform bookTransform;       // Kéo Object quyển sách vào đây
        public Vector3 localMoveOffset;       // Hướng dạt ra (Ví dụ: X = 0.4 để dạt sang phải, X = -0.4 dạt sang trái)
        [HideInInspector] public Vector3 targetPosition;
    }

    [Header("Cấu hình UI Text bằng TextMesh Pro")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string interactMessage = "Nhấn [E] để lục đống sách";

    [Header("Tham chiếu Mảnh ảnh 4")]
    [SerializeField] private GameObject puzzlePiece4; // Kéo Object World_Piece_4 vào đây

    [Header("Danh sách các quyển sách cần dạt ra")]
    [SerializeField] private List<BookMoveInfo> booksToScatter;
    [SerializeField] private float scatterSpeed = 3f;

    private bool isPlayerInside = false;
    private bool isInteracted = false;

    private void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);

        // Mặc định ẩn mảnh 4 khi bắt đầu game
        if (puzzlePiece4 != null) puzzlePiece4.SetActive(false);

        // Tính toán tọa độ đích đến cho từng quyển sách dựa theo hướng Local
        foreach (var book in booksToScatter)
        {
            if (book.bookTransform != null)
            {
                book.targetPosition = book.bookTransform.position + book.bookTransform.TransformDirection(book.localMoveOffset);
            }
        }
    }

    private void Update()
    {
        // 1. Nhận diện người chơi nhấn phím E để lục sách
        if (isPlayerInside && !isInteracted && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TriggerScatterEvent();
        }

        // 2. Di chuyển các quyển sách mượt mờ khi đã tương tác
        if (isInteracted)
        {
            bool allBooksMoved = true;

            foreach (var book in booksToScatter)
            {
                if (book.bookTransform != null)
                {
                    book.bookTransform.position = Vector3.Lerp(book.bookTransform.position, book.targetPosition, Time.deltaTime * scatterSpeed);

                    if (Vector3.Distance(book.bookTransform.position, book.targetPosition) > 0.01f)
                    {
                        allBooksMoved = false; 
                    }
                }
            }

            // Kích hoạt mảnh 4 lộ diện
            if (puzzlePiece4 != null && !puzzlePiece4.activeSelf)
            {
                puzzlePiece4.SetActive(true);
                Debug.Log("Sách đã tản ra! Mảnh ảnh cuối cùng lộ diện.");
                
                // TÚT LẠI CHỖ NÀY: Sau khi bật mảnh 4 lên, ta chủ động tắt luôn vùng kích hoạt của đống sách
                // để nhường toàn quyền điều khiển UI chữ [E] lại cho script CollectiblePiece của mảnh ảnh!
                GetComponent<BoxCollider>().enabled = false; 
            }

            if (allBooksMoved)
            {
                enabled = false; // Tắt hoàn toàn script sách khi đã hoàn thành nhiệm vụ
            }
        }
    }

    private void TriggerScatterEvent()
    {
        isInteracted = true;
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isInteracted)
        {
            isPlayerInside = true;
            if (promptText != null)
            {
                promptText.text = interactMessage;
                promptText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (promptText != null) promptText.gameObject.SetActive(false);
        }
    }
}