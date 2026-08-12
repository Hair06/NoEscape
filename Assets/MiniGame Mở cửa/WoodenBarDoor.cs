using UnityEngine;
using TMPro;
using System.Collections;

public class WoodenBarDoor : MonoBehaviour, IInteractable
{
    [Header("Yêu cầu item")]
    public string requiredItem = "Crowbar";

    [Header("Xà beng trên tay Player (ẩn sau khi dùng xong)")]
    [Tooltip("Kéo CÙNG object xà beng trên tay mà CrowbarCollectible đang dùng")]
    [SerializeField] private GameObject crowbarInHand;

    [Header("Liên kết bảng nhiệm vụ")]
    [SerializeField, Min(0)] private int questChapterIndex = 3;
    [SerializeField, Min(0)] private int questSubQuestIndex = 0;
    [SerializeField, Min(1)] private int requiredProgress = 2;

    [Header("Thanh gỗ chặn cửa (Kéo thứ tự từ ngoài vào trong)")]
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
    private bool isPrying = false;       // Khóa chống spam phím E quá nhanh
    private int currentBarIndex = 0;    // Đếm số thanh gỗ đã cậy

    private void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!MiniGameFlowManager.IsChapterActive(questChapterIndex))
        {
            if (promptText != null) promptText.gameObject.SetActive(false);
            return;
        }

        if (isPlayerInside && !isOpened && !isPrying && GameInputBridge.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    public string GetInteractPrompt()
    {
        if (!MiniGameFlowManager.IsChapterActive(questChapterIndex))
            return "";

        if (isOpened) return "";

        if (PlayerInventory.Count(requiredItem) <= 0)
            return "Cần tìm xà beng để phá cửa";

        return $"Nhấn [E] để cậy thanh gỗ ({currentBarIndex}/{woodenBars.Length})";
    }

    public void Interact()
    {
        if (!MiniGameFlowManager.IsChapterActive(questChapterIndex)) return;
        if (isOpened || isPrying) return;

        if (PlayerInventory.Count(requiredItem) <= 0)
        {
            Debug.Log("Cần có Crowbar!");
            return;
        }

        StartCoroutine(PrySingleBarRoutine());
    }

    private IEnumerator PrySingleBarRoutine()
    {
        isPrying = true;

        if (currentBarIndex < woodenBars.Length)
        {
            GameObject barGroup = woodenBars[currentBarIndex];

            if (barGroup != null)
            {
                // Tách nhóm thanh ván ra khỏi khung cửa
                barGroup.transform.SetParent(null);

                // Lấy tất cả Transform con (bao gồm cả barGroup) để bật vật lý rơi từng mảnh
                Transform[] allParts = barGroup.GetComponentsInChildren<Transform>();

                foreach (Transform part in allParts)
                {
                    // Nếu là object con có Mesh thì bật Rigidbody
                    if (part.GetComponent<MeshRenderer>() != null)
                    {
                        Rigidbody rb = part.gameObject.GetComponent<Rigidbody>();
                        if (rb == null) rb = part.gameObject.AddComponent<Rigidbody>();

                        rb.isKinematic = false;
                        rb.useGravity = true;

                        Collider col = part.gameObject.GetComponent<Collider>();
                        if (col != null) col.enabled = true;

                        // Tạo lực bật thanh gỗ văng ra hướng người chơi
                        Vector3 flyDir = (transform.forward + Vector3.up * 0.3f + new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f))).normalized;
                        rb.AddForce(flyDir * 4f, ForceMode.Impulse);
                        rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
                    }
                }

                // Tiếng cậy ván gỗ
                if (audioSource != null && breakSound != null)
                {
                    audioSource.PlayOneShot(breakSound);
                }

                // Tự hủy nhóm ván sau 3 giây
                Destroy(barGroup, 3f);
            }

            currentBarIndex++;

            if (promptText != null && isPlayerInside)
            {
                promptText.text = GetInteractPrompt();
            }

            yield return new WaitForSeconds(0.4f);
        }

        // Nếu đã cậy hết cả 4 nhóm ván -> Mở cửa
        if (currentBarIndex >= woodenBars.Length && !isOpened)
        {
            yield return StartCoroutine(OpenDoorSequence());
        }

        isPrying = false;
    }

    private IEnumerator OpenDoorSequence()
    {
        isOpened = true;

        // Dùng xong toàn bộ thanh gỗ -> xóa xà beng khỏi túi & ẩn khỏi tay
        PlayerInventory.RemoveAll(requiredItem);

        if (crowbarInHand != null)
        {
            crowbarInHand.SetActive(false);
            Debug.Log("Đã cất xà beng khỏi tay.");
        }

        // Phát tiếng mở cửa
        if (audioSource != null && doorSound != null)
        {
            audioSource.clip = doorSound;
            audioSource.Play();
        }

        // Xoay mở cửa
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
        Debug.Log("Đã cậy hết các thanh gỗ và mở cửa!");
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
        if (!other.CompareTag("Player") || !MiniGameFlowManager.IsChapterActive(questChapterIndex))
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