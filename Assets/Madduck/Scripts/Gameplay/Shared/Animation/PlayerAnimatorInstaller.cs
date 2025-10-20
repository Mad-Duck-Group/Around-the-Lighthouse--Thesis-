using System;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Madduck.Shared
{
    [Serializable]
    public class PlayerAnimatorInstaller : IInstaller
    {
        [Title("Player Animator")]
        [Required, 
         SerializeField] private PlayerAnimatorConfig playerAnimatorConfig;
        [Required, 
         SerializeField] private SkeletonAnimation skeletonAnimation;
        
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(playerAnimatorConfig).AsSelf();
            builder.RegisterInstance(skeletonAnimation).AsSelf();
            builder.Register<PlayerAnimator>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}