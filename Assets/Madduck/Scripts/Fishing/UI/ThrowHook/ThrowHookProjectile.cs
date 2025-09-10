using Cysharp.Threading.Tasks;
using Madduck.Scripts.Utils.Others;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Madduck.Scripts.Fishing.UI.ThrowHook
{
    public class ThrowHookProjectile : MonoBehaviour
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

        public async UniTask Nibble(int? cycle)
        {
            var finalCycle = cycle ?? 1;
            _nibbleSequence = Sequence.Create(finalCycle, CycleMode.Yoyo)
                .Group(Tween.LocalPosition(hookIcon, nibbleTween.ToVector3().ToRelative(hookIcon.localPosition)));
            await _nibbleSequence.ToYieldInstruction().ToUniTask();
        }
    }

    public class ThrowHookProjectileFactory
    {
        public ThrowHookProjectile CurrentHook { get; private set; }
        private readonly ThrowHookProjectile _prefab;
        private readonly Transform _parent;

        [Inject]
        public ThrowHookProjectileFactory(
            ThrowHookProjectile prefab, 
            [Key("ProjectileParent")] Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }
        
        public ThrowHookProjectile Create()
        {
            if (CurrentHook) return CurrentHook;
            CurrentHook = Object.Instantiate(_prefab, _parent.position, Quaternion.identity, _parent);
            return CurrentHook;
        }
    }
}