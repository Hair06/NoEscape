using UnityEngine;

public class StoneDoorPuzzle : MonoBehaviour
{
    private const int ChapterIndex = 4;
    private const int PlaceStonesSubQuestIndex = 1;
    private const int RequiredPlacedStones = 2;

    [Header("Bệ Đá Xanh")]
    [SerializeField] private GameObject ghostCrystal;
    [SerializeField] private GameObject realCrystal;

    [Header("Bệ Đá Đỏ")]
    [SerializeField] private GameObject ghostRed;
    [SerializeField] private GameObject realRed;

    [Header("Cửa đá")]
    [SerializeField] private Transform doorStoneTransform;
    [SerializeField, Min(0.1f)] private float sinkDistance = 5f;
    [SerializeField, Min(0.1f)] private float sinkSpeed = 2f;

    [Header("Âm thanh")]
    [SerializeField] private AudioSource placeStoneSound;
    [SerializeField] private AudioSource openDoorSound;

    public bool IsBluePlaced { get; private set; }
    public bool IsRedPlaced { get; private set; }

    private bool isDoorOpening;
    private bool doorQuestCompleted;
    private int reportedPlacementProgress;
    private Vector3 targetDoorPosition;

    private void Start()
    {
        SetStoneVisuals(
            ghostCrystal,
            realCrystal,
            IsBluePlaced
        );

        SetStoneVisuals(
            ghostRed,
            realRed,
            IsRedPlaced
        );

        if (doorStoneTransform == null)
        {
            Debug.LogError(
                "StoneDoorPuzzle: Chưa gán Door Stone Transform."
            );
            return;
        }

        targetDoorPosition =
            doorStoneTransform.position -
            Vector3.up * sinkDistance;
    }

    private void Update()
    {
        if (!isDoorOpening ||
            doorStoneTransform == null ||
            PauseMenu.IsPaused)
        {
            return;
        }

        doorStoneTransform.position = Vector3.MoveTowards(
            doorStoneTransform.position,
            targetDoorPosition,
            sinkSpeed * Time.deltaTime
        );

        if (Vector3.Distance(
                doorStoneTransform.position,
                targetDoorPosition) > 0.01f)
        {
            return;
        }

        isDoorOpening = false;
        doorStoneTransform.gameObject.SetActive(false);
        CompleteDoorQuest();

        Debug.Log(
            "Cửa đá đã hạ hoàn toàn; lối đi đã mở."
        );
    }

    public bool TryPlaceBlueStone()
    {
        if (IsBluePlaced ||
            !MiniGameFlowManager.IsChapterActive(
                ChapterIndex) ||
            !StonePickup.HasBlueStone)
        {
            return false;
        }

        IsBluePlaced = true;
        StonePickup.Consume(StoneType.Blue);
        SetStoneVisuals(
            ghostCrystal,
            realCrystal,
            true
        );
        PlayPlacementSound();
        HandleStonePlaced();
        return true;
    }

    public bool TryPlaceRedStone()
    {
        if (IsRedPlaced ||
            !MiniGameFlowManager.IsChapterActive(
                ChapterIndex) ||
            !StonePickup.HasRedStone)
        {
            return false;
        }

        IsRedPlaced = true;
        StonePickup.Consume(StoneType.Red);
        SetStoneVisuals(
            ghostRed,
            realRed,
            true
        );
        PlayPlacementSound();
        HandleStonePlaced();
        return true;
    }

    private void HandleStonePlaced()
    {
        if (!IsBluePlaced || !IsRedPlaced)
        {
            ReportPlacementProgress(1);
            return;
        }

        if (openDoorSound != null)
        {
            openDoorSound.Play();
        }

        isDoorOpening = true;

        Debug.Log(
            "Đã đặt đủ hai viên đá; cửa bắt đầu hạ xuống."
        );
    }

    private void CompleteDoorQuest()
    {
        if (doorQuestCompleted)
        {
            return;
        }

        doorQuestCompleted = true;
        int remainingProgress = Mathf.Max(
            0,
            RequiredPlacedStones -
            reportedPlacementProgress
        );

        if (remainingProgress > 0)
        {
            ReportPlacementProgress(
                remainingProgress
            );
        }
    }

    private void ReportPlacementProgress(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        reportedPlacementProgress = Mathf.Clamp(
            reportedPlacementProgress + amount,
            0,
            RequiredPlacedStones
        );

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ReportProgressForChapter(
                ChapterIndex,
                PlaceStonesSubQuestIndex,
                amount,
                RequiredPlacedStones
            );
        }
    }

    private void SetStoneVisuals(
        GameObject ghost,
        GameObject realStone,
        bool placed)
    {
        if (ghost != null)
        {
            ghost.SetActive(!placed);
        }

        if (realStone != null)
        {
            realStone.SetActive(placed);
        }
    }

    private void PlayPlacementSound()
    {
        if (placeStoneSound != null)
        {
            placeStoneSound.Play();
        }
    }
}