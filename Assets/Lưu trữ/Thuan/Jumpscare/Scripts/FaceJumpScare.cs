using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Jumpscare: camera SNAP toi diem moc da can san, nhin thang mat con ma.
// Rung MAN HINH bang cach xoay camera + man hinh bung do.
public class FaceJumpScare : MonoBehaviour
{
    [Header("Con ma hù")]
    [Tooltip("Kéo object con ma vào đây. ĐẶT SẴN đúng vị trí trong Scene rồi TẮT object đi")]
    [SerializeField] private GameObject scareFace;
    [Tooltip("Con ma hiện tổng cộng bao lâu (giây). PHẢI lớn hơn Shake Duration")]
    [SerializeField] private float faceDuration = 2.5f;

    [Header("═══ ĐIỂM MỐC CAMERA ═══")]
    [Tooltip("Kéo object CamAnchor_Scare vào đây. Camera nhảy tới đúng vị trí + góc này.\n" +
             "Mẹo: chọn object mốc rồi GameObject > Align View to Selected để xem trước khung hình")]
    [SerializeField] private Transform cameraAnchor;

    [Tooltip("BẬT: camera nhảy tức thì (sốc hơn). TẮT: bay mượt tới")]
    [SerializeField] private bool snapInstantly = true;
    [Tooltip("Chỉ dùng khi TẮT snap - tốc độ bay tới")]
    [SerializeField] private float flySpeed = 12f;
    [Tooltip("Tốc độ camera bay trở về chỗ cũ")]
    [SerializeField] private float returnSpeed = 6f;

    [Header("═══ RUNG MÀN HÌNH ═══")]
    [Tooltip("Rung bao lâu (giây)")]
    [SerializeField] private float shakeDuration = 1.2f;
    [Tooltip("Biên độ rung theo GÓC (độ). 2 = nhẹ, 4 = vừa, 8 = rất dữ dội")]
    [SerializeField] private float shakeAngle = 4f;
    [Tooltip("Tốc độ rung")]
    [SerializeField] private float shakeFrequency = 25f;

    [Header("═══ MÀN HÌNH ĐỎ ═══")]
    [Tooltip("Kéo Image đỏ phủ màn hình vào đây (Alpha để 0 sẵn)")]
    [SerializeField] private Image redOverlay;
    [Range(0f, 1f)]
    [Tooltip("Độ đậm tối đa của màu đỏ (0.45 = vừa, 0.7 = rất đậm)")]
    [SerializeField] private float redMaxAlpha = 0.45f;
    [Tooltip("Đỏ bừng lên nhanh thế nào (giây)")]
    [SerializeField] private float redFadeInTime = 0.08f;
    [Tooltip("Đỏ tan dần trong bao lâu (giây)")]
    [SerializeField] private float redFadeOutTime = 1.2f;
    [Tooltip("Bật: màu đỏ nhấp nháy theo nhịp tim. Tắt: đỏ đều rồi tan")]
    [SerializeField] private bool redPulse = true;

    [Header("Âm thanh")]
    [Tooltip("Kéo AudioSource riêng (Spatial Blend = 2D, bỏ Play On Awake)")]
    [SerializeField] private AudioSource scareAudioSource;
    [SerializeField] private AudioClip screamSound;
    [Range(0f, 3f)]
    [SerializeField] private float soundVolume = 1.5f;

    [Header("Cắt nhạc nền (tùy chọn)")]
    [SerializeField] private bool cutBackgroundMusic = true;
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private float resumeMusicDelay = 2f;

    [Header("Khóa điều khiển Player")]
    [Tooltip("Nên BẬT - để camera không bị Player ghi đè trong lúc hù")]
    [SerializeField] private bool freezePlayer = true;
    [Tooltip("Để trống sẽ tự tìm PlayerController trong scene")]
    [SerializeField] private MonoBehaviour playerController;

    [Header("Chỉ hù một lần?")]
    [SerializeField] private bool oneTimeOnly = true;

    private bool triggered = false;
    private Camera cam;
    private float musicOriginalVolume = 1f;

    private Vector3 camHomePosition;
    private Quaternion camHomeRotation;
    private Transform camHomeParent;

    private void Start()
    {
        cam = Camera.main;

        if (scareFace != null) scareFace.SetActive(false);

        // Dam bao lop do trong suot luc dau
        if (redOverlay != null) SetRedAlpha(0f);

        if (backgroundMusic == null && AudioManager.Instance != null)
            backgroundMusic = AudioManager.Instance.musicSource;

        if (scareFace == null)
            Debug.LogError("[FaceJumpScare] Chưa gán object con ma!");

        if (cameraAnchor == null)
            Debug.LogError("[FaceJumpScare] Chưa gán 'Camera Anchor'! Tạo object mốc và kéo vào.");

        if (scareAudioSource == null)
            Debug.LogWarning("[FaceJumpScare] Chưa gán 'Scare Audio Source' - tiếng sẽ nhỏ.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (oneTimeOnly && triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(ScareSequence());
    }

    private IEnumerator ScareSequence()
    {
        Debug.Log("[FaceJumpScare] HÙ!");

        if (cam == null) cam = Camera.main;
        if (cam == null || cameraAnchor == null) yield break;

        Transform ct = cam.transform;

        // ===== 1. LUU TRANG THAI CAMERA =====
        camHomeParent = ct.parent;
        camHomePosition = ct.position;
        camHomeRotation = ct.rotation;

        // ===== 2. KHOA DIEU KHIEN PLAYER =====
        if (freezePlayer)
        {
            if (playerController == null)
                playerController = FindFirstObjectByType<ElmanGameDevTools.PlayerSystem.PlayerController>();

            if (playerController != null) playerController.enabled = false;
        }

        // ===== 3. CAT NHAC NEN =====
        if (cutBackgroundMusic && backgroundMusic != null)
        {
            musicOriginalVolume = backgroundMusic.volume;
            backgroundMusic.Pause();
        }

        // ===== 4. BAT CON MA + DUA CAMERA TOI DIEM MOC =====
        if (scareFace != null) scareFace.SetActive(true);

        ct.SetParent(null, true);

        if (snapInstantly)
        {
            ct.position = cameraAnchor.position;
            ct.rotation = cameraAnchor.rotation;
        }
        else
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * flySpeed;
                ct.position = Vector3.Lerp(ct.position, cameraAnchor.position, Time.deltaTime * flySpeed);
                ct.rotation = Quaternion.Slerp(ct.rotation, cameraAnchor.rotation, Time.deltaTime * flySpeed);
                yield return null;
            }
            ct.position = cameraAnchor.position;
            ct.rotation = cameraAnchor.rotation;
        }

        // ===== 5. MAN HINH DO BUNG LEN =====
        if (redOverlay != null) StartCoroutine(RedFlashRoutine());

        // ===== 6. TIENG HET =====
        if (screamSound != null)
        {
            if (scareAudioSource != null)
                scareAudioSource.PlayOneShot(screamSound, soundVolume);
            else
                AudioSource.PlayClipAtPoint(screamSound, ct.position, Mathf.Clamp01(soundVolume));
        }

        // ===== 7. RUNG MAN HINH =====
        yield return StartCoroutine(ShakeAtAnchor());

        // ===== 8. CON MA DUNG IM NHIN CHAM CHAM ROI TAT =====
        float remain = faceDuration - shakeDuration;
        if (remain > 0f) yield return new WaitForSeconds(remain);

        if (scareFace != null) scareFace.SetActive(false);

        // ===== 9. CAMERA BAY VE CHO CU =====
        float back = 0f;
        while (back < 1f)
        {
            back += Time.deltaTime * returnSpeed;
            ct.position = Vector3.Lerp(ct.position, camHomePosition, Time.deltaTime * returnSpeed);
            ct.rotation = Quaternion.Slerp(ct.rotation, camHomeRotation, Time.deltaTime * returnSpeed);
            yield return null;
        }

        ct.position = camHomePosition;
        ct.rotation = camHomeRotation;

        if (camHomeParent != null)
            ct.SetParent(camHomeParent, true);

        // ===== 10. TRA LAI DIEU KHIEN =====
        if (freezePlayer && playerController != null)
            playerController.enabled = true;

        // ===== 11. PHAT LAI NHAC NEN =====
        if (cutBackgroundMusic && backgroundMusic != null)
        {
            yield return new WaitForSeconds(resumeMusicDelay);
            backgroundMusic.volume = musicOriginalVolume;
            backgroundMusic.UnPause();
        }

        Debug.Log("[FaceJumpScare] Kết thúc, trả camera về cho Player.");
    }

    // ===== HIEU UNG MAN HINH DO =====
    private IEnumerator RedFlashRoutine()
    {
        // 1. Bung do that nhanh
        float t = 0f;
        while (t < redFadeInTime)
        {
            t += Time.deltaTime;
            SetRedAlpha(Mathf.Lerp(0f, redMaxAlpha, t / redFadeInTime));
            yield return null;
        }
        SetRedAlpha(redMaxAlpha);

        // 2. Giu do trong luc rung (nhap nhay nhu nhip tim)
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            if (redPulse)
            {
                float pulse = 0.7f + 0.3f * Mathf.Sin(elapsed * 12f);
                SetRedAlpha(redMaxAlpha * pulse);
            }

            yield return null;
        }

        // 3. Tan dan
        t = 0f;
        float startAlpha = GetRedAlpha();
        while (t < redFadeOutTime)
        {
            t += Time.deltaTime;
            SetRedAlpha(Mathf.Lerp(startAlpha, 0f, t / redFadeOutTime));
            yield return null;
        }
        SetRedAlpha(0f);
    }

    private void SetRedAlpha(float a)
    {
        if (redOverlay == null) return;
        Color c = redOverlay.color;
        c.a = a;
        redOverlay.color = c;
    }

    private float GetRedAlpha()
    {
        return redOverlay != null ? redOverlay.color.a : 0f;
    }

    // Rung MAN HINH bang cach XOAY camera - model dung yen trong khung
    private IEnumerator ShakeAtAnchor()
    {
        Transform ct = cam.transform;
        Vector3 basePos = cameraAnchor.position;
        Quaternion baseRot = cameraAnchor.rotation;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float damper = 1f - (elapsed / shakeDuration);

            float pitch = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f) * 2f;
            float yaw = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f) * 2f;
            float roll = (Mathf.PerlinNoise(Time.time * shakeFrequency, 100f) - 0.5f) * 2f;

            ct.position = basePos;
            ct.rotation = baseRot * Quaternion.Euler(
                pitch * shakeAngle * damper,
                yaw * shakeAngle * damper,
                roll * shakeAngle * damper * 0.6f
            );

            yield return null;
        }

        ct.position = basePos;
        ct.rotation = baseRot;
    }

    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && col is BoxCollider box)
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }

        if (cameraAnchor != null)
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(cameraAnchor.position, 0.12f);
            Gizmos.DrawRay(cameraAnchor.position, cameraAnchor.forward * 2f);
        }
    }
}