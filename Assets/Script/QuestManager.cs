using System.Collections;
using TMPro;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Quest UI")]
    [SerializeField] private CanvasGroup questPanelGroup;
    [SerializeField] private TextMeshProUGUI chapterTitleText;
    [SerializeField] private TextMeshProUGUI mainQuestText;
    [SerializeField] private TextMeshProUGUI[] subQuestTexts;
    [SerializeField] private TextMeshProUGUI questHintText;

    [Header("Character Thought UI")]
    [SerializeField] private CanvasGroup characterThoughtPanelGroup;
    [SerializeField] private TextMeshProUGUI characterThoughtText;

    [Header("Sub Quest Hint UI")]
    [SerializeField] private CanvasGroup subQuestHintPanelGroup;
    [SerializeField] private TextMeshProUGUI activeSubQuestText;
    [SerializeField] private TextMeshProUGUI subQuestHintText;
    [SerializeField] private TextMeshProUGUI hintControlText;

    [Header("Quest Data")]
    [SerializeField] private QuestData[] chapters;

    [Header("Quest Effect")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float typeSpeed = 0.02f;
    [SerializeField] private float autoHideTime = 3f;

    [Header("Character Thought Effect")]
    [SerializeField] private float thoughtFadeDuration = 0.4f;
    [SerializeField] private float thoughtTypeSpeed = 0.035f;
    [SerializeField] private float thoughtHoldTime = 2.5f;

    [Header("Sub Quest Hint Effect")]
    [SerializeField] private float hintFadeDuration = 0.35f;
    [SerializeField] private float hintTypeSpeed = 0.025f;
    [SerializeField] private float hintHoldTime = 5f;

    [Tooltip("Tự động hiện gợi ý chi tiết khi người chơi bị kẹt quá lâu")]
    [SerializeField] private bool autoShowDetailedHint = true;

    [Tooltip("Tự hoàn thành chương khi toàn bộ nhiệm vụ nhỏ đã hoàn thành")]
    [SerializeField] private bool autoCompleteChapter = true;

    private int currentChapterIndex;
    private int currentSubQuestIndex = -1;

    private bool[] completedSubQuests;

    private bool isQuestVisible;
    private bool isShowingThought;
    private bool detailedHintUnlocked;

    private Coroutine chapterRoutine;
    private Coroutine questToggleRoutine;
    private Coroutine hintRoutine;
    private Coroutine detailedHintRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        HideCanvasGroupInstant(questPanelGroup);
        HideCanvasGroupInstant(characterThoughtPanelGroup);
        HideCanvasGroupInstant(subQuestHintPanelGroup);

        ShowChapterQuest(0);
    }

    private void Update()
    {
        if (GameInputBridge.GetKeyDown(KeyCode.Tab))
        {
            if (!isShowingThought)
            {
                ToggleQuestPanel();
            }
        }

        if (GameInputBridge.GetKeyDown(KeyCode.H))
        {
            if (!isShowingThought)
            {
                ShowCurrentSubQuestHint();
            }
        }
    }

    public void ShowChapterQuest(int chapterIndex)
    {
        if (chapters == null || chapters.Length == 0)
        {
            Debug.LogWarning("QuestManager chưa có dữ liệu chương.");
            return;
        }

        if (chapterIndex < 0 || chapterIndex >= chapters.Length)
        {
            Debug.LogWarning("Chapter Index không hợp lệ: " + chapterIndex);
            return;
        }

        StopManagedCoroutines();

        currentChapterIndex = chapterIndex;
        currentSubQuestIndex = -1;
        detailedHintUnlocked = false;

        QuestData data = chapters[currentChapterIndex];

        int subQuestCount = data.subQuests != null
            ? data.subQuests.Length
            : 0;

        completedSubQuests = new bool[subQuestCount];

        chapterRoutine = StartCoroutine(StartChapterRoutine(data));
    }

    private IEnumerator StartChapterRoutine(QuestData data)
    {
        HideCanvasGroupInstant(questPanelGroup);
        HideCanvasGroupInstant(subQuestHintPanelGroup);

        SetupQuestUI(data);

        if (!string.IsNullOrWhiteSpace(data.characterThought))
        {
            yield return ShowCharacterThoughtRoutine(
                data.characterThought
            );
        }

        yield return ShowQuestRoutine(data);

        chapterRoutine = null;

        ActivateNextIncompleteSubQuest(true);
    }

    private void SetupQuestUI(QuestData data)
    {
        if (chapterTitleText != null)
        {
            chapterTitleText.text = data.chapterTitle;
        }

        if (mainQuestText != null)
        {
            mainQuestText.text = "";
        }

        if (questHintText != null)
        {
            questHintText.text =
                "TAB: Ẩn / hiện nhiệm vụ";
        }

        for (int i = 0; i < subQuestTexts.Length; i++)
        {
            if (data.subQuests != null &&
                i < data.subQuests.Length)
            {
                subQuestTexts[i].text =
                    "☐ " + data.subQuests[i].title;

                subQuestTexts[i].color = Color.white;
            }
            else
            {
                subQuestTexts[i].text = "";
            }
        }
    }

    private IEnumerator ShowCharacterThoughtRoutine(
        string thoughtContent)
    {
        if (characterThoughtPanelGroup == null ||
            characterThoughtText == null)
        {
            yield break;
        }

        isShowingThought = true;
        characterThoughtText.text = "";

        yield return FadeCanvasGroup(
            characterThoughtPanelGroup,
            1f,
            thoughtFadeDuration
        );

        yield return TypeText(
            characterThoughtText,
            thoughtContent,
            thoughtTypeSpeed
        );

        yield return new WaitForSecondsRealtime(
            thoughtHoldTime
        );

        yield return FadeCanvasGroup(
            characterThoughtPanelGroup,
            0f,
            thoughtFadeDuration
        );

        characterThoughtText.text = "";
        isShowingThought = false;
    }

    private IEnumerator ShowQuestRoutine(QuestData data)
    {
        yield return FadeCanvasGroup(
            questPanelGroup,
            1f,
            fadeDuration
        );

        isQuestVisible = true;

        yield return TypeText(
            mainQuestText,
            "Nhiệm vụ chính: " + data.mainQuest,
            typeSpeed
        );

        yield return new WaitForSecondsRealtime(
            autoHideTime
        );

        yield return FadeCanvasGroup(
            questPanelGroup,
            0f,
            fadeDuration
        );

        isQuestVisible = false;
    }

    private void ActivateNextIncompleteSubQuest(
        bool showHint)
    {
        int nextIndex = FindFirstIncompleteSubQuest();

        if (nextIndex == -1)
        {
            if (autoCompleteChapter)
            {
                CompleteCurrentChapter();
            }

            return;
        }

        ActivateSubQuest(nextIndex, showHint);
    }

    private void ActivateSubQuest(
        int subQuestIndex,
        bool showHint)
    {
        QuestData chapter = chapters[currentChapterIndex];

        if (chapter.subQuests == null)
        {
            return;
        }

        if (subQuestIndex < 0 ||
            subQuestIndex >= chapter.subQuests.Length)
        {
            return;
        }

        currentSubQuestIndex = subQuestIndex;
        detailedHintUnlocked = false;

        if (detailedHintRoutine != null)
        {
            StopCoroutine(detailedHintRoutine);
        }

        SubQuestData subQuest =
            chapter.subQuests[currentSubQuestIndex];

        if (!string.IsNullOrWhiteSpace(
                subQuest.detailedHint) &&
            subQuest.detailedHintDelay > 0f)
        {
            detailedHintRoutine = StartCoroutine(
                UnlockDetailedHintRoutine(
                    currentSubQuestIndex,
                    subQuest.detailedHintDelay
                )
            );
        }

        if (showHint)
        {
            ShowCurrentSubQuestHint();
        }
    }

    public void ShowCurrentSubQuestHint()
    {
        if (currentSubQuestIndex < 0)
        {
            return;
        }

        QuestData chapter = chapters[currentChapterIndex];

        if (chapter.subQuests == null ||
            currentSubQuestIndex >=
            chapter.subQuests.Length)
        {
            return;
        }

        SubQuestData subQuest =
            chapter.subQuests[currentSubQuestIndex];

        string hintContent = subQuest.hint;

        if (detailedHintUnlocked &&
            !string.IsNullOrWhiteSpace(
                subQuest.detailedHint))
        {
            hintContent = subQuest.detailedHint;
        }

        if (hintRoutine != null)
        {
            StopCoroutine(hintRoutine);
        }

        hintRoutine = StartCoroutine(
            ShowSubQuestHintRoutine(
                subQuest.title,
                hintContent,
                detailedHintUnlocked
            )
        );
    }

    private IEnumerator ShowSubQuestHintRoutine(
        string objective,
        string hint,
        bool isDetailed)
    {
        if (subQuestHintPanelGroup == null)
        {
            yield break;
        }

        if (activeSubQuestText != null)
        {
            activeSubQuestText.text =
                "MỤC TIÊU HIỆN TẠI\n" + objective;
        }

        if (subQuestHintText != null)
        {
            subQuestHintText.text = "";
        }

        if (hintControlText != null)
        {
            hintControlText.text = isDetailed
                ? "Gợi ý chi tiết • H: Xem lại"
                : "H: Xem lại gợi ý";
        }

        yield return FadeCanvasGroup(
            subQuestHintPanelGroup,
            1f,
            hintFadeDuration
        );

        yield return TypeText(
            subQuestHintText,
            hint,
            hintTypeSpeed
        );

        yield return new WaitForSecondsRealtime(
            hintHoldTime
        );

        yield return FadeCanvasGroup(
            subQuestHintPanelGroup,
            0f,
            hintFadeDuration
        );

        hintRoutine = null;
    }

    private IEnumerator UnlockDetailedHintRoutine(
        int expectedSubQuestIndex,
        float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (currentSubQuestIndex !=
            expectedSubQuestIndex)
        {
            yield break;
        }

        if (completedSubQuests[
                expectedSubQuestIndex])
        {
            yield break;
        }

        detailedHintUnlocked = true;
        detailedHintRoutine = null;

        if (autoShowDetailedHint)
        {
            ShowCurrentSubQuestHint();
        }
    }

    public void CompleteSubQuest(int index)
    {
        if (completedSubQuests == null)
        {
            return;
        }

        if (index < 0 ||
            index >= completedSubQuests.Length)
        {
            Debug.LogWarning(
                "Sub Quest Index không hợp lệ: " +
                index
            );

            return;
        }

        if (completedSubQuests[index])
        {
            return;
        }

        completedSubQuests[index] = true;

        QuestData chapter =
            chapters[currentChapterIndex];

        if (index < subQuestTexts.Length)
        {
            subQuestTexts[index].text =
                "☑ " +
                chapter.subQuests[index].title;

            subQuestTexts[index].color =
                new Color(0.65f, 0.65f, 0.65f);
        }

        Debug.Log(
            "Hoàn thành nhiệm vụ nhỏ: " +
            chapter.subQuests[index].title
        );

        if (detailedHintRoutine != null)
        {
            StopCoroutine(detailedHintRoutine);
            detailedHintRoutine = null;
        }

        if (hintRoutine != null)
        {
            StopCoroutine(hintRoutine);
        }

        hintRoutine = StartCoroutine(
            ShowCompletedSubQuestRoutine(index)
        );
    }

    private IEnumerator ShowCompletedSubQuestRoutine(
        int completedIndex)
    {
        QuestData chapter =
            chapters[currentChapterIndex];

        if (activeSubQuestText != null)
        {
            activeSubQuestText.text =
                "ĐÃ HOÀN THÀNH";
        }

        if (subQuestHintText != null)
        {
            subQuestHintText.text =
                "☑ " +
                chapter.subQuests[
                    completedIndex
                ].title;
        }

        if (hintControlText != null)
        {
            hintControlText.text = "";
        }

        yield return FadeCanvasGroup(
            subQuestHintPanelGroup,
            1f,
            hintFadeDuration
        );

        yield return new WaitForSecondsRealtime(1.5f);

        yield return FadeCanvasGroup(
            subQuestHintPanelGroup,
            0f,
            hintFadeDuration
        );

        hintRoutine = null;

        if (AreAllSubQuestsCompleted())
        {
            if (autoCompleteChapter)
            {
                CompleteCurrentChapter();
            }
        }
        else
        {
            ActivateNextIncompleteSubQuest(true);
        }
    }

    private int FindFirstIncompleteSubQuest()
    {
        if (completedSubQuests == null)
        {
            return -1;
        }

        for (int i = 0;
             i < completedSubQuests.Length;
             i++)
        {
            if (!completedSubQuests[i])
            {
                return i;
            }
        }

        return -1;
    }

    private bool AreAllSubQuestsCompleted()
    {
        if (completedSubQuests == null ||
            completedSubQuests.Length == 0)
        {
            return false;
        }

        foreach (bool completed in completedSubQuests)
        {
            if (!completed)
            {
                return false;
            }
        }

        return true;
    }

    public void CompleteCurrentChapter()
    {
        StopManagedCoroutines();

        chapterRoutine = StartCoroutine(
            CompleteChapterRoutine()
        );
    }

    private IEnumerator CompleteChapterRoutine()
    {
        yield return FadeCanvasGroup(
            questPanelGroup,
            1f,
            fadeDuration
        );

        isQuestVisible = true;

        if (questHintText != null)
        {
            questHintText.text =
                "Toàn bộ mục tiêu đã hoàn thành...";
        }

        yield return new WaitForSecondsRealtime(1.5f);

        yield return FadeCanvasGroup(
            questPanelGroup,
            0f,
            fadeDuration
        );

        isQuestVisible = false;

        int nextChapter = currentChapterIndex + 1;

        chapterRoutine = null;

        if (nextChapter < chapters.Length)
        {
            ShowChapterQuest(nextChapter);
        }
        else
        {
            Debug.Log(
                "Đã hoàn thành toàn bộ nhiệm vụ."
            );
        }
    }

    private void ToggleQuestPanel()
    {
        if (questToggleRoutine != null)
        {
            StopCoroutine(questToggleRoutine);
        }

        isQuestVisible = !isQuestVisible;

        questToggleRoutine = StartCoroutine(
            ToggleQuestRoutine(
                isQuestVisible ? 1f : 0f
            )
        );
    }

    private IEnumerator ToggleQuestRoutine(
        float targetAlpha)
    {
        yield return FadeCanvasGroup(
            questPanelGroup,
            targetAlpha,
            fadeDuration
        );

        questToggleRoutine = null;
    }

    private IEnumerator FadeCanvasGroup(
        CanvasGroup canvasGroup,
        float targetAlpha,
        float duration)
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        float startAlpha = canvasGroup.alpha;
        float timer = 0f;

        if (duration <= 0f)
        {
            canvasGroup.alpha = targetAlpha;
        }
        else
        {
            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(
                    timer / duration
                );

                canvasGroup.alpha = Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    progress
                );

                yield return null;
            }
        }

        canvasGroup.alpha = targetAlpha;

        bool visible = targetAlpha > 0.01f;

        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    private IEnumerator TypeText(
        TextMeshProUGUI textTarget,
        string content,
        float speed)
    {
        if (textTarget == null)
        {
            yield break;
        }

        textTarget.text = "";

        if (string.IsNullOrEmpty(content))
        {
            yield break;
        }

        foreach (char character in content)
        {
            textTarget.text += character;

            if (character == '.' ||
                character == '!' ||
                character == '?' ||
                character == '…')
            {
                yield return new WaitForSecondsRealtime(
                    speed * 6f
                );
            }
            else if (character == ',' ||
                     character == ';' ||
                     character == ':')
            {
                yield return new WaitForSecondsRealtime(
                    speed * 3f
                );
            }
            else
            {
                yield return new WaitForSecondsRealtime(
                    speed
                );
            }
        }
    }

    private void HideCanvasGroupInstant(
        CanvasGroup canvasGroup)
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void StopManagedCoroutines()
    {
        if (chapterRoutine != null)
        {
            StopCoroutine(chapterRoutine);
            chapterRoutine = null;
        }

        if (questToggleRoutine != null)
        {
            StopCoroutine(questToggleRoutine);
            questToggleRoutine = null;
        }

        if (hintRoutine != null)
        {
            StopCoroutine(hintRoutine);
            hintRoutine = null;
        }

        if (detailedHintRoutine != null)
        {
            StopCoroutine(detailedHintRoutine);
            detailedHintRoutine = null;
        }

        isShowingThought = false;
    }
}