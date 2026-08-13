using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostCutsceneDashScare : MonoBehaviour
{
    [Header("Ghoul Lao Ngang")]
    [SerializeField] private Transform ghoulHolder;
    [SerializeField] private GameObject ghoulVisual;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float dashDuration = 1.5f;

    [Header("Camera Player")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform scareCameraPoint;
    [SerializeField] private float moveCameraDuration = 0.4f;
    [SerializeField] private float holdCameraBeforeDash = 0.3f;
    [SerializeField] private float holdCameraAfterDash = 1.5f;
    [SerializeField] private bool returnCameraAfterScare = true;

    [Header("Khóa điều khiển Player + Camera")]
    [Tooltip("Script điều khiển di chuyển chính của Player. Nếu để trống, hệ thống sẽ tự tìm PlayerController hoặc vThirdPersonInput.")]
    [SerializeField] private MonoBehaviour playerMovementController;
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    [Header("Menu hiện sau Jumpscare")]
    [SerializeField] private GameObject menuAfterScare;

    [Header("Âm thanh")]
    [SerializeField] private AudioClip scareSound;
    [SerializeField] private float volume = 1f;

    [Header("Hộp nhạc sau khi Ghoul lướt qua")]
    [Tooltip("Đoạn nhạc hộp nhạc dùng để dẫn vào Chương 2.")]
    [SerializeField] private AudioClip musicBoxClip;
    [SerializeField, Min(0f)] private float musicBoxDelay = 0.25f;
    [SerializeField, Min(0.1f)] private float musicBoxDuration = 3f;
    [SerializeField, Range(0f, 1f)] private float musicBoxVolume = 0.65f;
    [SerializeField, Min(0f)] private float musicBoxFadeOutDuration = 0.4f;

    [Header("Đèn chớp")]
    [SerializeField] private Light[] lightsToFlicker;
    [SerializeField] private GameObject[] lightObjectsToDisable;
    [SerializeField] private float flickerDuration = 0.8f;

    [Header("Debug")]
    [SerializeField] private bool testWithJKey = true;
    [SerializeField] private bool forceVisibleLarge = true;
    [SerializeField] private Vector3 debugScale = new Vector3(3f, 3f, 3f);
    [SerializeField] private bool keepGhoulVisibleAfterDash = false;

    private bool triggered;
    private bool forceLockCamera;
    private bool gameplayControlLocked;

    private readonly Dictionary<MonoBehaviour, bool>
        gameplayScriptStates = new Dictionary<MonoBehaviour, bool>();

    private Vector3 originalCameraPos;
    private Quaternion originalCameraRot;

    private void Awake()
    {
        ResolvePlayerMovementController();

        if (ghoulHolder != null)
            ghoulHolder.gameObject.SetActive(true);

        if (ghoulVisual != null)
            ghoulVisual.SetActive(false);
    }

    private void OnDisable()
    {
        // Không để Player bị khóa vĩnh viễn nếu object scare bị tắt giữa coroutine.
        RestoreGameplayControl();
        forceLockCamera = false;
    }

    private void Update()
    {
        if (testWithJKey && GameInputBridge.GetKeyDown(KeyCode.J))
        {
            TriggerScare();
        }
    }

    private void LateUpdate()
    {
        if (forceLockCamera && playerCamera != null && scareCameraPoint != null)
        {
            playerCamera.position = scareCameraPoint.position;
            playerCamera.rotation = scareCameraPoint.rotation;
        }
    }

    public void TriggerScare()
    {
        if (triggered) return;

        StartCoroutine(PlayScare());
    }

    private IEnumerator PlayScare()
    {
        triggered = true;

        if (!ValidateReferences())
        {
            triggered = false;
            FinishQuestTransition();
            yield break;
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.SetGameplayUiSuppressed(true);
        }

        if (menuAfterScare != null)
            menuAfterScare.SetActive(false);

        SetGameplayControl(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        originalCameraPos = playerCamera.position;
        originalCameraRot = playerCamera.rotation;

        yield return StartCoroutine(MoveCameraToScarePoint());

        forceLockCamera = true;

        yield return new WaitForSecondsRealtime(holdCameraBeforeDash);

        SetupGhoulAtStartPoint();
        PlaySound();

        yield return StartCoroutine(FlickerLightsAndDash());

        if (!keepGhoulVisibleAfterDash && ghoulVisual != null)
            ghoulVisual.SetActive(false);

        if (holdCameraAfterDash > 0f)
            yield return new WaitForSecondsRealtime(holdCameraAfterDash);

        forceLockCamera = false;

        if (returnCameraAfterScare)
            yield return StartCoroutine(ReturnCamera());

        SetGameplayControl(true);

        // Camera và điều khiển đã trở lại bình thường trước khi nhạc hộp nhạc phát.
        if (musicBoxDelay > 0f)
            yield return new WaitForSecondsRealtime(musicBoxDelay);

        yield return StartCoroutine(PlayMusicBoxCue());

        if (menuAfterScare != null)
            menuAfterScare.SetActive(true);

        FinishQuestTransition();

        Debug.Log(
            "Jumpscare kết thúc, trả lại điều khiển và bắt đầu chương kế tiếp."
        );
    }

    private void FinishQuestTransition()
    {
        if (QuestManager.Instance == null)
        {
            return;
        }

        QuestManager.Instance.SetGameplayUiSuppressed(
            false,
            false
        );
        QuestManager.Instance.RequestStartNextChapter();
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

        if (playerCamera == null)
        {
            Debug.LogError("Chưa gán Player Camera.");
            valid = false;
        }

        if (scareCameraPoint == null)
        {
            Debug.LogError("Chưa gán Scare Camera Point.");
            valid = false;
        }

        return valid;
    }

    private IEnumerator MoveCameraToScarePoint()
    {
        Vector3 startPos = playerCamera.position;
        Quaternion startRot = playerCamera.rotation;

        float timer = 0f;

        while (timer < moveCameraDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / moveCameraDuration);

            playerCamera.position = Vector3.Lerp(startPos, scareCameraPoint.position, t);
            playerCamera.rotation = Quaternion.Slerp(startRot, scareCameraPoint.rotation, t);

            yield return null;
        }

        playerCamera.position = scareCameraPoint.position;
        playerCamera.rotation = scareCameraPoint.rotation;
    }

    private IEnumerator ReturnCamera()
    {
        Vector3 startPos = playerCamera.position;
        Quaternion startRot = playerCamera.rotation;

        float timer = 0f;

        while (timer < moveCameraDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / moveCameraDuration);

            playerCamera.position = Vector3.Lerp(startPos, originalCameraPos, t);
            playerCamera.rotation = Quaternion.Slerp(startRot, originalCameraRot, t);

            yield return null;
        }

        playerCamera.position = originalCameraPos;
        playerCamera.rotation = originalCameraRot;
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
    }

    private void EnableAllRenderers(GameObject target)
    {
        if (target == null) return;

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer r in renderers)
        {
            r.enabled = true;
            r.gameObject.SetActive(true);
        }
    }

    private IEnumerator PlayMusicBoxCue()
    {
        if (musicBoxClip == null)
        {
            Debug.LogWarning(
                "Chưa gán Music Box Clip; bỏ qua đoạn nhạc chuyển sang Chương 2."
            );
            yield break;
        }

        GameObject audioObj = new GameObject("MusicBoxTransitionCue");
        AudioSource source = audioObj.AddComponent<AudioSource>();

        source.clip = musicBoxClip;
        source.volume = musicBoxVolume;
        source.spatialBlend = 0f;
        source.loop = false;
        source.Play();

        float duration = Mathf.Min(musicBoxDuration, musicBoxClip.length);
        float fadeDuration = Mathf.Min(musicBoxFadeOutDuration, duration);
        float fadeStart = duration - fadeDuration;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            if (fadeDuration > 0f && timer >= fadeStart)
            {
                float fadeT = Mathf.InverseLerp(
                    duration,
                    fadeStart,
                    timer
                );
                source.volume = musicBoxVolume * fadeT;
            }

            yield return null;
        }

        source.Stop();
        Destroy(audioObj);
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
        if (enabled)
        {
            RestoreGameplayControl();
            return;
        }

        if (gameplayControlLocked)
        {
            return;
        }

        ResolvePlayerMovementController();
        gameplayScriptStates.Clear();

        RememberAndDisable(playerMovementController);

        if (scriptsToDisable != null)
        {
            foreach (MonoBehaviour script in scriptsToDisable)
            {
                RememberAndDisable(script);
            }
        }

        gameplayControlLocked = true;
    }

    private void RememberAndDisable(MonoBehaviour script)
    {
        if (script == null ||
            script == this ||
            gameplayScriptStates.ContainsKey(script))
        {
            return;
        }

        gameplayScriptStates.Add(script, script.enabled);
        script.enabled = false;
    }

    private void RestoreGameplayControl()
    {
        if (!gameplayControlLocked && gameplayScriptStates.Count == 0)
        {
            return;
        }

        foreach (KeyValuePair<MonoBehaviour, bool> state
                 in gameplayScriptStates)
        {
            if (state.Key != null)
            {
                state.Key.enabled = state.Value;
            }
        }

        gameplayScriptStates.Clear();
        gameplayControlLocked = false;
    }

    private void ResolvePlayerMovementController()
    {
        if (playerMovementController != null)
        {
            return;
        }

        if (playerCamera != null)
        {
            playerMovementController = FindMovementController(
                playerCamera.GetComponentsInParent<MonoBehaviour>(true)
            );
        }

        if (playerMovementController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                playerMovementController = FindMovementController(
                    player.GetComponentsInChildren<MonoBehaviour>(true)
                );
            }
        }

        if (playerMovementController == null)
        {
            Debug.LogWarning(
                "[PostCutsceneDashScare] Không tìm thấy script di chuyển Player. " +
                "Hãy gán Player Movement Controller trong Inspector."
            );
        }
    }

    private static MonoBehaviour FindMovementController(
        MonoBehaviour[] candidates)
    {
        if (candidates == null)
        {
            return null;
        }

        foreach (MonoBehaviour candidate in candidates)
        {
            if (candidate == null)
            {
                continue;
            }

            string typeName = candidate.GetType().FullName;

            if (typeName ==
                    "ElmanGameDevTools.PlayerSystem.PlayerController" ||
                typeName ==
                    "Invector.vCharacterController.vThirdPersonInput")
            {
                return candidate;
            }
        }

        return null;
    }
}
