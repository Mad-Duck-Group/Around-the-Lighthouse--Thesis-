using System;
using Madduck.Scripts.Utils.Others;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Scripts.Fishing.UI.ThrowHook
{
    public class ThrowHookView : MonoBehaviour
    {
        [Title("References")]
        [Required]
        [SerializeField] private CanvasGroup canvasGroup;
        [Required]
        [SerializeField] private Slider throwHookSlider;
        [Required]
        [SerializeField] private HoldButton throwHookButton;
        
        private ThrowHookViewModel _viewModel;
        private ThrowHookCommander _commander;
        private IDisposable _isActiveSubscription;
        private IDisposable _bindings;
        
        [Inject]
        public void SetUp(ThrowHookViewModel viewModel, ThrowHookCommander commander)
        {
            _viewModel = viewModel;
            _commander = commander;
            _isActiveSubscription = _viewModel.IsActive
                .Subscribe(SetActive);
            Bind();
        }

        // private void Start()
        // {
        //     SetActive(false);
        // }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _viewModel.ThrowHookPercent
                .Subscribe(ChangeThrowHookSlider)
                .AddTo(ref disposableBuilder);
            throwHookButton.OnHold
                .AsObservable()
                .Subscribe(_ => _commander.ThrowHookHeldCommand.Execute(Unit.Default))
                .AddTo(ref disposableBuilder);
            throwHookButton.OnRelease
                .AsObservable()
                .Subscribe(_ => _commander.ThrowHookReleaseCommand.Execute(Unit.Default))
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private void SetActive(bool active)
        {
            DebugUtils.Log("ThrowHookView SetActive: " + active);
            _bindings?.Dispose();
            if (active)
            {
                Bind();
            }
            else
            {
                ChangeThrowHookSlider(0f);
            }
            gameObject.SetActive(active);
        }

        private void OnDestroy()
        {
            _bindings?.Dispose();
            _isActiveSubscription?.Dispose();
        }
        
        private void ChangeThrowHookSlider(float value)
        {
            throwHookSlider.value = value;
        }
    }
}