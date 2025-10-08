using Cysharp.Threading.Tasks;
using Madduck.GameData;
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
        UniTask Return(bool reelBack = true);
        UniTask MoveX(Percentage percent);
        UniTask MoveY(Percentage percent);
        UniTask Nibble(int? cycle);
        void SetPositionX(Percentage percent);
        void SetPositionY(Percentage percent);
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
        [SerializeField] private Vector2 yOffsetRange = new(-2f, 0f);
        [SerializeField] private float div = 4f;
        [SerializeField] private float power = 0.7f;

        [Title("Tween")] 
        [SerializeField] private TweenSettings moveTweenX;
        [SerializeField] private TweenSettings moveTweenY;
        [InfoBox("Duration property of reelBackTweenX is speed"),
         SerializeField] private TweenSettings reelBackTweenX;
        [SerializeField] private TweenSettings<Vector2> nibbleTween;

        #endregion

        #region Fields

        private Transform _rodTip;
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
            _rodTip = rodTip;
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
            var targetPos = _rodTip.position
                .WithX(CalculateTargetPositionX(percent))
                .WithY(CalculateTargetPositionY(Percentage.Zero));
            var velocity = CalculateLaunchVelocity(_rodTip.position, targetPos);
            _rigidbody.linearVelocity = velocity;
            var distance = Vector2.Distance(_rodTip.position, targetPos);
            line.CastLine(_flightTime, distance, true);
            await UniTask.WaitForSeconds(_flightTime);
            _rigidbody.constraints = RigidbodyConstraints2D.FreezePosition;
        }

        /// <summary>
        /// Returns the hook to the starting position.
        /// </summary>
        public async UniTask Return(bool reelBack = true)
        {
            if (!_isThrown) return;
            _isThrown = false;
            if (reelBack) await ReelBack();
            _rigidbody.constraints = RigidbodyConstraints2D.None;
            var velocity = CalculateLaunchVelocity(transform.position, _rodTip.position);
            _rigidbody.linearVelocity = velocity;
            var distance = Vector2.Distance(transform.position, _rodTip.position);
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

        public async UniTask MoveX(Percentage percent)
        {
            var targetX = CalculateTargetPositionX(percent);
            var targetPos = transform.position.WithX(targetX);
            var sequence = Sequence.Create()
                .Group(Tween.Custom(transform.position.x, targetX, moveTweenX, x =>
                {
                    var currentPos = transform.position.WithX(x);
                    transform.position = currentPos;
                    var distance = Vector2.Distance(currentPos, targetPos);
                    line.SetLength(distance);
                }));
            await sequence.ToYieldInstruction().ToUniTask();
        }

        public async UniTask MoveY(Percentage percent)
        {
            var targetY = CalculateTargetPositionY(percent);
            var targetPos = transform.position.WithY(targetY);
            var sequence = Sequence.Create()
                .Group(Tween.Custom(transform.position.y, targetY, moveTweenY, y =>
                {
                    var currentPos = transform.position.WithY(y);
                    transform.position = currentPos;
                    var distance = Vector2.Distance(currentPos, targetPos);
                    line.SetLength(distance);
                }));
            await sequence.ToYieldInstruction().ToUniTask();
        }
        
        private async UniTask ReelBack()
        {
            var targetPos = _rodTip.position
                .WithX(CalculateTargetPositionX(Percentage.Zero))
                .WithY(transform.position.y);
            var distance = Vector2.Distance(transform.position, targetPos);
            reelBackTweenX.duration = distance / reelBackTweenX.duration;
            await MoveTask(targetPos, reelBackTweenX);
        }

        private async UniTask MoveTask(Vector2 targetPos, TweenSettings settings)
        {
            _moveSequence = Sequence.Create()
                .Group(Tween.Custom((Vector2)transform.position, targetPos, settings, x =>
                {
                    transform.position = x;
                    var distance = Vector2.Distance(x, targetPos);
                    line.SetLength(distance);
                }));
            await _moveSequence.ToYieldInstruction().ToUniTask();
        }

        private float CalculateTargetPositionX(Percentage percentX)
        {
            var targetX = Mathf.Lerp(throwRange.x, throwRange.y, percentX.AsFraction);
            return targetX;
        }
        
        private float CalculateTargetPositionY(Percentage percentY)
        {
            var targetY = Mathf.Lerp(yOffsetRange.x, yOffsetRange.y, percentY.AsFraction);
            return targetY;
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

        public void SetPositionX(Percentage percent)
        {
            var targetPos = _rodTip.position
                    .WithX(CalculateTargetPositionX(percent))
                    .WithY(transform.position.y);
            transform.position = targetPos;
            var distance = Vector2.Distance(transform.position, targetPos);
            line.SetLength(distance);
        }
        
        public void SetPositionY(Percentage percent)
        {
            var targetPos = _rodTip.position
                .WithX(transform.position.x)
                .WithY(CalculateTargetPositionY(percent));
            transform.position = targetPos;
            var distance = Vector2.Distance(transform.position, targetPos);
            line.SetLength(distance);
        }

        public void StopNibble()
        {
            _nibbleSequence.Complete();
        }
        
        void OnDrawGizmosSelected()
        {
            var startPos = (Vector2)transform.position;
            var bottomLeft = startPos
                .WithX(CalculateTargetPositionX(Percentage.Zero))
                .WithY(CalculateTargetPositionY(Percentage.Zero));
            var bottomMiddle = startPos
                .WithX(CalculateTargetPositionX(Percentage.Half))
                .WithY(CalculateTargetPositionY(Percentage.Zero));
            var bottomRight = startPos
                .WithX(CalculateTargetPositionX(Percentage.Full))
                .WithY(CalculateTargetPositionY(Percentage.Zero));
            var topLeft = startPos
                .WithX(CalculateTargetPositionX(Percentage.Zero))
                .WithY(CalculateTargetPositionY(Percentage.Full));
            var topRight = startPos
                .WithX(CalculateTargetPositionX(Percentage.Full))
                .WithY(CalculateTargetPositionY(Percentage.Full));

            // Draw start and target points
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPos, 0.5f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(bottomMiddle, 0.5f);

            // Draw displacement
            Gizmos.color = Color.white;
            Gizmos.DrawLine(startPos, bottomMiddle);
            
            // Draw bottom line
            Gizmos.color = Color.white;
            Gizmos.DrawLine(bottomLeft, bottomRight);
            
            // Draw top line
            Gizmos.color = Color.white;
            Gizmos.DrawLine(topLeft, topRight);

            // Draw calculated trajectory
            var launchVelocity = CalculateLaunchVelocity(startPos, bottomMiddle);
            Gizmos.color = Color.yellow;
            var previousPoint = startPos;

            for (int i = 1; i <= 20; i++)
            {
                var simulationTime = i / 20f * _flightTime;
                var displacement = launchVelocity * simulationTime +
                                       Vector2.up * 0.5f * Physics.gravity.y * simulationTime * simulationTime;
                var currentPoint = startPos + displacement;

                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }
        }

        #endregion
    }

    public class HookProjectileMock : IHookProjectile
    {
        public UniTask Throw(Percentage percent) => UniTask.CompletedTask;

        public UniTask Return(bool reelBack = true) => UniTask.CompletedTask;

        public UniTask MoveX(Percentage percent) => UniTask.CompletedTask;
        public UniTask MoveY(Percentage percent) => UniTask.CompletedTask;

        public UniTask Nibble(int? cycle) => UniTask.CompletedTask;
        public void SetPositionX(Percentage percent){}
        public void SetPositionY(Percentage percent){}
        public void StopNibble(){}
    }
}