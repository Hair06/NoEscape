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

    [Header("Hint Control luôn hiện trong Gameplay")]
    [Tooltip("CanvasGroup riêng chứa dòng 'H để xem lại gợi ý'. Object này phải nằm NGOÀI SubQuestHintPanel.")]
    [SerializeField] private CanvasGroup hintControlPanelGroup;
    [SerializeField] private TextMeshProUGUI hintControlText;

    [Header("Quest Data")]
    [SerializeField] private QuestData[] chapters;

    [Header("Quest Start")]
    [Tooltip("Tắt mục này nếu nhiệm vụ chỉ bắt đầu khi Player đi qua trigger cửa chính.")]
    [SerializeField] private bool startQuestAutomatically;

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
    private bool subQuestHintsSuppressed;
    private bool gameplayUiSuppressed;
    private bool questFlowStarted;

    private int pendingNextChapterIndex = -1;
    private bool nextChapterStartRequested;

    public bool IsQuestFlowStarted => questFlowStarted;
    public int CurrentChapterIndex => currentChapterIndex;
    public bool IsChapterTransitioning =>
        isCompletingChapter || pendingNextChapterIndex >= 0;

    private Coroutine chapterRoutine;
    private Coroutine questToggleRoutine;
    private Coroutine hintRoutine;
    private Coroutine scheduledHintRoutine;
    private Coroutine detailedHintRoutine;
    private Coroutine progressDisplayRoutine;
    private Coroutine nextChapterStartRoutine;

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
        HideCanvasGroupInstant(hintControlPanelGroup);

        if (hintControlText != null)
        {
            hintControlText.text =
                "<b><color=#D9B66F>H</color></b> để xem lại gợi ý";
        }

        if (startQuestAutomatically)
        {
            BeginQuestFlow(0);
        }
    }

    private void Update()
    {
        RefreshHintControlVisibility();

        if (!questFlowStarted || gameplayUiSuppressed)
        {
            return;
        }

        if (GameInputBridge.GetKeyDown(KeyCode.Tab))
        {
            if (!isShowingThought &&
                !isCompletingChapter &&
                hintRoutine == null)
            {
                ToggleQuestPanel();
            }
        }

        if (GameInputBridge.GetKeyDown(KeyCode.H))
        {
            if (!isShowingThought &&
                !isCompletingChapter &&
                chapterRoutine == null &&
                hintRoutine == null &&
                !IsSubQuestHintPresentationBlocked())
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

        if (nextChapterStartRoutine != null)
        {
            StopCoroutine(nextChapterStartRoutine);
            nextChapterStartRoutine = null;
        }

        currentChapterIndex = chapterIndex;
        currentSubQuestIndex = -1;
        detailedHintUnlocked = false;
        isCompletingChapter = false;
        pendingNextChapterIndex = -1;
        nextChapterStartRequested = false;

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

    public bool BeginQuestFlow(int startingChapterIndex = 0)
    {
        if (questFlowStarted)
        {
            return false;
        }

        if (chapters == null ||
            chapters.Length == 0 ||
            startingChapterIndex < 0 ||
            startingChapterIndex >= chapters.Length)
        {
            Debug.LogWarning(
                "Không thể bắt đầu nhiệm vụ. Starting Chapter Index không hợp lệ: " +
                startingChapterIndex
            );
            return false;
        }

        questFlowStarted = true;
        ShowChapterQuest(startingChapterIndex);

        Debug.Log(
            "Đã kích hoạt hệ thống nhiệm vụ từ Chapter Index " +
            startingChapterIndex + "."
        );
        return true;
    }

    public bool CanOpenMiniGameForChapter(int chapterIndex)
    {
        return questFlowStarted &&
               currentChapterIndex == chapterIndex &&
               chapterRoutine == null &&
               !isShowingThought &&
               !isCompletingChapter &&
               pendingNextChapterIndex < 0 &&
               !gameplayUiSuppressed;
    }

    public void SetGameplayUiSuppressed(
        bool suppressed,
        bool showCurrentHintOnResume = true)
    {
        gameplayUiSuppressed = suppressed;

        if (suppressed)
        {
            if (questToggleRoutine != null)
            {
                StopCoroutine(questToggleRoutine);
                questToggleRoutine = null;
            }

            if (progressDisplayRoutine != null)
            {
                StopCoroutine(progressDisplayRoutine);
                progressDisplayRoutine = null;
            }

            isQuestVisible = false;

            HideCanvasGroupInstant(questPanelGroup);
            HideCanvasGroupInstant(characterThoughtPanelGroup);
            HideCanvasGroupInstant(subQuestHintPanelGroup);
            HideCanvasGroupInstant(hintControlPanelGroup);

            SetSubQuestHintsSuppressed(true, false);
            return;
        }

        SetSubQuestHintsSuppressed(
            false,
            showCurrentHintOnResume
        );
    }

    private IEnumerator StartChapterRoutine(QuestData data)
    {
        HideCanvasGroupInstant(questPanelGroup);
        HideCanvasGroupInstant(subQuestHintPanelGroup);

        SetupQuestUI(data);

        if (!string.IsNullOrWhiteSpace(data.characterThought))
        {
            if (data.characterThoughtDelay > 0f)
            {
                yield return WaitForSecondsRealtimePausable(
                    data.characterThoughtDelay
                );
            }

            float characterThoughtHoldDuration =
                data.characterThoughtHoldTime < 0f
                    ? thoughtHoldTime
                    : data.characterThoughtHoldTime;

            yield return ShowCharacterThoughtRoutine(
                data.characterThought,
                characterThoughtHoldDuration
            );
        }

        yield return ShowQuestRoutine(data);

        chapterRoutine = null;

        if (currentSubQuestIndex >= 0)
        {
            ActivateSubQuest(
                currentSubQuestIndex,
                !IsSubQuestHintPresentationBlocked()
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
        string thoughtContent,
        float holdDuration)
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

        yield return WaitForSecondsRealtimePausable(
            Mathf.Max(0f, holdDuration)
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

        yield return WaitForSecondsRealtimePausable(
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

        if (nextIndex == currentSubQuestIndex)
        {
            RefreshSubQuestLine(currentSubQuestIndex);
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

        if (scheduledHintRoutine != null)
        {
            StopCoroutine(scheduledHintRoutine);
            scheduledHintRoutine = null;
        }

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
            ScheduleCurrentSubQuestHint();
        }

        RefreshHintControlVisibility();
    }

    public void ShowCurrentSubQuestHint()
    {
        if (IsSubQuestHintPresentationBlocked() ||
            currentSubQuestIndex < 0)
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

        if (scheduledHintRoutine != null)
        {
            StopCoroutine(scheduledHintRoutine);
            scheduledHintRoutine = null;
        }

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
            ShowSubQuestHintWithCoordinationRoutine(
                subQuest.title,
                hintContent,
                subQuest.hintHoldTime < 0f
                    ? hintHoldTime
                    : subQuest.hintHoldTime
            )
        );
    }

    private IEnumerator ShowSubQuestHintWithCoordinationRoutine(
        string objective,
        string hint,
        float holdDuration)
    {
        if (progressDisplayRoutine != null)
        {
            StopCoroutine(progressDisplayRoutine);
            progressDisplayRoutine = null;
        }

        if (questToggleRoutine != null)
        {
            StopCoroutine(questToggleRoutine);
            questToggleRoutine = null;
        }

        if (questPanelGroup != null &&
            questPanelGroup.alpha > 0.01f)
        {
            yield return FadeCanvasGroup(
                questPanelGroup,
                0f,
                fadeDuration
            );
        }

        isQuestVisible = false;

        yield return ShowSubQuestHintRoutine(
            objective,
            hint,
            holdDuration
        );

        hintRoutine = null;
    }

    private IEnumerator ShowSubQuestHintRoutine(
        string objective,
        string hint,
        float holdDuration)
    {
        if (subQuestHintPanelGroup == null)
        {
            yield break;
        }

        if (activeSubQuestText != null)
        {
            activeSubQuestText.text =
                "<size=70%><color=#C9B27A>MỤC TIÊU HIỆN TẠI</color></size>\n" +
                "<b>" + objective + "</b>";
        }

        if (subQuestHintText != null)
        {
            subQuestHintText.text = "";
        }

        RefreshHintControlVisibility();

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

        yield return WaitForSecondsRealtimePausable(
            Mathf.Max(0f, holdDuration)
        );

        yield return FadeCanvasGroup(
            subQuestHintPanelGroup,
            0f,
            hintFadeDuration
        );

    }

    private void ScheduleCurrentSubQuestHint()
    {
        if (currentSubQuestIndex < 0 ||
            chapters == null ||
            currentChapterIndex < 0 ||
            currentChapterIndex >= chapters.Length)
        {
            return;
        }

        QuestData chapter = chapters[currentChapterIndex];

        if (chapter.subQuests == null ||
            currentSubQuestIndex >= chapter.subQuests.Length)
        {
            return;
        }

        if (scheduledHintRoutine != null)
        {
            StopCoroutine(scheduledHintRoutine);
        }

        scheduledHintRoutine = StartCoroutine(
            ShowInitialHintAfterDelayRoutine(
                currentSubQuestIndex,
                chapter.subQuests[currentSubQuestIndex]
                    .hintShowDelay
            )
        );
    }

    private IEnumerator ShowInitialHintAfterDelayRoutine(
        int expectedSubQuestIndex,
        float delay)
    {
        if (delay > 0f)
        {
            yield return WaitForSecondsRealtimePausable(delay);
        }

        while (isQuestVisible ||
               questToggleRoutine != null ||
               chapterRoutine != null ||
               hintRoutine != null ||
               gameplayUiSuppressed ||
               IsSubQuestHintPresentationBlocked())
        {
            if (!IsSubQuestStillAvailable(
                    expectedSubQuestIndex))
            {
                scheduledHintRoutine = null;
                yield break;
            }

            yield return null;
        }

        scheduledHintRoutine = null;

        if (!IsSubQuestStillAvailable(
                expectedSubQuestIndex))
        {
            yield break;
        }

        ShowCurrentSubQuestHint();
    }

    private bool IsSubQuestStillAvailable(int subQuestIndex)
    {
        return currentSubQuestIndex == subQuestIndex &&
               completedSubQuests != null &&
               subQuestIndex >= 0 &&
               subQuestIndex < completedSubQuests.Length &&
               !completedSubQuests[subQuestIndex];
    }

    private IEnumerator UnlockDetailedHintRoutine(
        int expectedSubQuestIndex,
        float delay)
    {
        yield return WaitForSecondsRealtimePausable(delay);

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
        RefreshHintControlVisibility();

        if (autoShowDetailedHint &&
            !IsSubQuestHintPresentationBlocked() &&
            !isQuestVisible &&
            questToggleRoutine == null &&
            chapterRoutine == null)
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

        if (subQuestIndex != currentSubQuestIndex &&
            !AllowsOutOfOrderCompletion())
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

        if (index != currentSubQuestIndex &&
            !AllowsOutOfOrderCompletion())
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

        if (scheduledHintRoutine != null)
        {
            StopCoroutine(scheduledHintRoutine);
            scheduledHintRoutine = null;
        }

        if (!AreAllSubQuestsCompleted())
        {
            // Mở mục tiêu kế tiếp ngay lập tức để các
            // tương tác liên tiếp không bị mất sự kiện.
            ActivateNextIncompleteSubQuest(true);
        }

        if (IsSubQuestHintPresentationBlocked())
        {
            HideCanvasGroupInstant(subQuestHintPanelGroup);

            if (AreAllSubQuestsCompleted() &&
                autoCompleteChapter)
            {
                CompleteCurrentChapter();
            }

            return;
        }

        hintRoutine = StartCoroutine(
            ShowCompletedSubQuestRoutine(index)
        );
    }

    public void CompleteCurrentSubQuest()
    {
        if (currentSubQuestIndex >= 0)
        {
            CompleteSubQuest(currentSubQuestIndex);
        }
    }

    public void SetSubQuestHintsSuppressed(
        bool suppressed,
        bool showCurrentHintOnResume = true)
    {
        subQuestHintsSuppressed = suppressed;
        RefreshHintControlVisibility();

        if (suppressed)
        {
            if (hintRoutine != null)
            {
                StopCoroutine(hintRoutine);
                hintRoutine = null;
            }

            HideCanvasGroupInstant(subQuestHintPanelGroup);
            HideCanvasGroupInstant(hintControlPanelGroup);
            return;
        }

        RefreshHintControlVisibility();

        if (showCurrentHintOnResume &&
            !isCompletingChapter &&
            currentSubQuestIndex >= 0 &&
            completedSubQuests != null &&
            currentSubQuestIndex < completedSubQuests.Length &&
            !completedSubQuests[currentSubQuestIndex])
        {
            ShowCurrentSubQuestHint();
        }
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
                "[X] " +
                chapter.subQuests[
                    completedIndex
                ].title;
        }

        RefreshHintControlVisibility();

        yield return FadeCanvasGroup(
            subQuestHintPanelGroup,
            1f,
            hintFadeDuration
        );

        yield return WaitForSecondsRealtimePausable(1.5f);

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

    private bool AllowsOutOfOrderCompletion()
    {
        return chapters != null &&
               currentChapterIndex >= 0 &&
               currentChapterIndex < chapters.Length &&
               chapters[currentChapterIndex] != null &&
               chapters[currentChapterIndex]
                   .allowOutOfOrderCompletion;
    }

    private void RefreshHintControlVisibility()
    {
        if (hintControlPanelGroup == null)
        {
            return;
        }

        bool hasReviewableHint =
            questFlowStarted &&
            !subQuestHintsSuppressed &&
            !isCompletingChapter &&
            currentSubQuestIndex >= 0 &&
            completedSubQuests != null &&
            currentSubQuestIndex < completedSubQuests.Length &&
            !completedSubQuests[currentSubQuestIndex];

        hintControlPanelGroup.alpha =
            hasReviewableHint ? 1f : 0f;
        hintControlPanelGroup.interactable = false;
        hintControlPanelGroup.blocksRaycasts = false;

        if (hintControlText != null && hasReviewableHint)
        {
            hintControlText.text = detailedHintUnlocked
                ? "<b><color=#D9B66F>H</color></b> để xem lại gợi ý chi tiết"
                : "<b><color=#D9B66F>H</color></b> để xem lại gợi ý";
        }
    }

    private bool IsSubQuestHintPresentationBlocked()
    {
        // Các mini game của dự án đều mở con trỏ và bỏ khóa chuột.
        // Trong trạng thái đó không tự bật hint đè lên UI đang thao tác.
        bool modalUiIsOpen =
            Cursor.visible &&
            Cursor.lockState != CursorLockMode.Locked;

        return subQuestHintsSuppressed || modalUiIsOpen;
    }

    public void CompleteCurrentChapter()
    {
        if (isCompletingChapter ||
            pendingNextChapterIndex >= 0)
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
            string completeMessage =
                chapters[currentChapterIndex]
                    .chapterCompleteMessage;

            questHintText.text =
                string.IsNullOrWhiteSpace(completeMessage)
                    ? "Toàn bộ mục tiêu đã hoàn thành..."
                    : completeMessage;
        }

        yield return WaitForSecondsRealtimePausable(1.5f);

        yield return FadeCanvasGroup(
            questPanelGroup,
            0f,
            fadeDuration
        );

        isQuestVisible = false;

        int completedChapterIndex = currentChapterIndex;
        int nextChapter = completedChapterIndex + 1;

        chapterRoutine = null;
        isCompletingChapter = false;

        if (nextChapter < chapters.Length)
        {
            pendingNextChapterIndex = nextChapter;

            if (!chapters[completedChapterIndex]
                    .waitForTransitionSignal)
            {
                nextChapterStartRequested = true;
            }

            TryStartPendingChapter();
        }
        else
        {
            Debug.Log(
                "Đã hoàn thành toàn bộ nhiệm vụ."
            );
        }
    }

    public bool RequestStartNextChapter()
    {
        if (!isCompletingChapter &&
            pendingNextChapterIndex < 0)
        {
            Debug.LogWarning(
                "Chưa có chương kế tiếp đang chờ để bắt đầu."
            );
            return false;
        }

        nextChapterStartRequested = true;
        TryStartPendingChapter();
        return true;
    }

    private void TryStartPendingChapter()
    {
        if (!nextChapterStartRequested ||
            pendingNextChapterIndex < 0 ||
            isCompletingChapter ||
            nextChapterStartRoutine != null)
        {
            return;
        }

        float delay = 0f;

        if (chapters != null &&
            currentChapterIndex >= 0 &&
            currentChapterIndex < chapters.Length)
        {
            delay = Mathf.Max(
                0f,
                chapters[currentChapterIndex]
                    .nextChapterStartDelay
            );
        }

        nextChapterStartRoutine = StartCoroutine(
            StartPendingChapterRoutine(delay)
        );
    }

    private IEnumerator StartPendingChapterRoutine(
        float delay)
    {
        if (delay > 0f)
        {
            yield return WaitForSecondsRealtimePausable(delay);
        }

        int chapterToStart = pendingNextChapterIndex;

        nextChapterStartRoutine = null;
        pendingNextChapterIndex = -1;
        nextChapterStartRequested = false;

        if (chapterToStart >= 0 &&
            chapters != null &&
            chapterToStart < chapters.Length)
        {
            ShowChapterQuest(chapterToStart);
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
        if (gameplayUiSuppressed ||
            isShowingThought ||
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
            ShowQuestProgressWhenAvailableRoutine()
        );
    }

    private IEnumerator ShowQuestProgressWhenAvailableRoutine()
    {
        while (hintRoutine != null ||
               subQuestHintPanelGroup != null &&
               subQuestHintPanelGroup.alpha > 0.01f)
        {
            yield return null;
        }

        yield return ShowQuestProgressRoutine();
    }

    private IEnumerator ShowQuestProgressRoutine()
    {
        yield return FadeCanvasGroup(
            questPanelGroup,
            1f,
            fadeDuration
        );

        isQuestVisible = true;

        yield return WaitForSecondsRealtimePausable(
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
                "[X] " + title;

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
                "> " + title + progressText;

            subQuestTexts[index].color = Color.white;
            return;
        }

        if (AllowsOutOfOrderCompletion())
        {
            subQuestTexts[index].text = "[ ] " + title;
            subQuestTexts[index].color =
                new Color(0.75f, 0.75f, 0.75f);
        }
        else
        {
            subQuestTexts[index].text =
                "[ ] " + title + " [Chưa mở]";

            subQuestTexts[index].color =
                new Color(0.45f, 0.45f, 0.45f);
        }
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
                if (PauseMenu.IsPaused)
                {
                    yield return null;
                    continue;
                }

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

        // Các CanvasGroup trong QuestManager chỉ dùng để hiển thị thông tin.
        // Không cho chúng chặn raycast để người chơi vẫn kéo/thả và bấm UI mini game.
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
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
            while (PauseMenu.IsPaused)
            {
                yield return null;
            }

            textTarget.text += character;

            if (character == '.' ||
                character == '!' ||
                character == '?' ||
                character == '…')
            {
                yield return WaitForSecondsRealtimePausable(
                    speed * 6f
                );
            }
            else if (character == ',' ||
                     character == ';' ||
                     character == ':')
            {
                yield return WaitForSecondsRealtimePausable(
                    speed * 3f
                );
            }
            else
            {
                yield return WaitForSecondsRealtimePausable(
                    speed
                );
            }
        }
    }

    private IEnumerator WaitForSecondsRealtimePausable(
        float duration)
    {
        float timer = 0f;
        duration = Mathf.Max(0f, duration);

        while (timer < duration)
        {
            if (PauseMenu.IsPaused)
            {
                yield return null;
                continue;
            }

            timer += Time.unscaledDeltaTime;
            yield return null;
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

        if (scheduledHintRoutine != null)
        {
            StopCoroutine(scheduledHintRoutine);
            scheduledHintRoutine = null;
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
