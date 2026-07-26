using UnityEngine;
using UnityEngine.Rendering; // Thêm thư viện này ở đầu file

public class CameraGlitchController : MonoBehaviour
{
    [Header("Cấu hình Volume Nhiễu")]
    [SerializeField] private Volume glitchVolume; // Kéo object CameraGlitchVolume vào đây

    // Gọi hàm này khi bật/tắt máy quay hoặc nhìn qua con mắt
    public void ToggleCameraGlitch(bool isUsingCamera)
    {
        if (glitchVolume != null)
        {
            // Nếu đang bật máy quay thì set Weight = 1 (nhiễu), ngược lại = 0 (bình thường)
            glitchVolume.weight = isUsingCamera ? 1f : 0f;
        }
    }
}