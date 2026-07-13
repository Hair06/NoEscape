using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MusicBoxRestore : MonoBehaviour, IInteractable
{
    public static MusicBoxRestore Instance;

    public enum MusicBoxPart
    {
        Shuttle,
        Spring,
        Disc,
        WindKey
    }

    private const int ChapterIndex = 2;

    [Header("Trạng thái 4 bộ phận (chỉ xem)")]
    [SerializeField] private bool hasShuttle = false;
    [SerializeField] private bool hasSpring = false;
    [SerializeField] private bool hasDisc = false;
    [SerializeField] private bool hasWindKey = false;

    [Header("UI hướng dẫn (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string notReadyMessage = "Hộp nhạc còn thiếu bộ phận...";
    [SerializeField] private string readyMessage = "Nhấn [E] để lắp ráp hộp nhạc";

    [Header("Âm thanh & Hiệu ứng khi hoàn thành")]
    [SerializeField] private AudioSource musicBoxAudio;
    [SerializeField] private GameObject brokenVisual;
    [SerializeField] private GameObject fixedVisual;

    [Header("Cutscene kết thúc Chapter 2")]
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
        if (brokenVisual != null) brokenVisual.SetActive(true);
        if (fixedVisual != null) fixedVisual.SetActive(false);

        if (musicBoxAudio == null)
            Debug.LogError("MusicBoxRestore: musicBoxAudio chưa được gán!");
        else if (musicBoxAudio.clip == null)
            Debug.LogError("MusicBoxRestore: Audio Source chưa có clip!");
    }

    private void Update()
    {
        if (isPlayerInside && !isAssembled
            && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    public void CollectPart(MusicBoxPart part)
    {
        if (HasPart(part))
        {
            Debug.LogWarning("Bộ phận đã được thu thập trước đó: " + part);
            return;
        }

        switch (part)
        {
            case MusicBoxPart.Shuttle: hasShuttle = true; break;
            case MusicBoxPart.Spring: hasSpring = true; break;
            case MusicBoxPart.Disc: hasDisc = true; break;
            case MusicBoxPart.WindKey: hasWindKey = true; break;
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteSubQuestForChapter(
                ChapterIndex,
                GetSubQuestIndex(part)
            );
        }

        Debug.Log($"Đã thu thập: {part} | Tiến độ: {CountParts()}/4");
    }

    private bool HasPart(MusicBoxPart part)
    {
        switch (part)
        {
            case MusicBoxPart.Shuttle: return hasShuttle;
            case MusicBoxPart.Spring: return hasSpring;
            case MusicBoxPart.Disc: return hasDisc;
            case MusicBoxPart.WindKey: return hasWindKey;
            default: return false;
        }
    }

    private int GetSubQuestIndex(MusicBoxPart part)
    {
        switch (part)
        {
            case MusicBoxPart.Shuttle: return 0;
            case MusicBoxPart.Spring: return 1;
            case MusicBoxPart.Disc: return 2;
            case MusicBoxPart.WindKey: return 3;
            default: return -1;
        }
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

    public bool IsComplete() =>
        hasShuttle && hasSpring && hasDisc && hasWindKey;

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
        Debug.Log("Hộp nhạc đã được khôi phục!");

        PlayerInventory.RemoveAll("ConThoi");
        PlayerInventory.RemoveAll("LoXo");
        PlayerInventory.RemoveAll("DiaNhac");
        PlayerInventory.RemoveAll("ChiaVan");

        if (promptText != null) promptText.gameObject.SetActive(false);
        if (brokenVisual != null) brokenVisual.SetActive(false);
        if (fixedVisual != null) fixedVisual.SetActive(true);

        if (musicBoxAudio != null)
        {
            Debug.Log($"Playing clip: {musicBoxAudio.clip?.name} | Volume: {musicBoxAudio.volume}");
            musicBoxAudio.Play();
        }
        else
        {
            Debug.LogError("musicBoxAudio là null — chưa gán trong Inspector!");
        }

        // Nhiệm vụ lắp ráp được xác nhận khi người chơi xem hết cutscene.
        // MapSealCutscenePlayer sẽ hoàn thành đúng sub quest đang hoạt động (mục 4).
        if (endCutscene != null)
        {
            endCutscene.PlayCutscene();
        }
        else
        {
            Debug.LogWarning("endCutscene chưa được gán! Hoàn thành nhiệm vụ Chapter 2 trực tiếp.");

            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.CompleteSubQuestForChapter(
                    ChapterIndex,
                    4
                );
                QuestManager.Instance.CompleteCurrentChapter();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = true;
        if (promptText != null && !isAssembled)
        {
            promptText.text = GetInteractPrompt();
            promptText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = false;
        if (promptText != null) promptText.gameObject.SetActive(false);
    }
}