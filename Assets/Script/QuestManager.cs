using System.Collections;
using TMPro;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("UI")]
    [SerializeField] private CanvasGroup questPanelGroup;
    [SerializeField] private TextMeshProUGUI chapterTitleText;
    [SerializeField] private TextMeshProUGUI mainQuestText;
    [SerializeField] private TextMeshProUGUI[] subQuestTexts;
    [SerializeField] private TextMeshProUGUI questHintText;

    [Header("Quest Data")]
    [SerializeField] private QuestData[] chapters;

    [Header("Effect")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float typeSpeed = 0.02f;
    [SerializeField] private float autoHideTime = 3f;

    private int currentChapterIndex = 0;
    private bool[] completedSubQuests;
    private bool isVisible = false;
    private Coroutine currentRoutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        HideInstant();
        ShowChapterQuest(0);
    }

    private void Update()
    {
        if (GameInputBridge.GetKeyDown(KeyCode.Tab))
        {
            ToggleQuestPanel();
        }
    }

    public void ShowChapterQuest(int chapterIndex)
    {
        if (chapterIndex < 0 || chapterIndex >= chapters.Length)
            return;

        currentChapterIndex = chapterIndex;

        QuestData data = chapters[chapterIndex];
        completedSubQuests = new bool[data.subQuests.Length];

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowQuestRoutine(data));
    }

    private IEnumerator ShowQuestRoutine(QuestData data)
    {
        SetPanelVisible(false);

        chapterTitleText.text = data.chapterTitle;
        mainQuestText.text = "";

        if (questHintText != null)
            questHintText.text = "TAB: Ẩn / hiện nhiệm vụ";

        for (int i = 0; i < subQuestTexts.Length; i++)
        {
            if (i < data.subQuests.Length)
            {
                subQuestTexts[i].text = "☐ " + data.subQuests[i];
                subQuestTexts[i].color = Color.white;
            }
            else
            {
                subQuestTexts[i].text = "";
            }
        }

        yield return FadePanel(1f);
        isVisible = true;

        yield return TypeText(mainQuestText, "Nhiệm vụ chính: " + data.mainQuest);

        yield return new WaitForSecondsRealtime(autoHideTime);

        yield return FadePanel(0f);
        isVisible = false;
    }

    public void CompleteSubQuest(int index)
    {
        if (completedSubQuests == null) return;
        if (index < 0 || index >= completedSubQuests.Length) return;
        if (completedSubQuests[index]) return;

        completedSubQuests[index] = true;

        QuestData data = chapters[currentChapterIndex];

        if (index < subQuestTexts.Length)
        {
            subQuestTexts[index].text = "☑ " + data.subQuests[index];
            subQuestTexts[index].color = new Color(0.65f, 0.65f, 0.65f);
        }

        Debug.Log("Hoàn thành nhiệm vụ nhỏ: " + data.subQuests[index]);
    }

    public void CompleteCurrentChapter()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(CompleteChapterRoutine());
    }

    private IEnumerator CompleteChapterRoutine()
    {
        yield return FadePanel(1f);
        isVisible = true;

        if (questHintText != null)
            questHintText.text = "Phong ấn đã được giải mã...";

        yield return new WaitForSecondsRealtime(1.2f);

        yield return FadePanel(0f);
        isVisible = false;

        int nextChapter = currentChapterIndex + 1;

        if (nextChapter < chapters.Length)
        {
            ShowChapterQuest(nextChapter);
        }
        else
        {
            Debug.Log("Đã hoàn thành toàn bộ nhiệm vụ.");
        }
    }

    private void ToggleQuestPanel()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        isVisible = !isVisible;

        if (isVisible)
            StartCoroutine(FadePanel(1f));
        else
            StartCoroutine(FadePanel(0f));
    }

    private IEnumerator FadePanel(float targetAlpha)
    {
        float startAlpha = questPanelGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            questPanelGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }

        questPanelGroup.alpha = targetAlpha;

        questPanelGroup.interactable = targetAlpha > 0;
        questPanelGroup.blocksRaycasts = targetAlpha > 0;
    }

    private IEnumerator TypeText(TextMeshProUGUI textTarget, string content)
    {
        textTarget.text = "";

        foreach (char c in content)
        {
            textTarget.text += c;
            yield return new WaitForSecondsRealtime(typeSpeed);
        }
    }

    private void HideInstant()
    {
        questPanelGroup.alpha = 0f;
        questPanelGroup.interactable = false;
        questPanelGroup.blocksRaycasts = false;
        isVisible = false;
    }

    private void SetPanelVisible(bool visible)
    {
        questPanelGroup.alpha = visible ? 1f : 0f;
        questPanelGroup.interactable = visible;
        questPanelGroup.blocksRaycasts = visible;
        isVisible = visible;
    }
}