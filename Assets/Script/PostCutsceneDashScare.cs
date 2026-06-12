using System.Collections;
using UnityEngine;

public class PostCutsceneDashScare : MonoBehaviour
{
    [Header("Theo dõi Cutscene Root")]
    [SerializeField] private GameObject watchedCutsceneRoot;

    [Header("Ghoul Lao Ngang")]
    [SerializeField] private Transform ghoulHolder;
    [SerializeField] private GameObject ghoulVisual;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float dashDuration = 1f;

    [Header("Âm thanh")]
    [SerializeField] private AudioClip scareSound;
    [SerializeField] private float volume = 1f;

    [Header("Đèn chớp")]
    [SerializeField] private Light[] lightsToFlicker;
    [SerializeField] private float flickerDuration = 0.4f;

    [Header("Khóa điều khiển khi jumpscare")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    [Header("Test")]
    [SerializeField] private bool testWithJKey = true;

    private bool hasSeenCutsceneActive = false;
    private bool wasCutsceneActive = false;
    private bool triggered = false;

    private void Awake()
    {
        if (ghoulHolder != null)
            ghoulHolder.gameObject.SetActive(true);

        if (ghoulVisual != null)
            ghoulVisual.SetActive(false);
    }

    private void Update()
    {
        if (testWithJKey && GameInputBridge.GetKeyDown(KeyCode.J))
{
    TriggerScare();
}

        if (triggered) return;
        if (watchedCutsceneRoot == null) return;

        bool isCutsceneActive = watchedCutsceneRoot.activeInHierarchy;

        if (isCutsceneActive)
            hasSeenCutsceneActive = true;

        if (hasSeenCutsceneActive && wasCutsceneActive && !isCutsceneActive)
            TriggerScare();

        wasCutsceneActive = isCutsceneActive;
    }

    public void TriggerScare()
    {
        if (triggered) return;

        Debug.Log("TriggerScare được gọi.");
        StartCoroutine(PlayScare());
    }

    private IEnumerator PlayScare()
    {
        Debug.Log("JUMPSCARE START");

        triggered = true;
        SetGameplayControl(false);

        if (ghoulHolder == null)
        {
            Debug.LogError("Chưa gán Ghoul Holder.");
            SetGameplayControl(true);
            yield break;
        }

        if (ghoulVisual == null)
        {
            Debug.LogError("Chưa gán Ghoul Visual.");
            SetGameplayControl(true);
            yield break;
        }

        if (startPoint == null || endPoint == null)
        {
            Debug.LogError("Chưa gán DashStart hoặc DashEnd.");
            SetGameplayControl(true);
            yield break;
        }

        ghoulHolder.gameObject.SetActive(true);
        ghoulHolder.position = startPoint.position;

        Vector3 lookTarget = endPoint.position;
        lookTarget.y = ghoulHolder.position.y;
        ghoulHolder.LookAt(lookTarget);

        ghoulVisual.SetActive(true);

        Debug.Log("Ghoul Visual đã bật tại: " + ghoulHolder.position);

        PlaySound();
        StartCoroutine(FlickerLights());

        float timer = 0f;

        while (timer < dashDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / dashDuration);
            ghoulHolder.position = Vector3.Lerp(startPoint.position, endPoint.position, t);

            yield return null;
        }

        ghoulVisual.SetActive(false);

        Debug.Log("Ghoul Visual đã tắt.");

        SetGameplayControl(true);
    }

    private IEnumerator FlickerLights()
    {
        if (lightsToFlicker == null || lightsToFlicker.Length == 0)
            yield break;

        foreach (Light l in lightsToFlicker)
        {
            if (l != null) l.enabled = false;
        }

        yield return new WaitForSecondsRealtime(flickerDuration / 2f);

        foreach (Light l in lightsToFlicker)
        {
            if (l != null) l.enabled = true;
        }
    }

    private void PlaySound()
    {
        if (scareSound == null) return;

        GameObject audioObj = new GameObject("GhoulDashSound");
        AudioSource source = audioObj.AddComponent<AudioSource>();

        source.clip = scareSound;
        source.volume = volume;
        source.spatialBlend = 0f;
        source.Play();

        Destroy(audioObj, scareSound.length + 0.2f);
    }

    private void SetGameplayControl(bool enabled)
    {
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null)
                script.enabled = enabled;
        }
    }
}