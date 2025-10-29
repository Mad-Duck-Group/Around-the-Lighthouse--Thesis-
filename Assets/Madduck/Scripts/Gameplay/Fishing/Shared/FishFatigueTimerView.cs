using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.Fishing.Shared
{
    public interface IFishFatigueTimerView : ITransitionable
    {
        void SetFishFatigueTimerProgress(Percentage percentage);
    }
    public class FishFatigueTimerView : MonoBehaviour, IFishFatigueTimerView
    {
        [Title("References")]
        [Required,
         SerializeField] private Slider fishFatigueTimerSlider;
        
        [Title("Tween")] 
        [SerializeField] private TweenSettings<Vector3> scaleTweenSettings;
        
        private Sequence _transitionSequence;
        
        private void Awake()
        {
            SetActive(false);
        }

        private void SetActive(bool active)
        {
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

        public void SetFishFatigueTimerProgress(Percentage percentage)
        {
            fishFatigueTimerSlider.value = percentage.AsFraction;
        }
    }
}