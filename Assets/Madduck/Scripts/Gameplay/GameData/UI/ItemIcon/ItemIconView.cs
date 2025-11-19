using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using PrimeTween;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.GameData
{
    public class ItemIconView : MonoBehaviour, ITransitionable
    {
        [Title("References")]
        [Required,
         SerializeField] private RectTransform elementParent;
        [Required,
         SerializeField] private RectTransform sign;
        [Required, 
         SerializeField] private Image iconImage;
        [Required, 
         SerializeField] private Image qualityStarImage;
        [Required, 
         SerializeField] private Animator qualityStarAnimator;
        [Required, 
         SerializeField] private TMP_Text itemNameText;
        
        [Title("Tween")]
        [SerializeField] private TweenSettings<Vector3> elementScaleTweenSettings;
        [SerializeField] private TweenSettings<Vector2> signPositionTweenSettings;
        
        [Title("Debug")]
        [Button("Preview Transition")]
        private void PreviewTransition(bool active)
        {
            Transition(active).Forget();
        }
        
        private Sequence _transitionSequence;
        
        public void SetItem(IFishableItemInstance itemInstance)
        {
            switch (itemInstance)
            {
                case FishItemInstance fishItemInstance:
                    iconImage.sprite = fishItemInstance.ItemData.Icon;
                    itemNameText.text = fishItemInstance.ItemData.FishName;
                    qualityStarImage.enabled = true;
                    AnimateQualityStar(fishItemInstance.CurrentFishQuality);
                    break;
                case ResourceItemInstance resourceItemInstance:
                    iconImage.sprite = resourceItemInstance.ItemData.Icon;
                    itemNameText.text = resourceItemInstance.ItemData.ResourceName;
                    qualityStarImage.enabled = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemInstance), itemInstance, null);
            }
            sign.anchoredPosition = signPositionTweenSettings.startValue;
            elementParent.localScale = elementScaleTweenSettings.startValue;
        }
        
        public void SetOffset(Vector2 offset)
        {
            elementParent.anchoredPosition += offset;
            signPositionTweenSettings.startValue += offset;
            signPositionTweenSettings.endValue += offset;
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

        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            await Transition(true);
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            await Transition(false);
        }
        
        private async UniTask Transition(bool active)
        {
            _transitionSequence = Sequence.Create()
                .Group(Tween.Scale(elementParent, elementScaleTweenSettings.WithDirection(active)))
                .Group(Tween.UIAnchoredPosition(sign, signPositionTweenSettings.WithDirection(active)));
            await _transitionSequence.ToYieldInstruction().ToUniTask();
        }
        
        private void CancelTransition()
        {
            _transitionSequence.Complete();
        }
    }
}