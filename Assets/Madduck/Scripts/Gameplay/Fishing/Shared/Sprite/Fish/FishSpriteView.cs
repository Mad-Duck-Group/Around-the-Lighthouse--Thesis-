using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using PrimeTween;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace Madduck.Fishing.Shared
{
    public interface IFishSpriteView : ITransitionable
    {
        void SetUp(Transform hook, FishItemInstance fishItemInstance);
        void Detach();
        ISpineAnimator<FishSpriteAnimationKey> Animator { get; }
        IFishFatigueTimerView FatigueTimerView { get; }
    }
    public class FishSpriteView : MonoBehaviour, IFishSpriteView
    {
        [Title("References")]
        [Required,
         SerializeField] private SkeletonAnimation skeletonAnimation;
        [Required,
         SerializeField] private SkeletonUtility skeletonUtility;
        [Required,
         SerializeField] private FishFatigueTimerView fatigueTimerView;
        
        [Title("Settings")] 
        [SerializeField] private TweenSettings<Vector2> spawnPositionTween;

        [Title("Debug")] 
        [InlineEditor, 
         SerializeField] 
        private FishItemData debugFish;

        private TweenSettings<Vector2> _spawnRelativePositionTween;
       
        public ISpineAnimator<FishSpriteAnimationKey> Animator { get; private set; }
        public IFishFatigueTimerView FatigueTimerView => fatigueTimerView;


        private Sequence _transitionSequence;
        
        public void SetUp(Transform hook, FishItemInstance fishItemInstance)
        {
            transform.position = hook.position;
            transform.position -= (Vector3)fishItemInstance.ItemData.SpriteAnchorOffset;
            _spawnRelativePositionTween = spawnPositionTween.ToRelative(transform.position);
            transform.position = _spawnRelativePositionTween.startValue;
            var isBoss = fishItemInstance.ItemData.EnemyType is FishEnemyType.Boss;
            skeletonAnimation.initialSkinName = isBoss ? string.Empty : fishItemInstance.ItemData.FishSkin;
            skeletonAnimation.skeletonDataAsset = fishItemInstance.ItemData.FishSkeletonDataAsset;
            ((RectTransform)fatigueTimerView.transform).anchoredPosition -= fishItemInstance.ItemData.FatigueSliderOffset;
            skeletonAnimation.Initialize(true);
            Animator = new FishSpriteAnimator(fishItemInstance.ItemData.FishSpriteAnimatorConfig, skeletonAnimation);
            transform.SetParent(hook);
            skeletonUtility.SpawnHierarchy(SkeletonUtilityBone.Mode.Follow, true, true, true);
        }

        public void Detach()
        {
            transform.SetParent(null);
        }


        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(CancelTransition);
            await Transition(true);
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            _spawnRelativePositionTween = spawnPositionTween.ToRelative(transform.position);
            cancellationToken.Register(CancelTransition);
            await Transition(false);
        }

        private async UniTask Transition(bool forward)
        {
            _transitionSequence = Sequence.Create()
                .Group(Tween.Position(transform, _spawnRelativePositionTween.ToVector3().WithDirection(forward)));
            await _transitionSequence.ToYieldInstruction().ToUniTask();
        }

        private void CancelTransition()
        {
            _transitionSequence.Complete();
        }

        private void OnDrawGizmosSelected()
        {
            var anchoredPos = transform.position + (Vector3)debugFish.SpriteAnchorOffset;
            var fatiguePos = ((RectTransform)fatigueTimerView.transform).anchoredPosition + debugFish.FatigueSliderOffset;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(anchoredPos, 0.5f);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(fatiguePos, 0.5f);
        }
    }
}