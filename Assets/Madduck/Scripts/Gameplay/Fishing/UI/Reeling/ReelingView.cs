using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Input;
using Madduck.Scripts.Input;
using Madduck.Utils;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Fishing.UI
{
    public class ReelingView : MonoBehaviour, ITransitionable
    {
        [Title("References")]
        [Required]
        [SerializeField] private Slider reelingSlider;
        [Required]
        [SerializeField] private HoldButton reelingButton;
        
        private ReelingViewModel _viewModel;
        private ReelingCommander _commander;
        private IDisposable _bindings;
        
        [Inject]
        public void SetUp(
            ReelingViewModel viewModel, 
            ReelingCommander commander)
        {
            _viewModel = viewModel;
            _commander = commander;
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
            _bindings?.Dispose();
        }
        
        #region Transitions
        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransitions);
            //await UniTask.WaitForSeconds(1f, cancellationToken: cancellationToken); // placeholder for actual transition animation
            SetActive(true);
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransitions);
            //await UniTask.WaitForSeconds(1f, cancellationToken: cancellationToken); // placeholder for actual transition animation
            SetActive(false);
        }
        
        private void CancelTransitions()
        {
            // Implement if there are any ongoing animations or transitions to cancel
        }
        #endregion

        private void SetActive(bool active)
        {
            _bindings?.Dispose();
            if (active)
            {
                Bind();
            }
            else
            {
                SetReelingProgress(Percentage.Zero);
            }
            gameObject.SetActive(active);
        }
        
        private void SetReelingProgress(Percentage progressPercent)
        {
            reelingSlider.value = progressPercent.AsFraction;
        }
    }
}