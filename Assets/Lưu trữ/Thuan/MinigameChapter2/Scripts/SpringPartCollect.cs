using UnityEngine;

public class SpringPartCollect : MonoBehaviour, IInteractable
{
    [Header("Chữ gợi ý khi nhìn vào")]
    [SerializeField] private string interactMessage = "Nhấn [E] để nhặt Lò Xo Nhạc";

    [Header("Âm thanh khi nhặt (có thể để trống)")]
    [SerializeField] private AudioClip collectSound;

    public string GetInteractPrompt()
    {
        return interactMessage;
    }

    public void Interact()
    {
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        if (MusicBoxRestore.Instance != null)
            MusicBoxRestore.Instance.CollectPart(MusicBoxRestore.MusicBoxPart.Spring);

        PlayerInventory.Add("LoXo");   // them vao hotbar

        Debug.Log("Đã nhặt Lò Xo Nhạc!");

        Destroy(gameObject);
    }
}