using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public Transform player;

    public float detectDistance = 10f;
    public float rotationSpeed = 2f;

    public AudioSource audioSource;

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if(distance <= detectDistance)
        {
            Vector3 dir = player.position - transform.position;
            dir.y = 0;

            Quaternion target = Quaternion.LookRotation(dir);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                target,
                rotationSpeed * Time.deltaTime);

            if(!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            if(audioSource.isPlaying)
                audioSource.Stop();
        }
    }
}