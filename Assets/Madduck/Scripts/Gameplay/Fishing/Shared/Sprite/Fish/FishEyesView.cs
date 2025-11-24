using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Madduck.Fishing.Shared
{
    public interface IFishEyesView : ITransitionable
    {
        void SetUp(Transform hook);
        UniTask Bite(CancellationToken cancellationToken = default);
    }
    public class FishEyesView : MonoBehaviour, IFishEyesView
    {
        [Title("References")]
        [Required,
         SerializeField] private SpriteRenderer spriteRenderer;
        
        [Title("Settings")] 
        [SerializeField] private Vector2 spawnOffset;
        [SerializeField] private Vector2 biteOffset;

        [Title("Tween")] 
        [SerializeField] private TweenSettings<Vector2> positionTweenSettings;
        [SerializeField] private TweenSettings biteSettings;
        [SerializeField] private TweenSettings<Vector2> biteTransitionOutSettings;

        private Transform _hook;
        private TweenSettings<Vector3> _relativeSettings; 
        private Sequence _transitionSequence;
        private Sequence _biteSequence;
        
        public void SetUp(Transform hook)
        {
            _hook = hook;
            transform.position = hook.position + (Vector3)spawnOffset;
            _relativeSettings = positionTweenSettings.ToVector3().ToRelative(transform.position);
        }
        
        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            await Transition(true);
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            await UniTask.WhenAll(Transition(false));
        }

        private async UniTask Transition(bool forward)
        {
            _transitionSequence = Sequence.Create()
                .Group(Tween.Position(transform, _relativeSettings.WithDirection(forward)));
            await _transitionSequence.ToYieldInstruction().ToUniTask();
        }

        private void CancelTransition()
        {
            _transitionSequence.Complete();
        }

        public async UniTask Bite(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(() => _biteSequence.Complete());
            // var bitePosition = _hook.position + (Vector3)biteOffset;
            // _biteSequence = Sequence.Create()
            //     .Group(Tween.Position(transform, transform.position, bitePosition,
            //         biteSettings))
            //     .Chain(Tween.Position(transform, biteTransitionOutSettings.ToVector3().ToRelative(bitePosition)));
            var bitePosition = _hook.position + (Vector3)biteTransitionOutSettings.endValue;
            _biteSequence = Sequence.Create()
                .Chain(Tween.Position(transform, bitePosition, biteTransitionOutSettings.settings));
            await _biteSequence.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
        }
    }
}