using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Madduck.Scripts.Input;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using Observable = R3.Observable;

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
                ShowInInspector] public SerializableReactiveProperty<Vector2> MouseUnitCircle { get; private set; } = new();
        [field: ReadOnly, 
                ShowInInspector] public SerializableReactiveProperty<Vector2> RightStickDelta { get; private set; } = new();
        [field: ReadOnly, 
                ShowInInspector] public SerializableReactiveProperty<Vector2> LeftStickDelta { get; private set; } = new();
        [field: ReadOnly, 
                ShowInInspector] public SerializableReactiveProperty<float> BaitSelectInput { get; private set; } = new();
        #endregion

        #region Buttons

        [field: ReadOnly, 
                ShowInInspector] public InputButton InteractButton { get; private set; }
        [field: ReadOnly, 
                ShowInInspector] public InputButton JerkBaitButton { get; private set; }
        
        public InputBinding[] JerkBindings
        {
            get
            {
                return _playerInputAction.Player.JerkBait.bindings
                    .Where(x => x.groups.Contains(_currentControlScheme))
                    .ToArray();
            }
        }
        
        [field: ReadOnly, 
                ShowInInspector] public InputButton Action0Button { get; private set; }
        [field: ReadOnly, 
                ShowInInspector] public InputButton Action1Button { get; private set; }
        [field: ReadOnly, 
                ShowInInspector] public InputButton ReelingButton { get; private set; }
        [field: ReadOnly, 
                ShowInInspector]public InputButton BaitButton { get; private set; }
        [field: ReadOnly, 
                ShowInInspector]public InputButton ConfirmBaitButton { get; private set; }
        [field: ReadOnly, 
                ShowInInspector] public InputButton PauseGameButton { get; private set; }

        #endregion
        [ReadOnly, ShowInInspector] public string CurrentControlScheme => _currentControlScheme;

        #endregion

        #region Fields

        private PlayerInputAction _playerInputAction;
        private string _currentControlScheme = "Mouse & Keyboard";
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
            ReelingButton = new InputButton(_playerInputAction.Player.Reeling);
            PauseGameButton = new InputButton(_playerInputAction.Player.PauseGame);
            BaitButton = new InputButton(_playerInputAction.Player.ToggleBait);
            ConfirmBaitButton = new InputButton(_playerInputAction.Player.ConfirmBait);
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
            switch (inputControl.device)
            {
                case Mouse:
                case Keyboard:
                    _currentControlScheme = "Mouse & Keyboard";
                    break;
                case Gamepad:
                    _currentControlScheme = "Gamepad";
                    break;
                case Touchscreen:
                    _currentControlScheme = "Touchscreen";
                    break;
                default:
                    Debug.LogWarning("Unknown control scheme detected. Fallback to Mouse & Keyboard.");
                    _currentControlScheme = "Mouse & Keyboard";
                    break;
            }
            AnyButtonPressed.Value = true;
            await UniTask.WaitForEndOfFrame();
            AnyButtonPressed.Value = false;
        }

        public void OnMovement(InputAction.CallbackContext context)
        {
            MovementInput.Value = context.ReadValue<Vector2>();
        }

        public void OnPauseGame(InputAction.CallbackContext context)
        {
            PauseGameButton.BindPressButton(context);
        }

        public void OnToggleBait(InputAction.CallbackContext context)
        {
            BaitButton.BindHoldButton(context);
        }

        public void OnSelectBait(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                float input = context.ReadValue<float>();
                BaitSelectInput.Value = input;
            }
            else if (context.canceled)
            {
                BaitSelectInput.Value = 0f;
            }
        }
        public void OnConfirmBait(InputAction.CallbackContext context)
        {
            ConfirmBaitButton.BindPressButton(context);
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

        public void OnReeling(InputAction.CallbackContext context)
        {
            ReelingButton.BindHoldButton(context);
        }

        public void OnMouseDelta(InputAction.CallbackContext context)
        {
            MouseDelta.Value = context.ReadValue<Vector2>();
        }

        public void OnMouseUnitCircle(InputAction.CallbackContext context)
        {
            var position = context.ReadValue<Vector2>();
            Vector2 screenCenter = new(Screen.currentResolution.width / 2f, Screen.currentResolution.height / 2f);
            var delta = position - screenCenter;
            MouseUnitCircle.Value = delta.normalized;
        }

        public void OnRightStickDelta(InputAction.CallbackContext context)
        {
            RightStickDelta.Value = context.ReadValue<Vector2>();
        }
        public void OnLeftStickDelta(InputAction.CallbackContext context)
        {
            LeftStickDelta.Value = context.ReadValue<Vector2>();
        }
        #endregion
        
        public void SetActiveInput(bool active)
        {
            if (active)
            {
                Subscribe();
            }
            else
            {
                Unsubscribe();
            }
        }
    }
}