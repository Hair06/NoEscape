using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private const int AssembleSubQuestIndex = 4;

    [Header("Trạng thái 4 bộ phận (chỉ xem)")]
    [SerializeField] private bool hasShuttle = false;
    [SerializeField] private bool hasSpring = false;
    [SerializeField] private bool hasDisc = false;
    [SerializeField] private bool hasWindKey = false;

    [Header("UI hướng dẫn (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI promptText;

    [SerializeField]
    private string notReadyMessage =
        "Hộp nhạc còn thiếu bộ phận...";

    [SerializeField]
    private string readyMessage =
        "Nhấn [E] để lắp ráp hộp nhạc";

    [Header("Âm thanh và hiệu ứng khi hoàn thành")]
    [SerializeField] private AudioSource musicBoxAudio;

    [Tooltip("Thời gian nhạc hộp nhạc phát từ lúc cutscene bắt đầu.")]
    [SerializeField, Min(0.1f)]
    private float musicDuringCutsceneDuration = 3f;

    [SerializeField] private GameObject brokenVisual;
    [SerializeField] private GameObject fixedVisual;

    [Header("Cutscene kết thúc Chapter 2")]
    [SerializeField] private MapSealCutscenePlayer endCutscene;

    private bool isPlayerInside = false;
    private bool isAssembled = false;
    private Coroutine musicRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }

        if (brokenVisual != null)
        {
            brokenVisual.SetActive(true);
        }

        if (fixedVisual != null)
        {
            fixedVisual.SetActive(false);
        }

        if (musicBoxAudio == null)
        {
            Debug.LogError(
                "MusicBoxRestore: Music Box Audio chưa được gán!"
            );
        }
        else if (musicBoxAudio.clip == null)
        {
            Debug.LogError(
                "MusicBoxRestore: AudioSource chưa có AudioClip!"
            );
        }
    }

    private void Update()
    {
        if (!isPlayerInside || isAssembled)
        {
            return;
        }

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    public void CollectPart(MusicBoxPart part)
    {
        if (HasPart(part))
        {
            Debug.LogWarning(
                "Bộ phận đã được thu thập trước đó: " + part
            );
            return;
        }

        switch (part)
        {
            case MusicBoxPart.Shuttle:
                hasShuttle = true;
                break;

            case MusicBoxPart.Spring:
                hasSpring = true;
                break;

            case MusicBoxPart.Disc:
                hasDisc = true;
                break;

            case MusicBoxPart.WindKey:
                hasWindKey = true;
                break;
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteSubQuestForChapter(
                ChapterIndex,
                GetSubQuestIndex(part)
            );
        }
        else
        {
            Debug.LogWarning(
                "MusicBoxRestore: Không tìm thấy QuestManager."
            );
        }

        Debug.Log(
            $"Đã thu thập: {part} | Tiến độ: {CountParts()}/4"
        );
    }

    private bool HasPart(MusicBoxPart part)
    {
        switch (part)
        {
            case MusicBoxPart.Shuttle:
                return hasShuttle;

            case MusicBoxPart.Spring:
                return hasSpring;

            case MusicBoxPart.Disc:
                return hasDisc;

            case MusicBoxPart.WindKey:
                return hasWindKey;

            default:
                return false;
        }
    }

    private int GetSubQuestIndex(MusicBoxPart part)
    {
        switch (part)
        {
            case MusicBoxPart.Shuttle:
                return 0;

            case MusicBoxPart.Spring:
                return 1;

            case MusicBoxPart.Disc:
                return 2;

            case MusicBoxPart.WindKey:
                return 3;

            default:
                return -1;
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

    public bool IsComplete()
    {
        return hasShuttle &&
               hasSpring &&
               hasDisc &&
               hasWindKey;
    }

    public string GetInteractPrompt()
    {
        if (isAssembled)
        {
            return "";
        }

        return IsComplete()
            ? readyMessage
            : notReadyMessage;
    }

    public void Interact()
    {
        if (isAssembled)
        {
            return;
        }

        if (!IsComplete())
        {
            Debug.Log(
                $"Chưa đủ bộ phận! Hiện có {CountParts()}/4"
            );
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

        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }

        if (brokenVisual != null)
        {
            brokenVisual.SetActive(false);
        }

        if (fixedVisual != null)
        {
            fixedVisual.SetActive(true);
        }

        // Bắt đầu phát nhạc trước.
        // StartCoroutine chạy đến lệnh yield đầu tiên ngay trong frame này.
        if (musicBoxAudio != null &&
            musicBoxAudio.clip != null)
        {
            musicRoutine = StartCoroutine(
                PlayMusicDuringCutscene()
            );
        }
        else
        {
            Debug.LogWarning(
                "MusicBoxRestore: Không thể phát nhạc vì chưa gán AudioSource hoặc AudioClip."
            );
        }

        // Cutscene bắt đầu trong cùng frame với âm thanh hộp nhạc.
        StartEndCutscene();
    }

    private IEnumerator PlayMusicDuringCutscene()
    {
        musicBoxAudio.Stop();
        musicBoxAudio.loop = false;
        musicBoxAudio.Play();

        Debug.Log(
            "Bắt đầu phát nhạc hộp nhạc trong 3 giây đầu cutscene."
        );

        yield return new WaitForSecondsRealtime(
            musicDuringCutsceneDuration
        );

        if (musicBoxAudio != null)
        {
            musicBoxAudio.Stop();
        }

        musicRoutine = null;

        Debug.Log(
            "Đã dừng nhạc hộp nhạc sau 3 giây."
        );
    }

    private void StartEndCutscene()
    {
        if (endCutscene != null)
        {
            endCutscene.PlayCutscene();
            return;
        }

        Debug.LogWarning(
            "End Cutscene chưa được gán. Hoàn thành Chương 2 trực tiếp."
        );

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteSubQuestForChapter(
                ChapterIndex,
                AssembleSubQuestIndex
            );

            QuestManager.Instance.CompleteCurrentChapter();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        isPlayerInside = true;

        if (promptText != null && !isAssembled)
        {
            promptText.text = GetInteractPrompt();
            promptText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        isPlayerInside = false;

        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }
}