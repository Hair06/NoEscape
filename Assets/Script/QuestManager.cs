using System.Collections;
using TMPro;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [System.Serializable]
    private class QuestProgressRequirement
    {
        [Min(0)] public int chapterIndex;
        [Min(0)] public int subQuestIndex;
        [Min(1)] public int requiredAmount = 1;
    }

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

    [Header("Quest Progress Requirements")]
    [Tooltip("Khai báo các nhiệm vụ có bộ đếm, ví dụ Chapter 0 / Sub Quest 0 / Required Amount 2")]
    [SerializeField] private QuestProgressRequirement[] progressRequirements;

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
    private int[] subQuestProgress;
    private int[] requiredSubQuestProgress;

    private bool isQuestVisible;
    private bool isShowingThought;
    private bool detailedHintUnlocked;
    private bool isCompletingChapter;

    private Coroutine chapterRoutine;
    private Coroutine questToggleRoutine;
    private Coroutine hintRoutine;
    private Coroutine detailedHintRoutine;
    private Coroutine progressDisplayRoutine;

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
            if (!isShowingThought &&
                !isCompletingChapter)
            {
                ToggleQuestPanel();
            }
        }

        if (GameInputBridge.GetKeyDown(KeyCode.H))
        {
            if (!isShowingThought &&
                !isCompletingChapter)
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
        isCompletingChapter = false;

        QuestData data = chapters[currentChapterIndex];

        int subQuestCount = data.subQuests != null
            ? data.subQuests.Length
            : 0;

        completedSubQuests = new bool[subQuestCount];
        subQuestProgress = new int[subQuestCount];
        requiredSubQuestProgress = new int[subQuestCount];

        for (int i = 0; i < subQuestCount; i++)
        {
            requiredSubQuestProgress[i] =
                GetConfiguredRequiredAmount(
                    currentChapterIndex,
                    i
                );
        }

        if (subQuestCount > 0)
        {
            currentSubQuestIndex = 0;
        }

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

        if (currentSubQuestIndex >= 0)
        {
            ActivateSubQuest(
                currentSubQuestIndex,
                true
            );
        }
        else if (autoCompleteChapter)
        {
            CompleteCurrentChapter();
        }
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
                RefreshSubQuestLine(i);
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

        RefreshSubQuestLine(
            currentSubQuestIndex
        );

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

    public void ReportProgressForChapter(
        int chapterIndex,
        int subQuestIndex,
        int amount,
        int requiredAmount)
    {
        if (!CanUpdateSubQuest(
                chapterIndex,
                subQuestIndex))
        {
            return;
        }

        if (amount <= 0)
        {
            Debug.LogWarning(
                "Số tiến độ cộng thêm phải lớn hơn 0."
            );
            return;
        }

        requiredSubQuestProgress[subQuestIndex] =
            Mathf.Max(1, requiredAmount);

        subQuestProgress[subQuestIndex] =
            Mathf.Clamp(
                subQuestProgress[subQuestIndex] + amount,
                0,
                requiredSubQuestProgress[subQuestIndex]
            );

        RefreshSubQuestLine(subQuestIndex);

        Debug.Log(
            "Tiến độ nhiệm vụ: " +
            chapters[currentChapterIndex]
                .subQuests[subQuestIndex].title +
            " " +
            subQuestProgress[subQuestIndex] +
            "/" +
            requiredSubQuestProgress[subQuestIndex]
        );

        if (subQuestProgress[subQuestIndex] >=
            requiredSubQuestProgress[subQuestIndex])
        {
            CompleteSubQuestForChapter(
                chapterIndex,
                subQuestIndex
            );
        }
        else
        {
            ShowQuestProgressTemporarily();
        }
    }

    public void CompleteSubQuestForChapter(
        int chapterIndex,
        int subQuestIndex)
    {
        if (!CanUpdateSubQuest(
                chapterIndex,
                subQuestIndex))
        {
            return;
        }

        CompleteSubQuest(subQuestIndex);
    }

    private bool CanUpdateSubQuest(
        int chapterIndex,
        int subQuestIndex)
    {
        if (chapters == null ||
            chapterIndex < 0 ||
            chapterIndex >= chapters.Length)
        {
            Debug.LogWarning(
                "Chapter Index không hợp lệ: " +
                chapterIndex
            );
            return false;
        }

        if (chapterIndex != currentChapterIndex)
        {
            Debug.LogWarning(
                "Không thể cập nhật Chapter " +
                chapterIndex +
                " vì Chapter hiện tại là " +
                currentChapterIndex + "."
            );
            return false;
        }

        if (completedSubQuests == null ||
            subQuestIndex < 0 ||
            subQuestIndex >= completedSubQuests.Length)
        {
            Debug.LogWarning(
                "Sub Quest Index không hợp lệ: " +
                subQuestIndex
            );
            return false;
        }

        if (completedSubQuests[subQuestIndex])
        {
            return false;
        }

        if (subQuestIndex != currentSubQuestIndex)
        {
            Debug.LogWarning(
                "Không thể hoàn thành nhiệm vụ số " +
                subQuestIndex +
                " trước nhiệm vụ hiện tại số " +
                currentSubQuestIndex + "."
            );
            return false;
        }

        return true;
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

        if (index != currentSubQuestIndex)
        {
            Debug.LogWarning(
                "Không thể hoàn thành nhiệm vụ số " +
                index +
                " trước nhiệm vụ hiện tại số " +
                currentSubQuestIndex + "."
            );
            return;
        }

        completedSubQuests[index] = true;

        if (subQuestProgress != null &&
            requiredSubQuestProgress != null)
        {
            subQuestProgress[index] =
                requiredSubQuestProgress[index];
        }

        QuestData chapter =
            chapters[currentChapterIndex];

        RefreshSubQuestLine(index);

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

        if (!AreAllSubQuestsCompleted())
        {
            // Mở mục tiêu kế tiếp ngay lập tức để các
            // tương tác liên tiếp không bị mất sự kiện.
            ActivateNextIncompleteSubQuest(false);
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
            ShowCurrentSubQuestHint();
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
        if (isCompletingChapter)
        {
            return;
        }

        if (!AreAllSubQuestsCompleted())
        {
            Debug.LogWarning(
                "Không thể hoàn thành chương vì vẫn còn nhiệm vụ nhỏ chưa hoàn thành."
            );
            return;
        }

        StopManagedCoroutines();

        isCompletingChapter = true;

        chapterRoutine = StartCoroutine(
            CompleteChapterRoutine()
        );
    }

    private IEnumerator CompleteChapterRoutine()
    {
        HideCanvasGroupInstant(
            subQuestHintPanelGroup
        );

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
        isCompletingChapter = false;

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
        if (progressDisplayRoutine != null)
        {
            StopCoroutine(progressDisplayRoutine);
            progressDisplayRoutine = null;
        }

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

    private void ShowQuestProgressTemporarily()
    {
        if (isShowingThought ||
            chapterRoutine != null ||
            isCompletingChapter)
        {
            return;
        }

        if (progressDisplayRoutine != null)
        {
            StopCoroutine(progressDisplayRoutine);
        }

        progressDisplayRoutine = StartCoroutine(
            ShowQuestProgressRoutine()
        );
    }

    private IEnumerator ShowQuestProgressRoutine()
    {
        yield return FadeCanvasGroup(
            questPanelGroup,
            1f,
            fadeDuration
        );

        isQuestVisible = true;

        yield return new WaitForSecondsRealtime(
            autoHideTime
        );

        yield return FadeCanvasGroup(
            questPanelGroup,
            0f,
            fadeDuration
        );

        isQuestVisible = false;
        progressDisplayRoutine = null;
    }

    private void RefreshSubQuestLine(int index)
    {
        if (subQuestTexts == null ||
            index < 0 ||
            index >= subQuestTexts.Length ||
            subQuestTexts[index] == null)
        {
            return;
        }

        QuestData chapter = chapters[currentChapterIndex];

        if (chapter.subQuests == null ||
            index >= chapter.subQuests.Length)
        {
            subQuestTexts[index].text = "";
            return;
        }

        string title = chapter.subQuests[index].title;

        if (completedSubQuests != null &&
            completedSubQuests[index])
        {
            subQuestTexts[index].text =
                "☑ " + title;

            subQuestTexts[index].color =
                new Color(0.65f, 0.65f, 0.65f);
            return;
        }

        if (index == currentSubQuestIndex)
        {
            string progressText = "";

            if (requiredSubQuestProgress != null &&
                requiredSubQuestProgress[index] > 1)
            {
                progressText =
                    " (" +
                    subQuestProgress[index] +
                    "/" +
                    requiredSubQuestProgress[index] +
                    ")";
            }

            subQuestTexts[index].text =
                "▶ " + title + progressText;

            subQuestTexts[index].color = Color.white;
            return;
        }

        subQuestTexts[index].text =
            "☐ " + title + " [Chưa mở]";

        subQuestTexts[index].color =
            new Color(0.45f, 0.45f, 0.45f);
    }

    private int GetConfiguredRequiredAmount(
        int chapterIndex,
        int subQuestIndex)
    {
        if (progressRequirements == null)
        {
            return 1;
        }

        foreach (QuestProgressRequirement requirement
                 in progressRequirements)
        {
            if (requirement != null &&
                requirement.chapterIndex == chapterIndex &&
                requirement.subQuestIndex == subQuestIndex)
            {
                return Mathf.Max(
                    1,
                    requirement.requiredAmount
                );
            }
        }

        return 1;
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

        if (progressDisplayRoutine != null)
        {
            StopCoroutine(progressDisplayRoutine);
            progressDisplayRoutine = null;
        }

        isShowingThought = false;
    }
}