using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CutscenePlayer : MonoBehaviour
{
    public Image imageA;
    public Image imageB;
    public TextMeshProUGUI dialogueText;

    public CutsceneFrame[] frames;

    public float fadeDuration = 0.8f;
    public float typeSpeed = 0.035f;
    public string nextSceneName = "map";

    [Header("Hold E để skip tới frame 27")]
    public float holdSkipTime = 2f;
    public int skipToFrameNumber = 27;

    private bool nextFrameRequested = false;
    private bool skipToFrameRequested = false;
    private int currentFrameIndex = 0;
    private float holdTimer = 0f;

    void Start()
    {
        StartCoroutine(PlayCutscene());
    }

    void Update()
    {
        if (GameInputBridge.GetKeyDown(KeyCode.Space) || GameInputBridge.GetMouseButtonDown(0))
        {
            nextFrameRequested = true;
        }

        HandleHoldSkip();
    }

    void HandleHoldSkip()
{
    if (frames == null || frames.Length == 0) return;

    if (currentFrameIndex >= skipToFrameNumber - 1) return;

    if (Keyboard.current == null) return;

    if (Keyboard.current.eKey.isPressed)
    {
        holdTimer += Time.deltaTime;

        if (holdTimer >= holdSkipTime)
        {
            skipToFrameRequested = true;
            holdTimer = 0f;
        }
    }
    else
    {
        holdTimer = 0f;
    }
}

    IEnumerator PlayCutscene()
    {
        if (frames.Length == 0)
        {
            EndCutscene();
            yield break;
        }

        imageA.sprite = frames[0].image;
        SetImageAlpha(imageA, 1);
        SetImageAlpha(imageB, 0);

        currentFrameIndex = 0;
        yield return ShowFrame(frames[0]);

        for (int i = 1; i < frames.Length; i++)
        {
            if (skipToFrameRequested)
            {
                i = skipToFrameNumber - 1;
                skipToFrameRequested = false;
            }

            currentFrameIndex = i;

            yield return CrossFade(frames[i]);
            yield return ShowFrame(frames[i]);
        }

        EndCutscene();
    }

    IEnumerator ShowFrame(CutsceneFrame frame)
    {
        nextFrameRequested = false;
        dialogueText.text = "";

        foreach (char c in frame.dialogue)
        {
            if (skipToFrameRequested)
            {
                yield break;
            }

            if (nextFrameRequested)
            {
                dialogueText.text = frame.dialogue;
                nextFrameRequested = false;
                break;
            }

            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        float timer = 0;

        while (timer < frame.waitTime)
        {
            if (skipToFrameRequested)
            {
                yield break;
            }

            if (nextFrameRequested)
            {
                nextFrameRequested = false;
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator CrossFade(CutsceneFrame frame)
    {
        imageB.sprite = frame.image;
        SetImageAlpha(imageB, 0);

        float timer = 0;

        while (timer < fadeDuration)
        {
            if (skipToFrameRequested)
            {
                yield break;
            }

            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            SetImageAlpha(imageA, 1 - t);
            SetImageAlpha(imageB, t);

            yield return null;
        }

        imageA.sprite = frame.image;
        SetImageAlpha(imageA, 1);
        SetImageAlpha(imageB, 0);
    }

    void EndCutscene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    void SetImageAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}