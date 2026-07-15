using UnityEngine;

[System.Serializable]
public class QuestData
{
    [Header("Thông tin chương")]
    public string chapterTitle;

    [TextArea(2, 5)]
    public string characterThought;

    [TextArea(2, 5)]
    public string mainQuest;

    [Header("Cách thực hiện nhiệm vụ nhỏ")]
    [Tooltip("Bật nếu các nhiệm vụ nhỏ trong chương có thể hoàn thành không theo thứ tự.")]
    public bool allowOutOfOrderCompletion;

    [Header("Danh sách nhiệm vụ nhỏ")]
    public SubQuestData[] subQuests;
}