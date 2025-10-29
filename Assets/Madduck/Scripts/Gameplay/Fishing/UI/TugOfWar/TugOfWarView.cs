using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using PrimeTween;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Fishing.UI
{
    public class TugOfWarView : MonoBehaviour, ITransitionable
    {
        [Title("References")]
        [Required, 
         SerializeField] private Slider tugOfWarSlider;

        [Title("Tween")] 
        [SerializeField] private TweenSettings<Vector3> scaleTweenSettings;

        private TugOfWarViewModel _viewModel;
        private IDisposable _bindings;
        private Sequence _transitionSequence;

        [Inject]
        public void SetUp(TugOfWarViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _viewModel.TugOfWarPercent
                .Subscribe(x => tugOfWarSlider.value = x.AsFraction)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private void OnDestroy()
        {
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
                tugOfWarSlider.value = 0f;
            }
            gameObject.SetActive(active);
        }

        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            transform.localScale = scaleTweenSettings.startValue;
            cancellationToken.Register(CancelTransition);
            SetActive(true);
            await Transition(true);
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            await Transition(false);
            SetActive(false);
        }
        
        private async UniTask Transition(bool forward)
        {
            _transitionSequence = Sequence.Create()
                .Group(Tween.Scale(transform, scaleTweenSettings.WithDirection(forward)));
            await _transitionSequence.ToYieldInstruction().ToUniTask();
        }
        
        private void CancelTransition()
        {
            _transitionSequence.Complete();
        }
    }
}