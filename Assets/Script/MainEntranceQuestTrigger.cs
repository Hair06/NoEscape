using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class MainEntranceQuestTrigger : MonoBehaviour
{
    [Header("Quest được kích hoạt")]
    [Tooltip("0 = Nhiệm vụ mở đầu, 1 = Chương 1, 2 = Chương 2")]
    [SerializeField, Min(0)] private int startingChapterIndex = 0;

    [Header("Cấu hình Trigger")]
    [Tooltip("Tự tắt Collider sau khi kích hoạt thành công.")]
    [SerializeField] private bool disableAfterTriggered = true;

    private BoxCollider triggerCollider;
    private bool hasTriggered;

    private void Awake()
    {
        triggerCollider = GetComponent<BoxCollider>();
        Rigidbody triggerBody = GetComponent<Rigidbody>();

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }

        if (triggerBody != null)
        {
            triggerBody.isKinematic = true;
            triggerBody.useGravity = false;
            triggerBody.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !IsPlayer(other))
        {
            return;
        }

        if (QuestManager.Instance == null)
        {
            Debug.LogWarning(
                "MainEntranceQuestTrigger không tìm thấy QuestManager trong Scene."
            );
            return;
        }

        bool started = QuestManager.Instance.BeginQuestFlow(
            startingChapterIndex
        );

        if (!started)
        {
            return;
        }

        hasTriggered = true;

        if (disableAfterTriggered && triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        Debug.Log(
            "Player đã đi qua cửa chính. Bắt đầu nội tâm và bảng nhiệm vụ."
        );
    }

    private bool IsPlayer(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        if (other.CompareTag("Player"))
        {
            return true;
        }

        Transform root = other.transform.root;
        return root != null && root.CompareTag("Player");
    }

    private void Reset()
    {
        BoxCollider box = GetComponent<BoxCollider>();

        if (box != null)
        {
            box.isTrigger = true;
            box.size = new Vector3(2.5f, 2.5f, 1f);
        }

        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.isKinematic = true;
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    private void OnValidate()
    {
        startingChapterIndex = Mathf.Max(0, startingChapterIndex);

        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            box.isTrigger = true;
        }
    }
}