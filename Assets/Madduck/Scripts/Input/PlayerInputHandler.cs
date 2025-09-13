using System;
using Cysharp.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using VContainer.Unity;

namespace Madduck.Scripts.Input
{
    public enum InputType
    {
        UI = 0,
        NonUI = 1
    }
    
    /// <summary>
    /// Handle player inputs.
    /// </summary>
    [Serializable]
    public class PlayerInputHandler : MonoBehaviour, PlayerInputAction.IPlayerActions
    {
        #region Data Structures

        [Serializable]
        public record InputButton(InputAction InputAction)
        {
            public InputAction InputAction { get; private set; } = InputAction;

            [ShowInInspector, DisplayAsString]
            public string ButtonName =>
                InputAction != null
                    ? InputAction.GetBindingDisplayString(UnityEngine.InputSystem.InputBinding.DisplayStringOptions.DontIncludeInteractions)
                    : string.Empty;
            public SerializableReactiveProperty<bool> IsDown { get; private set; } = new(false);
            public SerializableReactiveProperty<bool> IsUp { get; private set; } = new(false);
            public SerializableReactiveProperty<bool> IsHeld { get; private set; } = new(false);
            public SerializableReactiveProperty<bool> IsUpAfterHeld { get; private set; } = new(false);
            public InputBinding? InputBinding { get; private set; }
            private bool _heldLastTime;

            public void BindPressButton(InputAction.CallbackContext context)
            {
                IsDown.Value = context.performed;
                IsUp.Value = context.canceled;
                IsHeld.Value = context.performed;
                IsUpAfterHeld.Value = context.canceled;
                _heldLastTime = context.performed;
                InputBinding = context.action.GetBindingForControl(context.control);
                ButtonPressTask().Forget();
            }

            private async UniTaskVoid ButtonPressTask()
            {
                await UniTask.WaitForEndOfFrame();
                IsDown.Value = false;
                if (!IsHeld.Value)
                {
                    IsUp.Value = false;
                    IsUpAfterHeld.Value = false;
                }
            }

            public void BindHoldButton(InputAction.CallbackContext context)
            {
                InputBinding = context.action.GetBindingForControl(context.control);
                switch (context)
                {
                    case { started: true, performed: false }:
                        IsDown.Value = true;
                        IsHeld.Value = false;
                        IsUp.Value = false;
                        IsUpAfterHeld.Value = false;
                        _heldLastTime = false;
                        ButtonPressTask().Forget();
                        break;
                    case { performed: true }:
                        IsDown.Value = false;
                        IsHeld.Value = true;
                        IsUp.Value = false;
                        IsUpAfterHeld.Value = false;
                        _heldLastTime = true;
                        break;
                    case { canceled: true }:
                        IsDown.Value = false;
                        IsHeld.Value = false;
                        IsUp.Value = true;
                        IsUpAfterHeld.Value = _heldLastTime;
                        ButtonPressTask().Forget();
                        break;
                }
            }
        }

        #endregion

        #region Inspector

        #region Values

        [field: ShowInInspector, ReadOnly] public bool AnyButtonPressed { get; private set; }
        [field: ShowInInspector, ReadOnly] public Vector2 MovementInput { get; private set; }

        [field: ShowInInspector, ReadOnly]
        public SerializableReactiveProperty<Vector2> MouseDelta { get; private set; } = new();

        [field: ShowInInspector, ReadOnly]
        public SerializableReactiveProperty<Vector2> GamepadHookControl { get; private set; } = new();

        [field: ShowInInspector, ReadOnly] public float BoatInput { get; private set; }

        #endregion

        #region Buttons

        [field: ShowInInspector, ReadOnly] public InputButton InteractButton { get; private set; }
        [field: ShowInInspector, ReadOnly] public InputButton JerkBaitButton { get; private set; }
        [field: ShowInInspector, ReadOnly] public InputBinding[] JerkBindings { get; private set; }
        [field: ShowInInspector, ReadOnly] public InputButton Action0Button { get; private set; }
        [field: ShowInInspector, ReadOnly] public InputButton Action1Button { get; private set; }
        [field: ShowInInspector, ReadOnly] public InputButton ThrowHookButton { get; private set; }
        [field: ShowInInspector, ReadOnly] public InputButton ReelingButton { get; private set; }
        [field: ShowInInspector, ReadOnly] public InputButton PauseGameButton { get; private set; }

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
            _anyButtonPressListener = InputSystem.onAnyButtonPress.Call(x => OnAnyButton(x).Forget());
        }

        private void Unsubscribe()
        {
            _playerInputAction.Player.Disable();
            _anyButtonPressListener?.Dispose();
        }

        #endregion

        #region Event Handlers

        private async UniTaskVoid OnAnyButton(InputControl inputControl)
        {
            AnyButtonPressed = true;
            await UniTask.WaitForEndOfFrame();
            AnyButtonPressed = false;
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            MovementInput = context.ReadValue<Vector2>();
        }

        public void OnControlBoat(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                float input = context.ReadValue<float>();
                BoatInput = input;
            }
            else if (context.canceled)
            {
                BoatInput = 0f;
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
            JerkBaitButton.BindPressButton(context);
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

        #region Utils

        public void SetBoatInput(float input)
        {
            BoatInput = input;
        }

        #endregion
    }
}