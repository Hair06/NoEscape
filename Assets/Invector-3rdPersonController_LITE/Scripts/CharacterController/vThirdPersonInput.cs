using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace Invector.vCharacterController
{
    public class vThirdPersonInput : MonoBehaviour
    {
        #region Variables       

        [Header("Controller Input")]
        public string horizontalInput = "Horizontal";
        public string verticallInput = "Vertical";
        public KeyCode jumpInput = KeyCode.Space;
        public KeyCode strafeInput = KeyCode.Tab;
        public KeyCode sprintInput = KeyCode.LeftShift;

        [Header("Camera Input")]
        public string rotateCameraXInput = "Mouse X";
        public string rotateCameraYInput = "Mouse Y";

        [HideInInspector] public vThirdPersonController cc;
        [HideInInspector] public vThirdPersonCamera tpCamera;
        [HideInInspector] public Camera cameraMain;

        #endregion

        protected virtual void Start()
        {
            InitilizeController();
            InitializeTpCamera();
        }

        protected virtual void FixedUpdate()
        {
            cc.UpdateMotor();               // updates the ThirdPersonMotor methods
            cc.ControlLocomotionType();     // handle the controller locomotion type and movespeed
            cc.ControlRotationType();       // handle the controller rotation type
        }

        protected virtual void Update()
        {
            InputHandle();                  // update the input methods
            cc.UpdateAnimator();            // updates the Animator Parameters
        }

        public virtual void OnAnimatorMove()
        {
            cc.ControlAnimatorRootMotion(); // handle root motion animations 
        }

        #region Basic Locomotion Inputs

        protected virtual void InitilizeController()
        {
            cc = GetComponent<vThirdPersonController>();

            if (cc != null)
                cc.Init();
        }

        protected virtual void InitializeTpCamera()
        {
            if (tpCamera == null)
            {
                tpCamera = FindFirstObjectByType<vThirdPersonCamera>();
                if (tpCamera == null)
                    return;
                if (tpCamera)
                {
                    tpCamera.SetMainTarget(this.transform);
                    tpCamera.Init();
                }
            }
        }

        protected virtual void InputHandle()
        {
            MoveInput();
            CameraInput();
            SprintInput();
            StrafeInput();
            JumpInput();
        }

        public virtual void MoveInput()
        {
            cc.input.x = GameInputBridge.GetAxis(horizontalInput);
            cc.input.z = GameInputBridge.GetAxis(verticallInput);
        }

        protected virtual void CameraInput()
        {
            if (!cameraMain)
            {
                if (!Camera.main) Debug.Log("Missing a Camera with the tag MainCamera, please add one.");
                else
                {
                    cameraMain = Camera.main;
                    cc.rotateTarget = cameraMain.transform;
                }
            }

            if (cameraMain)
            {
                cc.UpdateMoveDirection(cameraMain.transform);
            }

            if (tpCamera == null)
                return;

            var Y = GameInputBridge.GetAxis(rotateCameraYInput);
            var X = GameInputBridge.GetAxis(rotateCameraXInput);

            tpCamera.RotateCamera(X, Y);
        }

        protected virtual void StrafeInput()
        {
            if (GameInputBridge.GetKeyDown(strafeInput))
                cc.Strafe();
        }

        protected virtual void SprintInput()
        {
            if (GameInputBridge.GetKeyDown(sprintInput))
                cc.Sprint(true);
            else if (GameInputBridge.GetKeyUp(sprintInput))
                cc.Sprint(false);
        }

        /// <summary>
        /// Conditions to trigger the Jump animation & behavior
        /// </summary>
        /// <returns></returns>
        protected virtual bool JumpConditions()
        {
            return cc.isGrounded && cc.GroundAngle() < cc.slopeLimit && !cc.isJumping && !cc.stopMove;
        }

        /// <summary>
        /// Input to trigger the Jump 
        /// </summary>
        protected virtual void JumpInput()
        {
            if (GameInputBridge.GetKeyDown(jumpInput) && JumpConditions())
                cc.Jump();
        }

        #endregion       
    }
}

public static class GameInputBridge
{
    private const float MouseDeltaScale = 0.05f;

    public static float GetAxis(string axisName)
    {
#if ENABLE_INPUT_SYSTEM
        if (string.Equals(axisName, "Horizontal", System.StringComparison.OrdinalIgnoreCase))
            return ReadHorizontalAxis();

        if (string.Equals(axisName, "Vertical", System.StringComparison.OrdinalIgnoreCase))
            return ReadVerticalAxis();

        if (string.Equals(axisName, "Mouse X", System.StringComparison.OrdinalIgnoreCase))
            return ReadMouseX();

        if (string.Equals(axisName, "Mouse Y", System.StringComparison.OrdinalIgnoreCase))
            return ReadMouseY();

        return 0f;
#else
        return Input.GetAxis(axisName);
#endif
    }

    public static bool GetKeyDown(KeyCode keyCode)
    {
#if ENABLE_INPUT_SYSTEM
        KeyControl key = GetKeyboardKey(keyCode);
        return key != null && key.wasPressedThisFrame;
#else
        return Input.GetKeyDown(keyCode);
#endif
    }

    public static bool GetKeyUp(KeyCode keyCode)
    {
#if ENABLE_INPUT_SYSTEM
        KeyControl key = GetKeyboardKey(keyCode);
        return key != null && key.wasReleasedThisFrame;
#else
        return Input.GetKeyUp(keyCode);
#endif
    }

    public static bool GetMouseButtonDown(int button)
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current == null)
            return false;

        return button switch
        {
            0 => Mouse.current.leftButton.wasPressedThisFrame,
            1 => Mouse.current.rightButton.wasPressedThisFrame,
            2 => Mouse.current.middleButton.wasPressedThisFrame,
            _ => false
        };
#else
        return Input.GetMouseButtonDown(button);
#endif
    }

#if ENABLE_INPUT_SYSTEM
    private static float ReadHorizontalAxis()
    {
        float value = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                value -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                value += 1f;
        }

        if (Gamepad.current != null)
            value += Gamepad.current.leftStick.x.ReadValue();

        return Mathf.Clamp(value, -1f, 1f);
    }

    private static float ReadVerticalAxis()
    {
        float value = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                value -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                value += 1f;
        }

        if (Gamepad.current != null)
            value += Gamepad.current.leftStick.y.ReadValue();

        return Mathf.Clamp(value, -1f, 1f);
    }

    private static float ReadMouseX()
    {
        float value = Mouse.current != null ? Mouse.current.delta.x.ReadValue() * MouseDeltaScale : 0f;

        if (Gamepad.current != null)
            value += Gamepad.current.rightStick.x.ReadValue();

        return value;
    }

    private static float ReadMouseY()
    {
        float value = Mouse.current != null ? Mouse.current.delta.y.ReadValue() * MouseDeltaScale : 0f;

        if (Gamepad.current != null)
            value += Gamepad.current.rightStick.y.ReadValue();

        return value;
    }

    private static KeyControl GetKeyboardKey(KeyCode keyCode)
    {
        if (Keyboard.current == null)
            return null;

        return keyCode switch
        {
            KeyCode.Space => Keyboard.current.spaceKey,
            KeyCode.Tab => Keyboard.current.tabKey,
            KeyCode.LeftShift => Keyboard.current.leftShiftKey,
            KeyCode.RightShift => Keyboard.current.rightShiftKey,
            KeyCode.E => Keyboard.current.eKey,
            KeyCode.F => Keyboard.current.fKey,
            KeyCode.J => Keyboard.current.jKey,
            KeyCode.W => Keyboard.current.wKey,
            KeyCode.A => Keyboard.current.aKey,
            KeyCode.S => Keyboard.current.sKey,
            KeyCode.D => Keyboard.current.dKey,
            KeyCode.UpArrow => Keyboard.current.upArrowKey,
            KeyCode.DownArrow => Keyboard.current.downArrowKey,
            KeyCode.LeftArrow => Keyboard.current.leftArrowKey,
            KeyCode.RightArrow => Keyboard.current.rightArrowKey,
            KeyCode.Escape => Keyboard.current.escapeKey,
            KeyCode.Return => Keyboard.current.enterKey,
            KeyCode.Backspace => Keyboard.current.backspaceKey,
            _ => null
        };
    }
#endif
}
