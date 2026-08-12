using UnityEngine;
using System.Collections;

// Jumpscare mat quai hien sat camera + rung man hinh.
// Nguoi choi buoc vao vung -> mat hien ra truoc mat -> rung man hinh -> bien mat.
public class FaceJumpScare : MonoBehaviour
{
    [Header("Khuôn mặt hù")]
    [Tooltip("Kéo object mặt quái vào đây (để tắt sẵn trong Scene)")]
    [SerializeField] private GameObject scareFace;
    [Tooltip("Mặt hiện trong bao lâu rồi biến mất (giây)")]
    [SerializeField] private float faceDuration = 0.8f;

    [Header("Vị trí mặt so với camera")]
    [Tooltip("Mặt hiện cách camera bao nhiêu mét")]
    [SerializeField] private float distanceFromCamera = 0.6f;
    [Tooltip("Lệch lên/xuống (mét). 0 = ngang tầm mắt")]
    [SerializeField] private float heightOffset = 0f;

    [Header("═══ RUNG MÀN HÌNH ═══")]
    [Tooltip("Rung bao lâu (giây)")]
    [SerializeField] private float shakeDuration = 0.6f;
    [Tooltip("Biên độ rung (0.15 là mạnh, 0.05 là nhẹ)")]
    [SerializeField] private float shakeMagnitude = 0.15f;
    [Tooltip("Tốc độ rung")]
    [SerializeField] private float shakeFrequency = 25f;

    [Header("Âm thanh")]
    [Tooltip("Kéo AudioSource riêng (Spatial Blend = 2D, bỏ Play On Awake)")]
    [SerializeField] private AudioSource scareAudioSource;
    [Tooltip("Tiếng hét / tiếng gào")]
    [SerializeField] private AudioClip screamSound;
    [Range(0f, 3f)]
    [SerializeField] private float soundVolume = 1.5f;

    [Header("Cắt nhạc nền (tùy chọn)")]
    [SerializeField] private bool cutBackgroundMusic = true;
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private float resumeMusicDelay = 2f;

    [Header("Khóa điều khiển khi hù (tùy chọn)")]
    [Tooltip("Bật để người chơi không di chuyển được trong lúc bị hù")]
    [SerializeField] private bool freezePlayer = false;
    [SerializeField] private MonoBehaviour playerController;

    [Header("Chỉ hù một lần?")]
    [SerializeField] private bool oneTimeOnly = true;

    private bool triggered = false;
    private Camera cam;
    private float musicOriginalVolume = 1f;

    private void Start()
    {
        cam = Camera.main;

        if (scareFace != null) scareFace.SetActive(false);

        if (backgroundMusic == null && AudioManager.Instance != null)
            backgroundMusic = AudioManager.Instance.musicSource;

        if (scareFace == null)
            Debug.LogError("[FaceJumpScare] Chưa gán object khuôn mặt!");

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
        if (cam == null) yield break;

        // ===== 1. CAT NHAC NEN =====
        if (cutBackgroundMusic && backgroundMusic != null)
        {
            musicOriginalVolume = backgroundMusic.volume;
            backgroundMusic.Pause();
        }

        // ===== 2. KHOA DIEU KHIEN (neu bat) =====
        if (freezePlayer && playerController != null)
            playerController.enabled = false;

        // ===== 3. HIEN MAT NGAY TRUOC CAMERA =====
        if (scareFace != null)
        {
            Transform ct = cam.transform;

            // Dat mat truoc mat camera
            scareFace.transform.position = ct.position
                                         + ct.forward * distanceFromCamera
                                         + Vector3.up * heightOffset;

            // Quay mat nhin thang vao camera
            scareFace.transform.rotation = Quaternion.LookRotation(
                scareFace.transform.position - ct.position, Vector3.up);

            scareFace.SetActive(true);
        }

        // ===== 4. TIENG HET =====
        if (screamSound != null)
        {
            if (scareAudioSource != null)
                scareAudioSource.PlayOneShot(screamSound, soundVolume);
            else
                AudioSource.PlayClipAtPoint(screamSound, cam.transform.position, Mathf.Clamp01(soundVolume));
        }

        // ===== 5. RUNG MAN HINH =====
        yield return StartCoroutine(ShakeCamera());

        // ===== 6. GIU MAT THEM MOT CHUT ROI AN =====
        float remain = faceDuration - shakeDuration;
        if (remain > 0f) yield return new WaitForSeconds(remain);

        if (scareFace != null) scareFace.SetActive(false);

        // ===== 7. TRA LAI DIEU KHIEN =====
        if (freezePlayer && playerController != null)
            playerController.enabled = true;

        // ===== 8. PHAT LAI NHAC NEN =====
        if (cutBackgroundMusic && backgroundMusic != null)
        {
            yield return new WaitForSeconds(resumeMusicDelay);
            backgroundMusic.volume = musicOriginalVolume;
            backgroundMusic.UnPause();
        }
    }

    // Rung camera bang cach dich chuyen ngau nhien quanh vi tri goc
    private IEnumerator ShakeCamera()
    {
        Transform ct = cam.transform;
        Vector3 originalLocalPos = ct.localPosition;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            // Giam dan bien do cho tu nhien
            float damper = 1f - (elapsed / shakeDuration);

            float x = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f) * 2f;
            float y = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f) * 2f;

            ct.localPosition = originalLocalPos
                             + new Vector3(x, y, 0f) * shakeMagnitude * damper;

            // Mat luon bam theo camera trong luc rung
            if (scareFace != null && scareFace.activeSelf)
            {
                scareFace.transform.position = ct.position
                                             + ct.forward * distanceFromCamera
                                             + Vector3.up * heightOffset;
                scareFace.transform.rotation = Quaternion.LookRotation(
                    scareFace.transform.position - ct.position, Vector3.up);
            }

            yield return null;
        }

        // Tra camera ve vi tri goc
        ct.localPosition = originalLocalPos;
    }

    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider box)
            Gizmos.DrawCube(box.center, box.size);
    }
}