using UnityEngine;

public class DoorData : MonoBehaviour
{
    [Header("Cấu hình riêng cho cánh cửa này")]
    public string doorName = "Cửa số 1";
    public Vector3 openRotationOffset = new Vector3(0, 90f, 0); // Mỗi cửa có thể xoay góc khác nhau
    public float openSpeed = 3f;
    public AudioClip doorOpenSound;

    [HideInInspector] public bool isPlayerInside = false;
    [HideInInspector] public bool isOpened = false;
    [HideInInspector] public Quaternion targetRotation;
    [HideInInspector] public Quaternion originalRotation;

    private void Start()
    {
        // Ghi nhớ lại góc xoay ban đầu của riêng nó
        originalRotation = transform.localRotation;
        targetRotation = originalRotation * Quaternion.Euler(openRotationOffset);
    }

    // Phát hiện Player đến gần cái cửa này
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpened)
        {
            isPlayerInside = true;
            // Báo cho Manager trung tâm hiển thị chữ gợi ý [E]
            if (GlobalDoorManager.Instance != null)
            {
                GlobalDoorManager.Instance.ShowPrompt(this);
            }
        }
    }

    // Khi Player đi xa khỏi cái cửa này
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            // Báo cho Manager trung tâm ẩn chữ gợi ý
            if (GlobalDoorManager.Instance != null)
            {
                GlobalDoorManager.Instance.HidePrompt(this);
            }
        }
    }
}