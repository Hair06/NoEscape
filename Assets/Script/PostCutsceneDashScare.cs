using System.Collections;
using UnityEngine;

public class PostCutsceneDashScare : MonoBehaviour
{
    [Header("Ghoul Lao Ngang")]
    [SerializeField] private Transform ghoulHolder;
    [SerializeField] private GameObject ghoulVisual;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float dashDuration = 1f;

    [Header("Test nhìn thấy Ghoul")]
    [SerializeField] private bool testWithJKey = true;
    [SerializeField] private bool forceVisibleLarge = true;
    [SerializeField] private Vector3 debugScale = new Vector3(3f, 3f, 3f);
    [SerializeField] private bool keepGhoulVisibleAfterDash = false;

    [Header("Âm thanh")]
    [SerializeField] private AudioClip scareSound;
    [SerializeField] private float volume = 1f;

    [Header("Đèn chớp")]
    [SerializeField] private Light[] lightsToFlicker;
    [SerializeField] private GameObject[] lightObjectsToDisable;
    [SerializeField] private float flickerDuration = 0.4f;

    [Header("Khóa điều khiển khi jumpscare")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    private bool triggered;

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

        if (!ValidateReferences())
        {
            SetGameplayControl(true);
            yield break;
        }

        SetupGhoulAtStartPoint();

        PlaySound();

        yield return StartCoroutine(FlickerLightsAndDash());

        if (!keepGhoulVisibleAfterDash && ghoulVisual != null)
        {
            ghoulVisual.SetActive(false);
            Debug.Log("Ghoul Visual đã tắt sau khi dash.");
        }

        SetGameplayControl(true);
    }

    private bool ValidateReferences()
    {
        bool valid = true;

        if (ghoulHolder == null)
        {
            Debug.LogError("Chưa gán Ghoul Holder.");
            valid = false;
        }

        if (ghoulVisual == null)
        {
            Debug.LogError("Chưa gán Ghoul Visual.");
            valid = false;
        }

        if (startPoint == null)
        {
            Debug.LogError("Chưa gán DashStart.");
            valid = false;
        }

        if (endPoint == null)
        {
            Debug.LogError("Chưa gán DashEnd.");
            valid = false;
        }

        return valid;
    }

    private void SetupGhoulAtStartPoint()
    {
        ghoulHolder.gameObject.SetActive(true);
        ghoulHolder.position = startPoint.position;

        Vector3 lookTarget = endPoint.position;
        lookTarget.y = ghoulHolder.position.y;
        ghoulHolder.LookAt(lookTarget);

        if (forceVisibleLarge)
            ghoulHolder.localScale = debugScale;

        ghoulVisual.SetActive(true);

        EnableAllRenderers(ghoulVisual);

        Debug.Log("Ghoul Holder Position: " + ghoulHolder.position);
        Debug.Log("Ghoul Visual Active: " + ghoulVisual.activeInHierarchy);
        Debug.Log("Distance Start-End: " + Vector3.Distance(startPoint.position, endPoint.position));
    }

    private IEnumerator FlickerLightsAndDash()
    {
        SetLights(false);

        float timer = 0f;

        while (timer < dashDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / dashDuration);
            ghoulHolder.position = Vector3.Lerp(startPoint.position, endPoint.position, t);

            yield return null;
        }

        ghoulHolder.position = endPoint.position;

        yield return new WaitForSecondsRealtime(flickerDuration);

        SetLights(true);
    }

    private void SetLights(bool state)
    {
        if (lightsToFlicker != null)
        {
            foreach (Light l in lightsToFlicker)
            {
                if (l != null)
                    l.enabled = state;
            }
        }

        if (lightObjectsToDisable != null)
        {
            foreach (GameObject obj in lightObjectsToDisable)
            {
                if (obj != null)
                    obj.SetActive(state);
            }
        }

        Debug.Log(state ? "Đèn đã bật lại." : "Đèn đã tắt.");
    }

    private void EnableAllRenderers(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);

        Debug.Log("Số Renderer trong Ghoul Visual: " + renderers.Length);

        foreach (Renderer r in renderers)
        {
            r.enabled = true;
            r.gameObject.SetActive(true);
        }

        SkinnedMeshRenderer[] skinnedRenderers = target.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        foreach (SkinnedMeshRenderer r in skinnedRenderers)
        {
            r.enabled = true;
            r.gameObject.SetActive(true);
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
        if (scriptsToDisable == null) return;

        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null)
                script.enabled = enabled;
        }
    }
}