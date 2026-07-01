using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FamilyPictureDetailPrompt : MonoBehaviour
{
[Header("Prompt")]
[SerializeField] private TextMeshProUGUI promptText;
[SerializeField] private string promptMessage = "F - Xem chi tiết ảnh";
[Header("Detail UI")]
[SerializeField] private GameObject detailPanel;
[SerializeField] private Image detailImage;
[SerializeField] private Sprite pictureSprite;
[SerializeField] private RawImage detailRawImage;
[SerializeField] private Texture pictureTexture;
[SerializeField] private Vector2 maxPictureSize = new Vector2(700f, 500f);

[Header("Back Side / Password Clue")]
[SerializeField] private Sprite backSprite;
[SerializeField] private Texture backTexture;
[SerializeField] private Button flipButton;
[SerializeField] private string frontSideButtonText = "Lật mặt sau";
[SerializeField] private string backSideButtonText = "Quay lại mặt trước";

[Header("Input")]
[SerializeField] private KeyCode openCloseKey = KeyCode.F;
[SerializeField] private KeyCode flipKey = KeyCode.R;
[SerializeField] private KeyCode closeKey = KeyCode.Escape;

private bool isPlayerInside;
private bool isDetailOpen;
private bool isBackSide;

private void Start()
{
    AutoAssignReferences();

    if (flipButton != null)
    {
        flipButton.onClick.RemoveListener(ToggleFlipSide);
        flipButton.onClick.AddListener(ToggleFlipSide);
    }

    HidePrompt();
    HideDetail();

    isBackSide = false;
    ApplyPicture();
    UpdateFlipButtonText();
}

private void Update()
{
    if (!isPlayerInside && !isDetailOpen)
        return;

    if (GameInputBridge.GetKeyDown(openCloseKey))
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

    if (isDetailOpen && GameInputBridge.GetKeyDown(flipKey))
    {
        ToggleFlipSide();
    }

    if (isDetailOpen && GameInputBridge.GetKeyDown(closeKey))
    {
        HideDetail();
    }
}

private void OnTriggerEnter(Collider other)
{
    if (!other.CompareTag("Player"))
        return;

    isPlayerInside = true;

    if (!isDetailOpen)
    {
        ShowPrompt();
    }
}

private void OnTriggerStay(Collider other)
{
    if (!other.CompareTag("Player"))
        return;

    isPlayerInside = true;

    if (!isDetailOpen)
    {
        ShowPrompt();
    }
}

private void OnTriggerExit(Collider other)
{
    if (!other.CompareTag("Player"))
        return;

    isPlayerInside = false;
    HidePrompt();
    HideDetail();
}

private void ShowPrompt()
{
    if (promptText == null)
        return;

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
        Debug.LogWarning(
            $"[FamilyPictureDetailPrompt] detailPanel is not assigned on '{name}'."
        );
        return;
    }

    isDetailOpen = true;
    isBackSide = false;

    HidePrompt();
    HideOtherInteractionPrompts();
    AutoAssignReferences();
    ApplyPicture();
    UpdateFlipButtonText();

    detailPanel.SetActive(true);

    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;
}

private void HideDetail()
{
    isDetailOpen = false;
    isBackSide = false;

    if (detailPanel != null)
    {
        detailPanel.SetActive(false);
    }

    UpdateFlipButtonText();

    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;

    if (isPlayerInside)
    {
        ShowPrompt();
    }
}

public void ToggleFlipSide()
{
    if (!isDetailOpen)
        return;

    isBackSide = !isBackSide;

    ApplyPicture();
    UpdateFlipButtonText();

    Debug.Log(
        $"[FamilyPictureDetailPrompt] Flip side on '{name}'. BackSide={isBackSide}"
    );
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

        if (flipButton == null)
        {
            Button[] buttons = detailPanel.GetComponentsInChildren<Button>(true);

            foreach (Button button in buttons)
            {
                if (button.name.ToLower().Contains("flip") ||
                    button.name.ToLower().Contains("lat") ||
                    button.name.ToLower().Contains("lật"))
                {
                    flipButton = button;
                    break;
                }
            }
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
    Sprite spriteToShow = isBackSide ? backSprite : pictureSprite;
    Texture textureToShow = isBackSide ? backTexture : pictureTexture;

    bool hasImageSprite = false;
    bool hasRawTexture = false;

    if (detailImage != null)
    {
        if (spriteToShow == null && textureToShow is Texture2D texture2D)
        {
            spriteToShow = Sprite.Create(
                texture2D,
                new Rect(0f, 0f, texture2D.width, texture2D.height),
                new Vector2(0.5f, 0.5f)
            );
        }

        if (spriteToShow != null)
        {
            detailImage.sprite = spriteToShow;
            detailImage.preserveAspect = true;
            detailImage.enabled = true;

            FitRectToAspect(
                detailImage.rectTransform,
                spriteToShow.rect.width / spriteToShow.rect.height
            );

            hasImageSprite = true;
        }
    }

    if (detailRawImage != null)
    {
        if (textureToShow != null)
        {
            detailRawImage.texture = textureToShow;
            detailRawImage.enabled = true;

            FitRectToAspect(
                detailRawImage.rectTransform,
                (float)textureToShow.width / textureToShow.height
            );

            hasRawTexture = true;
        }
    }

    if (!hasImageSprite && !hasRawTexture)
    {
        Debug.LogWarning(
            $"[FamilyPictureDetailPrompt] Missing {(isBackSide ? "back side" : "front side")} image on '{name}'."
        );
    }
}

private void UpdateFlipButtonText()
{
    if (flipButton == null)
        return;

    TextMeshProUGUI tmpText =
        flipButton.GetComponentInChildren<TextMeshProUGUI>(true);

    if (tmpText != null)
    {
        tmpText.text = isBackSide ? backSideButtonText : frontSideButtonText;
        return;
    }

    Text legacyText =
        flipButton.GetComponentInChildren<Text>(true);

    if (legacyText != null)
    {
        legacyText.text = isBackSide ? backSideButtonText : frontSideButtonText;
    }
}

private void FitRectToAspect(RectTransform rectTransform, float aspect)
{
    if (rectTransform == null || aspect <= 0f)
        return;

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
