using UnityEngine;
using UnityEngine.InputSystem; // Dùng Input System mới để tương thích tốt hơn với các asset/animator khác (vd Invector)

namespace ElmanGameDevTools.PlayerSystem
{
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("Elman Game Dev Tools/Player System/Player Controller")]
    public class PlayerController : MonoBehaviour
    {
        [Header("REFERENCES")]
        public CharacterController controller;
        public Transform playerCamera;
        public Animator anim; // Linh kiện điều khiển hiệu ứng cử động nhân vật

        [Header("MOVEMENT SETTINGS")]
        public float speed = 6f;
        public float runSpeed = 9f;
        public float jumpHeight = 1.2f;
        public float gravity = -25f;
        public float sensitivity = 0.1f; // Độ nhạy chuột tối ưu cho hệ thống mới

        [Header("CAMERA SETTINGS")]
        public float maxLookUpAngle = 90f;
        public float maxLookDownAngle = -90f;
        public bool enableHeadBob = true;
        [Range(0.01f, 0.15f)] public float bobAmountX = 0.04f;
        [Range(0.01f, 0.15f)] public float bobAmountY = 0.05f;
        public float walkBobFrequency = 12f;
        public float runBobFrequency = 16f;
        public float crouchBobFrequency = 8f;
        public float bobSmoothness = 10f;

        [Header("CAMERA INERTIA & WEIGHT")]
        [Range(1f, 30f)] public float cameraWeight = 12f;
        private float _targetYaw;
        private float _targetPitch;
        private float _currentYaw;
        private float _currentPitch;
        private float _smoothInputX;

        [Header("CAMERA EFFECTS")]
        public bool enableCameraTilt = true;
        public float tiltAmount = 2f;
        public float tiltSmoothness = 8f;
        public float runTiltMultiplier = 1.2f;
        public float crouchTiltMultiplier = 0.5f;
        [Space]
        public float turnTiltAmount = 1.5f;
        public float maxTotalTilt = 5f;

        [Header("CROUCH SETTINGS")]
        public float crouchHeight = 1.2f;
        public float crouchSmoothTime = 0.1f;
        [Tooltip("Cộng thêm độ cao camera khi crouch nếu thấy camera quá thấp so với capsule. Tăng giá trị này để nâng camera lên.")]
        public float crouchCameraHeightBoost = 0.15f;

        [Header("FOV SETTINGS")]
        public bool enableRunFov = true;
        public float normalFov = 60f;
        public float runFov = 70f;
        public float fovChangeSpeed = 8f;

        [Header("STANDING DETECTION & GROUND CHECK")]
        public GameObject standingHeightMarker;
        public float standingCheckRadius = 0.2f;
        public LayerMask obstacleLayerMask = ~0;
        public float minStandingClearance = 0.01f;
        public LayerMask groundLayer = 1;
        public float groundCheckDistance = 0.5f;

        [Header("PHYSICS SAFETY")]
        [Tooltip("Giới hạn vận tốc rơi tối đa để tránh xuyên sàn (tunneling) khi rơi từ độ cao lớn")]
        public float maxFallSpeed = -40f;

        private Vector3 _velocity;
        private float _currentTilt;
        private float _timer;
        private float _originalHeight;
        private float _targetHeight;
        private float _currentMovementSpeed;
        private float _cameraBaseHeight;
        private float _markerHeightOffset;

        private bool _isGrounded;
        private bool _isCrouching;
        private bool _hasJumped;

        // BIẾN QUẢN LÝ KHÓA CAMERA KHI CHƠI MINI-GAME VÀ JUMPSCARE
        private bool _isCameraLocked = false;

        // Cache Camera component thay vì gọi GetComponent mỗi frame
        private Camera _playerCameraComponent;

        // Input di chuyển được lấy 1 lần duy nhất mỗi Update() rồi truyền xuống các hàm khác
        private Vector2 _moveInput;
        private bool _isRunKeyHeld;

        public enum MovementState { Walking, Running, Crouching, Jumping }
        private MovementState _currentMovementState = MovementState.Walking;

        // Các thuộc tính hỗ trợ đồng bộ mượt mà với script âm thanh bước chân (PlayerMusic)
        public bool IsGrounded => _isGrounded;
        public bool IsCrouching => _isCrouching;
        public MovementState CurrentState => _currentMovementState;

        private void Start()
        {
            if (controller == null) controller = GetComponent<CharacterController>();
            if (anim == null) anim = GetComponentInChildren<Animator>(); // Tự động dò tìm Animator ở mô hình con nhân vật

            if (playerCamera != null)
                _playerCameraComponent = playerCamera.GetComponent<Camera>();

            Cursor.lockState = CursorLockMode.Locked;
            _originalHeight = controller.height;
            _targetHeight = _originalHeight;
            _cameraBaseHeight = playerCamera.localPosition.y;

            _targetYaw = transform.eulerAngles.y;
            _targetPitch = playerCamera.localEulerAngles.x;
            _currentYaw = _targetYaw;
            _currentPitch = _targetPitch;

            if (standingHeightMarker != null)
                _markerHeightOffset = standingHeightMarker.transform.position.y - transform.position.y;
        }

        private void Update()
        {
            // Đọc input di chuyển 1 lần duy nhất mỗi frame, dùng chung cho toàn bộ các hàm bên dưới
            _moveInput = ReadMoveInput();
            _isRunKeyHeld = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

            CheckGroundStatus();
            HandleCrouchLogic();
            UpdateMovementState();
            HandleMovement();
            HandleHeightAndCamera();
            HandleCameraControl();
            HandleCameraTilt();
            HandleFovChange();

            if (enableHeadBob) HandleHeadBob();
        }

        private void CheckGroundStatus()
        {
            Vector3 origin = transform.position + Vector3.up * controller.radius;
            bool groundHit = Physics.SphereCast(origin, controller.radius * 0.8f, Vector3.down, out _, groundCheckDistance, groundLayer);
            _isGrounded = groundHit || controller.isGrounded;

            if (_isGrounded && _velocity.y < 0)
            {
                _hasJumped = false;
                _velocity.y = -5f;
            }
        }

        private bool IsRunning()
        {
            // Nguồn duy nhất xác định "đang chạy nhanh", dùng chung cho animation, tốc độ và FOV
            return _isRunKeyHeld && _moveInput.y > 0.1f;
        }

        private void UpdateMovementState()
        {
            bool wantsToRun = IsRunning();

            if (!_isGrounded)
            {
                _currentMovementState = MovementState.Jumping;
                _currentMovementSpeed = wantsToRun ? runSpeed : speed;
            }
            else if (_isCrouching)
            {
                _currentMovementState = MovementState.Crouching;
                _currentMovementSpeed = speed * 0.5f;
            }
            else
            {
                _currentMovementState = wantsToRun ? MovementState.Running : MovementState.Walking;
                _currentMovementSpeed = wantsToRun ? runSpeed : speed;
            }

            // KÍCH HOẠT ANIMATION: Đồng bộ hoàn toàn với các Parameter của Invector Animator
            if (anim != null)
            {
                float moveMag = _moveInput.magnitude;

                // Nếu đang trạng thái chạy nhanh (Running), ta nhân đôi Magnitude để Blend Tree chuyển sang hoạt ảnh Run
                float targetMag = moveMag;
                if (_currentMovementState == MovementState.Running)
                {
                    targetMag *= 2f; // Ép giá trị lên cao để kích hoạt hoạt ảnh chạy nhanh công nghiệp của Invector
                }

                // Truyền mượt mà các giá trị vào Animator (Dùng thêm Lerp/Damp để chuyển trạng thái mượt hơn)
                anim.SetFloat("InputHoriz", _moveInput.x, 0.1f, Time.deltaTime);
                anim.SetFloat("InputVert", _moveInput.y, 0.1f, Time.deltaTime);
                anim.SetFloat("InputMag", targetMag, 0.1f, Time.deltaTime);

                // Đồng bộ các biến Bool (Lưu ý chữ I viết hoa trong "IsMoving")
                anim.SetBool("IsMoving", moveMag > 0.1f);
                anim.SetBool("IsGrounded", _isGrounded);
                anim.SetBool("IsSprinting", _currentMovementState == MovementState.Running);
            }
        }

        private void HandleMovement()
        {
            Vector3 moveDirection = transform.right * _moveInput.x + transform.forward * _moveInput.y;
            if (moveDirection.magnitude > 1f) moveDirection.Normalize();

            // Nhận diện phím Space để nhảy bằng hệ thống Input mới
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && _isGrounded && !_isCrouching)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                _hasJumped = true;
                _isGrounded = false;
            }

            if (standingHeightMarker != null)
                standingHeightMarker.transform.position = new Vector3(transform.position.x, transform.position.y + _markerHeightOffset, transform.position.z);

            controller.Move(moveDirection * _currentMovementSpeed * Time.deltaTime);
            _velocity.y += gravity * Time.deltaTime;

            // Giới hạn vận tốc rơi tối đa để tránh xuyên sàn khi rơi từ độ cao lớn
            if (_velocity.y < maxFallSpeed) _velocity.y = maxFallSpeed;

            controller.Move(_velocity * Time.deltaTime);
        }

        private void HandleCrouchLogic()
        {
            // Nhận diện phím Ctrl để ngồi bằng hệ thống Input mới
            bool crouchPressed = Keyboard.current != null && Keyboard.current.leftCtrlKey.isPressed;
            _isCrouching = crouchPressed || !CanStandUp();
            _targetHeight = _isCrouching ? crouchHeight : _originalHeight;
        }

        private void HandleHeightAndCamera()
        {
            controller.height = Mathf.Lerp(controller.height, _targetHeight, Time.deltaTime * (1f / crouchSmoothTime));

            // QUAN TRỌNG: neo đáy capsule tại gốc transform bằng cách luôn set center.y = height/2.
            // Nếu không làm việc này, khi height co lại (crouch) mà center không đổi thì đáy capsule
            // sẽ tự động nâng lên khỏi mặt đất -> player/camera bị "nổi" lên, gây giật/lệch camera lúc ngồi.
            Vector3 center = controller.center;
            center.y = controller.height / 2f;
            controller.center = center;

            float currentRelativeHeight = GetCameraRelativeHeight();
            Vector3 camPos = playerCamera.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, currentRelativeHeight, Time.deltaTime * (1f / crouchSmoothTime));
            playerCamera.localPosition = camPos;
        }

        /// <summary>
        /// Tính chiều cao camera tương ứng với chiều cao capsule hiện tại.
        /// Khi crouch, cộng thêm crouchCameraHeightBoost để camera không bị hạ quá thấp,
        /// đồng thời clamp để camera không vượt quá đỉnh capsule hiện tại.
        /// </summary>
        private float GetCameraRelativeHeight()
        {
            float proportional = _cameraBaseHeight * (controller.height / _originalHeight);

            if (_isCrouching)
            {
                proportional += crouchCameraHeightBoost;
                float maxAllowed = controller.height - 0.1f; // chừa khoảng hở nhỏ để camera không lú ra khỏi đỉnh đầu
                proportional = Mathf.Min(proportional, maxAllowed);
            }

            return proportional;
        }

        private void HandleCameraControl()
        {
            // === LOGIC KHÓA CAMERA NẾU ĐANG CHƠI MINI-GAME ===
            if (_isCameraLocked)
            {
                // Cưỡng ép đóng băng góc quay hiện tại, chặn đứng chuột xoay camera chính
                transform.rotation = Quaternion.Euler(0f, _currentYaw, 0f);
                if (playerCamera != null)
                {
                    playerCamera.localRotation = Quaternion.Euler(_currentPitch, 0f, _currentTilt);
                }
                return; // Thoát ra luôn, không đọc di chuột dưới nữa
            }

            // Đọc tín hiệu di chuyển của chuột qua Input mới độc lập, không cần kéo thả Component bên ngoài
            Vector2 mouseDelta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
            float mouseX = mouseDelta.x * sensitivity;
            float mouseY = mouseDelta.y * sensitivity;

            _smoothInputX = Mathf.Lerp(_smoothInputX, mouseX, Time.deltaTime * cameraWeight);

            _targetYaw += mouseX;
            _targetPitch -= mouseY;
            _targetPitch = Mathf.Clamp(_targetPitch, maxLookDownAngle, maxLookUpAngle);

            float smoothFactor = Mathf.Clamp01(Time.deltaTime * cameraWeight);
            _currentYaw = Mathf.Lerp(_currentYaw, _targetYaw, smoothFactor);
            _currentPitch = Mathf.Lerp(_currentPitch, _targetPitch, smoothFactor);

            transform.rotation = Quaternion.Euler(0f, _currentYaw, 0f);
            playerCamera.localRotation = Quaternion.Euler(_currentPitch, 0f, _currentTilt);
        }

        private void HandleCameraTilt()
        {
            if (!enableCameraTilt) { _currentTilt = 0; return; }

            float keyboardTilt = -_moveInput.x * tiltAmount;
            float mouseTilt = -_smoothInputX * turnTiltAmount;
            float targetTiltTotal = keyboardTilt + mouseTilt;

            if (_currentMovementState == MovementState.Running) targetTiltTotal *= runTiltMultiplier;
            if (_isCrouching) targetTiltTotal *= crouchTiltMultiplier;

            targetTiltTotal = Mathf.Clamp(targetTiltTotal, -maxTotalTilt, maxTotalTilt);
            _currentTilt = Mathf.Lerp(_currentTilt, targetTiltTotal, Time.deltaTime * tiltSmoothness);
        }

        private void HandleFovChange()
        {
            if (!enableRunFov || _playerCameraComponent == null) return;
            bool isActuallyRunning = _currentMovementState == MovementState.Running;
            _playerCameraComponent.fieldOfView = Mathf.Lerp(_playerCameraComponent.fieldOfView, isActuallyRunning ? runFov : normalFov, Time.deltaTime * fovChangeSpeed);
        }

        private void HandleHeadBob()
        {
            float moveMag = _moveInput.magnitude;
            float currentCamH = GetCameraRelativeHeight();

            if (!_isGrounded || moveMag <= 0.1f)
            {
                _timer = 0;
                playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, new Vector3(0, currentCamH, 0), Time.deltaTime * bobSmoothness);
                return;
            }

            float freq = (_currentMovementState == MovementState.Running) ? runBobFrequency : (_isCrouching ? crouchBobFrequency : walkBobFrequency);
            _timer += Time.deltaTime * freq;

            Vector3 newPos = new Vector3(
                Mathf.Cos(_timer * 0.5f) * bobAmountX,
                currentCamH + Mathf.Sin(_timer) * bobAmountY,
                0
            );
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, newPos, Time.deltaTime * bobSmoothness);
        }

        private Vector2 ReadMoveInput()
        {
            if (Keyboard.current == null) return Vector2.zero;
            float x = 0f;
            float y = 0f;
            if (Keyboard.current.dKey.isPressed) x += 1f;
            if (Keyboard.current.aKey.isPressed) x -= 1f;
            if (Keyboard.current.wKey.isPressed) y += 1f;
            if (Keyboard.current.sKey.isPressed) y -= 1f;
            return new Vector2(x, y);
        }

        public bool CanStandUp()
        {
            if (standingHeightMarker == null) return true;
            Collider[] hits = Physics.OverlapSphere(standingHeightMarker.transform.position, standingCheckRadius, obstacleLayerMask);
            foreach (Collider col in hits)
            {
                if (col.transform.IsChildOf(transform) || col.transform == transform || col.isTrigger) continue;
                if (col.bounds.min.y < standingHeightMarker.transform.position.y + minStandingClearance) return false;
            }
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            if (standingHeightMarker != null)
            {
                Gizmos.color = CanStandUp() ? Color.green : Color.red;
                Gizmos.DrawWireSphere(standingHeightMarker.transform.position, standingCheckRadius);
            }
        }

        // ==========================================
        // CÁC HÀM QUẢN LÝ KHÓA/MỞ KHÓA CAMERA
        // ==========================================

        /// <summary>
        /// Ép Camera xoay ngay lập tức về hướng chỉ định và khóa cứng (Dùng cho Jumpscare/Cutscene)
        /// </summary>
        public void ForceLookAtDirection(Quaternion targetWorldRotation)
        {
            _isCameraLocked = true;

            Vector3 euler = targetWorldRotation.eulerAngles;
            _targetYaw = euler.y;
            _currentYaw = euler.y;

            float pitch = euler.x;
            if (pitch > 180f) pitch -= 360f;

            _targetPitch = Mathf.Clamp(pitch, maxLookDownAngle, maxLookUpAngle);
            _currentPitch = _targetPitch;

            transform.rotation = Quaternion.Euler(0f, _currentYaw, 0f);
            if (playerCamera != null)
            {
                playerCamera.localRotation = Quaternion.Euler(_currentPitch, 0f, _currentTilt);
            }
        }

        /// <summary>
        /// Chỉ khóa cứng góc nhìn camera hiện tại (Dùng khi chơi Mini-game giải đố)
        /// </summary>
        public void LockCameraOnly()
        {
            _isCameraLocked = true;
        }

        /// <summary>
        /// Mở khóa camera, trả lại quyền xoay chuột tự do cho người chơi
        /// </summary>
        public void UnlockCamera()
        {
            _isCameraLocked = false;
        }
    }
}