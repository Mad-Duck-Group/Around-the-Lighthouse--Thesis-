using System;
using System.Collections.Generic;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Madduck.GameData
{
    [Flags]
    public enum BaitType
    {
        None = 0,
        Cheese = 1 << 0,
        Worm = 1 << 1,
        Fake = 1 << 2,
        Glow = 1 << 3,
    }
    
    [CreateAssetMenu(fileName = "New Bait Item Data", menuName = "Madduck/Bait/Bait Item Data")]
    public class BaitItemData : ItemData, IHasModifier
    {
        [Title("Bait Settings"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _baitSettingsTitle;
        [field: UnflagEnum,
            SerializeField] public BaitType BaitType { get; private set; }
        [field: SerializeField] public string BaitName { get; private set; }
        [field: TextArea,
            SerializeField] public string BaitDescription { get; private set; }
        [field: PreviewField,
            SerializeField] public Sprite BaitIcon { get; private set; }
        [field: PreviewField,
                SerializeField] public Sprite BaitSelectedIcon { get; private set; }
        [field: OdinSerialize] public List<BaseModifierData> Modifiers { get; private set; }
    }
}
