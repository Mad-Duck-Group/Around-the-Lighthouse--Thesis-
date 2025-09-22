using System;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    public enum ItemType
    {
        Fish,
        FishingRod,
        Fisherman,
        Card,
    }
    public abstract class ItemData : ScriptableObject
    {
        [Title("Base Settings"), 
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _baseSettingsTitle;

        [field: ShowInInspector] public Guid Guid { get; private set; } = Guid.NewGuid();
        [field: SerializeField] public ItemType ItemType { get; private set; }
        [field: SerializeField] public bool IsStackable { get; private set; }
        [field: ShowIf(nameof(IsStackable)), 
                SerializeField] public uint MaxStackSize { get; private set; } = 2;
    }
}
