using UnityEngine;
using TMPro;
using System.Collections;

public class WoodenBarDoor : MonoBehaviour, IInteractable
{
    [Header("Yêu cầu item")]
    public string requiredItem = "Crowbar";

    [Header("Thanh gỗ chặn cửa")]
    public GameObject[] woodenBars;

    [Header("Cửa")]
    public Transform door;
    public float doorOpenAngle = 90f;
    public float doorOpenSpeed = 2f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Âm thanh")]
    [SerializeField] private AudioClip breakSound;
    [SerializeField] private AudioClip doorSound;

    private bool isOpened = false;
    private bool isPlayerInside = false;

    private void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerInside && !isOpened && GameInputBridge.GetKeyDown(KeyCode.E))
            Interact();
    }

    public string GetInteractPrompt()
    {
        if (isOpened) return "";
        if (PlayerInventory.Count(requiredItem) > 0)
            return "Nhấn [E] để phá thanh gỗ";
        return "Cần xà beng để phá cửa";
    }

    public void Interact()
    {
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

        // Play sound phá gỗ
        if (breakSound != null)
            AudioSource.PlayClipAtPoint(breakSound, transform.position);

        // Thanh gỗ bay ra
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

        // Destroy thanh gỗ sau 2 giây
        foreach (GameObject bar in woodenBars)
        {
            if (bar != null) Destroy(bar, 2f);
        }

        // Play sound cửa mở
        if (doorSound != null)
            AudioSource.PlayClipAtPoint(doorSound, transform.position);

        // Mở cửa xoay
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

        Debug.Log("Cửa đã mở!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
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