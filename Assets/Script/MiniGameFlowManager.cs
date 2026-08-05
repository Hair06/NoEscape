using UnityEngine;

/// <summary>
/// Điều phối các UI mini game dạng toàn màn hình.
/// Mỗi thời điểm chỉ cho phép một mini game mở và chỉ mở khi đúng chương.
/// </summary>
public static class MiniGameFlowManager
{
    private static Object activeOwner;
    private static GameObject activePanel;
    private static bool activePanelHiddenByPause;

    public static bool HasActiveMiniGame => activeOwner != null;

    public static bool IsPaused => PauseMenu.IsPaused;

    public static bool IsOpenBy(Object owner)
    {
        return owner != null && activeOwner == owner;
    }

    /// <summary>
    /// Scene test không có QuestManager vẫn được phép chạy mini game độc lập.
    /// Trong scene gameplay, mini game chỉ hoạt động khi đúng chương đã bắt đầu.
    /// </summary>
    public static bool IsChapterActive(int chapterIndex)
    {
        if (IsPaused)
        {
            return false;
        }

        QuestManager questManager = QuestManager.Instance;

        if (questManager == null)
        {
            return true;
        }

        return questManager.CanOpenMiniGameForChapter(chapterIndex);
    }

    public static bool CanContinue(
        Object owner,
        int chapterIndex)
    {
        if (IsPaused)
        {
            return false;
        }

        if (!IsOpenBy(owner))
        {
            return IsChapterActive(chapterIndex);
        }

        QuestManager questManager = QuestManager.Instance;

        return questManager == null ||
               questManager.IsQuestFlowStarted &&
               questManager.CurrentChapterIndex == chapterIndex &&
               !questManager.IsChapterTransitioning;
    }

    public static bool TryOpen(
        Object owner,
        GameObject panel,
        int chapterIndex)
    {
        if (IsPaused)
        {
            return false;
        }

        if (owner == null)
        {
            Debug.LogWarning(
                "MiniGameFlowManager: Không thể mở mini game vì Owner bị trống."
            );
            return false;
        }

        if (!IsChapterActive(chapterIndex))
        {
            Debug.Log(
                $"Mini game '{owner.name}' chưa đến lượt mở ở Chapter {chapterIndex}."
            );
            return false;
        }

        if (activeOwner != null && activeOwner != owner)
        {
            Debug.LogWarning(
                $"Không thể mở '{owner.name}' vì mini game " +
                $"'{activeOwner.name}' vẫn đang hoạt động."
            );
            return false;
        }

        activeOwner = owner;
        activePanel = panel;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.SetGameplayUiSuppressed(true);
        }

        if (activePanel != null)
        {
            activePanel.SetActive(true);
            activePanelHiddenByPause = false;
        }

        return true;
    }

    public static void ApplyPauseState(bool paused)
    {
        if (activePanel == null)
        {
            activePanelHiddenByPause = false;
            return;
        }

        if (paused)
        {
            if (activePanel.activeSelf)
            {
                activePanel.SetActive(false);
                activePanelHiddenByPause = true;
            }

            return;
        }

        if (activePanelHiddenByPause && activeOwner != null)
        {
            activePanel.SetActive(true);
        }

        activePanelHiddenByPause = false;
    }

    /// <param name="resumeQuestUi">
    /// Đặt false khi chuyển thẳng từ mini game sang cutscene/jumpscare.
    /// </param>
    public static void Close(
        Object owner,
        GameObject panel = null,
        bool resumeQuestUi = true)
    {
        if (owner == null)
        {
            return;
        }

        if (activeOwner != null && activeOwner != owner)
        {
            return;
        }

        GameObject panelToClose =
            panel != null ? panel : activePanel;

        if (panelToClose != null)
        {
            panelToClose.SetActive(false);
        }

        activeOwner = null;
        activePanel = null;
        activePanelHiddenByPause = false;

        if (resumeQuestUi && QuestManager.Instance != null)
        {
            QuestManager.Instance.SetGameplayUiSuppressed(
                false,
                false
            );
        }
    }
}
