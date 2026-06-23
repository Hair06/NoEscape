using UnityEngine;
using System.Collections;

public class RoomLight : MonoBehaviour
{
    public Light[] lights;   // danh sach den se sang

    [Header("Hieu ung chop tat")]
    [Tooltip("Thoi gian moi nhip chop (giay) - so nho chop nhanh")]
    public float flickerSpeed = 0.1f;

    [Header("Am thanh tach tach")]
    [Tooltip("AudioSource phat tieng tach")]
    public AudioSource flickerAudio;
    [Tooltip("File tieng tach den")]
    public AudioClip flickerClip;

    void Start()
    {
        SetLights(false);   // luc dau tat het
    }

    // Goi tu Generator khi du xang
    public void TurnOn()
    {
        StartCoroutine(FlickerForever());
    }

    private IEnumerator FlickerForever()
    {
        // Lap vo han - den chop tat mai mai
        while (true)
        {
            // Bat den + phat tieng tach
            SetLights(true);
            PlayTick();
            yield return new WaitForSeconds(Random.Range(0.02f, flickerSpeed));

            // Tat den
            SetLights(false);
            yield return new WaitForSeconds(Random.Range(0.02f, flickerSpeed));
        }
    }

    private void PlayTick()
    {
        if (flickerAudio != null && flickerClip != null)
            flickerAudio.PlayOneShot(flickerClip);
    }

    void SetLights(bool on)
    {
        foreach (Light l in lights)
            if (l != null) l.enabled = on;
    }
}