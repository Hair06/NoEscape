using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapSealCutscenePlayer : MonoBehaviour
{
    [Header("UI")]
    public Image imageA;
    public Image imageB;
    public TextMeshProUGUI dialogueText;
    public AudioSource audioSource;

    [Header("Nhạc nền (tắt khi cutscene chạy)")]
    public AudioSource bgMusic;

    [Header("Cutscene Root")]
    public GameObject cutsceneRoot;

    [Header("Cutscene Data")]
    public CutsceneFrame[] frames;

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

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
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

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.SetSubQuestHintsSuppressed(true);
        }

        if (cutsceneRoot != null)
            cutsceneRoot.SetActive(true);

        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        isPlaying = true;
        DisableGameplay();

        // Tắt nhạc nền
        if (bgMusic != null) bgMusic.Pause();

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

    private IEnumerator ShowFrame(CutsceneFrame frame)
    {
        nextFrameRequested = false;
        dialogueText.text = "";

        PlayVoice(frame.voiceClip);

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

    private IEnumerator CrossFade(CutsceneFrame frame)
    {
        imageB.sprite = frame.image;
        SetImageAlpha(imageB, 0f);

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);

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

        if (audioSource != null) audioSource.Stop();

        // Bật lại nhạc nền
        if (bgMusic != null) bgMusic.UnPause();

        EnableGameplay();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cutsceneRoot != null)
            cutsceneRoot.SetActive(false);

        if (Chapter1Manager.Instance != null)
            Chapter1Manager.Instance.OnCutsceneFinished();

        if (QuestManager.Instance != null)
        {
            // Mỗi phong ấn hoàn thành nhiệm vụ đang hoạt động của chính chương đó.
            // Chapter 1 là mục 3, Chapter 2 là mục 4; không hard-code chỉ số nữa.
            QuestManager.Instance.CompleteCurrentSubQuest();
            QuestManager.Instance.CompleteCurrentChapter();
            QuestManager.Instance.SetSubQuestHintsSuppressed(
                false,
                false
            );
        }

        if (afterCutsceneScare != null)
            StartCoroutine(StartScareAfterDelay());
        else
            Debug.LogWarning("Chưa gán After Cutscene Scare.");
    }

    private IEnumerator StartScareAfterDelay()
    {
        yield return new WaitForSecondsRealtime(scareDelay);
        if (afterCutsceneScare != null)
            afterCutsceneScare.TriggerScare();
    }

    private void PlayVoice(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    private void DisableGameplay()
    {
        foreach (MonoBehaviour script in scriptsToDisable)
            if (script != null) script.enabled = false;

        foreach (GameObject obj in objectsToHide)
            if (obj != null) obj.SetActive(false);
    }

    private void EnableGameplay()
    {
        foreach (MonoBehaviour script in scriptsToDisable)
            if (script != null) script.enabled = true;

        foreach (GameObject obj in objectsToHide)
            if (obj != null) obj.SetActive(true);
    }

    private void SetImageAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}