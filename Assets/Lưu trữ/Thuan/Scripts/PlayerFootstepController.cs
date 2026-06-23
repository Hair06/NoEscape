using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Phát tiếng bước chân ngẫu nhiên khi Player di chuyển trên mặt đất.
/// Gắn script này vào cùng GameObject với PlayerController.
/// </summary>
public class PlayerFootstepController : MonoBehaviour
{
    // ---------------------------------------------------------------
    // INSPECTOR FIELDS
    // ---------------------------------------------------------------

    [Header("Âm thanh bước chân")]
    [Tooltip("Kéo nhiều clip tiếng bước chân vào đây để phát ngẫu nhiên (tránh lặp đơn điệu)")]
    [SerializeField] private AudioClip[] footstepClips;

    [Tooltip("AudioSource riêng cho bước chân. Nếu để trống sẽ tự tạo.")]
    [SerializeField] private AudioSource footstepSource;

    [Header("Chu kỳ bước chân (giây)")]
    [Tooltip("Thời gian giữa 2 bước chân khi đi bộ bình thường")]
    [SerializeField] private float stepInterval = 0.45f;

    [Header("Kiểm tra chạm đất")]
    [Tooltip("Khoảng cách raycast xuống đất để xác định Player đang đứng trên mặt đất")]
    [SerializeField] private float groundCheckDistance = 0.25f;
    [Tooltip("Layer của mặt đất (để raycast không bắn vào bản thân Player)")]
    [SerializeField] private LayerMask groundLayer = ~0; // Mặc định: tất cả layer

    [Header("Âm lượng")]
    [Range(0f, 1f)]
    [SerializeField] private float footstepVolume = 0.6f;

    // ---------------------------------------------------------------
    // PRIVATE STATE
    // ---------------------------------------------------------------

    private Player playerController;  // Tham chiếu để đọc trạng thái di chuyển
    private float stepTimer = 0f;               // Đếm ngược thời gian giữa 2 bước
    private int lastClipIndex = -1;             // Tránh phát cùng 1 clip 2 lần liên tiếp

    // ---------------------------------------------------------------
    // UNITY LIFECYCLE
    // ---------------------------------------------------------------

    private void Awake()
    {
        // Lấy PlayerController trên cùng GameObject
        playerController = GetComponent<Player>();

        // Tự tạo AudioSource nếu chưa gán
        if (footstepSource == null)
        {
            footstepSource = gameObject.AddComponent<AudioSource>();
            footstepSource.playOnAwake = false;
            footstepSource.spatialBlend = 0f; // 2D sound (nghe giống FPS hơn)
        }
    }

    private void Update()
    {
        // Không làm gì nếu không có clip nào
        if (footstepClips == null || footstepClips.Length == 0) return;

        // Chỉ phát khi: đang di chuyển VÀ đang đứng trên mặt đất
        if (IsMoving() && IsGrounded())
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = stepInterval; // Reset bộ đếm
            }
        }
        else
        {
            // Khi dừng lại, reset timer về 0 để bước tiếp theo phát ngay khi bắt đầu đi
            stepTimer = 0f;
        }
    }

    // ---------------------------------------------------------------
    // KIỂM TRA TRẠNG THÁI
    // ---------------------------------------------------------------

    /// <summary>Trả về true nếu Player đang nhấn phím di chuyển.</summary>
    private bool IsMoving()
    {
        // Đọc trực tiếp từ Input System thay vì phụ thuộc vào PlayerController
        if (Keyboard.current == null) return false;

        return Keyboard.current.wKey.isPressed
            || Keyboard.current.sKey.isPressed
            || Keyboard.current.aKey.isPressed
            || Keyboard.current.dKey.isPressed
            || Keyboard.current.upArrowKey.isPressed
            || Keyboard.current.downArrowKey.isPressed
            || Keyboard.current.leftArrowKey.isPressed
            || Keyboard.current.rightArrowKey.isPressed;
    }

    /// <summary>Raycast xuống đất kiểm tra Player có đang đứng trên mặt đất không.</summary>
    private bool IsGrounded()
    {
        // Bắn tia từ giữa thân Player xuống dưới
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, groundCheckDistance + 0.1f, groundLayer);
    }

    // ---------------------------------------------------------------
    // PHÁT ÂM THANH
    // ---------------------------------------------------------------

    private void PlayFootstep()
    {
        AudioClip clip = GetRandomClip();
        if (clip == null) return;

        // Tính âm lượng cuối: nhân với SFX volume từ AudioManager (nếu có)
        float finalVolume = footstepVolume;
        if (AudioManager.Instance != null && AudioManager.Instance.sfxSource != null)
            finalVolume *= AudioManager.Instance.sfxSource.volume;

        footstepSource.PlayOneShot(clip, finalVolume);
    }

    /// <summary>Chọn ngẫu nhiên 1 clip, tránh phát lại clip vừa phát.</summary>
    private AudioClip GetRandomClip()
    {
        if (footstepClips.Length == 1) return footstepClips[0];

        int index;
        do
        {
            index = Random.Range(0, footstepClips.Length);
        }
        while (index == lastClipIndex);

        lastClipIndex = index;
        return footstepClips[index];
    }

    // ---------------------------------------------------------------
    // GIZMOS (chỉ hiển thị trong Scene View để dễ debug)
    // ---------------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        // Vẽ đường raycast kiểm tra mặt đất màu vàng
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawLine(origin, origin + Vector3.down * (groundCheckDistance + 0.1f));
    }
}