using System;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Madduck.Day
{
    [CreateAssetMenu(fileName = "New Day Manager Config", menuName = "Madduck/Day/Day Manager Config")]
    public class DayManagerConfig : ScriptableObject
    {
        [Title("General Settings"),
            HideLabel,
            ShowInInspector] private InspectorPlaceholder _generalSettingsTitle;
        [field: SerializeField] public uint MaxDayCount { get; private set; } = 7;
        [field: SerializeField] public uint MaxRoomCount { get; private set; } = 7; 
        [field: InlineProperty,
            SerializeField] public Percentage DayNightRatio { get; private set; } = Percentage.FromPercentage(50);
        
        [Title("Fish Settings"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _fishSettingsTitle;
        [field: 
            SerializeField] public SerializableDictionary<uint, FishCountFormula> FishCountFormulas { get; private set; } = new();
    }

    [Serializable]
    public struct FishCountFormula
    {
        [field: SerializeField] public uint BaseCount { get; private set; }
        [field: SerializeField] public uint IncrementPerRoom { get; private set; }
        public uint Calculate(uint fishingRoomIndex)
        {
            var count = BaseCount + (IncrementPerRoom * fishingRoomIndex);
            return count;
        }
    }
}