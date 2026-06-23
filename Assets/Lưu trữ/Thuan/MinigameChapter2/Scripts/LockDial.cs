using UnityEngine;

// Gắn vào TỪNG vòng số của khóa (4 vòng = 4 script này).
// Vòng tự xoay mượt (Lerp) tới số được chỉ định khi người chơi gõ phím.
public class LockDial : MonoBehaviour
{
    [Header("Trục xoay của vòng")]
    [Tooltip("Trục mà vòng số quay quanh. Model này dùng X = (1,0,0).")]
    [SerializeField] private Vector3 rotateAxis = new Vector3(1, 0, 0);

    [Header("Số nấc (số chữ số trên vòng)")]
    [SerializeField] private int totalNumbers = 10;

    [Header("Căn chỉnh cho khớp số")]
    [Tooltip("Đảo chiều xoay nếu gõ 1 mà vòng đi tới 9")]
    [SerializeField] private bool invertDirection = true;
    [Tooltip("Bù góc gốc (độ). Chỉnh cho khớp số 0 ban đầu.")]
    [SerializeField] private float angleOffset = 0f;

    [Header("Tốc độ xoay mượt")]
    [SerializeField] private float rotateSpeed = 8f;

    [Header("Âm thanh tạch khi đổi số (có thể để trống)")]
    [SerializeField] private AudioSource tickAudio;

    private int currentNumber = 0;
    private float anglePerStep;
    private Quaternion baseRotation;
    private Quaternion targetRotation;
    private bool baseCaptured = false;

    private void Awake()
    {
        anglePerStep = 360f / totalNumbers;
        CaptureBase();
    }

    // Ghi lại góc gốc hiện tại làm mốc (gọi khi vào chế độ chỉnh để tránh lệch do đổi parent)
    public void CaptureBase()
    {
        baseRotation = transform.localRotation;
        targetRotation = baseRotation;
        baseCaptured = true;
    }

    private void Update()
    {
        if (!baseCaptured) return;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * rotateSpeed);
    }

    public int GetCurrentNumber() => currentNumber;

    // Đặt vòng về một số cụ thể (0-9), vòng sẽ xoay mượt tới đó
    public void SetNumber(int number)
    {
        number = ((number % totalNumbers) + totalNumbers) % totalNumbers;

        if (number != currentNumber && tickAudio != null)
            tickAudio.Play();

        currentNumber = number;

        float dir = invertDirection ? -1f : 1f;
        float angle = (anglePerStep * number * dir) + angleOffset;

        targetRotation = baseRotation * Quaternion.AngleAxis(angle, rotateAxis.normalized);
    }

    public void ResetDial()
    {
        SetNumber(0);
    }
}