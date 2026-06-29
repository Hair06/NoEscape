using UnityEngine;

[System.Serializable]
public class CutsceneFrame
{
    public Sprite image;
    [TextArea(3, 10)]
    public string dialogue;
    [Header("Thời gian giữ sau khi chữ chạy xong")]
    public float waitTime = 3f;
    [Header("Âm thanh lồng tiếng cho frame này")]
    public AudioClip voiceClip;
}