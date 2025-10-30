using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using PrimeTween;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Madduck.Shared
{
    public class QteButtonView : MonoBehaviour, IQteElement
    {
        [Title("References")]
        [Required,
         SerializeField] private CanvasGroup canvasGroup;
        [Required,
         SerializeField] private TMP_Text buttonNameText;
        [Required,
         SerializeField] private RectTransform outerRing;
        [Required,
         SerializeField] private RectTransform middleRing;
        [Required,
         SerializeField] private RectTransform innerRing;

        [Title("Settings")] 
        [SerializeField] private Vector2 outerRingSize;
        [SerializeField] private Vector2 innerRingSize;

        [Title("Tween")] 
        [SerializeField] private TweenSettings<Vector3> scaleTweenSettings;
        [SerializeField] private TweenSettings<Vector3> successTween;
        [SerializeField] private ShakeSettings failShakeSettings;

        [ShowInInspector] private QteButtonController _controller;
        private IDisposable _bindings;
        private Sequence _transitionSequence;
        
        [Inject]
        public void SetUp(QteButtonController controller)
        {
            _controller = controller;
            canvasGroup.alpha = 0;
            outerRing.sizeDelta = outerRingSize;
            innerRing.sizeDelta = innerRingSize;
            middleRing.sizeDelta = innerRingSize;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _controller.CurrentBinding
                .Subscribe(button =>
                {
                    buttonNameText.text = button.ToDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
                })
                .AddTo(ref disposableBuilder);
            _controller.RemainingPercentage
                .IgnoreFirstValueWhenSubscribe()
                .Subscribe(remaining =>
                {
                    var size = Vector2.Lerp(outerRingSize, innerRingSize, remaining.AsFraction);
                    outerRing.sizeDelta = size;
                })
                .AddTo(ref disposableBuilder);
            _controller.TimeFramePercentage
                .Subscribe(x =>
                {
                    var size = Vector2.Lerp(innerRingSize, outerRingSize, x.AsFraction);
                    middleRing.sizeDelta = size;
                })
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private void OnDestroy()
        {
            _bindings?.Dispose();
        }

        public async UniTask OnSuccess(CancellationToken cancellationToken = default)
        {
            await Tween.Scale(transform, successTween).ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
        }

        public async UniTask OnFail(CancellationToken cancellationToken = default)
        {
            await Tween.ShakeLocalPosition(transform, failShakeSettings).ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
        }

        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            transform.localScale = scaleTweenSettings.startValue;
            canvasGroup.alpha = 1;
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

        public void SetAsChild(IQteElement child)
        {
            child.SetParent(this);
        }
    }
}