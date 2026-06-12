using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]

public class SealCutsceneFrame
{
    public Sprite image;

    [TextArea(3,10)]
    public string dialogue;

    public float waitTime = 3f;
}

public class MapSealCutscenePlayer : MonoBehaviour
{
    [Header("UI")]
    public Image imageA;
    public Image imageB;
    public TextMeshProUGUI dialogueText;

    [Header("Cutscene Root")]
    public GameObject cutsceneRoot;

    [Header("Cutscene Data")]
    public SealCutsceneFrame[] frames;

    [Header("Effect Settings")]
    public float fadeDuration = 0.8f;
    public float typeSpeed = 0.035f;

    [Header("Disable During Cutscene")]
    public MonoBehaviour[] scriptsToDisable;
    public GameObject[] objectsToHide;

    [Header("After Cutscene Scare")]
    [SerializeField] private PostCutsceneDashScare afterCutsceneScare;
    [Header("Scare Delay")]
    [SerializeField] private float scareDelay = 2f;

    private bool nextFrameRequested;
    private bool isPlaying;

    private void Awake()
    {
        SetImageAlpha(imageA, 1f);
        SetImageAlpha(imageB, 0f);

        if (cutsceneRoot != null)
            cutsceneRoot.SetActive(false);
    }

    private void Update()
    {
        if (!isPlaying) return;

        if (GameInputBridge.GetKeyDown(KeyCode.Space) ||
            GameInputBridge.GetMouseButtonDown(0))
        {
            nextFrameRequested = true;
        }
    }

    public void PlayCutscene()
    {
        if (isPlaying) return;

        if (cutsceneRoot != null)
            cutsceneRoot.SetActive(true);

        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        isPlaying = true;

        DisableGameplay();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (frames == null || frames.Length == 0)
        {
            EndCutscene();
            yield break;
        }

        imageA.sprite = frames[0].image;
        SetImageAlpha(imageA, 1f);
        SetImageAlpha(imageB, 0f);

        yield return ShowFrame(frames[0]);

        for (int i = 1; i < frames.Length; i++)
        {
            yield return CrossFade(frames[i]);
            yield return ShowFrame(frames[i]);
        }

        EndCutscene();
    }

    private IEnumerator ShowFrame(SealCutsceneFrame frame)
    {
        nextFrameRequested = false;
        dialogueText.text = "";

        foreach (char c in frame.dialogue)
        {
            if (nextFrameRequested)
            {
                dialogueText.text = frame.dialogue;
                nextFrameRequested = false;
                break;
            }

            dialogueText.text += c;
            yield return new WaitForSecondsRealtime(typeSpeed);
        }

        float timer = 0f;

        while (timer < frame.waitTime)
        {
            if (nextFrameRequested)
            {
                nextFrameRequested = false;
                yield break;
            }

            timer += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private IEnumerator CrossFade(SealCutsceneFrame frame)
    {
        imageB.sprite = frame.image;
        SetImageAlpha(imageB, 0f);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / fadeDuration;

            SetImageAlpha(imageA, 1f - t);
            SetImageAlpha(imageB, t);

            yield return null;
        }

        imageA.sprite = frame.image;
        SetImageAlpha(imageA, 1f);
        SetImageAlpha(imageB, 0f);
    }

    private void EndCutscene()
    {
        isPlaying = false;

        EnableGameplay();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cutsceneRoot != null)
            cutsceneRoot.SetActive(false);

        if (Chapter1Manager.Instance != null)
            Chapter1Manager.Instance.OnCutsceneFinished();

        Debug.Log("Cutscene kết thúc thành công! Trả lại quyền điều khiển cho Player.");

        if (afterCutsceneScare != null)
{
        Debug.Log("Jumpscare sẽ chạy sau 2 giây.");
        StartCoroutine(StartScareAfterDelay());
}
        else
        {
            Debug.LogWarning("Chưa gán After Cutscene Scare trong MapSealCutscenePlayer.");
        }
    }

    private void DisableGameplay()
    {
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null)
                script.enabled = false;
        }

        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    private void EnableGameplay()
    {
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null)
                script.enabled = true;
        }

        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }

    private void SetImageAlpha(Image img, float alpha)
    {
        if (img == null) return;

        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
    private IEnumerator StartScareAfterDelay()
    {
        yield return new WaitForSeconds(scareDelay);

        if (afterCutsceneScare != null)
        {
            Debug.Log("Bắt đầu Ghoul Jumpscare.");
            afterCutsceneScare.TriggerScare();
        }
    }
}