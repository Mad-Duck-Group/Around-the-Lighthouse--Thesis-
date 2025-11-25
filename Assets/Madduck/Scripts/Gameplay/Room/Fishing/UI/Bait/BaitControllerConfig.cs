using FMODUnity;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Room
{
    [CreateAssetMenu(fileName = "BaitControllerConfig", menuName = "Madduck/Room/BaitControllerConfig")]
    public class BaitControllerConfig : ScriptableObject
    {
        [Title("Audio"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _audioTitle;
        [field: SerializeField] public EventReference CycleBaitSfx { get; private set; }
    }
}