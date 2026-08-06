using UnityEngine;

public class StoneDoorPuzzle : MonoBehaviour
{
    [Header("BỆ ĐÁ XANH")]
    [SerializeField] private GameObject ghostCrystal; // Đá mờ làm mẫu (Xanh)
    [SerializeField] private GameObject realCrystal;  // Đá thật hiện lên (Xanh)

    [Header("BỆ ĐÁ ĐỎ")]
    [SerializeField] private GameObject ghostRed;     // Đá mờ làm mẫu (Đỏ)
    [SerializeField] private GameObject realRed;      // Đá thật hiện lên (Đỏ)

    [Header("CỬA ĐÁ & CẤU HÌNH LÚN XUỐNG")]
    [SerializeField] private Transform doorStoneTransform; // Kéo Transform của DoorStone vào đây
    [SerializeField] private float sinkDistance = 5f;       // Khoảng cách cửa sẽ lún xuống (mét)
    [SerializeField] private float sinkSpeed = 2f;          // Tốc độ lún xuống

    [Header("ÂM THANH")]
    [SerializeField] private AudioSource placeStoneSound;
    [SerializeField] private AudioSource openDoorSound;

    private bool isBluePlaced = false;
    private bool isRedPlaced = false;
    private bool isDoorOpening = false;

    private Vector3 initialDoorPosition;
    private Vector3 targetDoorPosition;

    void Start()
    {
        if (doorStoneTransform != null)
        {
            // Lưu vị trí ban đầu của cửa
            initialDoorPosition = doorStoneTransform.position;
            // Tính toán vị trí hạ xuống đất (Giảm Y đi một khoảng sinkDistance)
            targetDoorPosition = initialDoorPosition - new Vector3(0, sinkDistance, 0);
        }
    }

    void Update()
    {
        // Khi kích hoạt mở cửa, cho cửa di chuyển mượt mà xuống dưới đất
        if (isDoorOpening && doorStoneTransform != null)
        {
            doorStoneTransform.position = Vector3.MoveTowards(
                doorStoneTransform.position,
                targetDoorPosition,
                sinkSpeed * Time.deltaTime
            );

            // Khi đã lún hoàn toàn xuống đất
            if (Vector3.Distance(doorStoneTransform.position, targetDoorPosition) < 0.01f)
            {
                isDoorOpening = false; // Dừng chạy hàm Update
                Debug.Log("Cửa đá đã lún xong -> Xoá vĩnh viễn!");

                // CÁCH 1: Tắt hẳn GameObject cửa đi (Khuyên dùng)
                doorStoneTransform.gameObject.SetActive(false);

                // CÁCH 2: Nếu muốn xoá sạch hoàn toàn khỏi bộ nhớ RAM thì dùng lệnh bên dưới:
                // Destroy(doorStoneTransform.gameObject);
            }
        }
    }

    // Gọi hàm này khi Player lại gần BỆ XANH bấm đặt đá
    public void TryPlaceBlueStone()
    {
        if (isBluePlaced) return;

        if (StonePickup.HasBlueStone)
        {
            isBluePlaced = true;

            if (ghostCrystal != null) ghostCrystal.SetActive(false); // Tắt đá mờ
            if (realCrystal != null) realCrystal.SetActive(true);   // Hiện đá thật
            if (placeStoneSound != null) placeStoneSound.Play();

            Debug.Log("Đã đặt Đá Xanh vào bệ!");
            CheckDoorOpen();
        }
        else
        {
            Debug.Log("Chưa nhặt Đá Xanh! Không thể đặt.");
        }
    }

    // Gọi hàm này khi Player lại gần BỆ ĐỎ bấm đặt đá
    public void TryPlaceRedStone()
    {
        if (isRedPlaced) return;

        if (StonePickup.HasRedStone)
        {
            isRedPlaced = true;

            if (ghostRed != null) ghostRed.SetActive(false); // Tắt đá mờ
            if (realRed != null) realRed.SetActive(true);   // Hiện đá thật
            if (placeStoneSound != null) placeStoneSound.Play();

            Debug.Log("Đã đặt Đá Đỏ vào bệ!");
            CheckDoorOpen();
        }
        else
        {
            Debug.Log("Chưa nhặt Đá Đỏ! Không thể đặt.");
        }
    }

    private void CheckDoorOpen()
    {
        // Khi CẢ 2 BỆ đều đã được đặt đá
        if (isBluePlaced && isRedPlaced)
        {
            Debug.Log("Đã đặt đủ 2 đá! Bắt đầu lún cửa...");

            if (openDoorSound != null) openDoorSound.Play();

            // Kích hoạt biến để Update() tự động hạ cửa xuống
            isDoorOpening = true;
        }
    }
}