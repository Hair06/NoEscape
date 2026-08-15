using UnityEngine;

public class StoneDoorPuzzle : MonoBehaviour
{
    private const int ChapterIndex = 4;
    private const int PlaceAndOpenSubQuestIndex = 1;

    [Header("Bệ Đá Xanh")]
    [SerializeField] private GameObject ghostCrystal;
    [SerializeField] private GameObject realCrystal;

    [Header("Bệ Đá Đỏ")]
    [SerializeField] private GameObject ghostRed;
    [SerializeField] private GameObject realRed;

    [Header("Cửa phong ấn")]
    [SerializeField] private Transform doorStoneTransform;
    [SerializeField, Min(0.1f)] private float sinkDistance = 5f;
    [SerializeField, Min(0.1f)] private float sinkSpeed = 2f;
    [SerializeField, Min(0f)] private float completeDistance = 0.01f;

    [Header("Âm thanh")]
    [SerializeField] private AudioSource placeStoneSound;
    [SerializeField] private AudioSource openDoorSound;

    [Header("Hiệu ứng khi mở cửa")]
    [SerializeField] private ParticleSystem openDoorVfx;

    public bool IsBluePlaced { get; private set; }
    public bool IsRedPlaced { get; private set; }
    public bool IsDoorOpen { get; private set; }

    private bool isDoorOpening;
    private bool questReported;
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
                targetDoorPosition) > completeDistance)
        {
            return;
        }

        doorStoneTransform.position = targetDoorPosition;
        isDoorOpening = false;
        IsDoorOpen = true;

        doorStoneTransform.gameObject.SetActive(false);
        CompletePlaceAndOpenQuest();

        Debug.Log(
            "Cửa phong ấn đã hạ hoàn toàn; lối đi đã mở."
        );
    }

    public bool IsPlacementStepActive()
    {
        QuestManager questManager = QuestManager.Instance;

        return !IsDoorOpen &&
               !isDoorOpening &&
               questManager != null &&
               MiniGameFlowManager.IsChapterActive(ChapterIndex) &&
               questManager.CurrentChapterIndex == ChapterIndex &&
               questManager.CurrentSubQuestIndex ==
                   PlaceAndOpenSubQuestIndex &&
               !questManager.IsChapterTransitioning;
    }

    public bool TryPlaceBlueStone()
    {
        if (!IsPlacementStepActive() ||
            IsBluePlaced ||
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
        if (!IsPlacementStepActive() ||
            IsRedPlaced ||
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
            Debug.Log(
                "Đã đặt một viên đá; vẫn còn thiếu viên còn lại."
            );
            return;
        }

        if (doorStoneTransform == null)
        {
            Debug.LogError(
                "Không thể mở cửa vì chưa gán Door Stone Transform."
            );
            return;
        }

        if (openDoorSound != null)
        {
            openDoorSound.Play();
        }

        if (openDoorVfx != null)
        {
            openDoorVfx.Play();
        }

        isDoorOpening = true;

        Debug.Log(
            "Đã đặt đủ hai viên đá; cửa phong ấn bắt đầu mở."
        );
    }

    private void CompletePlaceAndOpenQuest()
    {
        if (questReported)
        {
            return;
        }

        questReported = true;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteSubQuestForChapter(
                ChapterIndex,
                PlaceAndOpenSubQuestIndex
            );
        }
    }

    private static void SetStoneVisuals(
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
