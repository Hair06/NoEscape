using UnityEngine;

// Tu dong gan vao tung khung tranh boi PaintingDropScare.
// Bao ve khi tranh cham san de phat tieng dung luc.
public class PaintingImpact : MonoBehaviour
{
    [HideInInspector] public PaintingDropScare owner;

    private bool reported = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (reported) return;
        if (collision.collider.CompareTag("Player")) return;

        reported = true;

        if (owner != null)
        {
            Vector3 point = collision.contacts.Length > 0
                ? collision.contacts[0].point
                : transform.position;
            owner.OnPaintingHitGround(point);
        }
    }
}