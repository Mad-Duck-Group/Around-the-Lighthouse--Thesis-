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
using UnityEngine.UI;
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
         SerializeField] private Image buttonImage;
        [Required,
         SerializeField] private Image outerRing;
        [Required,
         SerializeField] private Image middleRing;
        [Required,
         SerializeField] private Image innerRing;

        [Title("Settings")] 
        [SerializeField] private Vector2 outerRingSize;
        [SerializeField] private Vector2 innerRingSize;
        [SerializeField] private SerializableDictionary<string, Color> ringColors = new();

        [Title("Tween")] 
        [SerializeField] private TweenSettings<Vector3> scaleTweenSettings;
        [SerializeField] private TweenSettings<Vector3> successShrinkTween;
        [SerializeField] private TweenSettings<Vector2> successExpandTween;
        [SerializeField] private TweenSettings<float> successFadeOutTween;
        [SerializeField] private ShakeSettings failShakeSettings;

        [HideInEditorMode, 
         ShowInInspector] private QteButtonController _controller;
        private IDisposable _bindings;
        private Sequence _transitionSequence;
        
        [Inject]
        public void SetUp(QteButtonController controller)
        {
            _controller = controller;
            canvasGroup.alpha = 0;
            ((RectTransform)outerRing.transform).sizeDelta = outerRingSize;
            ((RectTransform)innerRing.transform).sizeDelta = innerRingSize;
            ((RectTransform)middleRing.transform).sizeDelta = innerRingSize;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _controller.CurrentBinding
                .IgnoreFirstValueWhenSubscribe()
                .Subscribe(button =>
                {
                    var buttonDirection = button.name;
                    var color = ringColors.TryGetValue(buttonDirection, out var c) ? c : Color.white;
                    innerRing.color = color;
                    outerRing.color = color;
                    buttonNameText.text =
                        button.ToDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
                })
                .AddTo(ref disposableBuilder);
            _controller.RemainingPercentage
                .IgnoreFirstValueWhenSubscribe()
                .Subscribe(remaining =>
                {
                    var size = Vector2.Lerp(outerRingSize, innerRingSize, remaining.AsFraction);
                    ((RectTransform)outerRing.transform).sizeDelta = size;
                })
                .AddTo(ref disposableBuilder);
            _controller.TimeFramePercentage
                .Subscribe(x =>
                {
                    var size = Vector2.Lerp(innerRingSize, outerRingSize, x.AsFraction);
                    ((RectTransform)middleRing.transform).sizeDelta = size;
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
            outerRing.enabled = false;
            middleRing.enabled = false;
            var sequence = Sequence.Create()
                .Group(Tween.Scale(buttonImage.transform, successShrinkTween))
                .Group(Tween.UISizeDelta((RectTransform)innerRing.transform, successExpandTween))
                .Group(Tween.Alpha(innerRing, successFadeOutTween));
            await sequence.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
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