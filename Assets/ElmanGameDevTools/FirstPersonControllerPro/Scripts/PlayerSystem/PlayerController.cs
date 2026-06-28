using UnityEngine;
using UnityEngine.InputSystem; // Sử dụng thư viện Input mới để sửa hoàn toàn lỗi crash game

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
        
        // CÁC BIẾN LOGIC KHÓA CAMERA ĐÃ ĐƯỢC THÊM VÀO ĐÂY
        private bool _isCameraLocked = false; 
        private float _currentTiltState = 0f;

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

        private void UpdateMovementState()
        {
            // Nhận diện trạng thái giữ Shift (Chạy nhanh) từ phần cứng mới
            bool wantsToRun = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed && GetMoveInput().y > 0.1f;

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
                Vector2 input = GetMoveInput();
                float moveMag = input.magnitude;

                // Nếu đang trạng thái chạy nhanh (Running), ta nhân đôi Magnitude để Blend Tree chuyển sang hoạt ảnh Run
                float targetMag = moveMag;
                if (_currentMovementState == MovementState.Running)
                {
                    targetMag *= 2f; // Ép giá trị lên cao để kích hoạt hoạt ảnh chạy nhanh công nghiệp của Invector
                }

                // Truyền mượt mà các giá trị vào Animator (Dùng thêm Lerp/Damp để chuyển trạng thái mượt hơn)
                anim.SetFloat("InputHoriz", input.x, 0.1f, Time.deltaTime);
                anim.SetFloat("InputVert", input.y, 0.1f, Time.deltaTime);
                anim.SetFloat("InputMag", targetMag, 0.1f, Time.deltaTime);
                
                // Đồng bộ các biến Bool (Lưu ý chữ I viết hoa trong "IsMoving")
                anim.SetBool("IsMoving", moveMag > 0.1f);
                anim.SetBool("IsGrounded", _isGrounded);
                anim.SetBool("IsSprinting", _currentMovementState == MovementState.Running);
            }
        }

        private void HandleMovement()
        {
            Vector2 input = GetMoveInput();
            Vector3 moveDirection = transform.right * input.x + transform.forward * input.y;
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
            float prevHeight = controller.height;
            controller.height = Mathf.Lerp(controller.height, _targetHeight, Time.deltaTime * (1f / crouchSmoothTime));

            if (_isGrounded)
            {
                float heightDiff = controller.height - prevHeight;
                if (heightDiff > 0) controller.Move(Vector3.up * heightDiff);
            }

            float currentRelativeHeight = _cameraBaseHeight * (controller.height / _originalHeight);
            Vector3 camPos = playerCamera.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, currentRelativeHeight, Time.deltaTime * (1f / crouchSmoothTime));
            playerCamera.localPosition = camPos;
        }

        private void HandleCameraControl()
        {
            // === LOGIC KHÓA CAMERA QUYẾT ĐỊNH ĐÃ ĐƯỢC TÍCH HỢP ===
            if (_isCameraLocked)
            {
                // Cưỡng ép giữ nguyên góc xoay hiện tại, chặn đứng chuột can thiệp
                transform.rotation = Quaternion.Euler(0f, _currentYaw, 0f);
                if (playerCamera != null)
                {
                    playerCamera.localRotation = Quaternion.Euler(_currentPitch, 0f, _currentTilt);
                }
                return; // Thoát hàm luôn, đóng băng chuột
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

            float keyboardTilt = -GetMoveInput().x * tiltAmount;
            float mouseTilt = -_smoothInputX * turnTiltAmount;
            float targetTiltTotal = keyboardTilt + mouseTilt;

            if (_currentMovementState == MovementState.Running) targetTiltTotal *= runTiltMultiplier;
            if (_isCrouching) targetTiltTotal *= crouchTiltMultiplier;

            targetTiltTotal = Mathf.Clamp(targetTiltTotal, -maxTotalTilt, maxTotalTilt);
            _currentTilt = Mathf.Lerp(_currentTilt, targetTiltTotal, Time.deltaTime * tiltSmoothness);
        }

        private void HandleFovChange()
        {
            if (!enableRunFov || playerCamera.GetComponent<Camera>() == null) return;
            bool isActuallyRunning = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed && GetMoveInput().y > 0.1f;
            Camera cam = playerCamera.GetComponent<Camera>();
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, isActuallyRunning ? runFov : normalFov, Time.deltaTime * fovChangeSpeed);
        }

        private void HandleHeadBob()
        {
            float moveMag = GetMoveInput().magnitude;
            float currentCamH = _cameraBaseHeight * (controller.height / _originalHeight);

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

        // Hàm đọc phím mô phỏng GetAxis bằng cụm phím di chuyển W, A, S, D mới
        private Vector2 GetMoveInput()
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
        // CÁC HÀM TIỆN ÍCH QUẢN LÝ KHÓA CAM (MỚI NÂNG CẤP)
        // ==========================================

        /// <summary>
        /// Ép Camera phải quay ngay lập tức về một góc mong muốn và KHÓA CHẶT (Dùng cho Jumpscare/Cutscene)
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
        /// Chỉ khóa cứng camera tại hướng hiện tại mà không xoay đi đâu (Dùng khi chơi Mini-game/Puzzle)
        /// </summary>
        public void LockCameraOnly()
        {
            _isCameraLocked = true;
        }

        /// <summary>
        /// Hàm dùng để mở khóa góc quay chuột, trả lại tự do
        /// </summary>
        public void UnlockCamera()
        {
            _isCameraLocked = false;
        }
    }
}