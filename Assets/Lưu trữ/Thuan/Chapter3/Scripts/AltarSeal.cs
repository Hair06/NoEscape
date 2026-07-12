using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Ban tho co phong an chuong 3.
// Nguoi choi toi gan bam E -> dat het vat pham dang cam len ban tho.
// Du 4 vat pham -> phong an mo -> cutscene.
public class AltarSeal : MonoBehaviour
{
    public static AltarSeal Instance;

    // 4 vat pham can thiet (ten phai khop voi PlayerInventory)
    private static readonly string[] REQUIRED_ITEMS =
    {
        "ConMat",    // Con mat giao phai
        "KiTu",      // Ki tu giao phai
        "TraiTim",   // Trai tim giao phai
        "GiotMau"    // Giot mau giao phai
    };

    [Header("Trạng thái 4 vật phẩm (chỉ xem)")]
    [SerializeField] private bool hasConMat = false;
    [SerializeField] private bool hasKiTu = false;
    [SerializeField] private bool hasTraiTim = false;
    [SerializeField] private bool hasGiotMau = false;

    [Header("UI hướng dẫn (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI promptText;
    [Tooltip("Chữ hiện khi không cầm vật phẩm nào")]
    [SerializeField] private string notReadyMessage = "Bàn thờ cổ... còn thiếu vật phẩm";
    [Tooltip("Chữ hiện khi đang cầm vật phẩm có thể đặt")]
    [SerializeField] private string placeMessage = "Nhấn [E] để đặt vật phẩm lên bàn thờ";

    [Header("Model vật phẩm hiện trên bàn thờ (có thể để trống)")]
    [SerializeField] private GameObject conMatVisual;
    [SerializeField] private GameObject kiTuVisual;
    [SerializeField] private GameObject traiTimVisual;
    [SerializeField] private GameObject giotMauVisual;

    [Header("Âm thanh & Hiệu ứng khi hoàn thành")]
    [Tooltip("Tiếng khi đặt 1 vật phẩm lên bàn")]
    [SerializeField] private AudioSource placeAudio;
    [Tooltip("Tiếng khi đủ 4 vật phẩm, phong ấn mở")]
    [SerializeField] private AudioSource sealCompleteAudio;
    [Tooltip("Khói tím / VFX khi phong ấn mở")]
    [SerializeField] private ParticleSystem sealVFX;

    [Header("Xích cửa mở ra khi đủ 4 vật phẩm")]
    [Tooltip("Kéo object xích khóa cửa vào đây (sẽ tắt đi khi phong ấn mở)")]
    [SerializeField] private GameObject doorChain;

    [Header("Cutscene kết thúc Chapter 3")]
    [Tooltip("Kéo object cutscene (MapSealCutscenePlayer) vào đây")]
    [SerializeField] private MapSealCutscenePlayer endCutscene;

    private bool isPlayerInside = false;
    private bool isComplete = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);

        // Ẩn hết model vật phẩm lúc đầu (chưa đặt gì lên bàn)
        if (conMatVisual != null) conMatVisual.SetActive(false);
        if (kiTuVisual != null) kiTuVisual.SetActive(false);
        if (traiTimVisual != null) traiTimVisual.SetActive(false);
        if (giotMauVisual != null) giotMauVisual.SetActive(false);
    }

    private void Update()
    {
        if (isComplete || !isPlayerInside) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            PlaceItems();
        }
    }

    // Đặt HẾT vật phẩm đang cầm lên bàn thờ (1, 2, hoặc cả 4 đều được)
    private void PlaceItems()
    {
        int placedCount = 0;

        foreach (string item in REQUIRED_ITEMS)
        {
            if (PlayerInventory.Count(item) > 0 && !IsPlaced(item))
            {
                MarkPlaced(item);
                PlayerInventory.RemoveAll(item);   // bỏ khỏi hotbar
                placedCount++;
                Debug.Log("Đã đặt vật phẩm lên bàn thờ: " + item);
            }
        }

        if (placedCount == 0)
        {
            Debug.Log("Bạn không cầm vật phẩm nào để đặt.");
            return;
        }

        if (placeAudio != null) placeAudio.Play();

        Debug.Log($"Tiến độ phong ấn: {CountPlaced()}/4");

        if (promptText != null) promptText.text = GetCurrentPrompt();

        if (CountPlaced() >= 4)
        {
            CompleteSeal();
        }
    }

    private bool IsPlaced(string item)
    {
        switch (item)
        {
            case "ConMat": return hasConMat;
            case "KiTu": return hasKiTu;
            case "TraiTim": return hasTraiTim;
            case "GiotMau": return hasGiotMau;
        }
        return false;
    }

    private void MarkPlaced(string item)
    {
        switch (item)
        {
            case "ConMat":
                hasConMat = true;
                if (conMatVisual != null) conMatVisual.SetActive(true);
                break;
            case "KiTu":
                hasKiTu = true;
                if (kiTuVisual != null) kiTuVisual.SetActive(true);
                break;
            case "TraiTim":
                hasTraiTim = true;
                if (traiTimVisual != null) traiTimVisual.SetActive(true);
                break;
            case "GiotMau":
                hasGiotMau = true;
                if (giotMauVisual != null) giotMauVisual.SetActive(true);
                break;
        }
    }

    private int CountPlaced()
    {
        int n = 0;
        if (hasConMat) n++;
        if (hasKiTu) n++;
        if (hasTraiTim) n++;
        if (hasGiotMau) n++;
        return n;
    }

    public bool IsComplete()
    {
        return hasConMat && hasKiTu && hasTraiTim && hasGiotMau;
    }

    private void CompleteSeal()
    {
        isComplete = true;
        Debug.Log("ĐỦ 4 VẬT PHẨM! Phong ấn đã được mở. Xích cửa tháo ra...");

        if (promptText != null) promptText.gameObject.SetActive(false);

        // Phát tiếng phong ấn mở
        if (sealCompleteAudio != null) sealCompleteAudio.Play();

        // Bùng khói tím
        if (sealVFX != null)
        {
            sealVFX.gameObject.SetActive(true);
            sealVFX.Play();
        }

        // Tháo xích cửa
        if (doorChain != null) doorChain.SetActive(false);

        // Kích hoạt cutscene hồi tưởng kết chương
        if (endCutscene != null) endCutscene.PlayCutscene();
    }

    private string GetCurrentPrompt()
    {
        if (isComplete) return "";

        int holding = 0;
        foreach (string item in REQUIRED_ITEMS)
        {
            if (PlayerInventory.Count(item) > 0 && !IsPlaced(item)) holding++;
        }

        return holding > 0 ? placeMessage : notReadyMessage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isComplete)
        {
            isPlayerInside = true;
            if (promptText != null)
            {
                promptText.text = GetCurrentPrompt();
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