using System.Collections.Generic;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace Madduck.RoomPreset
{
    public enum EnvironmentAnimType
    {
        Normal,
        Storm
    }
    [CreateAssetMenu(fileName = "EnvironmentAnimConfig ", menuName = "Madduck/Animation/EnvironmentAnimConfig ")]
    public class EnvironmentAnimConfig : ScriptableObject
    {
        [Title("References"),
         HideLabel,
         ShowInInspector]
        private InspectorPlaceholder _referenceTitle;

        [field: Required, SerializeField]
        private SkeletonDataAsset skeletonDataAsset;

        [field: SerializeField]
        public SerializableDictionary<EnvironmentAnimType, string> Animations { get; private set; } = new();

        [HideIf("@deconstructedAnimations.Count == 0"),
         TableList,
         SerializeField]
        private List<DeconstructedAnimationWrapper<EnvironmentAnimType>> deconstructedAnimations;

        [Button("Deconstruct")]
        private void Deconstruct()
        {
            deconstructedAnimations.Clear();
            foreach (var animation in Animations)
            {
                deconstructedAnimations.Add(new DeconstructedAnimationWrapper<EnvironmentAnimType>
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
