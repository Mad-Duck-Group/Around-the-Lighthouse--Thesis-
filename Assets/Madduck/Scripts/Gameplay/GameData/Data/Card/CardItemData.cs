using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    public enum CardRarity
    {
        Common,
        Uncommon,
        Rare,
        Unique
    }
    
    [CreateAssetMenu(fileName = "CardItemData", menuName = "Madduck/Card/CardItemData", order = 3)]
    [ShowOdinSerializedPropertiesInInspector]
    public class CardItemData : ItemData
    {
        [Title("Card Settings"),
            HideLabel,
            ShowInInspector] private InspectorPlaceholder _cardSettingsTitle;
        [field: SerializeField] public SerializableDictionary<CardRarity, CardRarityData> RarityData { get; private set; } = new();
    }
}