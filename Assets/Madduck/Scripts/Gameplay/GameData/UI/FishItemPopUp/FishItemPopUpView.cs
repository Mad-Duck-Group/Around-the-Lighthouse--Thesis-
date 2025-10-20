using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using PrimeTween;
using R3;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.GameData
{
    public record FishItemPopUpObject(FishItemInstance FishItemInstance) : IPopUpObject
    {
        public FishItemInstance FishItemInstance { get; private set; } = FishItemInstance;
    }
    
    public class FishItemPopUpView : MonoBehaviour, IPopUpView<FishItemPopUpObject>, ITransitionable
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
         SerializeField] private TMP_Text fishDescriptionText;
        [Required,
         SerializeField] private TMP_Text fishWeightText;
        [Required,
         SerializeField] private TMP_Text fishRarityText;
        [Required,
         SerializeField] private Image fishIcon;
        [Required,
         SerializeField] private Button closeButton;
        
        [Title("Tween")]
        [SerializeField] private TweenSettings<float> backgroundAlphaTweenSettings;
        [SerializeField] private TweenSettings<Vector3> scaleTweenSettings;
        
        #endregion
        
        #region Fields
        
        public event Action OnOpen;
        public event Action OnClose;
        private Sequence _transitionSequence;
        private IDisposable _bindings;
        
        #endregion

        #region Injection
        public void SetPopUpObject(FishItemPopUpObject popUpObject)
        {
            canvasGroup.transform.localScale = scaleTweenSettings.startValue;
            backgroundImage.color = backgroundImage.color.WithA(backgroundAlphaTweenSettings.startValue);
            fishNameText.text = popUpObject.FishItemInstance.ItemData.FishName;
            fishDescriptionText.text = popUpObject.FishItemInstance.ItemData.FishDescription;
            fishWeightText.text = $"Weight:\n{popUpObject.FishItemInstance.ItemData.FishWeight:F2} kg";
            fishRarityText.text = $"Rarity:\n{popUpObject.FishItemInstance.CurrentFishQuality}";
            fishIcon.sprite = popUpObject.FishItemInstance.ItemData.FishIcon;
            Bind();
        }
        #endregion

        #region Binding

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            closeButton.OnClickAsObservable()
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
            OnOpen?.Invoke();
        }

        public async UniTask Hide(CancellationToken cancellationToken = default)
        {
            await TransitionOut(cancellationToken);
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