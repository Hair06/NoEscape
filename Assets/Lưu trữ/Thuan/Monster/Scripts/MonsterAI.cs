using UnityEngine;
using System.Collections;

// AI quai san duoi - KHONG dung NavMesh, di theo waypoint.
// Di tuan -> Player vao ban kinh thi DUOI (nhanh hon)
// -> Duoi qua lau khong bat duoc thi BO CUOC, quay ve tuan
// -> Bat duoc: camera nhay toi mat quai + animation can + man hinh den
// -> Player ve checkpoint gan nhat, quai quay ve tuan tiep.
public class MonsterAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack, Returning }

    [Header("=== DIEM TUAN TRA ===")]
    [Tooltip("Keo cac object trong lam diem tuan vao day, theo thu tu duong di")]
    [SerializeField] private Transform[] waypoints;
    [Tooltip("Toi gan diem bao nhieu met thi tinh la da toi")]
    [SerializeField] private float waypointReachDistance = 0.6f;
    [Tooltip("Dung lai bao lau o moi diem (giay)")]
    [SerializeField] private float waitAtWaypoint = 1f;

    [Header("=== TOC DO ===")]
    [Tooltip("Toc do khi di tuan")]
    [SerializeField] private float patrolSpeed = 1.8f;
    [Tooltip("Toc do khi duoi - nen CHAM HON toc do chay cua Player")]
    [SerializeField] private float chaseSpeed = 6.5f;
    [Tooltip("Toc do xoay than (do/giay)")]
    [SerializeField] private float turnSpeed = 240f;

    [Header("=== PHAT HIEN NGUOI CHOI ===")]
    [Tooltip("Vao ban kinh nay la quai bat dau duoi (met)")]
    [SerializeField] private float detectRadius = 10f;
    [Tooltip("Ra ngoai ban kinh nay thi quai bo cuoc, quay ve tuan")]
    [SerializeField] private float loseRadius = 16f;
    [Tooltip("Toi gan bao nhieu met thi tan cong")]
    [SerializeField] private float attackRadius = 1.8f;

    [Header("=== GIOI HAN THOI GIAN DUOI ===")]
    [Tooltip("Duoi toi da bao nhieu giay, khong bat duoc thi bo cuoc")]
    [SerializeField] private float maxChaseTime = 8f;
    [Tooltip("Sau khi bo cuoc, bao lau moi duoc phat hien lai (giay)")]
    [SerializeField] private float chaseCooldown = 3f;

    [Header("=== TRANH VAT CAN ===")]
    [Tooltip("Bat de quai tu ne tuong khi duoi")]
    [SerializeField] private bool avoidObstacles = true;
    [Tooltip("Layer cua tuong / vat can")]
    [SerializeField] private LayerMask obstacleLayer = ~0;
    [Tooltip("Tam do vat can phia truoc (met)")]
    [SerializeField] private float obstacleCheckDistance = 1.2f;

    [Header("=== ANIMATOR ===")]
    [SerializeField] private Animator animator;
    [Tooltip("Ten tham so Float toc do trong Animator")]
    [SerializeField] private string speedParam = "Speed";
    [Tooltip("Ten tham so Trigger tan cong")]
    [SerializeField] private string attackParam = "Attack";

    [Header("=== CAMERA KHI BI CAN ===")]
    [Tooltip("Keo CamAnchor_Bite (object con cua Monster) vao day")]
    [SerializeField] private Transform cameraBiteAnchor;
    [Tooltip("BAT: camera nhay tuc thi. TAT: bay muot toi")]
    [SerializeField] private bool snapCameraInstantly = true;
    [SerializeField] private float cameraFlySpeed = 12f;

    [Header("=== KHI BAT DUOC NGUOI CHOI ===")]
    [Tooltip("Animation can dai bao lau roi moi den man hinh (giay)")]
    [SerializeField] private float attackAnimTime = 1.5f;
    [Tooltip("Giu man hinh den bao lau (giay)")]
    [SerializeField] private float blackScreenTime = 1.5f;
    [Tooltip("Keo Image den phu man hinh vao day (Alpha de 0 san)")]
    [SerializeField] private UnityEngine.UI.Image blackOverlay;

    [Header("Diem hoi sinh Player")]
    [Tooltip("Player ve diem GAN NHAT luc bi bat")]
    [SerializeField] private Transform[] respawnPoints;

    [Header("=== AM THANH ===")]
    [Tooltip("AudioSource 2D cho tieng hu khi bi bat")]
    [SerializeField] private AudioSource scareAudioSource;
    [SerializeField] private AudioClip attackSound;
    [Range(0f, 3f)]
    [SerializeField] private float attackVolume = 1.8f;

    [Tooltip("AudioSource 3D gan tren quai - tieng gam gu khi duoi")]
    [SerializeField] private AudioSource growlSource;
    [SerializeField] private AudioClip growlClip;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;

    private State state = State.Patrol;
    private int currentWaypoint = 0;
    private Transform player;
    private MonoBehaviour playerController;
    private float waitTimer = 0f;
    private float chaseTimer = 0f;
    private float cooldownTimer = 0f;
    private bool isCatching = false;

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            player = p.transform;
        }
        else
        {
            Debug.LogError("[MonsterAI] Khong tim thay Player! Kiem tra Tag.");
            enabled = false;
            return;
        }

        if (animator == null) animator = GetComponent<Animator>();

        if (waypoints == null || waypoints.Length == 0)
            Debug.LogWarning("[MonsterAI] Chua gan waypoint - quai se dung im khi khong duoi.");

        if (cameraBiteAnchor == null)
            Debug.LogWarning("[MonsterAI] Chua gan 'Camera Bite Anchor' - camera se khong nhay toi mat quai.");

        if (blackOverlay != null) SetBlackAlpha(0f);
    }

    private void Update()
    {
        if (isCatching || player == null) return;

        // Dem nguoc thoi gian nghi sau khi bo cuoc
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        float dist = FlatDistance(transform.position, player.position);

        switch (state)
        {
            case State.Patrol:
            case State.Returning:
                // Chi phat hien khi da het thoi gian nghi
                if (dist <= detectRadius && cooldownTimer <= 0f)
                    StartChase();
                break;

            case State.Chase:
                // Dem thoi gian duoi
                chaseTimer += Time.deltaTime;

                if (dist <= attackRadius)
                {
                    StartCoroutine(CatchPlayer());
                    return;
                }

                // Het gio duoi -> bo cuoc
                if (chaseTimer >= maxChaseTime)
                {
                    Debug.Log("[MonsterAI] Duoi qua lau khong bat duoc. Bo cuoc.");
                    StopChase();
                    break;
                }

                // Player chay qua xa -> bo cuoc
                if (dist > loseRadius) StopChase();
                break;
        }

        if (state == State.Chase)
            MoveTowards(player.position, chaseSpeed);
        else
            PatrolUpdate();
    }

    // ===== TUAN TRA =====
    private void PatrolUpdate()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            SetAnimSpeed(0f);
            return;
        }

        Transform target = waypoints[currentWaypoint];
        if (target == null)
        {
            NextWaypoint();
            return;
        }

        float d = FlatDistance(transform.position, target.position);

        if (d <= waypointReachDistance)
        {
            SetAnimSpeed(0f);
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitAtWaypoint)
            {
                waitTimer = 0f;
                NextWaypoint();
                state = State.Patrol;
            }
            return;
        }

        MoveTowards(target.position, patrolSpeed);
    }

    private void NextWaypoint()
    {
        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
    }

    // ===== DI CHUYEN =====
    private void MoveTowards(Vector3 targetPos, float speed)
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;
        dir.Normalize();

        if (avoidObstacles)
            dir = AvoidObstacle(dir);

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRot, turnSpeed * Time.deltaTime);

        transform.position += dir * speed * Time.deltaTime;

        SetAnimSpeed(speed);
    }

    private Vector3 AvoidObstacle(Vector3 dir)
    {
        Vector3 origin = transform.position + Vector3.up * 1f;

        if (!Physics.Raycast(origin, dir, obstacleCheckDistance, obstacleLayer))
            return dir;

        Vector3 left = Quaternion.Euler(0f, -45f, 0f) * dir;
        Vector3 right = Quaternion.Euler(0f, 45f, 0f) * dir;

        bool leftBlocked = Physics.Raycast(origin, left, obstacleCheckDistance, obstacleLayer);
        bool rightBlocked = Physics.Raycast(origin, right, obstacleCheckDistance, obstacleLayer);

        if (!leftBlocked) return left;
        if (!rightBlocked) return right;

        return Quaternion.Euler(0f, 90f, 0f) * dir;
    }

    // ===== CHUYEN TRANG THAI =====
    private void StartChase()
    {
        state = State.Chase;
        waitTimer = 0f;
        chaseTimer = 0f;

        if (growlSource != null && growlClip != null && !growlSource.isPlaying)
        {
            growlSource.clip = growlClip;
            growlSource.loop = true;
            growlSource.Play();
        }

        Debug.Log("[MonsterAI] Phat hien Player! Bat dau duoi.");
    }

    private void StopChase()
    {
        state = State.Returning;
        chaseTimer = 0f;
        cooldownTimer = chaseCooldown;

        if (growlSource != null && growlSource.isPlaying)
            growlSource.Stop();

        currentWaypoint = FindNearestWaypointIndex(transform.position);

        Debug.Log("[MonsterAI] Quay ve tuan tra.");
    }

    private int FindNearestWaypointIndex(Vector3 from)
    {
        if (waypoints == null || waypoints.Length == 0) return 0;

        int best = 0;
        float bestDist = float.MaxValue;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            float d = FlatDistance(from, waypoints[i].position);
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }
        return best;
    }

    // ===== BAT DUOC PLAYER =====
    private IEnumerator CatchPlayer()
    {
        isCatching = true;
        state = State.Attack;
        SetAnimSpeed(0f);

        if (growlSource != null && growlSource.isPlaying)
            growlSource.Stop();

        Vector3 look = player.position - transform.position;
        look.y = 0f;
        if (look.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(look, Vector3.up);

        playerController = FindFirstObjectByType<ElmanGameDevTools.PlayerSystem.PlayerController>();
        if (playerController != null) playerController.enabled = false;

        // ===== DUA CAMERA TOI DIEM MOC CAN =====
        Camera cam = Camera.main;
        Transform camHomeParent = null;
        Vector3 camHomePos = Vector3.zero;
        Quaternion camHomeRot = Quaternion.identity;

        if (cam != null && cameraBiteAnchor != null)
        {
            Transform ct = cam.transform;
            camHomeParent = ct.parent;
            camHomePos = ct.localPosition;
            camHomeRot = ct.localRotation;

            ct.SetParent(null, true);

            if (snapCameraInstantly)
            {
                ct.position = cameraBiteAnchor.position;
                ct.rotation = cameraBiteAnchor.rotation;
            }
            else
            {
                float f = 0f;
                while (f < 1f)
                {
                    f += Time.deltaTime * cameraFlySpeed;
                    ct.position = Vector3.Lerp(ct.position, cameraBiteAnchor.position, Time.deltaTime * cameraFlySpeed);
                    ct.rotation = Quaternion.Slerp(ct.rotation, cameraBiteAnchor.rotation, Time.deltaTime * cameraFlySpeed);
                    yield return null;
                }
            }
        }

        if (animator != null && !string.IsNullOrEmpty(attackParam))
            animator.SetTrigger(attackParam);

        if (attackSound != null)
        {
            if (scareAudioSource != null)
                scareAudioSource.PlayOneShot(attackSound, attackVolume);
            else
                AudioSource.PlayClipAtPoint(attackSound, player.position, Mathf.Clamp01(attackVolume));
        }

        // Giu camera bam theo diem moc trong luc can
        float t = 0f;
        while (t < attackAnimTime)
        {
            t += Time.deltaTime;
            if (cam != null && cameraBiteAnchor != null)
            {
                cam.transform.position = cameraBiteAnchor.position;
                cam.transform.rotation = cameraBiteAnchor.rotation;
            }
            yield return null;
        }

        yield return StartCoroutine(FadeBlack(0f, 1f, 0.4f));
        yield return new WaitForSeconds(blackScreenTime);

        // ===== TRA CAMERA VE PLAYER =====
        if (cam != null && camHomeParent != null)
        {
            Transform ct = cam.transform;
            ct.SetParent(camHomeParent, false);
            ct.localPosition = camHomePos;
            ct.localRotation = camHomeRot;
        }

        RespawnPlayer();

        // Quai quay ve tuan tiep
        currentWaypoint = FindNearestWaypointIndex(transform.position);
        state = State.Patrol;
        waitTimer = 0f;
        chaseTimer = 0f;
        cooldownTimer = chaseCooldown;

        yield return StartCoroutine(FadeBlack(1f, 0f, 0.6f));

        if (playerController != null) playerController.enabled = true;

        isCatching = false;

        Debug.Log("[MonsterAI] Player hoi sinh. Quai quay ve tuan tra.");
    }

    private void RespawnPlayer()
    {
        if (respawnPoints == null || respawnPoints.Length == 0)
        {
            Debug.LogWarning("[MonsterAI] Chua gan Respawn Points!");
            return;
        }

        Transform best = null;
        float bestDist = float.MaxValue;

        foreach (Transform rp in respawnPoints)
        {
            if (rp == null) continue;
            float d = FlatDistance(player.position, rp.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = rp;
            }
        }

        if (best == null) return;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = best.position;
        player.rotation = best.rotation;

        if (cc != null) cc.enabled = true;

        Debug.Log("[MonsterAI] Da dua Player ve checkpoint: " + best.name);
    }

    private IEnumerator FadeBlack(float from, float to, float time)
    {
        if (blackOverlay == null) yield break;

        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            SetBlackAlpha(Mathf.Lerp(from, to, t / time));
            yield return null;
        }
        SetBlackAlpha(to);
    }

    private void SetBlackAlpha(float a)
    {
        if (blackOverlay == null) return;
        Color c = blackOverlay.color;
        c.a = a;
        blackOverlay.color = c;
    }

    // ===== TIEN ICH =====
    private void SetAnimSpeed(float speed)
    {
        if (animator != null && !string.IsNullOrEmpty(speedParam))
            animator.SetFloat(speedParam, speed);
    }

    private static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = new Color(1f, 0.6f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, loseRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        if (waypoints != null && waypoints.Length > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);

                int next = (i + 1) % waypoints.Length;
                if (waypoints[next] != null)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
            }
        }

        if (respawnPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform rp in respawnPoints)
                if (rp != null) Gizmos.DrawWireSphere(rp.position, 0.4f);
        }

        if (cameraBiteAnchor != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(cameraBiteAnchor.position, 0.15f);
            Gizmos.DrawRay(cameraBiteAnchor.position, cameraBiteAnchor.forward * 1f);
        }
    }
}