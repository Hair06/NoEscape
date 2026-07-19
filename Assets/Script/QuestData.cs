using UnityEngine;

[System.Serializable]
public class QuestData
{
    [Header("Thông tin chương")]
    public string chapterTitle;

    [TextArea(2, 5)]
    public string characterThought;

    [Header("Thời gian hiển thị nội tâm")]
    [Tooltip("Thời gian chờ từ lúc chương bắt đầu đến khi Character Thought xuất hiện.")]
    [Min(0f)]
    public float characterThoughtDelay = 0f;

    [Tooltip("Thời gian giữ Character Thought sau khi chạy xong hiệu ứng chữ. Đặt -1 để dùng thời gian mặc định trong QuestManager.")]
    [Min(-1f)]
    public float characterThoughtHoldTime = -1f;

    [TextArea(2, 5)]
    public string mainQuest;

    [Header("Chuyển tiếp sang chương kế tiếp")]
    [Tooltip("Bật nếu chương kế tiếp phải chờ cutscene, jumpscare hoặc một sự kiện bên ngoài kết thúc.")]
    public bool waitForTransitionSignal;

    [Tooltip("Khoảng nghỉ sau khi nhận tín hiệu chuyển tiếp rồi mới bắt đầu chương kế tiếp.")]
    [Min(0f)]
    public float nextChapterStartDelay = 0.5f;

    [Tooltip("Thông báo hiện khi toàn bộ mục tiêu của chương hoàn thành.")]
    [TextArea(1, 3)]
    public string chapterCompleteMessage = "Toàn bộ mục tiêu đã hoàn thành...";

    [Header("Cách thực hiện nhiệm vụ nhỏ")]
    [Tooltip("Bật nếu các nhiệm vụ nhỏ trong chương có thể hoàn thành không theo thứ tự.")]
    public bool allowOutOfOrderCompletion;

    [Header("Danh sách nhiệm vụ nhỏ")]
    public SubQuestData[] subQuests;
}
