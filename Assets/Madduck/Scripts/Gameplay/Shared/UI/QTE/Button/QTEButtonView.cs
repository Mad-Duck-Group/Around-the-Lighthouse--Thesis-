using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using PrimeTween;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using VContainer;

namespace Madduck.Shared
{
    public interface IQTEButtonView : ITransitionable
    {
        UniTask OnSuccess();
        UniTask OnFail();
        void Destroy();
    }
    public class QTEButtonView : MonoBehaviour, IQTEButtonView
    {
        [Title("References")]
        [Required,
         SerializeField] private CanvasGroup canvasGroup;
        [Required,
         SerializeField] private TMP_Text buttonNameText;
        [Required,
         SerializeField] private RectTransform outerRing;
        [Required,
         SerializeField] private RectTransform innerRing;

        [Title("Settings")] 
        [SerializeField] private Vector2 outerRingSize;
        [SerializeField] private Vector2 innerRingSize;

        [Title("Tween")] 
        [SerializeField] private TweenSettings<Vector3> scaleTweenSettings;
        [SerializeField] private TweenSettings<Vector3> successTween;
        [SerializeField] private ShakeSettings failShakeSettings;

        private QTEButtonViewModel _viewModel;
        private IDisposable _bindings;
        private Sequence _transitionSequence;
        
        [Inject]
        public void SetUp(QTEButtonViewModel viewModel)
        {
            _viewModel = viewModel;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _viewModel.ButtonName
                .Subscribe(buttonName =>
                {
                    buttonNameText.text = buttonName;
                })
                .AddTo(ref disposableBuilder);
            _viewModel.Remaining
                .IgnoreFirstValueWhenSubscribe()
                .Subscribe(remaining =>
                {
                    var size = Vector2.Lerp(outerRingSize, innerRingSize, remaining.AsFraction);
                    outerRing.sizeDelta = size;
                })
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private void OnDestroy()
        {
            _bindings?.Dispose();
        }

        public async UniTask OnSuccess()
        {
            await Tween.Scale(transform, successTween).ToYieldInstruction().ToUniTask();
        }

        public async UniTask OnFail()
        {
            await Tween.ShakeLocalPosition(transform, failShakeSettings).ToYieldInstruction().ToUniTask();
        }

        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            transform.localScale = scaleTweenSettings.startValue;
            await Transition(true);
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            await Transition(false);
        }

        private async UniTask Transition(bool forward)
        {
            _transitionSequence = Sequence.Create()
                .Group(Tween.Scale(transform, scaleTweenSettings.WithDirection(forward)));
            await _transitionSequence.ToYieldInstruction().ToUniTask();
        }
        
        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}