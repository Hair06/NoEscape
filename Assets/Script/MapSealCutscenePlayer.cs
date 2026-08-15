using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapSealCutscenePlayer : MonoBehaviour
{
    [Header("UI")]
    public Image imageA;
    public Image imageB;
    public TextMeshProUGUI dialogueText;
    public AudioSource audioSource;

    [Header("Nhạc nền tắt khi cutscene chạy")]
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

    [Header("Quest Transition")]
    [Tooltip(
        "Bật để hoàn thành nhiệm vụ hiện tại sau khi cutscene kết thúc.")]
    [SerializeField]
    private bool completeQuestOnEnd = true;

    [Tooltip(
        "Tắt ở cutscene cuối Chương 4 vì không còn Chương 5.")]
    [SerializeField]
    private bool startNextChapterOnEnd = true;

    [Tooltip(
        "Chỉ bật ở cutscene cần báo lại cho Chapter1Manager.")]
    [SerializeField]
    private bool notifyChapterOneManagerOnEnd = true;

    [Header("After Cutscene Scare")]
    [SerializeField]
    private PostCutsceneDashScare afterCutsceneScare;

    [Header("Scare Delay")]
    [SerializeField] private float scareDelay = 2f;

    private bool nextFrameRequested;
    private bool isPlaying;

    private void Awake()
    {
        SetImageAlpha(imageA, 1f);
        SetImageAlpha(imageB, 0f);

        if (cutsceneRoot != null)
        {
            cutsceneRoot.SetActive(false);
        }
    }

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        if (GameInputBridge.GetKeyDown(KeyCode.Space) ||
            GameInputBridge.GetMouseButtonDown(0))
        {
            nextFrameRequested = true;
        }
    }

    public void PlayCutscene()
    {
        if (isPlaying)
        {
            return;
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.SetGameplayUiSuppressed(true);
        }

        if (cutsceneRoot != null)
        {
            cutsceneRoot.SetActive(true);
        }

        StartCoroutine(PlayRoutine());
    }

    public void PlayFinalChapterCutscene()
    {
        // Cutscene cuối luôn hoàn thành nhiệm vụ hiện tại, không mở Chương 5,
        // không gọi logic riêng của Chương 1 và không nối sang jumpscare.
        completeQuestOnEnd = true;
        startNextChapterOnEnd = false;
        notifyChapterOneManagerOnEnd = false;
        afterCutsceneScare = null;

        PlayCutscene();
    }

    private IEnumerator PlayRoutine()
    {
        isPlaying = true;
        DisableGameplay();

        if (bgMusic != null)
        {
            bgMusic.Pause();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (frames == null || frames.Length == 0)
        {
            EndCutscene();
            yield break;
        }

        if (imageA != null)
        {
            imageA.sprite = frames[0].image;
        }

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

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        PlayVoice(frame.voiceClip);

        string dialogue = frame.dialogue ?? "";

        foreach (char character in dialogue)
        {
            if (nextFrameRequested)
            {
                if (dialogueText != null)
                {
                    dialogueText.text = dialogue;
                }

                nextFrameRequested = false;
                break;
            }

            if (dialogueText != null)
            {
                dialogueText.text += character;
            }

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
        if (imageA == null || imageB == null)
        {
            if (imageA != null)
            {
                imageA.sprite = frame.image;
            }

            yield break;
        }

        imageB.sprite = frame.image;
        SetImageAlpha(imageB, 0f);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float normalizedTime = fadeDuration <= 0f
                ? 1f
                : Mathf.Clamp01(timer / fadeDuration);

            SetImageAlpha(imageA, 1f - normalizedTime);
            SetImageAlpha(imageB, normalizedTime);

            yield return null;
        }

        imageA.sprite = frame.image;
        SetImageAlpha(imageA, 1f);
        SetImageAlpha(imageB, 0f);
    }

    private void EndCutscene()
    {
        isPlaying = false;

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        if (bgMusic != null)
        {
            bgMusic.UnPause();
        }

        EnableGameplay();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cutsceneRoot != null)
        {
            cutsceneRoot.SetActive(false);
        }

        if (notifyChapterOneManagerOnEnd &&
            Chapter1Manager.Instance != null)
        {
            Chapter1Manager.Instance.OnCutsceneFinished();
        }

        QuestManager questManager = QuestManager.Instance;

        if (questManager != null && completeQuestOnEnd)
        {
            questManager.CompleteCurrentSubQuest();
            questManager.CompleteCurrentChapter();
        }

        if (afterCutsceneScare != null)
        {
            StartCoroutine(StartScareAfterDelay());
            return;
        }

        if (questManager != null)
        {
            questManager.SetGameplayUiSuppressed(
                false,
                false
            );

            if (startNextChapterOnEnd)
            {
                questManager.RequestStartNextChapter();
            }
        }
    }

    private IEnumerator StartScareAfterDelay()
    {
        yield return new WaitForSecondsRealtime(scareDelay);

        if (afterCutsceneScare != null)
        {
            afterCutsceneScare.TriggerScare();
        }
    }

    private void PlayVoice(AudioClip clip)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    private void DisableGameplay()
    {
        if (scriptsToDisable != null)
        {
            foreach (MonoBehaviour script in scriptsToDisable)
            {
                if (script != null)
                {
                    script.enabled = false;
                }
            }
        }

        if (objectsToHide != null)
        {
            foreach (GameObject targetObject in objectsToHide)
            {
                if (targetObject != null)
                {
                    targetObject.SetActive(false);
                }
            }
        }
    }

    private void EnableGameplay()
    {
        if (scriptsToDisable != null)
        {
            foreach (MonoBehaviour script in scriptsToDisable)
            {
                if (script != null)
                {
                    script.enabled = true;
                }
            }
        }

        if (objectsToHide != null)
        {
            foreach (GameObject targetObject in objectsToHide)
            {
                if (targetObject != null)
                {
                    targetObject.SetActive(true);
                }
            }
        }
    }

    private static void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
        {
            return;
        }

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
