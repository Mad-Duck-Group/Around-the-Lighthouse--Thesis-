using System.Collections.Generic;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Madduck.GameData
{
    [CreateAssetMenu(fileName = "CardItemData", menuName = "Madduck/Card/CardItemData", order = 3)]
    [ShowOdinSerializedPropertiesInInspector]
    public class CardItemData : ItemData
    {
        [Title("Card Settings"),
            HideLabel,
            ShowInInspector] private InspectorPlaceholder _cardSettingsTitle;
        [field: SerializeField] public string CardName { get; private set; }
        [field: TextArea,
            SerializeField] public string CardDescription { get; private set; }
        [field: PreviewField,
            SerializeField] public Sprite CardIcon { get; private set; }
        [field: OdinSerialize] public List<BaseModifierData> Modifiers { get; private set; } = new();
    }
}