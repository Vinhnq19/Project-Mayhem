using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectMayhem.Player
{
    /// <summary>
    /// Debug script to test input system
    /// </summary>
    public class InputDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebug = true;
        [SerializeField] private float debugInterval = 0.5f;

        private float lastDebugTime = 0f;

        private void Update()
        {
            if (!enableDebug) return;

            if (Time.time - lastDebugTime >= debugInterval)
            {
                DebugInput();
                lastDebugTime = Time.time;
            }
        }

        private void DebugInput()
        {
            // Debug keyboard input
            bool wPressed = Keyboard.current != null && Keyboard.current.wKey.isPressed;
            bool sPressed = Keyboard.current != null && Keyboard.current.sKey.isPressed;
            bool aPressed = Keyboard.current != null && Keyboard.current.aKey.isPressed;
            bool dPressed = Keyboard.current != null && Keyboard.current.dKey.isPressed;
            bool upPressed = Keyboard.current != null && Keyboard.current.upArrowKey.isPressed;
            bool downPressed = Keyboard.current != null && Keyboard.current.downArrowKey.isPressed;
            bool leftPressed = Keyboard.current != null && Keyboard.current.leftArrowKey.isPressed;
            bool rightPressed = Keyboard.current != null && Keyboard.current.rightArrowKey.isPressed;

            if (wPressed || sPressed || aPressed || dPressed || upPressed || downPressed || leftPressed || rightPressed)
            {
                Debug.Log($"[InputDebugger] Keyboard Input - W:{wPressed} S:{sPressed} A:{aPressed} D:{dPressed} | Up:{upPressed} Down:{downPressed} Left:{leftPressed} Right:{rightPressed}");
            }

            // Debug gamepad input
            if (Gamepad.current != null)
            {
                Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
                bool jumpButton = Gamepad.current.buttonSouth.isPressed;

                if (leftStick.magnitude > 0.1f || jumpButton)
                {
                    Debug.Log($"[InputDebugger] Gamepad Input - LeftStick:{leftStick} Jump:{jumpButton}");
                }
            }
        }

        private void OnGUI()
        {
            if (!enableDebug) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Input Debugger", GUI.skin.box);
            
            if (Keyboard.current != null)
            {
                GUILayout.Label($"W: {Keyboard.current.wKey.isPressed}");
                GUILayout.Label($"S: {Keyboard.current.sKey.isPressed}");
                GUILayout.Label($"A: {Keyboard.current.aKey.isPressed}");
                GUILayout.Label($"D: {Keyboard.current.dKey.isPressed}");
                GUILayout.Label($"Up: {Keyboard.current.upArrowKey.isPressed}");
                GUILayout.Label($"Down: {Keyboard.current.downArrowKey.isPressed}");
                GUILayout.Label($"Left: {Keyboard.current.leftArrowKey.isPressed}");
                GUILayout.Label($"Right: {Keyboard.current.rightArrowKey.isPressed}");
            }

            if (Gamepad.current != null)
            {
                Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
                GUILayout.Label($"Left Stick: {leftStick}");
                GUILayout.Label($"Jump Button: {Gamepad.current.buttonSouth.isPressed}");
            }

            GUILayout.EndArea();
        }
    }
}

