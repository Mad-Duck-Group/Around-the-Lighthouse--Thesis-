using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Madduck.GameData
{
    [Flags]
    public enum WeatherType
    {
        Clear = 1 << 0,
        Rain = 1 << 1,
        Storm = 1 << 2,
        Cloudy = 1 << 3,
        StrongWinds = 1 << 4,
        Mist = 1 << 5,
        All = Clear | Rain | Storm | Cloudy | StrongWinds | Mist
    }
    
    public enum WindStrength
    {
        None,
        Low,
        Medium,
        High
    }
    
    public enum WindDirection
    {
        Left,
        Middle,
        Right
    }
    
    [CreateAssetMenu(fileName = "New Weather Item Data", menuName = "Madduck/Weather/Weather Item Data")]
    public class WeatherItemData : ItemData
    {
        [Title("Weather Settings"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _weatherSettingsTitle;
        [field: UnflagEnum,
            SerializeField] public WeatherType WeatherType { get; private set; } = WeatherType.Clear;
        [field: Required, InlineEditor,
                SerializeField] public WindDirectionWeightTable WindDirectionWeightTable { get; private set; }
        [field: Required, InlineEditor,
                SerializeField] public WindStrengthWeightTable WindStrengthWeightTable { get; private set; }
        [field: HideReferenceObjectPicker,
            OdinSerialize] private SerializableDictionary<WindDirection, WindStrengthModifier> windModifiers = new();
        public IReadOnlyDictionary<WindDirection, WindStrengthModifier> WindModifiers => 
            windModifiers.ToDictionary(
                kvp => kvp.Key, 
                kvp => kvp.Value);
    }

    [Serializable]
    public record WindStrengthModifier
    {
        [HideReferenceObjectPicker, 
         SerializeField] private SerializableDictionary<WindStrength, List<BaseModifierData>> modifiersDictionary = new();
        public IReadOnlyDictionary<WindStrength, IReadOnlyList<BaseModifierData>> ModifiersDictionary => 
            modifiersDictionary.ToDictionary(
                kvp => kvp.Key, 
                kvp =>
                {
                    if (kvp.Value is null || kvp.Value.Count == 0) return Array.Empty<BaseModifierData>();
                    return (IReadOnlyList<BaseModifierData>)kvp.Value.Select(x => x.Copy()).ToList();
                });
    }
}