using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform camTransform;

    private void Start()
    {
        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (camTransform == null)
        {
            if (Camera.main != null) camTransform = Camera.main.transform;
            return;
        }

        // Tự động xoay mặt phẳng luôn hướng thẳng diện mạo về phía Camera
        transform.LookAt(transform.position + camTransform.rotation * Vector3.forward,
                         camTransform.rotation * Vector3.up);
    }
}