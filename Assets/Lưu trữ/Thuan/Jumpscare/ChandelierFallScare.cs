using UnityEngine;
using System.Collections;

// Jumpscare den chum roi sap.
// Nguoi choi buoc vao vung -> den rung nhe -> CAT NHAC NEN (im lang) -> day dut, den roi.
public class ChandelierFallScare : MonoBehaviour
{
    [Header("Đèn chùm sẽ rơi")]
    [Tooltip("Kéo object đèn chùm vào đây")]
    [SerializeField] private Transform chandelier;

    [Header("Giai đoạn RUNG báo hiệu")]
    [Tooltip("Rung bao lâu trước khi rơi (giây)")]
    [SerializeField] private float shakeDuration = 1.5f;
    [Tooltip("Biên độ rung (mét)")]
    [SerializeField] private float shakeAmount = 0.025f;
    [Tooltip("Tốc độ rung")]
    [SerializeField] private float shakeSpeed = 30f;

    [Header("═══ KHOẢNG IM LẶNG TRƯỚC KHI SẬP ═══")]
    [Tooltip("Cắt nhạc nền im lặng bao lâu trước khi đèn rơi (giây)")]
    [SerializeField] private float silenceDuration = 0.5f;
    [Tooltip("Kéo AudioSource nhạc nền vào đây. Để trống sẽ tự tìm AudioManager")]
    [SerializeField] private AudioSource backgroundMusic;
    [Tooltip("Có phát lại nhạc nền sau khi hù xong không")]
    [SerializeField] private bool resumeMusicAfter = true;
    [Tooltip("Sau bao lâu thì nhạc nền quay lại (giây)")]
    [SerializeField] private float resumeMusicDelay = 3f;

    [Header("Vật lý khi rơi")]
    [SerializeField] private float sideForce = 0.5f;
    [SerializeField] private float torqueForce = 2f;
    [SerializeField] private float chandelierMass = 80f;

    [Header("Âm thanh")]
    [Tooltip("Tiếng xích cọt kẹt lúc rung")]
    [SerializeField] private AudioClip creakSound;
    [Tooltip("Tiếng va đập lớn khi chạm sàn")]
    [SerializeField] private AudioClip crashSound;
    [Range(0f, 1f)]
    [SerializeField] private float soundVolume = 1f;

    [Header("Hiệu ứng bụi khi chạm sàn (tùy chọn)")]
    [SerializeField] private ParticleSystem dustVFX;

    [Header("Dọn dẹp")]
    [Tooltip("Xóa đèn sau bao nhiêu giây (0 = giữ lại làm vật cản)")]
    [SerializeField] private float destroyAfter = 0f;

    private bool triggered = false;
    private Rigidbody chandelierRb;
    private Vector3 originalPosition;
    private float musicOriginalVolume = 1f;

    private void Start()
    {
        if (chandelier == null)
        {
            Debug.LogError("[ChandelierFallScare] Chưa gán object đèn chùm!");
            return;
        }

        originalPosition = chandelier.position;

        chandelierRb = chandelier.GetComponent<Rigidbody>();
        if (chandelierRb == null)
            chandelierRb = chandelier.gameObject.AddComponent<Rigidbody>();

        chandelierRb.isKinematic = true;
        chandelierRb.mass = chandelierMass;
        chandelierRb.useGravity = false;

        if (chandelier.GetComponent<Collider>() == null)
            Debug.LogWarning("[ChandelierFallScare] Đèn chùm chưa có Collider - sẽ rơi xuyên sàn!");

        if (dustVFX != null) dustVFX.gameObject.SetActive(false);

        // Tu tim nhac nen neu chua gan
        if (backgroundMusic == null && AudioManager.Instance != null)
            backgroundMusic = AudioManager.Instance.musicSource;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(FallSequence());
    }

    private IEnumerator FallSequence()
    {
        Debug.Log("[ChandelierFallScare] Đèn chùm bắt đầu rung...");

        // ===== 1. RUNG BAO HIEU =====
        if (creakSound != null)
            AudioSource.PlayClipAtPoint(creakSound, chandelier.position, soundVolume);

        float t = 0f;
        while (t < shakeDuration)
        {
            t += Time.deltaTime;

            float x = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            float z = Mathf.Cos(Time.time * shakeSpeed * 1.3f) * shakeAmount;
            chandelier.position = originalPosition + new Vector3(x, 0f, z);

            yield return null;
        }

        chandelier.position = originalPosition;

        // ===== 2. CAT NHAC NEN - KHOANG IM LANG =====
        if (backgroundMusic != null)
        {
            musicOriginalVolume = backgroundMusic.volume;
            backgroundMusic.Pause();
            Debug.Log("[ChandelierFallScare] Cắt nhạc nền... im lặng.");
        }

        if (silenceDuration > 0f)
            yield return new WaitForSeconds(silenceDuration);

        // ===== 3. DUT DAY, ROI XUONG =====
        Debug.Log("[ChandelierFallScare] Dây đứt! Đèn chùm rơi!");

        chandelier.SetParent(null, true);

        chandelierRb.isKinematic = false;
        chandelierRb.useGravity = true;

        Vector3 randomSide = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        chandelierRb.AddForce(randomSide * sideForce, ForceMode.VelocityChange);
        chandelierRb.AddTorque(Random.insideUnitSphere * torqueForce, ForceMode.VelocityChange);

        // ===== 4. TIENG VA DAP + BUI =====
        yield return new WaitForSeconds(0.35f);

        if (crashSound != null)
            AudioSource.PlayClipAtPoint(crashSound, chandelier.position, soundVolume);

        if (dustVFX != null)
        {
            dustVFX.gameObject.SetActive(true);
            dustVFX.transform.position = chandelier.position;
            dustVFX.Play();
        }

        // ===== 5. PHAT LAI NHAC NEN =====
        if (resumeMusicAfter && backgroundMusic != null)
        {
            yield return new WaitForSeconds(resumeMusicDelay);

            backgroundMusic.volume = musicOriginalVolume;
            backgroundMusic.UnPause();
            Debug.Log("[ChandelierFallScare] Nhạc nền quay lại.");
        }

        // ===== 6. DON DEP (tuy chon) =====
        if (destroyAfter > 0f)
        {
            yield return new WaitForSeconds(destroyAfter);
            Destroy(chandelier.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.35f);
        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider box)
            Gizmos.DrawCube(box.center, box.size);
    }
}