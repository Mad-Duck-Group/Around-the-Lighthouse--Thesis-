using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Scripts.Input;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Madduck.Input
{
    public interface IPlayerInputHandler
    {
        #region Values
        public SerializableReactiveProperty<bool> AnyButtonPressed { get; }
        public SerializableReactiveProperty<Vector2> MovementInput { get; }
        public SerializableReactiveProperty<Vector2> MouseDelta { get; }
        public SerializableReactiveProperty<Vector2> GamepadHookControl { get; }
        public SerializableReactiveProperty<float> BoatInput { get; }
        #endregion

        #region Buttons
        public InputButton InteractButton { get; }
        public InputButton JerkBaitButton { get; }
        public InputBinding[] JerkBindings { get; }
        public InputButton Action0Button { get; }
        public InputButton Action1Button { get; }
        public InputButton ThrowHookButton { get; }
        public InputButton ReelingButton { get; }
        public InputButton PauseGameButton { get; }
        #endregion
    }

    public class PlayerInputHandlerMock : IPlayerInputHandler
    {
        public SerializableReactiveProperty<bool> AnyButtonPressed { get; set; }
        public SerializableReactiveProperty<Vector2> MovementInput { get; set; }
        public SerializableReactiveProperty<Vector2> MouseDelta { get; set; }
        public SerializableReactiveProperty<Vector2> GamepadHookControl { get; set; }
        public SerializableReactiveProperty<float> BoatInput { get; set; }
        public InputButton InteractButton { get; set; }
        public InputButton JerkBaitButton { get; set; }
        public InputBinding[] JerkBindings { get; set; }
        public InputButton Action0Button { get; set; }
        public InputButton Action1Button { get; set; }
        public InputButton ThrowHookButton { get; set; }
        public InputButton ReelingButton { get; set; }
        public InputButton PauseGameButton { get; set; }
    }
    public enum InputType
    {
        UI = 0,
        NonUI = 1
    }
    
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
            private CancellationTokenSource _cts = new();

            public void BindPressButton(InputAction.CallbackContext context)
            {
                IsDown.Value = context.performed;
                IsUp.Value = context.canceled;
                IsHeld.Value = context.performed;
                IsUpAfterHeld.Value = context.canceled;
                _heldLastTime = context.performed;
                InputBinding = context.action.GetBindingForControl(context.control);
                _cts = new();
                ButtonPressTask(_cts.Token).Forget();
            }

            private async UniTaskVoid ButtonPressTask(CancellationToken token)
            {
                await UniTask.WaitForEndOfFrame(token);
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
                        _cts = new();
                        ButtonPressTask(_cts.Token).Forget();
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
                        _cts = new();
                        ButtonPressTask(_cts.Token).Forget();
                        break;
                }
            }
        }

        #endregion
}