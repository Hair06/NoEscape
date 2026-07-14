using UnityEngine;

// Quan ly chuoi dau chan dan duong trong me cung.
// Dau chan hien lan luot: toi gan cai nay thi cai tiep theo moi hien ra.
// Di het chuoi -> vat pham hien ra tren ban da de nhat.
public class FootprintTrail : MonoBehaviour
{
    [Header("Danh sách dấu chân (kéo theo thứ tự đường đi)")]
    [Tooltip("Kéo các object dấu chân vào đây, từ điểm bắt đầu tới bàn đá")]
    [SerializeField] private GameObject[] footprints;

    [Header("Bán kính phát hiện Player")]
    [Tooltip("Player vào trong bán kính này (đo ngang, bỏ qua độ cao) thì dấu chân kế tiếp hiện ra")]
    [SerializeField] private float triggerRadius = 3f;

    [Header("Phần thưởng khi đi hết chuỗi")]
    [Tooltip("Vật phẩm hiện ra trên bàn đá (Trái tim hoặc Giọt máu)")]
    [SerializeField] private GameObject[] rewards;

    [Header("Âm thanh bước chân (random)")]
    [Tooltip("Kéo NHIỀU tiếng bước chân vào đây - mỗi lần hiện dấu chân sẽ phát ngẫu nhiên 1 tiếng")]
    [SerializeField] private AudioClip[] stepSounds;
    [Range(0f, 1f)]
    [SerializeField] private float stepVolume = 0.8f;

    [Header("Âm thanh khi hoàn thành (có thể để trống)")]
    [Tooltip("Tiếng khi đi hết chuỗi, vật phẩm lộ diện")]
    [SerializeField] private AudioClip completeSound;

    [Header("Debug")]
    [Tooltip("Bật để in log chẩn đoán ra Console")]
    [SerializeField] private bool showDebugLog = false;

    private Transform player;
    private int currentIndex = 0;
    private bool isComplete = false;
    private int lastSoundIndex = -1;   // tránh phát lại đúng tiếng vừa phát

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            if (showDebugLog)
                Debug.Log($"[FootprintTrail] Tim thay Player: '{p.name}' tai vi tri {p.transform.position}");
        }
        else
        {
            Debug.LogError("[FootprintTrail] KHONG tim thay Player! Kiem tra Tag cua Player.");
            return;
        }

        if (footprints == null || footprints.Length == 0)
        {
            Debug.LogError("[FootprintTrail] Mang Footprints dang TRONG!");
            return;
        }

        // Ẩn hết dấu chân lúc đầu
        foreach (GameObject f in footprints)
        {
            if (f != null) f.SetActive(false);
        }

        // Ẩn hết phần thưởng lúc đầu
        foreach (GameObject r in rewards)
        {
            if (r != null) r.SetActive(false);
        }

        // Hiện dấu chân đầu tiên
        if (footprints[0] != null)
        {
            footprints[0].SetActive(true);
            if (showDebugLog)
                Debug.Log($"[FootprintTrail] Da hien dau chan dau tien.");
        }
    }

    private void Update()
    {
        if (isComplete || player == null) return;
        if (footprints == null || currentIndex >= footprints.Length) return;

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
        {
            Debug.Log($"[FootprintTrail] Khoang cach ngang = {dist:F2} (can <= {triggerRadius})");
        }

        if (dist <= triggerRadius)
        {
            AdvanceToNext();
        }
    }

    private void AdvanceToNext()
    {
        currentIndex++;

        if (currentIndex < footprints.Length)
        {
            GameObject next = footprints[currentIndex];

            if (next != null)
            {
                next.SetActive(true);

                // Phat tieng buoc chan ngau nhien NGAY TAI vi tri dau chan moi
                PlayRandomStepSound(next.transform.position);
            }

            if (showDebugLog)
                Debug.Log($"[FootprintTrail] Hien dau chan so: {currentIndex + 1}/{footprints.Length}");
        }
        else
        {
            CompleteTrail();
        }
    }

    // Chon ngau nhien 1 tieng buoc chan, tranh lap lai tieng vua phat
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
            // Chon ngau nhien nhung khong trung tieng vua phat
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

        if (completeSound != null)
            AudioSource.PlayClipAtPoint(completeSound, transform.position);

        foreach (GameObject r in rewards)
        {
            if (r != null) r.SetActive(true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (footprints == null) return;
        Gizmos.color = Color.cyan;
        foreach (GameObject f in footprints)
        {
            if (f != null)
                Gizmos.DrawWireSphere(f.transform.position, triggerRadius);
        }
    }
}