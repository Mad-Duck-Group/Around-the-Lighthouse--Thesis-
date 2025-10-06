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
        [Required, 
         SerializeField] private Transform hookIcon;
        [Required,
         SerializeField] private VerletFishingLine2D line;
        
        [Title("Settings")]
        [PropertyTooltip("Range of the throw distance when the throw hook value is between 0 and max."), 
         SerializeField] private Vector2 throwRange = new(0f, 7f);
        [SerializeField] private float yOffset;
        [SerializeField] private float div = 4f;
        [SerializeField] private float power = 0.7f;

        [Title("Tween")] 
        [SerializeField] private TweenSettings moveTweenX;
        [InfoBox("Duration property of reelBackTweenX is speed"),
         SerializeField] private TweenSettings reelBackTweenX;
        [SerializeField] private TweenSettings<Vector2> nibbleTween;

        #endregion

        #region Fields

        private Vector2 _startPosition;
        private bool _isThrown;
        private Sequence _moveSequence;
        private Sequence _throwSequence;
        private Sequence _nibbleSequence;
        private float _flightTime;
        private Rigidbody2D _rigidbody;

        #endregion

        #region Injection

        public void SetUp(Transform rodTip)
        {
            _startPosition = transform.localPosition;
            line.SetUp(rodTip, transform);
        }

        #endregion

        #region Lifecycle

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
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
            _rigidbody.constraints = RigidbodyConstraints2D.None;
            var targetPos = CalculateTargetPosition(_startPosition, percent);
            var velocity = CalculateLaunchVelocity(_startPosition, targetPos);
            _rigidbody.linearVelocity = velocity;
            var distance = Vector2.Distance(_startPosition, CalculateTargetPosition(_startPosition, percent));
            line.CastLine(_flightTime, distance, true);
            await UniTask.WaitForSeconds(_flightTime);
            _rigidbody.constraints = RigidbodyConstraints2D.FreezePosition;
        }

        /// <summary>
        /// Returns the hook to the starting position.
        /// </summary>
        public async UniTask Return()
        {
            if (!_isThrown) return;
            _isThrown = false;
            await ReelBack();
            _rigidbody.constraints = RigidbodyConstraints2D.None;
            var velocity = CalculateLaunchVelocity(transform.localPosition, _startPosition);
            _rigidbody.linearVelocity = velocity;
            var distance = Vector2.Distance(transform.localPosition, _startPosition);
            line.CastLine(_flightTime, distance, false);
            await UniTask.WaitForSeconds(_flightTime);
            _rigidbody.constraints = RigidbodyConstraints2D.FreezePosition;
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

        public async UniTask Move(Percentage percent)
        {
            var targetPos = CalculateTargetPosition(_startPosition, percent);
            await MoveTask(targetPos, moveTweenX);
        }
        
        private async UniTask ReelBack()
        {
            var targetPos = CalculateTargetPosition(_startPosition, Percentage.FromPercentage(0));
            var distance = Vector2.Distance(transform.localPosition, targetPos);
            reelBackTweenX.duration = distance / reelBackTweenX.duration;
            await MoveTask(targetPos, reelBackTweenX);
        }

        private async UniTask MoveTask(Vector2 targetPos, TweenSettings settings)
        {
            _moveSequence = Sequence.Create()
                .Group(Tween.Custom((Vector2)transform.localPosition, targetPos, settings, x =>
                {
                    transform.localPosition = x;
                    var distance = Vector2.Distance(x, targetPos);
                    line.SetLength(distance);
                }));
            await _moveSequence.ToYieldInstruction().ToUniTask();
        }

        private Vector2 CalculateTargetPosition(Vector2 startPosition, Percentage percent)
        {
            var targetX = Mathf.Lerp(throwRange.x, throwRange.y, percent.AsFraction);
            return new Vector2(targetX, startPosition.y + yOffset);
        }
        
        private Vector2 CalculateLaunchVelocity(Vector2 startPosition, Vector2 targetPosition)
        {
            var distance = Vector2.Distance(startPosition, targetPosition) / div;
            _flightTime = Mathf.Pow(distance, power);

            // Calculate displacement
            var displacement = targetPosition - startPosition;

            // Calculate horizontal velocity (constant)
            var velocityX = displacement.x / _flightTime;

            // Calculate vertical velocity (accounts for gravity)
            // Using the equation: y = vy*t + 0.5*g*t²
            // Rearranged: vy = (y - 0.5*g*t²) / t
            var gravity = Physics.gravity.magnitude;
            var velocityY = (displacement.y - 0.5f * -gravity * _flightTime * _flightTime) / _flightTime;

            var launchVelocity = new Vector2(velocityX, velocityY);
            return launchVelocity;
        }

        public void SetPosition(Percentage percent)
        {
            var targetPos = CalculateTargetPosition(_startPosition, percent);
            transform.localPosition = targetPos;
        }

        public void StopNibble()
        {
            _nibbleSequence.Complete();
        }
        
        void OnDrawGizmosSelected()
        {
            var startPos = transform.position;
            var targetPos = CalculateTargetPosition(startPos, Percentage.FromFraction(0.5f));

            // Draw start and target points
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPos, 0.5f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetPos, 0.5f);

            // Draw line between points
            Gizmos.color = Color.white;
            Gizmos.DrawLine(startPos, targetPos);

            // Draw calculated trajectory
            Vector3 launchVelocity = CalculateLaunchVelocity(startPos, targetPos);
            Gizmos.color = Color.yellow;
            Vector3 previousPoint = startPos;

            for (int i = 1; i <= 20; i++)
            {
                float simulationTime = i / 20f * _flightTime;
                Vector3 displacement = launchVelocity * simulationTime +
                                       Vector3.up * 0.5f * Physics.gravity.y * simulationTime * simulationTime;
                Vector3 currentPoint = startPos + displacement;

                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }
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