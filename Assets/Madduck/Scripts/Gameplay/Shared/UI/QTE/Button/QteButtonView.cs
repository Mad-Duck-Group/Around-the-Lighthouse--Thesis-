using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using PrimeTween;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Shared
{
    public class QteButtonView : MonoBehaviour, IQteElement
    {
        [Serializable]
        private struct QteButtonColor
        {
            public Color ringColor;
            public Color bgColor;
        }
        
        [Title("References")]
        [Required,
         SerializeField] private CanvasGroup canvasGroup;
        // [Required,
        //  SerializeField] private TMP_Text buttonNameText;
        [Required,
         SerializeField] private SerializableDictionary<string, SpriteLibraryAsset> spriteLibraryAssets = new();
        [Required,
         SerializeField] private Image backgroundImage;
        [Required,
         SerializeField] private Image buttonImage;
        [Required,
         SerializeField] private Image outerRing;
        [Required,
         SerializeField] private Image outerRingIcon;
        [Required,
         SerializeField] private Image middleRing;
        [Required,
         SerializeField] private Image innerRing;

        [Title("Settings")] 
        [SerializeField] private Vector2 outerRingSize;
        [SerializeField] private Vector2 innerRingSize;
        [SerializeField] private SerializableDictionary<string, QteButtonColor> ringColors = new();
        [SerializeField] private SerializableDictionary<string, string> directionButtonMapping = new();

        [Title("Tween")] 
        [SerializeField] private TweenSettings<Vector3> scaleTweenSettings;
        [SerializeField] private TweenSettings<Vector3> successShrinkTween;
        [SerializeField] private TweenSettings<Vector2> successExpandTween;
        [SerializeField] private TweenSettings<float> successFadeOutTween;
        [SerializeField] private ShakeSettings failShakeSettings;

        [HideInEditorMode, 
         ShowInInspector] private QteButtonController _controller;
        private string _currentDirection;
        private string _currentControlScheme;
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
            innerRing.enabled = false;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _controller.CurrentBinding
                .IgnoreFirstValueWhenSubscribe()
                .Subscribe(binding =>
                {
                    _currentControlScheme = _controller.CurrentControlScheme.CurrentValue;
                    CurrentBindingChanged(binding);
                    CurrentControlSchemeChanged(_currentControlScheme, _currentDirection);
                })
                .AddTo(ref disposableBuilder);
            _controller.CurrentControlScheme
                .IgnoreFirstValueWhenSubscribe()
                .Subscribe(scheme =>
                {
                    _currentControlScheme = scheme;
                    CurrentControlSchemeChanged(scheme, _currentDirection);
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
        
        private void CurrentBindingChanged(InputBinding binding)
        {
            var buttonDirection = binding.name;
            _currentDirection = buttonDirection;
            if (!ringColors.TryGetValue(buttonDirection, out var qteButtonColor))
            {
                Debug.LogWarning($"QTE Button color for direction {buttonDirection} not found!");
                return;
            }
            outerRing.color = qteButtonColor.ringColor;
            innerRing.color = qteButtonColor.ringColor;
            outerRingIcon.color = qteButtonColor.ringColor;
            backgroundImage.color = qteButtonColor.bgColor;
        }

        private void CurrentControlSchemeChanged(string scheme, string currentDirection)
        {
            if (scheme == null || currentDirection == null)
            {
                return;
            }
            if (!spriteLibraryAssets.TryGetValue(scheme, out var libraryAsset))
            {
                Debug.LogWarning($"SpriteLibraryAsset for control scheme {scheme} not found!");
                return;
            }
            if (!directionButtonMapping.TryGetValue(currentDirection, out var spriteKey))
            {
                Debug.LogWarning($"Sprite key for direction {currentDirection} not found!");
                return;
            }
            buttonImage.sprite = libraryAsset.GetSprite("QTE", spriteKey);
        }

        public async UniTask OnSuccess(CancellationToken cancellationToken = default)
        {
            outerRing.enabled = false;
            outerRingIcon.enabled = false;
            middleRing.enabled = false;
            innerRing.enabled = true;
            var sequence = Sequence.Create()
                .Group(Tween.Scale(buttonImage.transform, successShrinkTween))
                .Group(Tween.UISizeDelta((RectTransform)innerRing.transform, successExpandTween))
                .Group(Tween.Alpha(innerRing, successFadeOutTween));
            await sequence.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
        }

        public async UniTask OnFail(CancellationToken cancellationToken = default)
        {
            outerRing.color = Color.darkRed;
            outerRingIcon.color = Color.darkRed;
            middleRing.color = Color.darkRed;
            innerRing.color = Color.darkRed;
            buttonImage.color = Color.darkRed;
            backgroundImage.color = Color.darkRed;
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