using UnityEngine;

// Bản dùng IInteractable (tương thích Player mới Elman + PlayerInteraction raycast).
// Nhìn vào Lò Xo Nhạc và nhấn E để nhặt -> báo hộp nhạc.
public class SpringPartCollect : MonoBehaviour, IInteractable
{
    [Header("Chữ gợi ý khi nhìn vào")]
    [SerializeField] private string interactMessage = "Nhấn [E] để nhặt Lò Xo Nhạc";

    [Header("Âm thanh khi nhặt (có thể để trống)")]
    [SerializeField] private AudioClip collectSound;

    // PlayerInteraction gọi hàm này để lấy chữ hiển thị
    public string GetInteractPrompt()
    {
        return interactMessage;
    }

    // PlayerInteraction gọi hàm này khi người chơi nhìn vào và nhấn E
    public void Interact()
    {
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        // Báo về hộp nhạc: đã có Lò Xo Nhạc
        if (MusicBoxRestore.Instance != null)
            MusicBoxRestore.Instance.CollectPart(MusicBoxRestore.MusicBoxPart.Spring);

        Debug.Log("Đã nhặt Lò Xo Nhạc!");

        Destroy(gameObject);
    }
}