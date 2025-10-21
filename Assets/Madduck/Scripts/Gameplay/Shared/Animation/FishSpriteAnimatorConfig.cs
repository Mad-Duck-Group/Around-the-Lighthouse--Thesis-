using System.Collections.Generic;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace Madduck.Shared
{
    public enum FishSpriteAnimationKey
    {
        Idle,
        Pulling,
        Exhausted
    }
    
    [CreateAssetMenu(fileName = "FishSpriteAnimatorConfig", menuName = "Madduck/Animation/FishSpriteAnimatorConfig")]
    public class FishSpriteAnimatorConfig : ScriptableObject
    {
        [Title("References"),
         HideLabel,
         ShowInInspector]
        private InspectorPlaceholder _referenceTitle;
        [field: Required, SerializeField] private SkeletonDataAsset skeletonDataAsset;
        [field: SerializeField] public SerializableDictionary<FishSpriteAnimationKey, string> Animations { get; private set; } = new();
        [HideIf("@deconstructedAnimations.Count == 0"),
         TableList,
         SerializeField] private List<DeconstructedAnimationWrapper<FishSpriteAnimationKey>> deconstructedAnimations;
        
        [Button("Deconstruct")]
        private void Deconstruct()
        {
            deconstructedAnimations.Clear();
            foreach (var animation in Animations)
            {
                deconstructedAnimations.Add(new DeconstructedAnimationWrapper<FishSpriteAnimationKey>
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
    }
}