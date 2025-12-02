using FMODUnity;
using Madduck.Utils;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Core
{
    [CreateAssetMenu(fileName = "LoadSceneManagerConfig", menuName = "Madduck/Core/LoadSceneManagerConfig")]
    [ShowOdinSerializedPropertiesInInspector]
    public class LoadSceneManagerConfig : ScriptableObject
    {
        [Title("Scenes"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _sceneTitle;
        [field: SerializeField] public SerializableDictionary<SceneType, SceneReference> SceneReferences { get; private set; }

        [Title("Transition"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _transitionTitle;
        [field: SerializeField] public bool MinimumLoadingScreenDuration { get; private set; } = true;
        [field: ShowIf(nameof(MinimumLoadingScreenDuration)),
                SerializeField] public float LoadingScreenDuration { get; private set; } = 1f;
        [field: SerializeField] public EventReference TransitionSfx { get; private set; }
    }
}