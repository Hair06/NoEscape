using UnityEngine;
using System.Collections;

public class RoomLight : MonoBehaviour
{
    public Light[] lights;   // danh sach den se sang

    [Header("Am thanh tach tach")]
    public AudioSource flickerAudio;
    public AudioClip flickerClip;

    [Header("VFX tia lua (moi den mot cai)")]
    [Tooltip("Keo cac object tia lua vao day - dat canh tung den")]
    public ParticleSystem[] sparkVFXs;

    [Header("Cau hinh do ngau nhien")]
    [Tooltip("Thoi gian sang on dinh toi thieu/toi da (giay)")]
    public float steadyMin = 1.5f;
    public float steadyMax = 4f;
    [Tooltip("Thoi gian chop lia toi thieu/toi da (giay)")]
    public float flickerMin = 0.3f;
    public float flickerMax = 1.2f;
    [Tooltip("Thoi gian tat toi den toi thieu/toi da (giay)")]
    public float blackoutMin = 0.2f;
    public float blackoutMax = 0.8f;
    [Tooltip("Toc do moi nhip chop khi dang chop lia")]
    public float flickerSpeed = 0.08f;

    void Start()
    {
        SetLights(false);   // luc dau tat het
    }

    // Goi tu Generator khi du xang
    public void TurnOn()
    {
        StartCoroutine(RandomLightLoop());
    }

    private IEnumerator RandomLightLoop()
    {
        while (true)
        {
            int mode = Random.Range(0, 3);

            if (mode == 0)
            {
                // Sang on dinh mot luc - khong toe lua
                SetLights(true);
                yield return new WaitForSeconds(Random.Range(steadyMin, steadyMax));
            }
            else if (mode == 1)
            {
                // Chop lia lich + toe tia lua o tat ca den
                PlayAllSparks();

                float dur = Random.Range(flickerMin, flickerMax);
                float t = 0f;
                while (t < dur)
                {
                    SetLights(true);
                    PlayTick();
                    float on = Random.Range(0.02f, flickerSpeed);
                    yield return new WaitForSeconds(on);

                    SetLights(false);
                    float off = Random.Range(0.02f, flickerSpeed);
                    yield return new WaitForSeconds(off);

                    t += on + off;
                }

                StopAllSparks();
            }
            else
            {
                // Tat toi vai giay
                SetLights(false);
                yield return new WaitForSeconds(Random.Range(blackoutMin, blackoutMax));
            }
        }
    }

    private void PlayAllSparks()
    {
        if (sparkVFXs == null) return;
        foreach (ParticleSystem s in sparkVFXs)
        {
            if (s != null)
            {
                s.gameObject.SetActive(true);
                s.Play();
            }
        }
    }

    private void StopAllSparks()
    {
        if (sparkVFXs == null) return;
        foreach (ParticleSystem s in sparkVFXs)
        {
            if (s != null) s.Stop();
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