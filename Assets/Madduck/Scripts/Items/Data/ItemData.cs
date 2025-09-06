using System;
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
        [Title("Base Settings")]
        [field: SerializeField, HideInInspector]
        public byte[] Guid { get; private set; } = System.Guid.NewGuid().ToByteArray();
        [ShowInInspector, DisplayAsString] private string GuidString => new Guid(Guid).ToString();
        [Button("Generate New GUID")]
        private void GenerateNewGuid()
        {
            Guid = System.Guid.NewGuid().ToByteArray();
        }
        [field: SerializeField] public ItemType ItemType { get; private set; }
        [field: SerializeField] public bool CanPutInContainer { get; private set; }
        [field: SerializeField] public bool CanPlaceOnGround { get; private set; }
        [field: SerializeField] public bool IsStackable { get; private set; }
        [field: SerializeField, ShowIf(nameof(IsStackable))] public uint MaxStackSize { get; private set; } = 2;
    }
}
