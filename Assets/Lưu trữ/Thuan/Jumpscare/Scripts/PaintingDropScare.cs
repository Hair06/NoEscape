using UnityEngine;
using System.Collections;

// Jumpscare: phat am thanh bao hieu -> loat khung tranh dong loat roi xuong san.
public class PaintingDropScare : MonoBehaviour
{
    [Header("Các khung tranh sẽ rơi")]
    [Tooltip("Kéo tất cả khung tranh muốn rơi vào đây")]
    [SerializeField] private GameObject[] paintings;

    [Header("Cách rơi")]
    [Tooltip("BẬT: rơi lần lượt từng cái. TẮT: rơi đồng loạt cùng lúc")]
    [SerializeField] private bool dropOneByOne = true;
    [Tooltip("Khoảng cách giữa mỗi tranh rơi (giây) - chỉ dùng khi rơi lần lượt")]
    [SerializeField] private float dropInterval = 0.15f;
    [Tooltip("Chờ bao lâu sau tiếng báo hiệu rồi mới rơi (giây)")]
    [SerializeField] private float delayBeforeDrop = 0.3f;

    [Header("Vật lý")]
    [SerializeField] private float paintingMass = 5f;
    [Tooltip("Lực đẩy bật ra khỏi tường")]
    [SerializeField] private float pushForce = 1.5f;
    [SerializeField] private float torqueForce = 2f;

    [Header("═══ ÂM THANH ═══")]
    [Tooltip("BẮT BUỘC: AudioSource riêng (Spatial Blend = 2D, bỏ Play On Awake)")]
    [SerializeField] private AudioSource scareAudioSource;
    [Tooltip("Tiếng báo hiệu vang lên đầu tiên")]
    [SerializeField] private AudioClip impactSound;
    [Range(0f, 3f)]
    [SerializeField] private float impactVolume = 2f;

    [Tooltip("Tiếng khung tranh chạm sàn (phát cho từng tranh)")]
    [SerializeField] private AudioClip[] dropSounds;
    [Range(0f, 3f)]
    [SerializeField] private float dropVolume = 1.8f;

    [Header("Cắt nhạc nền (tùy chọn)")]
    [SerializeField] private bool cutBackgroundMusic = true;
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private float resumeMusicDelay = 3f;

    [Header("Rung màn hình nhẹ (tùy chọn)")]
    [SerializeField] private bool shakeCamera = true;
    [SerializeField] private float shakeDuration = 0.5f;
    [Tooltip("Biên độ rung theo góc (độ)")]
    [SerializeField] private float shakeAngle = 2f;
    [SerializeField] private float shakeFrequency = 22f;

    [Header("Dọn dẹp")]
    [Tooltip("Xóa tranh sau bao nhiêu giây (0 = giữ lại trên sàn)")]
    [SerializeField] private float destroyAfter = 0f;

    [Header("Chỉ hù một lần?")]
    [SerializeField] private bool oneTimeOnly = true;

    private bool triggered = false;
    private Camera cam;
    private float musicOriginalVolume = 1f;
    private int lastDropSoundIndex = -1;

    private void Start()
    {
        cam = Camera.main;

        // Chuan bi Rigidbody cho tung tranh nhung chua cho roi
        foreach (GameObject p in paintings)
        {
            if (p == null) continue;

            Rigidbody rb = p.GetComponent<Rigidbody>();
            if (rb == null) rb = p.AddComponent<Rigidbody>();

            rb.isKinematic = true;
            rb.useGravity = false;
            rb.mass = paintingMass;

            if (p.GetComponent<Collider>() == null)
                Debug.LogWarning($"[PaintingDropScare] '{p.name}' chưa có Collider - sẽ rơi xuyên sàn!");

            // Gan script bao va cham de phat tieng dung luc cham san
            PaintingImpact pi = p.GetComponent<PaintingImpact>();
            if (pi == null) pi = p.AddComponent<PaintingImpact>();
            pi.owner = this;
        }

        if (backgroundMusic == null && AudioManager.Instance != null)
            backgroundMusic = AudioManager.Instance.musicSource;

        if (scareAudioSource == null)
            Debug.LogWarning("[PaintingDropScare] Chưa gán 'Scare Audio Source' - tiếng sẽ nhỏ!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (oneTimeOnly && triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(DropSequence());
    }

    private IEnumerator DropSequence()
    {
        Debug.Log("[PaintingDropScare] Bắt đầu!");

        // ===== 1. CAT NHAC NEN =====
        if (cutBackgroundMusic && backgroundMusic != null)
        {
            musicOriginalVolume = backgroundMusic.volume;
            backgroundMusic.Pause();
        }

        // ===== 2. TIENG BAO HIEU =====
        PlaySound(impactSound, impactVolume);

        // ===== 3. RUNG MAN HINH =====
        if (shakeCamera && cam != null)
            StartCoroutine(ShakeCameraRoutine());

        // ===== 4. CHO MOT CHUT ROI CHO TRANH ROI =====
        yield return new WaitForSeconds(delayBeforeDrop);

        foreach (GameObject p in paintings)
        {
            if (p == null) continue;

            DropOne(p);

            if (dropOneByOne)
                yield return new WaitForSeconds(dropInterval);
        }

        // ===== 5. PHAT LAI NHAC NEN =====
        if (cutBackgroundMusic && backgroundMusic != null)
        {
            yield return new WaitForSeconds(resumeMusicDelay);
            backgroundMusic.volume = musicOriginalVolume;
            backgroundMusic.UnPause();
        }

        // ===== 6. DON DEP =====
        if (destroyAfter > 0f)
        {
            yield return new WaitForSeconds(destroyAfter);
            foreach (GameObject p in paintings)
                if (p != null) Destroy(p);
        }
    }

    private void DropOne(GameObject painting)
    {
        Rigidbody rb = painting.GetComponent<Rigidbody>();
        if (rb == null) return;

        painting.transform.SetParent(null, true);

        rb.isKinematic = false;
        rb.useGravity = true;

        // Day bat ra khoi tuong theo huong mat truoc cua tranh
        Vector3 push = painting.transform.forward * pushForce;
        rb.AddForce(push, ForceMode.VelocityChange);
        rb.AddTorque(Random.insideUnitSphere * torqueForce, ForceMode.VelocityChange);
    }

    // PaintingImpact goi ve khi tranh cham san
    public void OnPaintingHitGround(Vector3 hitPoint)
    {
        PlayRandomDropSound();
    }

    private void PlayRandomDropSound()
    {
        if (dropSounds == null || dropSounds.Length == 0) return;

        int index;
        if (dropSounds.Length == 1)
        {
            index = 0;
        }
        else
        {
            int safety = 0;
            do
            {
                index = Random.Range(0, dropSounds.Length);
                safety++;
            }
            while (index == lastDropSoundIndex && safety < 10);
        }
        lastDropSoundIndex = index;

        PlaySound(dropSounds[index], dropVolume);
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip == null) return;

        if (scareAudioSource != null)
            scareAudioSource.PlayOneShot(clip, volume);
        else if (cam != null)
            AudioSource.PlayClipAtPoint(clip, cam.transform.position, Mathf.Clamp01(volume));
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
            Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
    }
}