using UnityEngine;
using UnityEngine.InputSystem;
using System;
using ProjectMayhem.Manager;

namespace ProjectMayhem.Manager
{
    public class InputManager : GenericSingleton<InputManager>
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionAsset inputActions;

        // Player 1 Input Actions
        private InputAction player1MoveAction;
        private InputAction player1JumpAction;
        private InputAction player1ShootAction;
        private InputAction player1SpecialAction;

        // Player 2 Input Actions
        private InputAction player2MoveAction;
        private InputAction player2JumpAction;
        private InputAction player2ShootAction;
        private InputAction player2SpecialAction;

        // UI Input Actions
        private InputAction pauseAction;
        private InputAction menuAction;

        // Events for Player 1
        public event Action<Vector2> OnPlayer1_Move;
        public event Action<bool> OnPlayer1_Jump;
        public event Action OnPlayer1_Shoot;
        public event Action OnPlayer1_Special;

        // Events for Player 2
        public event Action<Vector2> OnPlayer2_Move;
        public event Action<bool> OnPlayer2_Jump;
        public event Action OnPlayer2_Shoot;
        public event Action OnPlayer2_Special;

        // UI Events
        public event Action OnPausePressed;
        public event Action OnMenuPressed;

        protected override void Awake()
        {
            base.Awake();
            InitializeInputActions();
        }

        private void OnEnable()
        {
            EnableInputActions();
        }

        private void OnDisable()
        {
            DisableInputActions();
        }

        private void InitializeInputActions()
        {
            if (inputActions == null)
            {
                Debug.LogError("[InputManager] Input Action Asset is not assigned!");
                return;
            }

            // Get Player 1 actions
            player1MoveAction = inputActions.FindAction("Player1/Move");
            player1JumpAction = inputActions.FindAction("Player1/Jump");
            player1ShootAction = inputActions.FindAction("Player1/Shoot");
            player1SpecialAction = inputActions.FindAction("Player1/Special");

            // Get Player 2 actions
            player2MoveAction = inputActions.FindAction("Player2/Move");
            player2JumpAction = inputActions.FindAction("Player2/Jump");
            player2ShootAction = inputActions.FindAction("Player2/Shoot");
            player2SpecialAction = inputActions.FindAction("Player2/Special");

            // Get UI actions
            pauseAction = inputActions.FindAction("UI/Pause");
            menuAction = inputActions.FindAction("UI/Menu");

            // Subscribe to input events
            SubscribeToInputEvents();
        }

        private void SubscribeToInputEvents()
        {
            // Player 1 events
            if (player1MoveAction != null)
            {
                player1MoveAction.performed += OnPlayer1MovePerformed;
                player1MoveAction.canceled += OnPlayer1MoveCanceled;
            }

            if (player1JumpAction != null)
            {
                player1JumpAction.performed += OnPlayer1JumpPerformed;
                player1JumpAction.canceled += OnPlayer1JumpCanceled;
            }

            if (player1ShootAction != null)
                player1ShootAction.performed += OnPlayer1ShootPerformed;

            if (player1SpecialAction != null)
                player1SpecialAction.performed += OnPlayer1SpecialPerformed;

            // Player 2 events
            if (player2MoveAction != null)
            {
                player2MoveAction.performed += OnPlayer2MovePerformed;
                player2MoveAction.canceled += OnPlayer2MoveCanceled;
            }

            if (player2JumpAction != null)
            {
                player2JumpAction.performed += OnPlayer2JumpPerformed;
                player2JumpAction.canceled += OnPlayer2JumpCanceled;
            }

            if (player2ShootAction != null)
                player2ShootAction.performed += OnPlayer2ShootPerformed;

            if (player2SpecialAction != null)
                player2SpecialAction.performed += OnPlayer2SpecialPerformed;

            // UI events
            if (pauseAction != null)
                pauseAction.performed += OnPausePerformed;

            if (menuAction != null)
                menuAction.performed += OnMenuPerformed;
        }

        private void EnableInputActions()
        {
            inputActions?.Enable();
        }

        private void DisableInputActions()
        {
            inputActions?.Disable();
        }

        #region Player 1 Input Handlers

        private void OnPlayer1MovePerformed(InputAction.CallbackContext context)
        {
            Vector2 moveInput = context.ReadValue<Vector2>();
            OnPlayer1_Move?.Invoke(moveInput);
        }

        private void OnPlayer1MoveCanceled(InputAction.CallbackContext context)
        {
            OnPlayer1_Move?.Invoke(Vector2.zero);
        }

        private void OnPlayer1JumpPerformed(InputAction.CallbackContext context)
        {
            OnPlayer1_Jump?.Invoke(true);
        }

        private void OnPlayer1JumpCanceled(InputAction.CallbackContext context)
        {
            OnPlayer1_Jump?.Invoke(false);
        }

        private void OnPlayer1ShootPerformed(InputAction.CallbackContext context)
        {
            OnPlayer1_Shoot?.Invoke();
        }

        private void OnPlayer1SpecialPerformed(InputAction.CallbackContext context)
        {
            OnPlayer1_Special?.Invoke();
        }

        #endregion

        #region Player 2 Input Handlers

        private void OnPlayer2MovePerformed(InputAction.CallbackContext context)
        {
            Vector2 moveInput = context.ReadValue<Vector2>();
            OnPlayer2_Move?.Invoke(moveInput);
        }

        private void OnPlayer2MoveCanceled(InputAction.CallbackContext context)
        {
            OnPlayer2_Move?.Invoke(Vector2.zero);
        }

        private void OnPlayer2JumpPerformed(InputAction.CallbackContext context)
        {
            OnPlayer2_Jump?.Invoke(true);
        }

        private void OnPlayer2JumpCanceled(InputAction.CallbackContext context)
        {
            OnPlayer2_Jump?.Invoke(false);
        }

        private void OnPlayer2ShootPerformed(InputAction.CallbackContext context)
        {
            OnPlayer2_Shoot?.Invoke();
        }

        private void OnPlayer2SpecialPerformed(InputAction.CallbackContext context)
        {
            OnPlayer2_Special?.Invoke();
        }

        #endregion

        #region UI Input Handlers

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            OnPausePressed?.Invoke();
        }

        private void OnMenuPerformed(InputAction.CallbackContext context)
        {
            OnMenuPressed?.Invoke();
        }

        #endregion

        public Vector2 GetPlayerMoveInput(int playerID)
        {
            if (playerID == 1 && player1MoveAction != null)
                return player1MoveAction.ReadValue<Vector2>();
            else if (playerID == 2 && player2MoveAction != null)
                return player2MoveAction.ReadValue<Vector2>();

            return Vector2.zero;
        }

        public bool IsPlayerJumpPressed(int playerID)
        {
            if (playerID == 1 && player1JumpAction != null)
                return player1JumpAction.IsPressed();
            else if (playerID == 2 && player2JumpAction != null)
                return player2JumpAction.IsPressed();

            return false;
        }

        public bool WasPlayerShootPressedThisFrame(int playerID)
        {
            if (playerID == 1 && player1ShootAction != null)
                return player1ShootAction.WasPressedThisFrame();
            else if (playerID == 2 && player2ShootAction != null)
                return player2ShootAction.WasPressedThisFrame();

            return false;
        }

        public bool WasPlayerSpecialPressedThisFrame(int playerID)
        {
            if (playerID == 1 && player1SpecialAction != null)
                return player1SpecialAction.WasPressedThisFrame();
            else if (playerID == 2 && player2SpecialAction != null)
                return player2SpecialAction.WasPressedThisFrame();

            return false;
        }

        public void EnablePlayerInput(int playerID)
        {
            if (playerID == 1)
            {
                player1MoveAction?.Enable();
                player1JumpAction?.Enable();
                player1ShootAction?.Enable();
                player1SpecialAction?.Enable();
            }
            else if (playerID == 2)
            {
                player2MoveAction?.Enable();
                player2JumpAction?.Enable();
                player2ShootAction?.Enable();
                player2SpecialAction?.Enable();
            }
        }

        public void DisablePlayerInput(int playerID)
        {
            if (playerID == 1)
            {
                player1MoveAction?.Disable();
                player1JumpAction?.Disable();
                player1ShootAction?.Disable();
                player1SpecialAction?.Disable();
            }
            else if (playerID == 2)
            {
                player2MoveAction?.Disable();
                player2JumpAction?.Disable();
                player2ShootAction?.Disable();
                player2SpecialAction?.Disable();
            }
        }
    }
}
