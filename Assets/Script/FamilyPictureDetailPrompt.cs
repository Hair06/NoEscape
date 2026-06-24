using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FamilyPictureDetailPrompt : MonoBehaviour
{
    [Header("Prompt")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string promptMessage = "F - Xem chi tiet anh";

    [Header("Detail UI")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Image detailImage;
    [SerializeField] private Sprite pictureSprite;
    [SerializeField] private RawImage detailRawImage;
    [SerializeField] private Texture pictureTexture;
    [SerializeField] private Vector2 maxPictureSize = new Vector2(700f, 500f);

    private bool isPlayerInside;
    private bool isDetailOpen;

    private void Start()
    {
        AutoAssignReferences();
        HidePrompt();
        HideDetail();
        ApplyPicture();
    }

    private void Update()
    {
        if (!isPlayerInside && !isDetailOpen) return;

        if (GameInputBridge.GetKeyDown(KeyCode.F))
        {
            if (isDetailOpen)
            {
                HideDetail();
            }
            else if (isPlayerInside)
            {
                ShowDetail();
            }
        }

        if (isDetailOpen && GameInputBridge.GetKeyDown(KeyCode.Escape))
        {
            HideDetail();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = true;
        if (!isDetailOpen)
        {
            ShowPrompt();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = true;
        if (!isDetailOpen)
        {
            ShowPrompt();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = false;
        HidePrompt();
        HideDetail();
    }

    private void ShowPrompt()
    {
        if (promptText == null) return;

        promptText.text = promptMessage;
        promptText.gameObject.SetActive(true);
    }

    private void HidePrompt()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    private void ShowDetail()
    {
        if (detailPanel == null)
        {
            Debug.LogWarning($"[FamilyPictureDetailPrompt] detailPanel is not assigned on '{name}'.");
            return;
        }

        isDetailOpen = true;
        HidePrompt();
        HideOtherInteractionPrompts();
        AutoAssignReferences();
        ApplyPicture();

        detailPanel.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void HideDetail()
    {
        isDetailOpen = false;

        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (isPlayerInside)
        {
            ShowPrompt();
        }
    }

    private void AutoAssignReferences()
    {
        if (detailPanel != null)
        {
            if (detailImage == null)
            {
                detailImage = detailPanel.GetComponentInChildren<Image>(true);
            }

            if (detailRawImage == null)
            {
                detailRawImage = detailPanel.GetComponentInChildren<RawImage>(true);
            }
        }

        if (pictureSprite == null)
        {
            SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
            if (spriteRenderer != null)
            {
                pictureSprite = spriteRenderer.sprite;
            }
        }

        if (pictureTexture == null)
        {
            Renderer renderer = GetComponentInChildren<Renderer>(true);
            if (renderer != null && renderer.sharedMaterial != null)
            {
                pictureTexture = renderer.sharedMaterial.mainTexture;
            }
        }
    }

    private void ApplyPicture()
    {
        if (detailImage != null)
        {
            if (pictureSprite == null && pictureTexture is Texture2D texture)
            {
                pictureSprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
            }

            if (pictureSprite != null)
            {
                detailImage.sprite = pictureSprite;
                detailImage.preserveAspect = true;
                detailImage.enabled = true;
                FitRectToAspect(detailImage.rectTransform, pictureSprite.rect.width / pictureSprite.rect.height);
            }
            else
            {
                Debug.LogWarning($"[FamilyPictureDetailPrompt] No picture sprite assigned or found on '{name}'.");
            }
        }

        if (detailRawImage != null)
        {
            if (pictureTexture != null)
            {
                detailRawImage.texture = pictureTexture;
                detailRawImage.enabled = true;
                FitRectToAspect(detailRawImage.rectTransform, (float)pictureTexture.width / pictureTexture.height);
            }
            else if (detailImage == null || detailImage.sprite == null)
            {
                Debug.LogWarning($"[FamilyPictureDetailPrompt] No picture texture assigned or found on '{name}'.");
            }
        }
    }

    private void FitRectToAspect(RectTransform rectTransform, float aspect)
    {
        if (rectTransform == null || aspect <= 0f) return;

        float width = maxPictureSize.x;
        float height = width / aspect;

        if (height > maxPictureSize.y)
        {
            height = maxPictureSize.y;
            width = height * aspect;
        }

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    private void HideOtherInteractionPrompts()
    {
        foreach (CollectiblePiece collectiblePiece in GetComponentsInChildren<CollectiblePiece>(true))
        {
            collectiblePiece.SuppressPromptUntilExit();
        }

        CollectiblePiece parentCollectiblePiece = GetComponentInParent<CollectiblePiece>();
        if (parentCollectiblePiece != null)
        {
            parentCollectiblePiece.SuppressPromptUntilExit();
        }

        foreach (PlayerInteraction playerInteraction in FindObjectsByType<PlayerInteraction>(FindObjectsSortMode.None))
        {
            playerInteraction.SuppressCurrentPromptUntilExit();
        }
    }
}
