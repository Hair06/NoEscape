using UnityEngine;

[System.Serializable]
public class SubQuestData
{
    [Header("Nội dung nhiệm vụ")]
    public string title;

    [Header("Gợi ý ban đầu")]
    [TextArea(2, 4)]
    public string hint;

    [Header("Gợi ý chi tiết")]
    [TextArea(2, 4)]
    public string detailedHint;

    [Header("Thời gian hiển thị Sub Quest Hint")]
    [Tooltip("Thời gian chờ tính từ lúc nhiệm vụ nhỏ được kích hoạt rồi mới tự động hiện gợi ý.")]
    [Min(0f)]
    public float hintShowDelay = 5f;

    [Tooltip("Thời gian giữ bảng gợi ý sau khi chạy xong hiệu ứng chữ. Đặt -1 để dùng thời gian mặc định trong QuestManager.")]
    [Min(-1f)]
    public float hintHoldTime = -1f;

    [Header("Thời gian mở gợi ý chi tiết")]
    [Min(0f)]
    public float detailedHintDelay = 45f;
} 
