using Madduck.Utils;
using Spine;
using Spine.Unity;
using VContainer;

namespace Madduck.Shared
{
    public class FishSpriteAnimator : ISpineAnimator<FishSpriteAnimationKey>
    {
        private readonly FishSpriteAnimatorConfig _config;
        private readonly SkeletonAnimation _skeletonAnimation;
        
        [Inject]
        public FishSpriteAnimator(
            FishSpriteAnimatorConfig config,
            SkeletonAnimation skeletonAnimation)
        {
            _config = config;
            _skeletonAnimation = skeletonAnimation;
        }
        
        public TrackEntry Set(FishSpriteAnimationKey key, int index, bool loop)
        {
            if (!_config.Animations.TryGetValue(key, out var animation))
            {
                return null;
            }
            return _skeletonAnimation.AnimationState.SetAnimation(index, animation, loop);
        }

        public TrackEntry Add(FishSpriteAnimationKey key, int index, bool loop, float delay)
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
    }

    public class FishSpriteAnimatorMock : ISpineAnimator<FishSpriteAnimationKey>
    {
        public TrackEntry Set(FishSpriteAnimationKey key, int index, bool loop) => null;
        public TrackEntry Add(FishSpriteAnimationKey key, int index, bool loop, float delay) => null;
        public TrackEntry SetEmpty(int index, float mixDuration) => null;
        public void SetEmptyAll(float mixDuration) {}
        public TrackEntry AddEmpty(int index, float mixDuration, float delay) => null;
        public void ClearTrack(int index) {}
        public void ClearTracks() {}
        public TrackEntry GetCurrent(int index) => null;
    }
}
