using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MiniHorrorPuzzle : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text instructionText;
    public TMP_Text timerText;
    public Image jumpScareImage;
    public Button[] symbolButtons;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip wrongSound;
    public AudioClip jumpScareSound;
    public AudioClip winSound;

    [Header("Game Settings")]
    public float timeLimit = 20f;

    private List<int> correctSequence = new List<int>() { 2, 4, 1, 5, 3 };
    private int currentStep = 0;
    private float timer;
    private bool gameEnded = false;

    void Start()
    {
        timer = timeLimit;

        jumpScareImage.gameObject.SetActive(false);

        instructionText.text = "NHẤN ĐÚNG THỨ TỰ BIỂU TƯỢNG";

        for (int i = 0; i < symbolButtons.Length; i++)
        {
            int buttonNumber = i + 1;
            symbolButtons[i].onClick.AddListener(() => PressSymbol(buttonNumber));
        }
    }

    void Update()
    {
        if (gameEnded || PauseMenu.IsPaused) return;

        timer -= Time.deltaTime;
        timerText.text = "Time: " + Mathf.Ceil(timer).ToString();

        if (timer <= 0)
        {
            StartCoroutine(JumpScare());
        }
    }

    void PressSymbol(int number)
    {
        if (gameEnded || PauseMenu.IsPaused) return;

        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);

        if (number == correctSequence[currentStep])
        {
            currentStep++;

            instructionText.text = "Đúng... tiếp tục";

            if (currentStep >= correctSequence.Count)
            {
                WinGame();
            }
        }
        else
        {
            StartCoroutine(WrongAnswer());
        }
    }

    IEnumerator WrongAnswer()
    {
        gameEnded = true;

        instructionText.text = "Sai rồi...";

        if (audioSource != null && wrongSound != null)
            audioSource.PlayOneShot(wrongSound);

        yield return new WaitForSeconds(1f);

        StartCoroutine(JumpScare());
    }

    void WinGame()
    {
        gameEnded = true;

        instructionText.text = "Cánh cửa đã mở... nhưng có thứ gì đó đang nhìn bạn.";

        if (audioSource != null && winSound != null)
            audioSource.PlayOneShot(winSound);

        StartCoroutine(DelayedScare());
    }

    IEnumerator DelayedScare()
    {
        yield return new WaitForSeconds(2f);
        StartCoroutine(JumpScare());
    }

    IEnumerator JumpScare()
    {
        gameEnded = true;

        foreach (Button btn in symbolButtons)
        {
            btn.interactable = false;
        }

        instructionText.text = "";
        timerText.text = "";

        yield return new WaitForSeconds(0.3f);

        jumpScareImage.gameObject.SetActive(true);

        if (audioSource != null && jumpScareSound != null)
            audioSource.PlayOneShot(jumpScareSound);

        yield return new WaitForSeconds(1.5f);

        instructionText.text = "GAME OVER";
    }
}
