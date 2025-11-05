using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Input;
using Madduck.Scripts.Input;
using Madduck.Utils;
using R3;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Fishing.UI
{
    public class ThrowHookView : MonoBehaviour, ITransitionable
    {
        [Title("References")]
        [Required, 
         SerializeField] private CanvasGroup canvasGroup;
        [Required,
         SerializeField] private Slider throwHookSlider;
        [Required,
         SerializeField] private HoldButton throwHookButton;

        [Title("Settings")] 
        [SerializeField] private float throwHookSliderMaxLength = 350f;
        
        private ThrowHookViewModel _viewModel;
        private ThrowHookCommander _commander;
        private IDisposable _bindings;
        
        [Inject]
        public void SetUp(
            ThrowHookViewModel viewModel, 
            ThrowHookCommander commander)
        {
            _viewModel = viewModel;
            _commander = commander;
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _viewModel.ThrowHookPercentRelative
                .Subscribe(ChangeThrowHookSlider)
                .AddTo(ref disposableBuilder);
            _viewModel.LockedRangePercent
                .Subscribe(OnLockedRangeChanged)
                .AddTo(ref disposableBuilder);
            _viewModel.ShowSlider
                //DistinctUntilChanged()
                .Subscribe(active => throwHookSlider.gameObject.SetActive(active))
                .AddTo(ref disposableBuilder);
            throwHookButton.OnFirstHold
                .AsObservable()
                .Subscribe(_ => _commander.ThrowHookFirstHeldCommand.Execute(InputType.UI))
                .AddTo(ref disposableBuilder);
            throwHookButton.OnHold
                .AsObservable()
                .Subscribe(_ => _commander.ThrowHookHeldCommand.Execute(InputType.UI))
                .AddTo(ref disposableBuilder);
            throwHookButton.OnRelease
                .AsObservable()
                .Subscribe(_ => _commander.ThrowHookReleaseCommand.Execute(InputType.UI))
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private void OnLockedRangeChanged(Percentage current)
        {
            var rectTransform = (RectTransform)throwHookSlider.transform;
            var sizeDelta = rectTransform.sizeDelta;
            if (throwHookSlider.direction is Slider.Direction.BottomToTop or Slider.Direction.TopToBottom)
            {
                rectTransform.sizeDelta = sizeDelta.WithY(throwHookSliderMaxLength * current.AsFraction);
            }
            else
            {
                rectTransform.sizeDelta = sizeDelta.WithX(throwHookSliderMaxLength * current.AsFraction);
            }
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
            throwHookSlider.gameObject.SetActive(false);
            if (active)
            {
                Bind();
            }
            else
            {
                ChangeThrowHookSlider(Percentage.Zero);
            }
            gameObject.SetActive(active);
        }

        private void OnDestroy()
        {
            _bindings?.Dispose();
        }
        
        private void ChangeThrowHookSlider(Percentage throwPercent)
        {
            throwHookSlider.value = throwPercent.AsFraction;
        }
    }
}