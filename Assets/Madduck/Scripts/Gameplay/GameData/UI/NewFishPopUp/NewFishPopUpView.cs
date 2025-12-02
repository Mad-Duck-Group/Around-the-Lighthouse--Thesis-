using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using FMODUnity;
using Madduck.Audio;
using Madduck.Input;
using Madduck.Shared;
using Madduck.Utils;
using PrimeTween;
using R3;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Madduck.GameData
{
    public record NewFishPopUpObject(FishItemInstance FishItemInstance) : IPopUpObject
    {
        public FishItemInstance FishItemInstance { get; private set; } = FishItemInstance;
    }
    
    public class NewFishPopUpView : MonoBehaviour, IPopUpView<NewFishPopUpObject>, ITransitionable
    {
        #region Inspector

        [Title("References")]
        [Required,
         SerializeField] private CanvasGroup canvasGroup;
        [Required,
         SerializeField] private RectTransform elementParent;
        [Required,
         SerializeField] private RectTransform sign;
        [Required,
         SerializeField] private Image backgroundImage;
        [Required,
         SerializeField] private TMP_Text fishNameText;
        [Required, 
         SerializeField] private Animator qualityStarAnimator;
        [Required,
         SerializeField] private Image fishIcon;
        
        [Title("Tween")]
        [SerializeField] private TweenSettings<float> canvasAlphaTweenSettings;
        [SerializeField] private TweenSettings<float> backgroundAlphaTweenSettings;
        [SerializeField] private TweenSettings<Vector2> signPositionTweenSettings;
        [SerializeField] private TweenSettings<Vector3> elementParentScaleTweenSettings;
        
        [Title("Audio")]
        [SerializeField] public EventReference openingSfx;

        [Title("Debug")]
        [Button("Preview Transition")]
        private void PreviewTransition(bool active)
        {
            Transition(active).Forget();
        }
        #endregion
        
        #region Fields
        
        public event Action OnOpen;
        public event Action OnClose;
        private IAudioManager _audioManager;
        private IPlayerInputHandler _inputHandler;
        private Sequence _transitionSequence;
        private IDisposable _bindings;
        
        #endregion

        #region Injection
        public void SetUp(
            IPlayerInputHandler inputHandler,
            IAudioManager audioManager)
        {
            _audioManager = audioManager;
            _inputHandler = inputHandler;
        }
        public void SetPopUpObject(NewFishPopUpObject popUpObject)
        {
            elementParent.transform.localScale = elementParentScaleTweenSettings.startValue;
            sign.anchoredPosition = signPositionTweenSettings.startValue;
            canvasGroup.alpha = canvasAlphaTweenSettings.startValue;
            backgroundImage.color = backgroundImage.color.WithA(backgroundAlphaTweenSettings.startValue);
            fishNameText.text = popUpObject.FishItemInstance.ItemData.FishName;
            fishIcon.sprite = popUpObject.FishItemInstance.ItemData.FishIcon;
            AnimateQualityStar(popUpObject.FishItemInstance.CurrentFishQuality);
        }
        
        private void AnimateQualityStar(FishQuality fishQuality)
        {
            if (fishQuality is FishQuality.None)
            {
                qualityStarAnimator.gameObject.SetActive(false);
                return;
            }
            switch (fishQuality)
            {
                case FishQuality.Copper:
                    qualityStarAnimator.Play("Copper");
                    break;
                case FishQuality.Silver:
                    qualityStarAnimator.Play("Silver");
                    break;
                case FishQuality.Gold:
                    qualityStarAnimator.Play("Gold");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fishQuality), fishQuality, null);
            }
        }
        #endregion

        #region Binding

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _inputHandler.AnyButtonPressed
                .IgnoreFirstValueWhenSubscribe()
                .Where(x => x)
                .Subscribe(_ => OnCloseButtonClicked())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        private void OnDestroy()
        {
            _bindings?.Dispose();
        }

        #endregion

        #region Events

        private void OnCloseButtonClicked()
        {
            Hide().Forget();
        }

        #endregion

        #region Pop Up

        public async UniTask Show(CancellationToken cancellationToken = default)
        {
            _audioManager.PlayAudioOneShot(openingSfx, Vector3.zero);
            await TransitionIn(cancellationToken);
            Bind();
            OnOpen?.Invoke();
        }

        public async UniTask Hide(CancellationToken cancellationToken = default)
        {
            _bindings?.Dispose();
            await TransitionOut(cancellationToken);
            Destroy(gameObject);
            OnClose?.Invoke();
        }

        #endregion

        #region Transition

        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            await Transition(true, cancellationToken);
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            await Transition(false, cancellationToken);
        }

        private async UniTask Transition(bool active, CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            _transitionSequence = Sequence.Create()
                .Group(Tween.Scale(elementParent, elementParentScaleTweenSettings.WithDirection(active)))
                .Group(Tween.UIAnchoredPosition(sign, signPositionTweenSettings.WithDirection(active)))
                .Group(Tween.Alpha(canvasGroup, canvasAlphaTweenSettings.WithDirection(active)))
                .Group(Tween.Alpha(backgroundImage, backgroundAlphaTweenSettings.WithDirection(active)));
            await _transitionSequence.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
        }

        private void CancelTransition()
        {
            _transitionSequence.Complete();
        }

        #endregion
    }
}