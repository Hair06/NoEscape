using UnityEngine;
// Bắt buộc phải có thư viện này để dùng Input System mới
using UnityEngine.InputSystem; 

public class PianoInteract : MonoBehaviour
{
    [Header("Cấu hình UI Text hướng dẫn")]
    [SerializeField] private GameObject promptCanvasObject; // Hiện chữ: "Nhấn [E] để đánh đàn"

    [Header("Tham chiếu Mảnh ảnh 3D")]
    [SerializeField] private GameObject puzzlePiece1;       // Kéo Object World_Piece_1 vào đây

    [Header("Âm thanh")]
    [SerializeField] private AudioSource pianoAudio;        // Thành phần phát tiếng đàn piano

    private bool isPlayerInside = false;
    private bool isPianoPlayed = false;

    private void Awake()
    {
        if (promptCanvasObject != null) promptCanvasObject.SetActive(false);
    }

    private void Update()
    {
        // SỬA TẠI ĐÂY: Dùng Keyboard.current để check phím theo hệ thống mới thay cho Input.GetKeyDown
        if (isPlayerInside && !isPianoPlayed && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            PlayPiano();
        }
    }

    private void PlayPiano()
    {
        isPianoPlayed = true;
        
        if (promptCanvasObject != null) promptCanvasObject.SetActive(false);

        if (pianoAudio != null) pianoAudio.Play();

        if (puzzlePiece1 != null)
        {
            puzzlePiece1.SetActive(true);
            Debug.Log("Tiếng đàn vang lên! Mảnh ảnh 1 đã xuất hiện trên phím đàn.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPianoPlayed)
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