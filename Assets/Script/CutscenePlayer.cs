using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CutscenePlayer : MonoBehaviour
{
    public Image imageA;
    public Image imageB;
    public TextMeshProUGUI dialogueText;

    public CutsceneFrame[] frames;

    public float fadeDuration = 0.8f;
    public float typeSpeed = 0.035f;
    public string nextSceneName = "map";

    private bool nextFrameRequested = false;

    void Start()
    {
        StartCoroutine(PlayCutscene());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            nextFrameRequested = true;
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

        yield return ShowFrame(frames[0]);

        for (int i = 1; i < frames.Length; i++)
        {
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