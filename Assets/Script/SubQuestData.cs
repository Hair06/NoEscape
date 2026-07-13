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

    [Header("Thời gian mở gợi ý chi tiết")]
    [Min(0f)]
    public float detailedHintDelay = 45f;
} 