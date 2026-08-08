using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class SubQuestData
{
    [Header("Nội dung nhiệm vụ")]
    public string title;

    [Header("Gợi ý tầng 1 - Định hướng")]
    [TextArea(2, 4)]
    public string hint;

    [Header("Gợi ý tầng 2 - Khu vực cần tìm")]
    [Tooltip("Nói rõ khu vực hoặc mốc cảnh quan, nhưng chưa chỉ thẳng thao tác cuối cùng.")]
    [TextArea(2, 4)]
    public string locationHint;

    [Header("Gợi ý tầng 3 - Cách thực hiện")]
    [Tooltip("Nêu rõ chuỗi hành động, phím bấm hoặc điều kiện để hoàn thành mục tiêu.")]
    [FormerlySerializedAs("detailedHint")]
    [TextArea(2, 5)]
    public string actionHint;

    [Header("Thời gian hiển thị Sub Quest Hint")]
    [Tooltip("Thời gian chờ sau khi nhiệm vụ nhỏ được kích hoạt rồi mới tự động hiện gợi ý.")]
    [Min(0f)]
    public float hintShowDelay = 0f;

    [Tooltip("Thời gian giữ bảng gợi ý sau khi chạy xong hiệu ứng chữ. Đặt -1 để dùng thời gian mặc định trong QuestManager.")]
    [Min(-1f)]
    public float hintHoldTime = -1f;

    [Header("Thời gian mở khóa gợi ý thích ứng")]
    [Tooltip("Số giây khám phá thực tế trước khi mở gợi ý khu vực. Đồng hồ dừng khi pause, cutscene hoặc mini game đang mở.")]
    [Min(0f)]
    public float locationHintDelay = 25f;

    [Tooltip("Số giây khám phá thực tế trước khi mở gợi ý cách thực hiện.")]
    [FormerlySerializedAs("detailedHintDelay")]
    [Min(0f)]
    public float actionHintDelay = 55f;
} 
