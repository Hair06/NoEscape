using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ElmanGameDevTools.PlayerSystem;

public class Generator : MonoBehaviour, IInteractable
{
    [Header("Cấu hình nhiên liệu")]
    [SerializeField] private string fuelItemName = "Xang";
    [SerializeField] private int requiredCans = 2;

    [Header("Trạng thái")]
    [SerializeField] private bool isPowered;

    [Header("Thanh đổ xăng")]
    [SerializeField] private Slider fuelBar;

    [Header("Khóa Player khi đổ xăng")]
    [SerializeField] private PlayerController playerController;

    [Tooltip("Lượng xăng tăng sau mỗi lần bấm chuột trái")]
    [SerializeField, Range(0.01f, 1f)]
    private float fillPerPress = 0.06f;

    [Tooltip("Lượng xăng bị tụt mỗi giây khi không bấm chuột")]
    [SerializeField, Min(0f)]
    private float drainPerSecond = 0.25f;

    [Header("Hiệu ứng")]
    [SerializeField] private AudioSource generatorAudio;
    [SerializeField] private ParticleSystem exhaustSmoke;

    [Header("Sự kiện khi máy phát hoạt động")]
    [SerializeField] private UnityEvent onPowerOn;

    [Header("Nhiệm vụ")]
    [SerializeField] private int chapterIndex = 0;
    [SerializeField] private int generatorSubQuestIndex = 1;

    private bool isFilling;
    private float fillAmount;

    public bool IsPowered => isPowered;

    private void Start()
    {
        if (playerController == null)
        {
            playerController =
                FindFirstObjectByType<PlayerController>();
        }

        fillAmount = 0f;
        isFilling = false;

        if (fuelBar != null)
        {
            fuelBar.minValue = 0f;
            fuelBar.maxValue = 1f;
            fuelBar.value = 0f;
            fuelBar.gameObject.SetActive(false);
        }

        if (exhaustSmoke != null && !isPowered)
        {
            exhaustSmoke.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }

    private void Update()
    {
        if (!isFilling || isPowered)
        {
            return;
        }

        bool pressedLeftMouse =
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame;

        if (pressedLeftMouse)
        {
            fillAmount += fillPerPress;
        }
        else
        {
            fillAmount -= drainPerSecond * Time.deltaTime;
        }

        fillAmount = Mathf.Clamp01(fillAmount);

        if (fuelBar != null)
        {
            fuelBar.value = fillAmount;
        }

        if (fillAmount >= 1f)
        {
            PowerOn();
        }
    }

    public string GetInteractPrompt()
    {
        if (!MiniGameFlowManager.IsChapterActive(chapterIndex))
        {
            return "";
        }

        if (isPowered)
        {
            return "";
        }

        if (isFilling)
        {
            return "Bấm chuột trái liên tục để đổ xăng";
        }

        int currentCans = PlayerInventory.Count(fuelItemName);

        if (currentCans < requiredCans)
        {
            return $"Cần tìm đủ can xăng ({currentCans}/{requiredCans})";
        }

        return "Nhấn E để bắt đầu đổ xăng";
    }

    public void Interact()
    {
        if (isPowered)
        {
            Debug.Log("Máy phát điện đã hoạt động.");
            return;
        }

        if (isFilling)
        {
            return;
        }

        int currentCans = PlayerInventory.Count(fuelItemName);

        if (currentCans < requiredCans)
        {
            Debug.Log(
                $"Chưa đủ can xăng. Hiện có {currentCans}/{requiredCans}."
            );

            return;
        }

        BeginFueling();
    }

    private void BeginFueling()
    {
        if (fuelBar == null)
        {
            Debug.LogError(
                "Generator chưa được gán Fuel Bar trong Inspector."
            );

            return;
        }

        if (!MiniGameFlowManager.TryOpen(
                this,
                fuelBar.gameObject,
                chapterIndex))
        {
            return;
        }

        isFilling = true;
        fillAmount = 0f;

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        fuelBar.value = 0f;
        fuelBar.gameObject.SetActive(true);

        Debug.Log(
            "Bắt đầu đổ xăng. Bấm chuột trái liên tục để làm đầy thanh."
        );
    }

    private void PowerOn()
    {
        if (isPowered)
        {
            return;
        }

        isPowered = true;
        isFilling = false;
        fillAmount = 1f;

        if (fuelBar != null)
        {
            fuelBar.value = 1f;
            fuelBar.gameObject.SetActive(false);
        }

        MiniGameFlowManager.Close(
            this,
            fuelBar != null ? fuelBar.gameObject : null
        );

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Xóa các can xăng khỏi kho sau khi sử dụng.
        PlayerInventory.RemoveAll(fuelItemName);

        if (generatorAudio != null)
        {
            generatorAudio.loop = true;

            if (!generatorAudio.isPlaying)
            {
                generatorAudio.Play();
            }
        }

        if (exhaustSmoke != null)
        {
            exhaustSmoke.Play();
        }

        // Gọi RoomLight.TurnOn() được gán trong Inspector.
        onPowerOn?.Invoke();

        // Hoàn thành nhiệm vụ:
        // "Đổ xăng và khởi động máy phát điện".
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteSubQuestForChapter(
                chapterIndex,
                generatorSubQuestIndex
            );
        }
        else
        {
            Debug.LogWarning(
                "Không tìm thấy QuestManager trong Scene."
            );
        }

        Debug.Log("Máy phát đã hoạt động và hệ thống đèn đã được bật.");
    }
}
