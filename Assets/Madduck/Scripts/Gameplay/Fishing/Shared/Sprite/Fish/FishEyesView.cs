using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

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
        [SerializeField] private Vector2 offset;

        [Title("Tween")] 
        [SerializeField] private TweenSettings<Vector2> positionTweenSettings;
        [SerializeField] private TweenSettings biteSettings;

        private Transform _hook;
        private TweenSettings<Vector3> _relativeSettings; 
        private Sequence _transitionSequence;
        private Sequence _biteSequence;
        
        public void SetUp(Transform hook)
        {
            _hook = hook;
            transform.position = hook.position + (Vector3)offset;
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
            _biteSequence = Sequence.Create()
                .Group(Tween.Position(transform, transform.position, _hook.position,
                    biteSettings));
            await _biteSequence.ToYieldInstruction().ToUniTask(cancellationToken: cancellationToken);
        }
    }
}