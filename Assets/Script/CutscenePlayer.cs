using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CutscenePlayer : MonoBehaviour
{
    public Image imageA;
    public Image imageB;

    public TextMeshProUGUI dialogueText;

    public CutsceneFrame[] frames;
    public float typeSpeed = 0.04f;

    public float fadeDuration = 1f;

    int currentFrame = 0;
    bool waitingInput = false;

    private void Start()
    {
        StartCoroutine(StartCutscene());
    }

    private void Update()
    {
        if(waitingInput)
        {
            if(Input.GetMouseButtonDown(0)
               || Input.GetKeyDown(KeyCode.Space))
            {
                waitingInput = false;
            }
        }
    }

    IEnumerator StartCutscene()
{
    imageA.sprite = frames[0].image;

    yield return StartCoroutine(
        TypeDialogue(frames[0].dialogue));

    yield return new WaitForSeconds(
        frames[0].waitTime);

    for(int i = 1; i < frames.Length; i++)
    {
        yield return StartCoroutine(
            CrossFade(frames[i]));
    }

    EndCutscene();
}

    IEnumerator WaitForInput()
    {
        waitingInput = true;

        yield return new WaitUntil(() => waitingInput == false);
    }

    IEnumerator CrossFade(CutsceneFrame frame)
    {
        imageB.sprite = frame.image;

        Color b = imageB.color;
        b.a = 0;
        imageB.color = b;

        float t = 0;

        while(t < fadeDuration)
        {
            t += Time.deltaTime;

            float alpha = t / fadeDuration;

            Color a = imageA.color;
            a.a = 1 - alpha;
            imageA.color = a;

            b.a = alpha;
            imageB.color = b;

            yield return null;
        }

        imageA.sprite = frame.image;

        Color resetA = imageA.color;
        resetA.a = 1;
        imageA.color = resetA;

        b.a = 0;
        imageB.color = b;

        yield return StartCoroutine(
        TypeDialogue(frame.dialogue));

        yield return new WaitForSeconds(
            frame.waitTime);
    }
    IEnumerator TypeDialogue(string text)
{
    dialogueText.text = "";

    foreach(char letter in text)
    {
        dialogueText.text += letter;

        yield return new WaitForSeconds(typeSpeed);
    }
}

    void EndCutscene()
    {
        Debug.Log("Cutscene End");

        // chuyển scene gameplay
        // SceneManager.LoadScene("Gameplay");
    }
}