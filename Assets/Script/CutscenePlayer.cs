using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CutscenePlayer : MonoBehaviour
{
    [Header("UI References")]
    public Image imageA;
    public Image imageB;
    public TextMeshProUGUI dialogueText;
    public AudioSource audioSource;

    [Header("Nhạc nền (tắt khi cutscene chạy)")]
    public AudioSource bgMusic;

    [Header("Frames")]
    public CutsceneFrame[] frames;

    [Header("Settings")]
    public float fadeDuration = 0.8f;
    public float typeSpeed = 0.035f;
    public string nextSceneName = "map";

    [Header("Skip Settings")]
    public float holdSkipTime = 2f;
    public int skipToFrameIndex = 26;

    private int _currentIndex = 0;
    private float _holdTimer = 0f;
    private bool _nextRequested = false;
    private bool _skipRequested = false;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        StartCoroutine(PlayCutscene());
    }

    void Update()
    {
        HandleNextInput();
        HandleHoldSkip();
    }

    void HandleNextInput()
    {
        bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool mouseClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (spacePressed || mouseClicked)
            _nextRequested = true;
    }

    void HandleHoldSkip()
    {
        if (frames == null || frames.Length <= 1) return;

        int safeSkipIndex = GetSafeSkipFrameIndex();

        if (_currentIndex >= safeSkipIndex) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.isPressed)
        {
            _holdTimer += Time.deltaTime;
            if (_holdTimer >= holdSkipTime)
            {
                _skipRequested = true;
                _holdTimer = 0f;
            }
        }
        else
        {
            _holdTimer = 0f;
        }
    }

    IEnumerator PlayCutscene()
    {
        if (frames == null || frames.Length == 0)
        {
            EndCutscene();
            yield break;
        }

        int safeSkipFrameIndex = GetSafeSkipFrameIndex();

        if (skipToFrameIndex != safeSkipFrameIndex)
        {
            Debug.LogWarning(
                "Skip To Frame Index " + skipToFrameIndex +
                " vượt phạm vi Frames. Tự điều chỉnh thành " +
                safeSkipFrameIndex + "."
            );
        }

        // Tắt nhạc nền
        if (bgMusic != null) bgMusic.Pause();

        imageA.sprite = frames[0].image;
        SetAlpha(imageA, 1f);
        SetAlpha(imageB, 0f);
        _currentIndex = 0;

        yield return ShowFrame(frames[0]);

        for (int i = 1; i < frames.Length; i++)
        {
            if (_skipRequested)
            {
                // Không bao giờ cho i vượt quá frames.Length - 1.
                // Mathf.Max tránh nhảy ngược nếu người chơi đã đi qua frame đích.
                i = Mathf.Max(i, safeSkipFrameIndex);
                _skipRequested = false;
            }

            if (i < 0 || i >= frames.Length)
            {
                Debug.LogError(
                    "Frame Index không hợp lệ: " + i +
                    ". Tổng số frame: " + frames.Length
                );
                break;
            }

            _currentIndex = i;
            yield return CrossFade(frames[i]);
            yield return ShowFrame(frames[i]);
        }

        EndCutscene();
    }

    private int GetSafeSkipFrameIndex()
    {
        if (frames == null || frames.Length <= 1)
        {
            return 0;
        }

        // Frame 0 đã được phát riêng trước vòng lặp,
        // nên frame skip hợp lệ nằm từ 1 đến phần tử cuối.
        return Mathf.Clamp(
            skipToFrameIndex,
            1,
            frames.Length - 1
        );
    }

    IEnumerator ShowFrame(CutsceneFrame frame)
    {
        _nextRequested = false;
        dialogueText.text = "";

        PlayVoice(frame.voiceClip);

        foreach (char c in frame.dialogue)
        {
            if (_skipRequested) yield break;

            if (_nextRequested)
            {
                dialogueText.text = frame.dialogue;
                _nextRequested = false;
                break;
            }

            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        float elapsed = 0f;
        while (elapsed < frame.waitTime)
        {
            if (_skipRequested) yield break;
            if (_nextRequested) { _nextRequested = false; yield break; }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator CrossFade(CutsceneFrame frame)
    {
        imageB.sprite = frame.image;
        SetAlpha(imageB, 0f);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            if (_skipRequested) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            SetAlpha(imageA, 1f - t);
            SetAlpha(imageB, t);

            yield return null;
        }

        imageA.sprite = frame.image;
        SetAlpha(imageA, 1f);
        SetAlpha(imageB, 0f);
    }

    void PlayVoice(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    void SetAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    void EndCutscene()
    {
        if (audioSource != null) audioSource.Stop();

        // Bật lại nhạc nền
        if (bgMusic != null) bgMusic.UnPause();

        SceneManager.LoadScene(nextSceneName);
    }
}