using UnityEngine;
using System.Collections;

// Moi den co nhip rieng, ngau nhien, doc lap voi nhau.
// Vong lap: chop tat mot loat -> toi -> chop lai -> toi...
// KHONG co giai doan sang on dinh.
public class RoomLight : MonoBehaviour
{
    public Light[] lights;   // danh sach den

    [Header("Số lần chớp mỗi loạt")]
    [Tooltip("Mỗi lần bừng lên sẽ chớp bao nhiêu cái")]
    public int flickerCountMin = 2;
    public int flickerCountMax = 7;

    [Header("Tốc độ mỗi nhịp chớp (giây)")]
    [Tooltip("Số nhỏ = chớp nhanh, dữ dội")]
    public float flickerSpeed = 0.08f;

    [Header("Thời gian TỐI giữa các loạt chớp (giây)")]
    [Tooltip("Sau khi chớp xong, đèn tắt tối bao lâu rồi chớp lại")]
    public float darkTimeMin = 1f;
    public float darkTimeMax = 4f;

    [Header("Âm thanh tách tách")]
    public AudioSource flickerAudio;
    public AudioClip flickerClip;
    [Range(0f, 1f)]
    public float flickerVolume = 0.7f;

    [Header("VFX tia lửa (tùy chọn - ánh xạ theo chỉ số đèn)")]
    [Tooltip("Element 0 = tia lửa của đèn 0. Để None nếu đèn đó không có lửa")]
    public ParticleSystem[] sparkVFXs;

    private bool isRunning = false;

    void Start()
    {
        SetAllLights(false);   // luc dau tat het
    }

    // Goi tu Generator khi du xang
    public void TurnOn()
    {
        if (isRunning) return;
        isRunning = true;

        // Moi den chay mot coroutine RIENG -> nhip doc lap, ngau nhien
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
                StartCoroutine(LightCycle(lights[i], i));
        }
    }

    // Vong doi cua MOT den: chop mot loat -> toi -> lap lai
    private IEnumerator LightCycle(Light light, int index)
    {
        // Lech pha ngau nhien luc dau de cac den khong dong bo nhau
        yield return new WaitForSeconds(Random.Range(0f, 2f));

        while (true)
        {
            // 1. CHOP TAT MOT LOAT
            int flickers = Random.Range(flickerCountMin, flickerCountMax + 1);

            // Toe tia lua trong luc chop (neu den nay co)
            PlaySpark(index, true);

            for (int i = 0; i < flickers; i++)
            {
                light.enabled = true;
                PlayTick();
                yield return new WaitForSeconds(Random.Range(0.02f, flickerSpeed));

                light.enabled = false;
                yield return new WaitForSeconds(Random.Range(0.02f, flickerSpeed));
            }

            PlaySpark(index, false);

            // 2. TAT TOI mot khoang roi chop lai
            light.enabled = false;
            yield return new WaitForSeconds(Random.Range(darkTimeMin, darkTimeMax));
        }
    }

    private void PlayTick()
    {
        if (flickerAudio != null && flickerClip != null)
            flickerAudio.PlayOneShot(flickerClip, flickerVolume);
    }

    // Bat/tat tia lua theo chi so den (neu co)
    private void PlaySpark(int index, bool play)
    {
        if (sparkVFXs == null || index >= sparkVFXs.Length) return;
        if (sparkVFXs[index] == null) return;

        if (play)
        {
            sparkVFXs[index].gameObject.SetActive(true);
            sparkVFXs[index].Play();
        }
        else
        {
            sparkVFXs[index].Stop();
        }
    }

    void SetAllLights(bool on)
    {
        foreach (Light l in lights)
            if (l != null) l.enabled = on;
    }
}