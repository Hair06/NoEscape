using UnityEngine;

// Script nay tu dong duoc gan vao den chum boi ChandelierFallScare.
// Nhiem vu: bao ve dung khoanh khac den cham san that.
public class ChandelierImpact : MonoBehaviour
{
    [HideInInspector] public ChandelierFallScare owner;

    private bool reported = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (reported) return;

        // Bo qua va cham voi Player
        if (collision.collider.CompareTag("Player")) return;

        reported = true;

        if (owner != null && collision.contacts.Length > 0)
            owner.OnChandelierHitGround(collision.contacts[0].point);
        else if (owner != null)
            owner.OnChandelierHitGround(transform.position);
    }
}