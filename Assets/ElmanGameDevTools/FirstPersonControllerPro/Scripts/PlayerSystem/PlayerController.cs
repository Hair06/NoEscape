using UnityEngine;
using UnityEngine.InputSystem;

namespace ElmanGameDevTools.PlayerSystem
{
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("Elman Game Dev Tools/Player System/Player Controller")]
    public class PlayerController : MonoBehaviour
    {
        [Header("REFERENCES")]
        public CharacterController controller;
        public Transform playerCamera;
        public Animator anim;

        [Header("MOVEMENT SETTINGS")]
        public float speed = 6f;
        public float runSpeed = 9f;
        public float jumpHeight = 1.2f;
        public float gravity = -25f;
        public float sensitivity = 0.1f;

        [Header("CAMERA SETTINGS")]
        public float maxLookUpAngle = 90f;
        public float maxLookDownAngle = -90f;
        public bool enableHeadBob = true;
        [Range(0.01f, 0.15f)] public float bobAmountX = 0.04f;
        [Range(0.01f, 0.15f)] public float bobAmountY = 0.05f;
        public float walkBobFrequency = 12f;
        public float runBobFrequency = 16f;
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
        [Space]
        public float turnTiltAmount = 1.5f;
        public float maxTotalTilt = 5f;

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
        public float maxFallSpeed = -40f;

        private Vector3 _velocity;
        private float _currentTilt;
        private float _timer;
        private float _originalHeight;
        private float _currentMovementSpeed;
        private float _cameraBaseHeight;
        private float _markerHeightOffset;

        private bool _isGrounded;
        private bool _hasJumped;
        private bool _isCameraLocked = false;

        private Camera _playerCameraComponent;
        private Vector2 _moveInput;
        private bool _isRunKeyHeld;

        public enum MovementState { Walking, Running, Jumping }
        private MovementState _currentMovementState = MovementState.Walking;

        public bool IsGrounded => _isGrounded;
        public MovementState CurrentState => _currentMovementState;

        private void Start()
        {
            if (controller == null) controller = GetComponent<CharacterController>();
            if (anim == null) anim = GetComponentInChildren<Animator>();

            if (playerCamera != null)
                _playerCameraComponent = playerCamera.GetComponent<Camera>();

            Cursor.lockState = CursorLockMode.Locked;
            _originalHeight = controller.height;
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
            _moveInput = ReadMoveInput();
            _isRunKeyHeld = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

            CheckGroundStatus();
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
            bool groundHit = Physics.SphereCast(
                origin, controller.radius * 0.8f,
                Vector3.down, out _,
                groundCheckDistance, groundLayer);

            _isGrounded = groundHit || controller.isGrounded;

            if (_isGrounded && _velocity.y < 0)
            {
                _hasJumped = false;
                _velocity.y = -5f;
            }
        }

        private bool IsRunning()
        {
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
            else
            {
                _currentMovementState = wantsToRun ? MovementState.Running : MovementState.Walking;
                _currentMovementSpeed = wantsToRun ? runSpeed : speed;
            }

            if (anim != null)
            {
                float moveMag = _moveInput.magnitude;
                float targetMag = moveMag;

                if (_currentMovementState == MovementState.Running)
                    targetMag *= 2f;

                anim.SetFloat("InputHoriz", _moveInput.x, 0.1f, Time.deltaTime);
                anim.SetFloat("InputVert", _moveInput.y, 0.1f, Time.deltaTime);
                anim.SetFloat("InputMag", targetMag, 0.1f, Time.deltaTime);
                anim.SetBool("IsMoving", moveMag > 0.1f);
                anim.SetBool("IsGrounded", _isGrounded);
                anim.SetBool("IsSprinting", _currentMovementState == MovementState.Running);
            }
        }

        private void HandleMovement()
        {
            Vector3 moveDirection = transform.right * _moveInput.x + transform.forward * _moveInput.y;
            if (moveDirection.magnitude > 1f) moveDirection.Normalize();

            if (Keyboard.current != null &&
                Keyboard.current.spaceKey.wasPressedThisFrame &&
                _isGrounded)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                _hasJumped = true;
                _isGrounded = false;
            }

            if (standingHeightMarker != null)
                standingHeightMarker.transform.position = new Vector3(
                    transform.position.x,
                    transform.position.y + _markerHeightOffset,
                    transform.position.z);

            controller.Move(moveDirection * _currentMovementSpeed * Time.deltaTime);
            _velocity.y += gravity * Time.deltaTime;

            if (_velocity.y < maxFallSpeed)
                _velocity.y = maxFallSpeed;

            controller.Move(_velocity * Time.deltaTime);
        }

        private void HandleHeightAndCamera()
        {
            // Neo đáy capsule tại gốc transform
            Vector3 center = controller.center;
            center.y = controller.height / 2f;
            controller.center = center;

            Vector3 camPos = playerCamera.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, _cameraBaseHeight, Time.deltaTime * 10f);
            playerCamera.localPosition = camPos;
        }

        private void HandleCameraControl()
        {
            if (_isCameraLocked)
            {
                transform.rotation = Quaternion.Euler(0f, _currentYaw, 0f);
                if (playerCamera != null)
                    playerCamera.localRotation = Quaternion.Euler(_currentPitch, 0f, _currentTilt);
                return;
            }

            Vector2 mouseDelta = Mouse.current != null ?
                Mouse.current.delta.ReadValue() : Vector2.zero;

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

            if (_currentMovementState == MovementState.Running)
                targetTiltTotal *= runTiltMultiplier;

            targetTiltTotal = Mathf.Clamp(targetTiltTotal, -maxTotalTilt, maxTotalTilt);
            _currentTilt = Mathf.Lerp(_currentTilt, targetTiltTotal, Time.deltaTime * tiltSmoothness);
        }

        private void HandleFovChange()
        {
            if (!enableRunFov || _playerCameraComponent == null) return;
            bool isActuallyRunning = _currentMovementState == MovementState.Running;
            _playerCameraComponent.fieldOfView = Mathf.Lerp(
                _playerCameraComponent.fieldOfView,
                isActuallyRunning ? runFov : normalFov,
                Time.deltaTime * fovChangeSpeed);
        }

        private void HandleHeadBob()
        {
            float moveMag = _moveInput.magnitude;

            if (!_isGrounded || moveMag <= 0.1f)
            {
                _timer = 0;
                playerCamera.localPosition = Vector3.Lerp(
                    playerCamera.localPosition,
                    new Vector3(0, _cameraBaseHeight, 0),
                    Time.deltaTime * bobSmoothness);
                return;
            }

            float freq = (_currentMovementState == MovementState.Running) ?
                         runBobFrequency : walkBobFrequency;

            _timer += Time.deltaTime * freq;

            Vector3 newPos = new Vector3(
                Mathf.Cos(_timer * 0.5f) * bobAmountX,
                _cameraBaseHeight + Mathf.Sin(_timer) * bobAmountY,
                0
            );

            playerCamera.localPosition = Vector3.Lerp(
                playerCamera.localPosition,
                newPos,
                Time.deltaTime * bobSmoothness);
        }

        private Vector2 ReadMoveInput()
        {
            if (Keyboard.current == null) return Vector2.zero;
            float x = 0f, y = 0f;
            if (Keyboard.current.dKey.isPressed) x += 1f;
            if (Keyboard.current.aKey.isPressed) x -= 1f;
            if (Keyboard.current.wKey.isPressed) y += 1f;
            if (Keyboard.current.sKey.isPressed) y -= 1f;
            return new Vector2(x, y);
        }

        public bool CanStandUp()
        {
            if (standingHeightMarker == null) return true;
            Collider[] hits = Physics.OverlapSphere(
                standingHeightMarker.transform.position,
                standingCheckRadius,
                obstacleLayerMask);

            foreach (Collider col in hits)
            {
                if (col.transform.IsChildOf(transform) ||
                    col.transform == transform ||
                    col.isTrigger) continue;

                if (col.bounds.min.y < standingHeightMarker.transform.position.y - minStandingClearance ||
                    col.bounds.max.y > standingHeightMarker.transform.position.y + minStandingClearance)
                    return false;
            }
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw standing height check sphere
            if (standingHeightMarker != null)
            {
                bool canStandUp = CanStandUp();
                Gizmos.color = canStandUp ? Color.green : Color.red;
                Gizmos.DrawWireSphere(
                    standingHeightMarker.transform.position,
                    standingCheckRadius);
                
                // Draw a filled sphere at reduced opacity to show obstruction area
                Gizmos.color = new Color(canStandUp ? 0f : 1f, canStandUp ? 1f : 0f, 0f, 0.1f);
                DrawFilledSphere(standingHeightMarker.transform.position, standingCheckRadius, 8);
            }

            // Draw ground check sphere cast
            if (controller != null)
            {
                Vector3 origin = transform.position + Vector3.up * controller.radius;
                Gizmos.color = _isGrounded ? Color.cyan : Color.yellow;
                Gizmos.DrawWireSphere(origin, controller.radius * 0.8f);
                
                // Draw ground check ray
                Gizmos.color = _isGrounded ? Color.green : Color.red;
                Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
            }

            // Draw camera height indicator
            if (playerCamera != null)
            {
                Gizmos.color = Color.blue;
                Vector3 cameraPos = playerCamera.position;
                Gizmos.DrawLine(cameraPos + Vector3.left * 0.1f, cameraPos + Vector3.right * 0.1f);
                Gizmos.DrawLine(cameraPos + Vector3.back * 0.1f, cameraPos + Vector3.forward * 0.1f);
            }

            // Draw movement speed indicator
            if (controller != null)
            {
                Gizmos.color = _currentMovementState == MovementState.Running ? Color.red : Color.white;
                Vector3 speedIndicator = transform.position + transform.forward * (_currentMovementSpeed * 0.1f);
                Gizmos.DrawLine(transform.position, speedIndicator);
            }
        }

        // Helper method to draw filled sphere (approximation)
        private void DrawFilledSphere(Vector3 position, float radius, int segments)
        {
            float angleStep = 360f / segments;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep * Mathf.Deg2Rad;
                float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;
                
                Vector3 p1 = position + new Vector3(Mathf.Cos(angle1) * radius, 0, Mathf.Sin(angle1) * radius);
                Vector3 p2 = position + new Vector3(Mathf.Cos(angle2) * radius, 0, Mathf.Sin(angle2) * radius);
                Gizmos.DrawLine(p1, p2);
            }
        }

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
                playerCamera.localRotation = Quaternion.Euler(_currentPitch, 0f, _currentTilt);
        }

        public void LockCameraOnly()
        {
            _isCameraLocked = true;
        }

        public void UnlockCamera()
        {
            _isCameraLocked = false;
        }
    }
}