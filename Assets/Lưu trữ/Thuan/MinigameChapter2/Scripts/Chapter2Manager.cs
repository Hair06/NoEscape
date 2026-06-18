using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Quản lý trạng thái thu thập 4 bộ phận Hộp Nhạc trong Chương 2.
/// Đặt script này lên một GameObject trống tên "Chapter2Manager" trong Scene.
/// </summary>
public class Chapter2Manager : MonoBehaviour
{
    public static Chapter2Manager Instance;

    [Header("Trạng thái 4 bộ phận Hộp Nhạc")]
    public bool hasShuttle = false;   // Con Thoi Nhạc
    public bool hasSpring = false;   // Lò Xo Nhạc
    public bool hasDisc = false;   // Đĩa Nhạc
    public bool hasKey = false;   // Chìa Vặn

    [Header("Sự kiện khi thu thập đủ 4 bộ phận")]
    public UnityEvent onAllPartsCollected;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Gọi từ script từng bộ phận khi người chơi thu thập thành công.
    /// partName: "shuttle" | "spring" | "disc" | "key"
    /// </summary>
    public void CollectPart(string partName)
    {
        switch (partName)
        {
            case "shuttle": hasShuttle = true; break;
            case "spring": hasSpring = true; break;
            case "disc": hasDisc = true; break;
            case "key": hasKey = true; break;
            default:
                Debug.LogWarning($"[Ch2Manager] Tên bộ phận không hợp lệ: '{partName}'");
                return;
        }

        Debug.Log($"[Ch2] Thu thập '{partName}' | Tiến độ: {GetCollectedCount()}/4");

        if (HasAllParts())
        {
            Debug.Log("[Ch2] Đã đủ cả 4 bộ phận! Sẵn sàng sửa Hộp Nhạc.");
            onAllPartsCollected?.Invoke();
        }
    }

    public bool HasAllParts() => hasShuttle && hasSpring && hasDisc && hasKey;

    public int GetCollectedCount()
    {
        int n = 0;
        if (hasShuttle) n++;
        if (hasSpring) n++;
        if (hasDisc) n++;
        if (hasKey) n++;
        return n;
    }
}