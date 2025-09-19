using System;
using Cysharp.Threading.Tasks;
using Madduck.Shared;
using Madduck.Utils;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using Object = UnityEngine.Object;

namespace Madduck.Fishing.Shared
{
    public class HookProjectile : MonoBehaviour
    {
        [Title("References")] 
        [Required] 
        [SerializeField] private Transform hookIcon;
        
        [Title("Tween")] 
        [SerializeField] private TweenSettings throwTweenX;
        [SerializeField] private TweenSettings<float> throwTweenY;
        [SerializeField] private TweenSettings<Vector2> nibbleTween;
        
        private Vector2 _startPosition;
        private float _targetDistance;
        private bool _isThrown;
        private Sequence _throwSequence;
        private Sequence _nibbleSequence;
        
        private void Awake()
        {
            _startPosition = transform.localPosition;
        }

        /// <summary>
        /// Throws the hook to the specified distance.
        /// </summary>
        /// <param name="distance"></param>
        public async UniTask Throw(float distance)
        {
            if (_isThrown) return;
            _isThrown = true;
            _targetDistance = distance;
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

        public void StopNibble()
        {
            _nibbleSequence.Complete();
        }
    }

    [Serializable]
    public class HookProjectileFactory
    {
        [Required, AssetsOnly,
         SerializeField] private HookProjectile prefab;
        [Required, 
         SerializeField] private Transform parent;
        public HookProjectile CurrentHook { get; private set; }
        
        public HookProjectileFactory(
            HookProjectile prefab, 
            Transform parent)
        {
            this.prefab = prefab;
            this.parent = parent;
        }
        
        public HookProjectile Create()
        {
            if (CurrentHook) return CurrentHook;
            CurrentHook = Object.Instantiate(prefab, parent.position, Quaternion.identity, parent);
            return CurrentHook;
        }
        
        public void DestroyHook()
        {
            if (!CurrentHook) return;
            Object.Destroy(CurrentHook.gameObject);
            CurrentHook = null;
        }
    }
}