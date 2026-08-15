using TMPro;
using UnityEngine;

public class SoulJar : MonoBehaviour
{
    private const int ChapterIndex = 4;
    private const int PickupSubQuestIndex = 2;

    [Header("Cấu hình Bình Linh Hồn")]
    [Tooltip("Kéo đúng Bệ Cổ ngoài khu phong ấn vào đây. Nếu để trống, code sẽ tự tìm.")]
    public RitualAltar targetAltar;

    [Header("Giao diện UI")]
    public TextMeshProUGUI promptText;
    public string promptMessage =
        "Nhấn [E] để nhặt Bình Linh Hồn";

    private bool isNearPlayer;
    private bool collected;

    private void Start()
    {
        if (targetAltar == null)
        {
            RitualAltar[] altars = FindObjectsByType<RitualAltar>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

            foreach (RitualAltar altar in altars)
            {
                if (altar != null && altar.IsConfiguredForSoulJar)
                {
                    targetAltar = altar;
                    break;
                }
            }
        }

        if (targetAltar == null)
        {
            Debug.LogError(
                "[SoulJar] Không tìm thấy RitualAltar trong Scene."
            );
        }

        SetPromptVisible(false);
    }

    private void Update()
    {
        // PromptText là UI dùng chung, nên object ở xa Player không được
        // tắt prompt của một tương tác khác trong mỗi frame.
        if (!isNearPlayer)
        {
            return;
        }

        bool canCollect = isNearPlayer && CanCollect();
        SetPromptVisible(canCollect);

        if (canCollect && GameInputBridge.GetKeyDown(KeyCode.E))
        {
            PickUpJar();
        }
    }

    private bool CanCollect()
    {
        QuestManager questManager = QuestManager.Instance;

        return !collected &&
               targetAltar != null &&
               questManager != null &&
               MiniGameFlowManager.IsChapterActive(ChapterIndex) &&
               questManager.CurrentChapterIndex == ChapterIndex &&
               questManager.CurrentSubQuestIndex == PickupSubQuestIndex &&
               !questManager.IsChapterTransitioning;
    }

    private void PickUpJar()
    {
        if (!CanCollect())
        {
            return;
        }

        collected = true;
        targetAltar.OnPickUpJar();

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteSubQuestForChapter(
                ChapterIndex,
                PickupSubQuestIndex
            );
        }

        Debug.Log(
            "[SoulJar] Đã nhặt Bình Linh Hồn và hoàn thành nhiệm vụ Chương 4 / bước 3."
        );

        SetPromptVisible(false);
        gameObject.SetActive(false);
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptText == null)
        {
            return;
        }

        if (visible)
        {
            promptText.text = promptMessage;
        }

        promptText.gameObject.SetActive(visible);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNearPlayer = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        isNearPlayer = false;
        SetPromptVisible(false);
    }

    private void OnDisable()
    {
        SetPromptVisible(false);
    }
}
