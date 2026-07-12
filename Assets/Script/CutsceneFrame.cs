using UnityEngine;

[System.Serializable]
public class CutsceneFrame
{
    public Sprite image;
    [TextArea(3, 10)]
    public string dialogue;
    public float waitTime = 3f;
    public AudioClip voiceClip;
}