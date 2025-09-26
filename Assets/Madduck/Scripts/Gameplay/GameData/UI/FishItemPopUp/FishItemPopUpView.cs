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
        
        private Sequence _transitionSequence;
        private PopUpManager<FishItemPopUpObject> _popUpManager;
        private IDisposable _closeButtonDisposable;

        public void SetUp(PopUpManager<FishItemPopUpObject> popUpManager)
        {
            _popUpManager = popUpManager;
            canvasGroup.transform.localScale = scaleTweenSettings.startValue;
            backgroundImage.color = backgroundImage.color.WithA(backgroundAlphaTweenSettings.startValue);
            _closeButtonDisposable = closeButton.OnClickAsObservable().Subscribe(_ => OnCloseButtonClicked());
        }

        private void OnDestroy()
        {
            _closeButtonDisposable?.Dispose();
        }

        private void OnCloseButtonClicked()
        {
            _popUpManager.HidePopUp().Forget();
        }
        
        public async UniTask ShowPopUp(FishItemPopUpObject popUpObject, CancellationToken cancellationToken = default)
        {
            fishNameText.text = popUpObject.FishItemInstance.ItemData.FishName;
            fishDescriptionText.text = popUpObject.FishItemInstance.ItemData.FishDescription;
            fishWeightText.text = popUpObject.FishItemInstance.ItemData.FishWeight.ToString();
            fishRarityText.text = popUpObject.FishItemInstance.CurrentFishQuality.ToString();
            fishIcon.sprite = popUpObject.FishItemInstance.ItemData.FishSprite;
            await TransitionIn(cancellationToken); 
        }

        public async UniTask HidePopUp(CancellationToken cancellationToken = default)
        {
            await TransitionOut(cancellationToken);
        }

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
    }
}