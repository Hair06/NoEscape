using UnityEngine;
using System.Collections;

// Jumpscare den chum roi sap.
// Nguoi choi vao vung -> den rung + tieng cot ket -> CAT NHAC NEN -> day dut, den roi NHANH.
// Cham san: tieng va dap + RUNG MAN HINH manh + den NAM IM tai cho (khong lan).
public class ChandelierFallScare : MonoBehaviour
{
    [Header("Đèn chùm sẽ rơi")]
    [Tooltip("Kéo object đèn chùm vào đây")]
    [SerializeField] private Transform chandelier;

    [Header("Giai đoạn RUNG báo hiệu")]
    [Tooltip("Rung bao lâu trước khi rơi (giây) - nên khớp độ dài tiếng cọt kẹt")]
    [SerializeField] private float shakeDuration_Warning = 2.3f;
    [Tooltip("Biên độ rung của đèn (mét)")]
    [SerializeField] private float shakeAmount = 0.025f;
    [Tooltip("Tốc độ rung của đèn")]
    [SerializeField] private float shakeSpeed = 30f;

    [Header("═══ KHOẢNG IM LẶNG TRƯỚC KHI SẬP ═══")]
    [Tooltip("Cắt nhạc nền im lặng bao lâu trước khi đèn rơi (giây)")]
    [SerializeField] private float silenceDuration = 0.5f;
    [Tooltip("Kéo AudioSource nhạc nền vào đây. Để trống sẽ tự tìm AudioManager")]
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private bool resumeMusicAfter = true;
    [SerializeField] private float resumeMusicDelay = 3f;

    [Header("═══ VẬT LÝ KHI RƠI ═══")]
    [Tooltip("Lực đẩy ngang - để 0 nếu muốn rơi thẳng, không văng lệch")]
    [SerializeField] private float sideForce = 0f;
    [Tooltip("Lực xoay khi rơi - để nhỏ (0.5) để đèn không lăn lung tung")]
    [SerializeField] private float torqueForce = 0.5f;
    [SerializeField] private float chandelierMass = 80f;
    [Tooltip("Lực đẩy xuống thêm để rơi NHANH hơn (0 = tự nhiên, 6 = nhanh, 12 = rất nhanh)")]
    [SerializeField] private float extraDownForce = 6f;

    [Header("═══ KHÓA ĐÈN SAU KHI CHẠM SÀN ═══")]
    [Tooltip("BẬT: đèn nằm im tại chỗ chạm sàn, không lăn tiếp (khuyên dùng)")]
    [SerializeField] private bool freezeAfterImpact = true;
    [Tooltip("Chờ bao lâu sau khi chạm sàn rồi mới khóa (giây). 0.2 cho phép nảy nhẹ một cái")]
    [SerializeField] private float freezeDelay = 0.2f;
    [Tooltip("BẬT: tắt luôn Collider để đèn không chặn đường người chơi")]
    [SerializeField] private bool disableColliderAfterImpact = false;

    [Header("═══ ÂM THANH ═══")]
    [Tooltip("BẮT BUỘC: AudioSource riêng (Spatial Blend = 2D, bỏ Play On Awake)")]
    [SerializeField] private AudioSource scareAudioSource;
    [Tooltip("Tiếng xích cọt kẹt lúc rung")]
    [SerializeField] private AudioClip creakSound;
    [Tooltip("Tiếng va đập lớn khi chạm sàn")]
    [SerializeField] private AudioClip crashSound;
    [Range(0f, 3f)]
    [SerializeField] private float soundVolume = 1.5f;

    [Header("═══ RUNG MÀN HÌNH KHI CHẠM SÀN ═══")]
    [SerializeField] private bool shakeOnImpact = true;
    [Tooltip("Rung bao lâu (giây)")]
    [SerializeField] private float camShakeDuration = 0.8f;
    [Tooltip("Biên độ rung theo GÓC (độ). 6 = mạnh, 10 = rất dữ dội")]
    [SerializeField] private float camShakeAngle = 6f;
    [SerializeField] private float camShakeFrequency = 28f;

    [Header("═══ THỜI ĐIỂM PHÁT TIẾNG VA ĐẬP ═══")]
    [Tooltip("BẬT: phát đúng lúc đèn chạm sàn thật (khuyên dùng)")]
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
            Debug.LogWarning("[ChandelierFallScare] Đèn chùm chưa có Collider - sẽ rơi xuyên sàn!");

        // Gan script bao va cham vao den chum
        ChandelierImpact impact = chandelier.GetComponent<ChandelierImpact>();
        if (impact == null)
            impact = chandelier.gameObject.AddComponent<ChandelierImpact>();
        impact.owner = this;

        if (dustVFX != null) dustVFX.gameObject.SetActive(false);

        if (backgroundMusic == null && AudioManager.Instance != null)
            backgroundMusic = AudioManager.Instance.musicSource;

        if (scareAudioSource == null)
            Debug.LogWarning("[ChandelierFallScare] Chưa gán 'Scare Audio Source' - tiếng sẽ nhỏ.");
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

        // ===== 1. RUNG BAO HIEU + TIENG COT KET =====
        PlayScareSound(creakSound);

        float t = 0f;
        while (t < shakeDuration_Warning)
        {
            t += Time.deltaTime;

            float x = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            float z = Mathf.Cos(Time.time * shakeSpeed * 1.3f) * shakeAmount;
            chandelier.position = originalPosition + new Vector3(x, 0f, z);

            yield return null;
        }

        chandelier.position = originalPosition;

        // ===== 2. CAT NHAC NEN - IM LANG =====
        if (backgroundMusic != null)
        {
            musicOriginalVolume = backgroundMusic.volume;
            backgroundMusic.Pause();
            Debug.Log("[ChandelierFallScare] Cắt nhạc nền... im lặng.");
        }

        if (silenceDuration > 0f)
            yield return new WaitForSeconds(silenceDuration);

        // ===== 3. DUT DAY, ROI THANG XUONG =====
        Debug.Log("[ChandelierFallScare] Dây đứt! Đèn chùm rơi!");

        chandelier.SetParent(null, true);

        chandelierRb.isKinematic = false;
        chandelierRb.useGravity = true;

        // Luc day ngang (de 0 thi roi thang)
        if (sideForce > 0f)
        {
            Vector3 randomSide = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;
            chandelierRb.AddForce(randomSide * sideForce, ForceMode.VelocityChange);
        }

        // Xoay nhe cho tu nhien (de nho de khong lan)
        if (torqueForce > 0f)
            chandelierRb.AddTorque(Random.insideUnitSphere * torqueForce, ForceMode.VelocityChange);

        // Day xuong them de roi NHANH hon
        if (extraDownForce > 0f)
            chandelierRb.AddForce(Vector3.down * extraDownForce, ForceMode.VelocityChange);

        // ===== 4. TIENG VA DAP =====
        if (playCrashOnRealImpact)
        {
            float timeout = 0f;
            while (!crashPlayed && timeout < 5f)
            {
                timeout += Time.deltaTime;
                yield return null;
            }

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

        // ===== 6. DON DEP =====
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

        // RUNG MAN HINH MANH khi cham san
        if (shakeOnImpact && Camera.main != null)
            StartCoroutine(ShakeCameraRoutine());

        // KHOA DEN LAI de khong lan lung tung
        if (freezeAfterImpact)
            StartCoroutine(FreezeChandelierRoutine());

        if (dustVFX != null)
        {
            dustVFX.gameObject.SetActive(true);
            dustVFX.transform.position = position;
            dustVFX.Play();
        }

        Debug.Log("[ChandelierFallScare] Đèn chạm sàn! Phát tiếng va đập + rung màn hình.");
    }

    // Khoa den lai sau khi cham san de no nam im, khong lan di
    private IEnumerator FreezeChandelierRoutine()
    {
        if (freezeDelay > 0f)
            yield return new WaitForSeconds(freezeDelay);

        if (chandelierRb != null)
        {
            // Dung het chuyen dong
            chandelierRb.linearVelocity = Vector3.zero;
            chandelierRb.angularVelocity = Vector3.zero;

            // Khoa cung tai cho
            chandelierRb.isKinematic = true;
            chandelierRb.useGravity = false;
        }

        // Tuy chon: tat Collider de khong chan duong nguoi choi
        if (disableColliderAfterImpact && chandelier != null)
        {
            Collider col = chandelier.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        Debug.Log("[ChandelierFallScare] Đèn đã nằm im tại chỗ.");
    }

    // Rung man hinh bang cach XOAY camera
    private IEnumerator ShakeCameraRoutine()
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;

        Transform ct = cam.transform;
        Quaternion baseRot = ct.localRotation;

        float elapsed = 0f;
        while (elapsed < camShakeDuration)
        {
            elapsed += Time.deltaTime;
            float damper = 1f - (elapsed / camShakeDuration);

            float pitch = (Mathf.PerlinNoise(Time.time * camShakeFrequency, 0f) - 0.5f) * 2f;
            float yaw = (Mathf.PerlinNoise(0f, Time.time * camShakeFrequency) - 0.5f) * 2f;
            float roll = (Mathf.PerlinNoise(Time.time * camShakeFrequency, 100f) - 0.5f) * 2f;

            ct.localRotation = baseRot * Quaternion.Euler(
                pitch * camShakeAngle * damper,
                yaw * camShakeAngle * damper,
                roll * camShakeAngle * damper * 0.5f
            );

            yield return null;
        }

        ct.localRotation = baseRot;
    }

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