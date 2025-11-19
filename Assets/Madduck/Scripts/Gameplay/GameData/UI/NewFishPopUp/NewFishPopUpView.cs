using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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
         SerializeField] private Image backgroundImage;
        [Required,
         SerializeField] private TMP_Text fishNameText;
        [Required, 
         SerializeField] private Animator qualityStarAnimator;
        [Required,
         SerializeField] private Image fishIcon;
        
        [Title("Tween")]
        [SerializeField] private TweenSettings<float> backgroundAlphaTweenSettings;
        [SerializeField] private TweenSettings<Vector3> scaleTweenSettings;
        
        #endregion
        
        #region Fields
        
        public event Action OnOpen;
        public event Action OnClose;
        private IPlayerInputHandler _inputHandler;
        private Sequence _transitionSequence;
        private IDisposable _bindings;
        
        #endregion

        #region Injection
        public void SetUp(IPlayerInputHandler inputHandler)
        {
            _inputHandler = inputHandler;
        }
        public void SetPopUpObject(NewFishPopUpObject popUpObject)
        {
            canvasGroup.transform.localScale = scaleTweenSettings.startValue;
            backgroundImage.color = backgroundImage.color.WithA(backgroundAlphaTweenSettings.startValue);
            fishNameText.text = popUpObject.FishItemInstance.ItemData.FishName;
            fishIcon.sprite = popUpObject.FishItemInstance.ItemData.FishIcon;
            AnimateQualityStar(popUpObject.FishItemInstance.CurrentFishQuality);
        }
        
        private void AnimateQualityStar(FishQuality fishQuality)
        {
            switch (fishQuality)
            {
                case FishQuality.Common:
                    qualityStarAnimator.Play("Copper");
                    break;
                case FishQuality.Good:
                    qualityStarAnimator.Play("Silver");
                    break;
                case FishQuality.Premium:
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
                .Group(Tween.Scale(canvasGroup.transform, scaleTweenSettings.WithDirection(active)))
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