using UnityEngine;
using System.Collections;

// Jumpscare den chum roi sap.
// Nguoi choi buoc vao vung -> den rung + tieng cot ket -> CAT NHAC NEN -> day dut, den roi.
// Tieng va dap phat DUNG LUC den cham san that.
public class ChandelierFallScare : MonoBehaviour
{
    [Header("Đèn chùm sẽ rơi")]
    [Tooltip("Kéo object đèn chùm vào đây")]
    [SerializeField] private Transform chandelier;

    [Header("Giai đoạn RUNG báo hiệu")]
    [Tooltip("Rung bao lâu trước khi rơi (giây) - nên khớp độ dài tiếng cọt kẹt")]
    [SerializeField] private float shakeDuration = 2.3f;
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

    [Header("═══ ÂM THANH ═══")]
    [Tooltip("BẮT BUỘC: kéo AudioSource riêng vào đây (Spatial Blend = 2D, bỏ Play On Awake)")]
    [SerializeField] private AudioSource scareAudioSource;
    [Tooltip("Tiếng xích cọt kẹt lúc rung")]
    [SerializeField] private AudioClip creakSound;
    [Tooltip("Tiếng va đập lớn khi chạm sàn")]
    [SerializeField] private AudioClip crashSound;
    [Range(0f, 3f)]
    [Tooltip("Âm lượng - có thể đẩy lên trên 1 để to hơn")]
    [SerializeField] private float soundVolume = 1.5f;

    [Header("═══ THỜI ĐIỂM PHÁT TIẾNG VA ĐẬP ═══")]
    [Tooltip("BẬT: phát đúng lúc đèn chạm sàn thật (chính xác, khuyên dùng).\nTẮT: phát sau số giây cố định bên dưới")]
    [SerializeField] private bool playCrashOnRealImpact = true;
    [Tooltip("Chỉ dùng khi TẮT tùy chọn trên")]
    [SerializeField] private float crashDelay = 0.35f;

    [Header("Hiệu ứng bụi khi chạm sàn (tùy chọn)")]
    [SerializeField] private ParticleSystem dustVFX;

    [Header("Dọn dẹp")]
    [Tooltip("Xóa đèn sau bao nhiêu giây (0 = giữ lại làm vật cản)")]
    [SerializeField] private float destroyAfter = 0f;

    private bool triggered = false;
    private bool crashPlayed = false;
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
            Debug.LogWarning("[ChandelierFallScare] Đèn chùm chưa có Collider - sẽ rơi xuyên sàn và không phát tiếng va đập!");

        // Gan script bao va cham vao den chum
        ChandelierImpact impact = chandelier.GetComponent<ChandelierImpact>();
        if (impact == null)
            impact = chandelier.gameObject.AddComponent<ChandelierImpact>();
        impact.owner = this;

        if (dustVFX != null) dustVFX.gameObject.SetActive(false);

        if (backgroundMusic == null && AudioManager.Instance != null)
            backgroundMusic = AudioManager.Instance.musicSource;

        if (scareAudioSource == null)
            Debug.LogWarning("[ChandelierFallScare] Chưa gán 'Scare Audio Source'. " +
                             "Âm thanh sẽ phát 3D và nghe rất nhỏ. Nên thêm AudioSource 2D.");
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

        // ===== 1. RUNG + TIENG COT KET =====
        PlayScareSound(creakSound);

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

        // ===== 4. TIENG VA DAP =====
        if (playCrashOnRealImpact)
        {
            // Doi den cham san that (ChandelierImpact se goi ve)
            float timeout = 0f;
            while (!crashPlayed && timeout < 5f)
            {
                timeout += Time.deltaTime;
                yield return null;
            }

            // Neu qua 5 giay ma khong cham gi (roi hut) thi phat luon
            if (!crashPlayed)
            {
                Debug.LogWarning("[ChandelierFallScare] Đèn không chạm gì sau 5 giây - kiểm tra Collider của sàn!");
                PlayCrashNow(chandelier.position);
            }
        }
        else
        {
            yield return new WaitForSeconds(crashDelay);
            PlayCrashNow(chandelier.position);
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
            if (chandelier != null) Destroy(chandelier.gameObject);
        }
    }

    // ChandelierImpact goi ve khi den cham san that
    public void OnChandelierHitGround(Vector3 hitPoint)
    {
        if (crashPlayed) return;
        PlayCrashNow(hitPoint);
    }

    private void PlayCrashNow(Vector3 position)
    {
        if (crashPlayed) return;
        crashPlayed = true;

        PlayScareSound(crashSound);

        if (dustVFX != null)
        {
            dustVFX.gameObject.SetActive(true);
            dustVFX.transform.position = position;
            dustVFX.Play();
        }

        Debug.Log("[ChandelierFallScare] Đèn chạm sàn! Phát tiếng va đập.");
    }

    // Phat am thanh qua AudioSource 2D (to ro), neu chua gan thi phat 3D
    private void PlayScareSound(AudioClip clip)
    {
        if (clip == null) return;

        if (scareAudioSource != null)
        {
            scareAudioSource.PlayOneShot(clip, soundVolume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, chandelier.position, Mathf.Clamp01(soundVolume));
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