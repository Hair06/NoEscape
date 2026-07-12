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
    [Tooltip("Player vào trong bán kính này thì dấu chân kế tiếp hiện ra")]
    [SerializeField] private float triggerRadius = 2f;

    [Header("Phần thưởng khi đi hết chuỗi")]
    [Tooltip("Vật phẩm hiện ra trên bàn đá (Trái tim hoặc Giọt máu)")]
    [SerializeField] private GameObject[] rewards;

    [Header("Âm thanh (có thể để trống)")]
    [Tooltip("Tiếng khi dấu chân mới hiện ra")]
    [SerializeField] private AudioClip stepRevealSound;
    [Tooltip("Tiếng khi đi hết chuỗi, vật phẩm lộ diện")]
    [SerializeField] private AudioClip completeSound;

    private Transform player;
    private int currentIndex = 0;   // dấu chân đang chờ người chơi tới
    private bool isComplete = false;

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;

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

        // Hiện dấu chân đầu tiên để dẫn đường
        if (footprints.Length > 0 && footprints[0] != null)
            footprints[0].SetActive(true);
    }

    private void Update()
    {
        if (isComplete || player == null) return;
        if (currentIndex >= footprints.Length) return;

        GameObject current = footprints[currentIndex];
        if (current == null)
        {
            currentIndex++;
            return;
        }

        float dist = Vector3.Distance(player.position, current.transform.position);

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
            // Hiện dấu chân tiếp theo
            if (footprints[currentIndex] != null)
                footprints[currentIndex].SetActive(true);

            if (stepRevealSound != null)
                AudioSource.PlayClipAtPoint(stepRevealSound, transform.position);
        }
        else
        {
            CompleteTrail();
        }
    }

    private void CompleteTrail()
    {
        isComplete = true;
        Debug.Log("Đã đi hết chuỗi dấu chân! Vật phẩm hiện ra trên bàn đá.");

        if (completeSound != null)
            AudioSource.PlayClipAtPoint(completeSound, transform.position);

        foreach (GameObject r in rewards)
        {
            if (r != null) r.SetActive(true);
        }
    }

    // Vẽ vòng tròn bán kính trong Scene để dễ căn chỉnh
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