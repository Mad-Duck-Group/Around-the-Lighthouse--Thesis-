using System;
using Cysharp.Threading.Tasks;
using Madduck.Scripts.Input;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Madduck.Input
{
    /// <summary>
    /// Handle player inputs.
    /// </summary>
    [Serializable]
    public class PlayerInputHandler : 
        MonoBehaviour, 
        PlayerInputAction.IPlayerActions, 
        IPlayerInputHandler
    {
        #region Inspector

        #region Values
        [field: ReadOnly, 
                ShowInInspector] public SerializableReactiveProperty<bool> AnyButtonPressed { get; private set; } = new();
        [field: ReadOnly, 
                ShowInInspector] public SerializableReactiveProperty<Vector2> MovementInput { get; private set; } = new();
        [field: ReadOnly, 
                ShowInInspector] public SerializableReactiveProperty<Vector2> MouseDelta { get; private set; } = new();
        [field: ReadOnly, 
                ShowInInspector] public SerializableReactiveProperty<Vector2> GamepadHookControl { get; private set; } = new();
        [field: ReadOnly, 
                ShowInInspector] public SerializableReactiveProperty<float> BoatInput { get; private set; } = new();
        #endregion

        #region Buttons

        [field: ReadOnly, 
                ShowInInspector] public InputButton InteractButton { get; private set; }
        [field: ReadOnly, 
                ShowInInspector] public InputButton JerkBaitButton { get; private set; }
        [field: ReadOnly, 
                ShowInInspector] public InputBinding[] JerkBindings { get; private set; }
        [field: ReadOnly, 
                ShowInInspector] public InputButton Action0Button { get; private set; }
        [field: ReadOnly, 
                ShowInInspector] public InputButton Action1Button { get; private set; }
        [field: ReadOnly, 
                ShowInInspector] public InputButton ThrowHookButton { get; private set; }
        [field: ReadOnly, 
                ShowInInspector] public InputButton ReelingButton { get; private set; }
        [field: ReadOnly, 
                ShowInInspector] public InputButton PauseGameButton { get; private set; }

        #endregion

        #endregion

        #region Fields

        private PlayerInputAction _playerInputAction;
        private IDisposable _anyButtonPressListener;

        #endregion

        #region Life Cycle

        private void OnEnable()
        {
            Subscribe();
            RegisterInputAction();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void RegisterInputAction()
        {
            InteractButton = new InputButton(_playerInputAction.Player.Interact);
            JerkBaitButton = new InputButton(_playerInputAction.Player.JerkBait);
            Action0Button = new InputButton(_playerInputAction.Player.Action0);
            Action1Button = new InputButton(_playerInputAction.Player.Action1);
            ThrowHookButton = new InputButton(_playerInputAction.Player.ThrowHook);
            ReelingButton = new InputButton(_playerInputAction.Player.Reeling);
            PauseGameButton = new InputButton(_playerInputAction.Player.PauseGame);
            JerkBindings = _playerInputAction.Player.JerkBait.bindings.ToArray();
        }

        #endregion

        #region Subscriptions

        private void Subscribe()
        {
            if (_playerInputAction == null)
            {
                _playerInputAction = new PlayerInputAction();
                _playerInputAction.Player.SetCallbacks(this);
            }

            _playerInputAction.Player.Enable();
            _anyButtonPressListener = InputSystem.onAnyButtonPress.Call(_ => OnAnyButton().Forget());
        }

        private void Unsubscribe()
        {
            _playerInputAction.Player.Disable();
            _anyButtonPressListener?.Dispose();
        }

        #endregion

        #region Event Handlers

        private async UniTaskVoid OnAnyButton()
        {
            AnyButtonPressed.Value = true;
            await UniTask.WaitForEndOfFrame();
            AnyButtonPressed.Value = false;
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            MovementInput.Value = context.ReadValue<Vector2>();
        }

        public void OnControlBoat(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                float input = context.ReadValue<float>();
                BoatInput.Value = input;
            }
            else if (context.canceled)
            {
                BoatInput.Value = 0f;
            }
        }

        public void OnPauseGame(InputAction.CallbackContext context)
        {
            PauseGameButton.BindPressButton(context);
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            InteractButton.BindPressButton(context);
        }

        public void OnJerkBait(InputAction.CallbackContext context)
        {
            JerkBaitButton.BindPassThroughButton(context);
        }

        public void OnAction0(InputAction.CallbackContext context)
        {
            Action0Button.BindHoldButton(context);
        }

        public void OnAction1(InputAction.CallbackContext context)
        {
            Action1Button.BindPressButton(context);
        }

        public void OnThrowHook(InputAction.CallbackContext context)
        {
            ThrowHookButton.BindHoldButton(context);
        }

        public void OnReeling(InputAction.CallbackContext context)
        {
            ReelingButton.BindHoldButton(context);
        }

        public void OnMouseDelta(InputAction.CallbackContext context)
        {
            MouseDelta.Value = context.ReadValue<Vector2>();
        }

        public void OnGamepadHookControl(InputAction.CallbackContext context)
        {
            GamepadHookControl.Value = context.ReadValue<Vector2>();
        }
        #endregion
    }
}