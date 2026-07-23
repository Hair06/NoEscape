using UnityEngine;
using TMPro;
using System.Collections;

public class WoodenBarDoor : MonoBehaviour, IInteractable
{
    [Header("Yêu cầu item")]
    public string requiredItem = "Crowbar";

    [Header("Liên kết bảng nhiệm vụ")]
    [SerializeField, Min(0)] private int questChapterIndex = 3;
    [SerializeField, Min(0)] private int questSubQuestIndex = 0;
    [SerializeField, Min(1)] private int requiredProgress = 2;

    [Header("Thanh gỗ chặn cửa")]
    public GameObject[] woodenBars;

    [Header("Cửa")]
    public Transform door;
    public float doorOpenAngle = -90f;
    public float doorOpenSpeed = 2f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Âm thanh")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip breakSound;
    [SerializeField] private AudioClip doorSound;

    private bool isOpened = false;
    private bool isPlayerInside = false;

    private void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!MiniGameFlowManager.IsChapterActive(
                questChapterIndex))
        {
            if (promptText != null)
                promptText.gameObject.SetActive(false);
            return;
        }

        if (isPlayerInside && !isOpened && GameInputBridge.GetKeyDown(KeyCode.E))
            Interact();
    }

    public string GetInteractPrompt()
    {
        if (!MiniGameFlowManager.IsChapterActive(
                questChapterIndex))
            return "";

        if (isOpened) return "";
        if (PlayerInventory.Count(requiredItem) > 0)
            return "Nhấn [E] để dùng xà beng phá các tấm gỗ";
        return "Cần tìm xà beng để phá cửa";
    }

    public void Interact()
    {
        if (!MiniGameFlowManager.IsChapterActive(
                questChapterIndex))
            return;

        if (isOpened) return;

        if (PlayerInventory.Count(requiredItem) <= 0)
        {
            Debug.Log("Cần có Crowbar!");
            return;
        }

        StartCoroutine(OpenDoor());
    }

    private IEnumerator OpenDoor()
    {
        isOpened = true;
        PlayerInventory.RemoveAll(requiredItem);

        if (audioSource != null && breakSound != null)
        {
            audioSource.clip = breakSound;
            audioSource.Play();
        }

        foreach (GameObject bar in woodenBars)
        {
            if (bar == null) continue;

            Rigidbody rb = bar.GetComponent<Rigidbody>();
            if (rb == null) rb = bar.AddComponent<Rigidbody>();

            Vector3 flyDir = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1f),
                Random.Range(-1f, 1f)
            ).normalized;

            rb.AddForce(flyDir * 5f, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(0.5f);

        foreach (GameObject bar in woodenBars)
        {
            if (bar != null) Destroy(bar, 2f);
        }

        if (audioSource != null && doorSound != null)
        {
            audioSource.clip = doorSound;
            audioSource.Play();
        }

        if (door != null)
        {
            float elapsed = 0f;
            Quaternion startRot = door.localRotation;
            Quaternion endRot = startRot * Quaternion.Euler(0f, doorOpenAngle, 0f);

            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * doorOpenSpeed;
                door.localRotation = Quaternion.Lerp(startRot, endRot, elapsed);
                yield return null;
            }
        }

        if (promptText != null) promptText.gameObject.SetActive(false);

        ReportQuestProgress();
        Debug.Log("Đã phá các tấm gỗ và mở cửa phòng chứa Con Mắt!");
    }

    private void ReportQuestProgress()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("WoodenBarDoor: Không tìm thấy QuestManager.");
            return;
        }

        QuestManager.Instance.ReportProgressForChapter(
            questChapterIndex,
            questSubQuestIndex,
            1,
            requiredProgress
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") ||
            !MiniGameFlowManager.IsChapterActive(
                questChapterIndex))
            return;
        isPlayerInside = true;

        if (promptText != null && !isOpened)
        {
            promptText.text = GetInteractPrompt();
            promptText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (promptText != null && !isOpened)
        {
            promptText.text = GetInteractPrompt();
            promptText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInside = false;
        if (promptText != null) promptText.gameObject.SetActive(false);
    }
}
