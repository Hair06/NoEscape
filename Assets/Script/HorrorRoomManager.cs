using UnityEngine;

public class HorrorRoomManager : MonoBehaviour
{
    [Header("CẤU HÌNH CỬA")]
    [SerializeField] private Transform doorLeft;
    [SerializeField] private Transform doorRight;
    [SerializeField] private float doorCloseSpeed = 5f;

    [Header("ÂM THANH (TÙY CHỌN)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip doorSlamSound; 

    private bool _isDoorClosed = false;

    // Góc KHÉP CHẶT (Góc mà bro xếp kín cửa ngoài Editor)
    private Quaternion _leftClosedRotation;
    private Quaternion _rightClosedRotation;

    // Góc MỞ TOANG ĐỂ ĐÓN PLAYER
    private Quaternion _leftOpenRotation;
    private Quaternion _rightOpenRotation;

    void Start()
    {
        // 1. Ghi lại góc khép chặt chuẩn ngoài Editor làm đích đến khi sập bẫy
        if (doorLeft != null) _leftClosedRotation = doorLeft.localRotation;
        if (doorRight != null) _rightClosedRotation = doorRight.localRotation;

        // 2. Tính toán góc mở sẵn (Xoay thêm 90 độ để mở toang ra)
        if (doorLeft != null) _leftOpenRotation = _leftClosedRotation * Quaternion.Euler(0, 90, 0);
        if (doorRight != null) _rightOpenRotation = _rightClosedRotation * Quaternion.Euler(0, -90, 0);

        // 3. Ép hai cánh cửa mở toang ra ngay từ giây đầu tiên vào game để có lối đi
        if (doorLeft != null) doorLeft.localRotation = _leftOpenRotation;
        if (doorRight != null) doorRight.localRotation = _rightOpenRotation;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Khi đi qua Trigger (Sập bẫy) -> _isDoorClosed chuyển thành true
        // Cửa sẽ tự động xoay mượt mà từ góc đang mở về góc khép chặt ban đầu!
        if (_isDoorClosed)
        {
            if (doorLeft != null)
            {
                doorLeft.localRotation = Quaternion.Slerp(doorLeft.localRotation, _leftClosedRotation, Time.deltaTime * doorCloseSpeed);
            }
            if (doorRight != null)
            {
                doorRight.localRotation = Quaternion.Slerp(doorRight.localRotation, _rightClosedRotation, Time.deltaTime * doorCloseSpeed);
            }
        }
    }

    public void PlayerEnteredRoom()
    {
        if (_isDoorClosed) return; 
        _isDoorClosed = true;

        if (audioSource != null && doorSlamSound != null)
        {
            audioSource.PlayOneShot(doorSlamSound);
        }

        Debug.Log("💀 BẪY ĐÃ SẬP! Cửa đang tự động đóng sầm lại!");
    }
}