using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

public class RitualAltar : MonoBehaviour
{
    [Header("Các đối tượng liên quan")]
    public GameObject phongAn;      // Kí tự phong ấn gợi ý (Material X-Ray đỏ ZTest Always)
    public GameObject jarOnAltar;   // Chiếc bình trên bệ
    public ParticleSystem soulVFX;  // Hiệu ứng linh hồn

    [Header("Cấu hình Hiệu ứng")]
    [Tooltip("Thời gian hiệu ứng linh hồn chạy (tính bằng giây)")]
    public float soulVFXDuration = 5f;

    [Header("Giao diện UI")]
    public TextMeshProUGUI promptText;
    public string promptMessage = "Bấm [E] để đặt Bình lên Bệ Tế";

    private bool hasJar = false;
    private bool isPlaced = false;
    private bool isNearPlayer = false;

    private void Start()
    {
        if (phongAn) phongAn.SetActive(false);
        if (jarOnAltar) jarOnAltar.SetActive(false);
        if (soulVFX) soulVFX.Stop();
        if (promptText) promptText.gameObject.SetActive(false);
    }

    // Hàm gọi từ SoulJar khi người chơi nhặt bình thành công
    public void OnPickUpJar()
    {
        hasJar = true;

        // Bật phong ấn đỏ nhìn xuyên tường
        if (phongAn) 
        {
            phongAn.SetActive(true); 
            Debug.Log("[RitualAltar] Đã nhặt bình -> Bật Phong Ấn gợi ý!");
        }

        // Nếu người chơi đang đứng sẵn trong vùng Trigger của Bệ Tế thì hiện UI ngay
        if (isNearPlayer && !isPlaced && promptText != null)
        {
            promptText.text = promptMessage;
            promptText.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        bool eKeyPressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        if (hasJar && !isPlaced && isNearPlayer && eKeyPressed)
        {
            PlaceJar();
        }
    }

    private void PlaceJar()
    {
        isPlaced = true;

        if (phongAn) phongAn.SetActive(false);      // Tắt phong ấn gợi ý
        if (jarOnAltar) jarOnAltar.SetActive(true); // Hiện chiếc bình trên bệ

        if (soulVFX)
        {
            StartCoroutine(PlaySoulVFXRoutine());
        }

        if (promptText) promptText.gameObject.SetActive(false);
    }

    private IEnumerator PlaySoulVFXRoutine()
    {
        soulVFX.Play();
        yield return new WaitForSeconds(soulVFXDuration);
        soulVFX.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNearPlayer = true;
            if (hasJar && !isPlaced && promptText != null)
            {
                promptText.text = promptMessage;
                promptText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNearPlayer = false;
            if (promptText != null) promptText.gameObject.SetActive(false);
        }
    }
}