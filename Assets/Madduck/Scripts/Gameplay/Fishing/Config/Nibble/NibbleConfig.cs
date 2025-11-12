using FMODUnity;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Fishing.Config
{
    [CreateAssetMenu(fileName = "Nibble Config", menuName = "Madduck/Fishing/Nibble Config", order = 2)]
    public class NibbleConfig : ScriptableObject
    {
        [Title("Qte"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _qteTitle;
        [field: SerializeField] public Vector2 QteIntervalRange { get; private set; } = new(3, 8);
        
        [Title("Audio"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _audioTitle;
        [field: SerializeField] public EventReference ReelingSfx { get; private set; }
        [field: SerializeField] public EventReference PullHookSfx { get; private set; }
        [field: SerializeField] public EventReference FishBiteSfx { get; private set; }
    }
}