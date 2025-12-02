using System.Collections.Generic;
using Madduck.GameData.Fisherman;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [CreateAssetMenu(fileName = "Fisherman Item Data", menuName = "Madduck/Fisherman/Fisherman Item Data", order = 0)]
    public class FishermanItemData : ItemData
    {
        [Title("Fisherman Settings"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _fishermanSettingsTitle;
        [field: InlineProperty,
            SerializeField] public UFloat MaxStamina { get; private set; } = 100f;
    }
}