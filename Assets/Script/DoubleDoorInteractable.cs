using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DoubleDoorInteractable : MonoBehaviour
{
    [Header("Cấu hình UI Text bằng TextMesh Pro")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string interactMessage = "Nhấn [E] để mở/đóng cửa";

    [Header("Door Components")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    [Header("Rotation Settings")]
    public float openAngle = 90f;
    public float speed = 3f;

    // Tự động lấy DoorAudioController trên cùng GameObject
    private DoorAudioController doorAudio;

    private bool isOpen = false;
    private bool isPlayerInside = false;

    private Quaternion leftTargetRotation;
    private Quaternion rightTargetRotation;
    private Quaternion leftInitialRotation;
    private Quaternion rightInitialRotation;

    private void Start()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);

        if (leftDoor != null) leftInitialRotation = leftDoor.localRotation;
        if (rightDoor != null) rightInitialRotation = rightDoor.localRotation;

        leftTargetRotation = leftInitialRotation;
        rightTargetRotation = rightInitialRotation;

        doorAudio = GetComponent<DoorAudioController>();
    }

    private void Update()
    {
        if (leftDoor != null)
            leftDoor.localRotation = Quaternion.Slerp(leftDoor.localRotation, leftTargetRotation, Time.deltaTime * speed);
        if (rightDoor != null)
            rightDoor.localRotation = Quaternion.Slerp(rightDoor.localRotation, rightTargetRotation, Time.deltaTime * speed);

        if (isPlayerInside && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Interact();
    }

    public void Interact()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            leftTargetRotation = leftInitialRotation * Quaternion.Euler(0, -openAngle, 0);
            rightTargetRotation = rightInitialRotation * Quaternion.Euler(0, openAngle, 0);
            doorAudio?.PlayOpen();
        }
        else
        {
            leftTargetRotation = leftInitialRotation;
            rightTargetRotation = rightInitialRotation;
            doorAudio?.PlayClose();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            if (promptText != null)
            {
                promptText.text = interactMessage;
                promptText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (promptText != null) promptText.gameObject.SetActive(false);
        }
    }
}