using UnityEngine;

[System.Serializable]
public class QuestData
{
    public string chapterTitle;

    [TextArea(2, 5)]
    public string mainQuest;

    [TextArea(1, 3)]
    public string[] subQuests;
}