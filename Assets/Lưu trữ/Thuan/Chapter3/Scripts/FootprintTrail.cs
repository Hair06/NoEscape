using UnityEngine;

// Chuoi dau chan trong me cung.
// Chi NHIN THAY khi cam Con Mat va giu chuot phai (soi).
// Van phai DI HET chuoi thi vat pham moi hien ra tren ban da.
public class FootprintTrail : MonoBehaviour
{
    [Header("Danh sách dấu chân (kéo theo thứ tự đường đi)")]
    [Tooltip("Kéo các object dấu chân vào đây, từ điểm bắt đầu tới bàn đá")]
    [SerializeField] private GameObject[] footprints;

    [Header("Bán kính phát hiện Player")]
    [Tooltip("Player vào trong bán kính này (đo ngang) thì tính là đã đi qua dấu chân")]
    [SerializeField] private float triggerRadius = 3f;

    [Header("Phần thưởng khi đi hết chuỗi")]
    [Tooltip("Vật phẩm hiện ra trên bàn đá (Trái tim hoặc Giọt máu)")]
    [SerializeField] private GameObject[] rewards;

    [Header("VFX ánh sáng xanh lá cho dấu chân")]
    [Tooltip("Bật để tự tạo ánh sáng cho mỗi dấu chân")]
    [SerializeField] private bool useGlowLight = true;
    [SerializeField] private Color glowColor = new Color(0.2f, 1f, 0.3f);
    [Tooltip("Độ sáng")]
    [SerializeField] private float glowIntensity = 3f;
    [Tooltip("Bán kính lan của ánh sáng (mét)")]
    [SerializeField] private float glowRange = 2.5f;
    [Tooltip("Nhấc ánh sáng lên khỏi sàn bao nhiêu mét")]
    [SerializeField] private float glowHeightOffset = 0.3f;
    [Tooltip("Tùy chọn: kéo prefab particle xanh lá vào đây")]
    [SerializeField] private GameObject glowVFXPrefab;

    [Header("Âm thanh bước chân (random)")]
    [Tooltip("Mỗi lần đi qua 1 dấu chân sẽ phát ngẫu nhiên 1 tiếng")]
    [SerializeField] private AudioClip[] stepSounds;
    [Range(0f, 1f)]
    [SerializeField] private float stepVolume = 0.8f;

    [Header("Âm thanh khi hoàn thành (có thể để trống)")]
    [SerializeField] private AudioClip completeSound;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = false;

    private Transform player;
    private int currentIndex = 0;
    private bool isComplete = false;
    private int lastSoundIndex = -1;
    private bool wasVisible = false;   // trang thai hien tai cua chuoi dau chan
    private bool glowAttached = false; // da gan anh sang chua

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            player = p.transform;
        }
        else
        {
            Debug.LogError("[FootprintTrail] KHONG tim thay Player! Kiem tra Tag.");
            return;
        }

        if (footprints == null || footprints.Length == 0)
        {
            Debug.LogError("[FootprintTrail] Mang Footprints dang TRONG!");
            return;
        }

        // Gan anh sang xanh la cho tat ca dau chan ngay tu dau
        AttachGlowToAll();

        // An het dau chan luc dau (chi hien khi soi bang Con Mat)
        SetTrailVisible(false);

        // An het phan thuong luc dau
        foreach (GameObject r in rewards)
            if (r != null) r.SetActive(false);
    }

    private void Update()
    {
        if (player == null || footprints == null) return;

        if (!MiniGameFlowManager.IsChapterActive(3))
        {
            if (wasVisible)
            {
                SetTrailVisible(false);
                wasVisible = false;
            }
            return;
        }

        // 1. HIEN THI: chuoi dau chan chi thay khi dang soi bang Con Mat
        bool shouldBeVisible = IsAimingWithEye() && !isComplete;

        if (shouldBeVisible != wasVisible)
        {
            SetTrailVisible(shouldBeVisible);
            wasVisible = shouldBeVisible;

            if (showDebugLog)
                Debug.Log($"[FootprintTrail] Chuoi dau chan {(shouldBeVisible ? "HIEN" : "AN")}");
        }

        // 2. TIEN DO: van theo doi du khong soi, de biet khi nao di het chuoi
        if (isComplete || currentIndex >= footprints.Length) return;

        GameObject current = footprints[currentIndex];
        if (current == null)
        {
            currentIndex++;
            return;
        }

        // Chi do khoang cach NGANG (bo qua do cao)
        Vector3 a = player.position;
        Vector3 b = current.transform.position;
        a.y = 0f;
        b.y = 0f;
        float dist = Vector3.Distance(a, b);

        if (showDebugLog && Time.frameCount % 60 == 0)
            Debug.Log($"[FootprintTrail] Dau chan {currentIndex} | Khoang cach = {dist:F2} (can <= {triggerRadius})");

        if (dist <= triggerRadius)
            AdvanceToNext(current.transform.position);
    }

    // ===== VFX ANH SANG =====

    // Gan anh sang xanh la cho tat ca dau chan (chi lam 1 lan)
    private void AttachGlowToAll()
    {
        if (glowAttached) return;
        glowAttached = true;

        foreach (GameObject f in footprints)
            AttachGlow(f);
    }

    private void AttachGlow(GameObject footprint)
    {
        if (footprint == null) return;

        // Neu da co anh sang roi thi khong tao lai
        if (footprint.GetComponentInChildren<Light>(true) != null) return;

        if (useGlowLight)
        {
            GameObject lightObj = new GameObject("GlowLight");
            lightObj.transform.SetParent(footprint.transform, false);
            lightObj.transform.localPosition = new Vector3(0f, glowHeightOffset, 0f);

            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = glowColor;
            l.intensity = glowIntensity;
            l.range = glowRange;
            l.shadows = LightShadows.None;   // tat bong de nhe may
        }

        // Neu co prefab particle thi tao them
        if (glowVFXPrefab != null)
        {
            GameObject vfx = Instantiate(glowVFXPrefab, footprint.transform);
            vfx.transform.localPosition = Vector3.zero;
        }
    }

    // Kiem tra nguoi choi co dang soi bang Con Mat khong
    private bool IsAimingWithEye()
    {
        if (CultEyeAutoDetector.Instance == null) return false;
        return CultEyeAutoDetector.Instance.IsAiming;
    }

    // Bat/tat toan bo chuoi dau chan cung luc
    private void SetTrailVisible(bool visible)
    {
        foreach (GameObject f in footprints)
            if (f != null) f.SetActive(visible);
    }

    private void AdvanceToNext(Vector3 stepPosition)
    {
        currentIndex++;

        // Phat tieng buoc chan tai vi tri vua di qua
        PlayRandomStepSound(stepPosition);

        if (showDebugLog)
            Debug.Log($"[FootprintTrail] Da di qua dau chan {currentIndex}/{footprints.Length}");

        if (currentIndex >= footprints.Length)
            CompleteTrail();
    }

    private void PlayRandomStepSound(Vector3 position)
    {
        if (stepSounds == null || stepSounds.Length == 0) return;

        int index;
        if (stepSounds.Length == 1)
        {
            index = 0;
        }
        else
        {
            int safety = 0;
            do
            {
                index = Random.Range(0, stepSounds.Length);
                safety++;
            }
            while (index == lastSoundIndex && safety < 10);
        }
        lastSoundIndex = index;

        AudioClip clip = stepSounds[index];
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, position, stepVolume);
    }

    private void CompleteTrail()
    {
        isComplete = true;
        Debug.Log("[FootprintTrail] Da di het chuoi dau chan! Vat pham hien ra tren ban da.");

        // An han chuoi dau chan (da xong nhiem vu dan duong)
        SetTrailVisible(false);
        wasVisible = false;

        if (completeSound != null)
            AudioSource.PlayClipAtPoint(completeSound, transform.position);

        foreach (GameObject r in rewards)
            if (r != null) r.SetActive(true);
    }

    private void OnDrawGizmosSelected()
    {
        if (footprints == null) return;
        Gizmos.color = Color.cyan;
        foreach (GameObject f in footprints)
            if (f != null)
                Gizmos.DrawWireSphere(f.transform.position, triggerRadius);
    }
}