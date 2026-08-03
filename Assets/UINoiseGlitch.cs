using UnityEngine;
using UnityEngine.UI;

public class UINoiseGlitch : MonoBehaviour
{
    private RawImage rawImage;
    [SerializeField] private float speed = 0.04f;
    [SerializeField] private int textureSize = 256; // Kích thước ảnh nhiễu

    private float timer;
    private Texture2D noiseTexture;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
        
        // TỰ ĐỘNG TẠO ẢNH NHIỄU HẠT TV BẰNG CODE
        CreateNoiseTexture();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= speed && rawImage != null)
        {
            timer = 0f;
            
            // Dịch chuyển UV liên tục để hạt nhiễu nhảy tạch tạch
            rawImage.uvRect = new Rect(Random.value, Random.value, 1f, 1f);
        }
    }

    // Hàm tạo texture hạt nhiễu đen trắng/xám ngẫu nhiên
    private void CreateNoiseTexture()
    {
        noiseTexture = new Texture2D(textureSize, textureSize);
        Color[] pixels = new Color[textureSize * textureSize];

        for (int i = 0; i < pixels.Length; i++)
        {
            // Lấy một giá trị xám ngẫu nhiên từ 0 (đen) đến 1 (trắng)
            float randomValue = Random.value;
            pixels[i] = new Color(randomValue, randomValue, randomValue, 1f);
        }

        noiseTexture.SetPixels(pixels);
        noiseTexture.Apply();

        // Gán texture vừa tạo vào RawImage
        if (rawImage != null)
        {
            rawImage.texture = noiseTexture;
        }
    }

    private void OnDestroy()
    {
        // Giải phóng bộ nhớ khi tắt game
        if (noiseTexture != null)
        {
            Destroy(noiseTexture);
        }
    }
}