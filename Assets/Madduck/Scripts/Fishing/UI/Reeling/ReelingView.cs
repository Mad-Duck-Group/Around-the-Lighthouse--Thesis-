using System;
using Madduck.Scripts.Input;
using Madduck.Scripts.Utils.Others;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Scripts.Fishing.UI.Reeling
{
    public class ReelingView : MonoBehaviour
    {
        [Title("References")]
        [Required]
        [SerializeField] private Slider reelingSlider;
        [Required]
        [SerializeField] private HoldButton reelingButton;
        
        private ReelingViewModel _viewModel;
        private ReelingCommander _commander;
        private IDisposable _isActiveSubscription;
        private IDisposable _bindings;
        
        [Inject]
        public void SetUp(
            ReelingViewModel viewModel, 
            ReelingCommander commander)
        {
            _viewModel = viewModel;
            _commander = commander;
            _isActiveSubscription = _viewModel.IsActive
                .Subscribe(SetActive);
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _viewModel.ReelingProgressPercent
                .Subscribe(SetReelingProgress)
                .AddTo(ref disposableBuilder);
            reelingButton.OnHold
                .AsObservable()
                .Subscribe(_ => _commander.OnReelingHold.Execute(InputType.UI))
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        private void OnDestroy()
        {
            _isActiveSubscription?.Dispose();
            _bindings?.Dispose();
        }

        private void SetActive(bool active)
        {
            _bindings?.Dispose();
            if (active)
            {
                Bind();
            }
            else
            {
                SetReelingProgress(0f);
            }
            gameObject.SetActive(active);
        }
        
        private void SetReelingProgress(float progress)
        {
            reelingSlider.value = progress;
        }
    }
}