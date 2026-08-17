using UnityEngine;
using System.Collections;

// Jumpscare: xe dap dat san trong phong (luon hien), dot ngot TU CHAY ngang qua.
// Banh xe tu quay, than xe lac lu. Nhac nen tat luc chay, bat lai khi xong.
public class BicycleDashScare : MonoBehaviour
{
    [Header("Xe đạp")]
    [Tooltip("Kéo object xe đạp vào đây. Xe LUÔN HIỆN trong Scene, đặt sẵn tại vị trí BikeStart")]
    [SerializeField] private GameObject bicycle;

    [Header("Bánh xe (để tự quay)")]
    [Tooltip("Kéo Wheel_rl, Wheel_rr, Handlebars vào đây")]
    [SerializeField] private Transform[] wheels;
    [Tooltip("Trục quay của bánh. Thử (1,0,0), nếu sai thì đổi (0,0,1)")]
    [SerializeField] private Vector3 wheelAxis = new Vector3(1, 0, 0);
    [Tooltip("Tốc độ quay bánh (độ/giây). Số âm để quay ngược")]
    [SerializeField] private float wheelSpinSpeed = 720f;

    [Header("═══ ĐƯỜNG CHẠY ═══")]
    [Tooltip("Điểm xuất phát - đặt TRÙNG với vị trí xe đang đứng trong Scene")]
    [SerializeField] private Transform startPoint;
    [Tooltip("Điểm kết thúc - nơi xe chạy tới rồi dừng lại")]
    [SerializeField] private Transform endPoint;
    [Tooltip("Xe chạy hết quãng đường trong bao lâu (giây)")]
    [SerializeField] private float travelTime = 1.4f;

    [Tooltip("BẬT: xe tự xoay theo hướng chạy. TẮT: giữ nguyên góc bạn đã đặt trong Scene")]
    [SerializeField] private bool rotateTowardsDirection = true;

    [Header("Lắc lư cho tự nhiên")]
    [Tooltip("Biên độ lắc trái phải (độ)")]
    [SerializeField] private float wobbleAngle = 5f;
    [SerializeField] private float wobbleSpeed = 8f;
    [Tooltip("Xe nảy lên xuống bao nhiêu (mét)")]
    [SerializeField] private float bounceHeight = 0.04f;
    [SerializeField] private float bounceSpeed = 14f;

    [Header("═══ ÂM THANH ═══")]
    [Tooltip("BẮT BUỘC: AudioSource riêng (Spatial Blend = 2D, bỏ Play On Awake)")]
    [SerializeField] private AudioSource scareAudioSource;
    [Tooltip("Tiếng xe lăn / kim loại cót két")]
    [SerializeField] private AudioClip rollSound;
    [Range(0f, 3f)]
    [SerializeField] private float soundVolume = 1.8f;

    [Header("═══ TẮT NHẠC NỀN KHI XE CHẠY ═══")]
    [SerializeField] private bool cutBackgroundMusic = true;
    [Tooltip("Để trống sẽ tự tìm AudioManager")]
    [SerializeField] private AudioSource backgroundMusic;
    [Tooltip("Chờ bao lâu sau khi xe chạy xong thì bật nhạc lại (giây)")]
    [SerializeField] private float resumeMusicDelay = 1f;

    [Header("Rung màn hình nhẹ (tùy chọn)")]
    [SerializeField] private bool shakeCamera = true;
    [SerializeField] private float shakeDuration = 0.4f;
    [Tooltip("Biên độ rung theo góc (độ) - để nhỏ vì đây là hù nhẹ")]
    [SerializeField] private float shakeAngle = 1.5f;
    [SerializeField] private float shakeFrequency = 20f;

    [Header("Chỉ hù một lần?")]
    [SerializeField] private bool oneTimeOnly = true;

    private bool triggered = false;
    private Camera cam;
    private float musicOriginalVolume = 1f;

    private void Start()
    {
        cam = Camera.main;

        // KHONG tat xe - xe luon hien trong phong nhu do vat binh thuong

        if (backgroundMusic == null && AudioManager.Instance != null)
            backgroundMusic = AudioManager.Instance.musicSource;

        if (bicycle == null)
            Debug.LogError("[BicycleDashScare] Chưa gán object xe đạp!");

        if (startPoint == null || endPoint == null)
            Debug.LogError("[BicycleDashScare] Chưa gán điểm xuất phát / kết thúc!");

        if (scareAudioSource == null)
            Debug.LogWarning("[BicycleDashScare] Chưa gán 'Scare Audio Source' - tiếng sẽ nhỏ.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (oneTimeOnly && triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(DashSequence());
    }

    private IEnumerator DashSequence()
    {
        if (bicycle == null || startPoint == null || endPoint == null) yield break;

        Debug.Log("[BicycleDashScare] Xe đạp tự chạy!");

        // ===== 1. TAT NHAC NEN =====
        if (cutBackgroundMusic && backgroundMusic != null)
        {
            musicOriginalVolume = backgroundMusic.volume;
            backgroundMusic.Pause();
        }

        // ===== 2. CHUAN BI (xe da hien san, khong can bat) =====
        Vector3 fromPos = startPoint.position;
        Vector3 toPos = endPoint.position;
        Vector3 dir = (toPos - fromPos).normalized;

        Quaternion baseRot;
        if (rotateTowardsDirection && dir.sqrMagnitude > 0.0001f)
        {
            baseRot = Quaternion.LookRotation(dir, Vector3.up);
            bicycle.transform.rotation = baseRot;
        }
        else
        {
            // Giu nguyen goc da dat trong Scene
            baseRot = bicycle.transform.rotation;
        }

        // ===== 3. TIENG XE LAN =====
        if (rollSound != null)
        {
            if (scareAudioSource != null)
                scareAudioSource.PlayOneShot(rollSound, soundVolume);
            else if (cam != null)
                AudioSource.PlayClipAtPoint(rollSound, cam.transform.position, Mathf.Clamp01(soundVolume));
        }

        // ===== 4. RUNG MAN HINH NHE =====
        if (shakeCamera && cam != null)
            StartCoroutine(ShakeCameraRoutine());

        // ===== 5. CHAY TU START TOI END =====
        float elapsed = 0f;
        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / travelTime);

            Vector3 pos = Vector3.Lerp(fromPos, toPos, t);

            // Nay len xuong nhe nhu di tren san
            pos.y += Mathf.Sin(elapsed * bounceSpeed) * bounceHeight;

            bicycle.transform.position = pos;

            // Lac lu trai phai
            float wobble = Mathf.Sin(elapsed * wobbleSpeed) * wobbleAngle;
            bicycle.transform.rotation = baseRot * Quaternion.Euler(0f, 0f, wobble);

            SpinWheels();

            yield return null;
        }

        // Ket thuc: xe DUNG LAI o diem cuoi, van hien binh thuong
        bicycle.transform.position = toPos;
        bicycle.transform.rotation = baseRot;

        // ===== 6. BAT LAI NHAC NEN =====
        if (cutBackgroundMusic && backgroundMusic != null)
        {
            yield return new WaitForSeconds(resumeMusicDelay);
            backgroundMusic.volume = musicOriginalVolume;
            backgroundMusic.UnPause();
            Debug.Log("[BicycleDashScare] Nhạc nền quay lại.");
        }
    }

    private void SpinWheels()
    {
        if (wheels == null) return;

        foreach (Transform w in wheels)
        {
            if (w == null) continue;
            w.Rotate(wheelAxis.normalized, wheelSpinSpeed * Time.deltaTime, Space.Self);
        }
    }

    private IEnumerator ShakeCameraRoutine()
    {
        Transform ct = cam.transform;
        Quaternion baseRot = ct.localRotation;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float damper = 1f - (elapsed / shakeDuration);

            float pitch = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f) * 2f;
            float yaw = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f) * 2f;

            ct.localRotation = baseRot * Quaternion.Euler(
                pitch * shakeAngle * damper,
                yaw * shakeAngle * damper,
                0f);

            yield return null;
        }

        ct.localRotation = baseRot;
    }

    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && col is BoxCollider box)
        {
            Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }

        Gizmos.matrix = Matrix4x4.identity;

        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPoint.position, 0.15f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(endPoint.position, 0.15f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPoint.position, endPoint.position);
        }
    }
}