using Madduck.GameData;
using Spine.Unity;
using UnityEngine;

namespace Madduck.RoomPreset
{
    public class EnvironmentAnim : MonoBehaviour
    {
        [SerializeField] private EnvironmentAnimConfig config;
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        public void SetAnimator(WeatherType weatherType)
        {
            EnvironmentAnimType animType =
                weatherType == WeatherType.Storm
                    ? EnvironmentAnimType.Storm
                    : EnvironmentAnimType.Normal;

            string animName = config.Animations[animType];
            skeletonAnimation.AnimationState.SetAnimation(0, animName, true);
        }
    }
}
