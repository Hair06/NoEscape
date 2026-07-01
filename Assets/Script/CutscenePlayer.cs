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

    [Header("Frames")]
    public CutsceneFrame[] frames;

    [Header("Settings")]
    public float fadeDuration = 0.8f;
    public float typeSpeed = 0.035f;
    public string nextSceneName = "map";

    [Header("Skip Settings")]
    public float holdSkipTime = 2f;
    public int skipToFrameIndex = 26; // Frame bắt đầu từ 0

    // Private
    private int _currentIndex = 0;
    private float _holdTimer = 0f;
    private bool _nextRequested = false;
    private bool _skipRequested = false;
    private bool _isPlaying = false;

    void Start()
    {
        StartCoroutine(PlayCutscene());
    }

    void Update()
    {
        HandleNextInput();
        HandleHoldSkip();
    }

    // ── Input ──────────────────────────────────────────────

    void HandleNextInput()
    {
        bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool mouseClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (spacePressed || mouseClicked)
            _nextRequested = true;
    }

    void HandleHoldSkip()
    {
        if (frames == null || _currentIndex >= skipToFrameIndex) return;
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

    // ── Playback ───────────────────────────────────────────

    IEnumerator PlayCutscene()
    {
        if (frames == null || frames.Length == 0)
        {
            EndCutscene();
            yield break;
        }

        _isPlaying = true;

        // Frame đầu tiên không cần crossfade
        imageA.sprite = frames[0].image;
        SetAlpha(imageA, 1f);
        SetAlpha(imageB, 0f);
        _currentIndex = 0;

        yield return ShowFrame(frames[0]);

        for (int i = 1; i < frames.Length; i++)
        {
            // Skip tới frame chỉ định
            if (_skipRequested)
            {
                i = skipToFrameIndex;
                _skipRequested = false;
            }

            _currentIndex = i;

            yield return CrossFade(frames[i]);
            yield return ShowFrame(frames[i]);
        }

        EndCutscene();
    }

    IEnumerator ShowFrame(CutsceneFrame frame)
    {
        _nextRequested = false;
        dialogueText.text = "";

        // Play voice
        PlayVoice(frame.voiceClip);

        // Typewriter effect
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

        // Chờ sau khi text hiện xong
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

        // Swap A/B để chuẩn bị frame tiếp theo
        imageA.sprite = frame.image;
        SetAlpha(imageA, 1f);
        SetAlpha(imageB, 0f);
    }

    // ── Helpers ────────────────────────────────────────────

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
        SceneManager.LoadScene(nextSceneName);
    }
}