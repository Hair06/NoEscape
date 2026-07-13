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

    [Header("Danh sách nhiệm vụ nhỏ")]
    public SubQuestData[] subQuests;
}