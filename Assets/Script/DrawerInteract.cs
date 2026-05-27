using UnityEngine;

public class DrawerInteract : MonoBehaviour
{
    [Header("UI")]
    public GameObject interactText;

    [Header("Drawer Settings")]
    public Transform drawer;
    public Vector3 openOffset = new Vector3(0, 0, 0.3f);
    public float speed = 3f;

    private Vector3 closedPos;
    private Vector3 openPos;

    private bool playerNear = false;
    private bool isOpen = false;

    void Start()
    {
        closedPos = drawer.localPosition;
        openPos = closedPos + openOffset;

        if (interactText != null)
            interactText.SetActive(false);
    }

    void Update()
    {
        if (playerNear && GameInputBridge.GetKeyDown(KeyCode.E))
        {
            isOpen = !isOpen;
        }

        Vector3 targetPos = isOpen ? openPos : closedPos;

        drawer.localPosition = Vector3.Lerp(
            drawer.localPosition,
            targetPos,
            Time.deltaTime * speed
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;

            if (interactText != null)
                interactText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;

            if (interactText != null)
                interactText.SetActive(false);
        }
    }
}
