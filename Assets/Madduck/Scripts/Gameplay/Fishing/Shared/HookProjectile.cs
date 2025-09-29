using Cysharp.Threading.Tasks;
using Madduck.Utils;
using PrimeTween;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.Shared
{
    public interface IHookProjectile
    {
        UniTask Throw(Percentage percent);
        UniTask Return();
        UniTask Move(Percentage percent);
        UniTask Nibble(int? cycle);
        void SetPosition(Percentage percent);
        void StopNibble();
    }
    public class HookProjectile : MonoBehaviour, IHookProjectile
    {
        #region Inspector

        [Title("References")] 
        [Required] 
        [SerializeField] private Transform hookIcon;
        
        [Title("Settings")]
        [PropertyTooltip("Range of the throw distance when the throw hook value is between 0 and max.")]
        [SerializeField] public Vector2 throwRange = new(0f, 7f);

        [Title("Tween")] 
        [SerializeField] private TweenSettings moveTweenX;
        [SerializeField] private TweenSettings throwTweenX;
        [SerializeField] private TweenSettings<float> throwTweenY;
        [SerializeField] private TweenSettings<Vector2> nibbleTween;

        #endregion

        #region Fields

        private Vector2 _startPosition;
        private float _targetDistance;
        private bool _isThrown;
        private Sequence _moveSequence;
        private Sequence _throwSequence;
        private Sequence _nibbleSequence;

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _startPosition = transform.localPosition;
        }

        #endregion

        #region Utils

        /// <summary>
        /// Throws the hook to the specified distance.
        /// </summary>
        public async UniTask Throw(Percentage percent)
        {
            if (_isThrown) return;
            _isThrown = true;
            _targetDistance = Mathf.Lerp(throwRange.x, throwRange.y, percent.AsFraction);
            _throwSequence = Sequence.Create()
                .Group(Tween.LocalPositionX(transform, _startPosition.x, _targetDistance, throwTweenX))
                .Group(Tween.LocalPositionY(transform, throwTweenY));
            await _throwSequence.ToYieldInstruction().ToUniTask();
        }

        /// <summary>
        /// Returns the hook to the starting position.
        /// </summary>
        public async UniTask Return()
        {
            if (!_isThrown) return;
            _isThrown = false;
            _throwSequence = Sequence.Create()
                .Group(Tween.LocalPositionX(transform, _startPosition.x, _targetDistance, throwTweenX))
                .Group(Tween.LocalPositionY(transform, throwTweenY))
                .ApplyDirection(false);
            await _throwSequence.ToYieldInstruction().ToUniTask();
        }

        /// <summary>
        /// Animates the nibble effect on the hook icon.
        /// </summary>
        /// <param name="cycle">Set to -1 for infinite cycles.</param>
        public async UniTask Nibble(int? cycle)
        {
            var finalCycle = cycle ?? 1;
            _nibbleSequence = Sequence.Create(finalCycle, CycleMode.Yoyo)
                .Group(Tween.LocalPosition(hookIcon, nibbleTween.ToVector3().ToRelative(hookIcon.localPosition)));
            await _nibbleSequence.ToYieldInstruction().ToUniTask();
        }

        public async UniTask Move(Percentage percentage)
        {
            _targetDistance = Mathf.Lerp(throwRange.x, throwRange.y, percentage.AsFraction);
            _moveSequence = Sequence.Create()
                .Group(Tween.LocalPositionX(transform, transform.localPosition.x, _targetDistance, moveTweenX));
            await _moveSequence.ToYieldInstruction().ToUniTask();
        }

        public void SetPosition(Percentage percent)
        {
            _targetDistance = Mathf.Lerp(throwRange.x, throwRange.y, percent.AsFraction);
            transform.localPosition = transform.localPosition.WithX(_targetDistance);
        }

        public void StopNibble()
        {
            _nibbleSequence.Complete();
        }

        #endregion
    }

    public class HookProjectileMock : IHookProjectile
    {
        public UniTask Throw(Percentage percent) => UniTask.CompletedTask;

        public UniTask Return() => UniTask.CompletedTask;

        public UniTask Move(Percentage percent) => UniTask.CompletedTask;

        public UniTask Nibble(int? cycle) => UniTask.CompletedTask;
        public void SetPosition(Percentage percent){}
        public void StopNibble(){}
    }
}