using System.Collections.Generic;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace Madduck.Shared
{
    public enum PlayerAnimationKey
    {
        Idle1,
        Idle2,
        IdleRod,
        PrepareThrow,
        ChargingThrow,
        ReleaseThrow,
        PullHookUp,
        Reeling,
        Pulling,
        GotFish
    }

    [CreateAssetMenu(fileName = "PlayerAnimatorConfig", menuName = "Madduck/Animation/PlayerAnimatorConfig")]
    public class PlayerAnimatorConfig : ScriptableObject
    {
        [Title("References"),
         HideLabel,
         ShowInInspector]
        private InspectorPlaceholder _referenceTitle;
        [field: Required, SerializeField] private SkeletonDataAsset skeletonDataAsset;
        [field: SerializeField] public SerializableDictionary<PlayerAnimationKey, string> Animations { get; private set; } = new();
        [HideIf("@deconstructedAnimations.Count == 0"),
         TableList,
         SerializeField] private List<DeconstructedAnimationWrapper<PlayerAnimationKey>> deconstructedAnimations;
        
        [Button("Deconstruct")]
        private void Deconstruct()
        {
            deconstructedAnimations.Clear();
            foreach (var animation in Animations)
            {
                deconstructedAnimations.Add(new DeconstructedAnimationWrapper<PlayerAnimationKey>
                    (skeletonDataAsset, animation.Key, animation.Value));
            }
        }
        
        [Button("Apply Changes")]
        private void ApplyChanges()
        {
            Animations.Clear();
            foreach (var animation in deconstructedAnimations)
            {
                Animations.Add(animation.key, animation.animation);
            }
            deconstructedAnimations.Clear();
        }
        
        [Title("Settings"),
         HideLabel,
         ShowInInspector]
        private InspectorPlaceholder _settingTitle;
        [field: SerializeField] public Vector2 IdleSwitchInterval { get; private set; } = new(5f, 10f);
    }
}