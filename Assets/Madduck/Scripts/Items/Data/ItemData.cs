using System;
using MadDuck.Scripts.Utils.Inspectors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MadDuck.Scripts.Items.Data
{
    public enum ItemType
    {
        Fish,
        FishingRod,
    }
    public abstract class ItemData : ScriptableObject
    {
        [Title("Base Settings"), 
         HideLabel,
         ShowInInspector] private InspectorVoid _baseSettingsTitle;
        [field: HideInInspector, 
                SerializeField]
        public byte[] Guid { get; private set; } = System.Guid.NewGuid().ToByteArray();
        [DisplayAsString, 
         ShowInInspector] private string GuidString => new Guid(Guid).ToString();
        [Button("Generate New GUID")]
        private void GenerateNewGuid()
        {
            Guid = System.Guid.NewGuid().ToByteArray();
        }
        [field: SerializeField] public ItemType ItemType { get; private set; }
        [field: SerializeField] public bool IsStackable { get; private set; }
        [field: ShowIf(nameof(IsStackable)), 
                SerializeField] public uint MaxStackSize { get; private set; } = 2;
    }
}
