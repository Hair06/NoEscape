using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ElmanGameDevTools.PlayerSystem;
using System.Collections;

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
    [Header("Startup Effect")]
[SerializeField] private float shakeDuration = 2f;
[SerializeField] private float shakeStrength = 0.03f;
[SerializeField] private float shakeSpeed = 35f;

private Vector3 originalLocalPosition;
private bool isStarting;

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
        originalLocalPosition = transform.localPosition;
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

       if (fillAmount >= 1f && !isStarting)
{
    StartCoroutine(StartGeneratorSequence());
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

   private IEnumerator StartGeneratorSequence()
{
    isStarting = true;

    isPowered = true;
    isFilling = false;

    if (fuelBar != null)
    {
        fuelBar.value = 1f;
        fuelBar.gameObject.SetActive(false);
    }

    MiniGameFlowManager.Close(
        this,
        fuelBar != null ? fuelBar.gameObject : null);

    if (playerController != null)
        playerController.enabled = true;

    PlayerInventory.RemoveAll(fuelItemName);

    //------------------------------------------------
    // RUNG MÁY
    //------------------------------------------------

    float timer = 0;

    while (timer < shakeDuration)
    {
        timer += Time.deltaTime;

        Vector3 offset = new Vector3(
            Random.Range(-shakeStrength, shakeStrength),
            Random.Range(-shakeStrength * 0.5f, shakeStrength * 0.5f),
            Random.Range(-shakeStrength, shakeStrength));

        transform.localPosition = originalLocalPosition + offset;

        transform.localRotation =
            Quaternion.Euler(
                Random.Range(-2f,2f),
                -90 + Random.Range(-2f,2f),
                Random.Range(-2f,2f));

        yield return null;
    }

    transform.localPosition = originalLocalPosition;
    transform.localRotation = Quaternion.Euler(0,-90,0);

    //------------------------------------------------
    // KHÓI
    //------------------------------------------------

    if(exhaustSmoke!=null)
    {
        exhaustSmoke.Play();
    }

    //------------------------------------------------
    // ÂM THANH
    //------------------------------------------------

    if(generatorAudio!=null)
    {
        generatorAudio.loop=true;

        if(!generatorAudio.isPlaying)
            generatorAudio.Play();
    }

    //------------------------------------------------
    // BẬT ĐIỆN
    //------------------------------------------------

    onPowerOn?.Invoke();

    //------------------------------------------------
    // QUEST
    //------------------------------------------------

    if(QuestManager.Instance!=null)
    {
        QuestManager.Instance.CompleteSubQuestForChapter(
            chapterIndex,
            generatorSubQuestIndex);
    }

    Debug.Log("Generator Started!");

    isStarting = false;
}
}
