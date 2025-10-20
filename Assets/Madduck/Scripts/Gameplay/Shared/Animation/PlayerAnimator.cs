using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using Spine;
using Spine.Unity;
using UnityEngine;
using VContainer;

namespace Madduck.Shared
{
    public interface IIdleAnimator
    {
        void StartIdle();
        void StopIdle();
    }
    public class PlayerAnimator : ISpineAnimator<PlayerAnimationKey>, IIdleAnimator
    {
        private readonly PlayerAnimatorConfig _config;
        private readonly SkeletonAnimation _skeletonAnimation;
        private CancellationTokenSource _idleCts = new();
        
        [Inject]
        public PlayerAnimator(
            PlayerAnimatorConfig config,
            SkeletonAnimation skeletonAnimation)
        {
            _config = config;
            _skeletonAnimation = skeletonAnimation;
        }
        
        public TrackEntry Set(PlayerAnimationKey key, int index, bool loop)
        {
            if (!_config.Animations.TryGetValue(key, out var animation))
            {
                return null;
            }
            return _skeletonAnimation.AnimationState.SetAnimation(index, animation, loop);
        }

        public TrackEntry Add(PlayerAnimationKey key, int index, bool loop, float delay)
        {
            if (!_config.Animations.TryGetValue(key, out var animation))
            {
                return null;
            }
            return _skeletonAnimation.AnimationState.AddAnimation(index, animation, loop, delay);
        }

        public TrackEntry SetEmpty(int index, float mixDuration)
        {
            return _skeletonAnimation.AnimationState.SetEmptyAnimation(index, mixDuration);
        }

        public void SetEmptyAll(float mixDuration)
        {
            _skeletonAnimation.AnimationState.SetEmptyAnimations(mixDuration);
        }

        public TrackEntry AddEmpty(int index, float mixDuration, float delay)
        {
            return _skeletonAnimation.AnimationState.AddEmptyAnimation(index, mixDuration, delay);
        }

        public void ClearTrack(int index)
        {
            _skeletonAnimation.AnimationState.ClearTrack(index);
        }

        public void ClearTracks()
        {
            _skeletonAnimation.AnimationState.ClearTracks();
        }

        public TrackEntry GetCurrent(int index)
        {
            return _skeletonAnimation.AnimationState.GetCurrent(index);
        }

        public void StartIdle()
        {
            _idleCts = new();
            Set(PlayerAnimationKey.Idle1, 0, true);
            SwitchIdle(_idleCts.Token).Forget();
        }

        public void StopIdle()
        {
            _idleCts.Cancel();
        }
        
        private async UniTask SwitchIdle(CancellationToken cancellationToken)
        {
            var randomInterval = Random.Range(_config.IdleSwitchInterval.x, _config.IdleSwitchInterval.y);
            await UniTask.WaitForSeconds(randomInterval, cancellationToken: cancellationToken);
            await Set(PlayerAnimationKey.Idle2, 0, false).WaitUntilComplete(cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            Set(PlayerAnimationKey.Idle1, 0, true);
            SwitchIdle(cancellationToken).Forget();
        }
    }

    public class PlayerAnimatorMock : ISpineAnimator<PlayerAnimationKey>, IIdleAnimator
    {
        public TrackEntry Set(PlayerAnimationKey key, int index, bool loop) => null;

        public TrackEntry Add(PlayerAnimationKey key, int index, bool loop, float delay) => null;

        public TrackEntry SetEmpty(int index, float mixDuration) => null;

        public void SetEmptyAll(float mixDuration){}

        public TrackEntry AddEmpty(int index, float mixDuration, float delay) => null;

        public void ClearTrack(int index){}

        public void ClearTracks(){}

        public TrackEntry GetCurrent(int index) => null;
        public void StartIdle(){}

        public void StopIdle(){}
    }
}