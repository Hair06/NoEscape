using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MusicBoxRestore : MonoBehaviour, IInteractable
{
    public static MusicBoxRestore Instance;

    // 4 bộ phận của hộp nhạc
    public enum MusicBoxPart
    {
        Shuttle,   // Con Thoi Nhạc
        Spring,    // Lò Xo Nhạc
        Disc,      // Đĩa Nhạc
        WindKey    // Chìa Vặn
    }

    [Header("Trạng thái 4 bộ phận (chỉ xem)")]
    [SerializeField] private bool hasShuttle = false;
    [SerializeField] private bool hasSpring = false;
    [SerializeField] private bool hasDisc = false;
    [SerializeField] private bool hasWindKey = false;

    [Header("UI hướng dẫn (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI promptText;
    [Tooltip("Chữ hiện khi chưa đủ bộ phận")]
    [SerializeField] private string notReadyMessage = "Hộp nhạc còn thiếu bộ phận...";
    [Tooltip("Chữ hiện khi đã đủ 4 bộ phận")]
    [SerializeField] private string readyMessage = "Nhấn [E] để lắp ráp hộp nhạc";

    [Header("Âm thanh & Hiệu ứng khi hoàn thành")]
    [SerializeField] private AudioSource musicBoxAudio;   // Giai điệu hộp nhạc
    [SerializeField] private GameObject brokenVisual;     // Model hộp vỡ (tắt khi xong)
    [SerializeField] private GameObject fixedVisual;      // Model hộp đã sửa (bật khi xong)

    [Header("Cutscene kết thúc Chapter 2")]
    [Tooltip("Kéo Object cutscene ký ức vào đây (vd MapSealCutscenePlayer)")]
    [SerializeField] private MapSealCutscenePlayer endCutscene;

    private bool isPlayerInside = false;
    private bool isAssembled = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);

        // Đảm bảo hiển thị đúng model lúc bắt đầu
        if (brokenVisual != null) brokenVisual.SetActive(true);
        if (fixedVisual != null) fixedVisual.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerInside && !isAssembled
            && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    // Hàm cho các script bộ phận gọi về khi thu thập xong
    public void CollectPart(MusicBoxPart part)
    {
        switch (part)
        {
            case MusicBoxPart.Shuttle: hasShuttle = true; break;
            case MusicBoxPart.Spring: hasSpring = true; break;
            case MusicBoxPart.Disc: hasDisc = true; break;
            case MusicBoxPart.WindKey: hasWindKey = true; break;
        }

        Debug.Log($"Đã thu thập bộ phận: {part}. Tiến độ: {CountParts()}/4");
    }

    private int CountParts()
    {
        int count = 0;
        if (hasShuttle) count++;
        if (hasSpring) count++;
        if (hasDisc) count++;
        if (hasWindKey) count++;
        return count;
    }

    public bool IsComplete()
    {
        return hasShuttle && hasSpring && hasDisc && hasWindKey;
    }

    // --- IInteractable: dùng được cả với PlayerInteraction nếu muốn ---
    public string GetInteractPrompt()
    {
        if (isAssembled) return "";
        return IsComplete() ? readyMessage : notReadyMessage;
    }

    public void Interact()
    {
        if (isAssembled) return;

        if (!IsComplete())
        {
            Debug.Log($"Chưa đủ bộ phận! Hiện có {CountParts()}/4");
            return;
        }

        AssembleMusicBox();
    }

    private void AssembleMusicBox()
    {
        isAssembled = true;
        Debug.Log("Hộp nhạc đã được khôi phục! Giai điệu vang lên...");

        if (promptText != null) promptText.gameObject.SetActive(false);

        // Đổi model vỡ -> model hoàn chỉnh
        if (brokenVisual != null) brokenVisual.SetActive(false);
        if (fixedVisual != null) fixedVisual.SetActive(true);

        // Phát giai điệu hộp nhạc
        if (musicBoxAudio != null) musicBoxAudio.Play();

        // Kích hoạt cutscene ký ức kết thúc Chapter 2
        if (endCutscene != null) endCutscene.PlayCutscene();
    }

    // --- Phát hiện Player đến gần ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            if (promptText != null && !isAssembled)
            {
                promptText.text = GetInteractPrompt();
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