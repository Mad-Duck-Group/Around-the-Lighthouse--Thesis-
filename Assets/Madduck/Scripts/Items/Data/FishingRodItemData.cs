using MadDuck.Scripts.FishingRods;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MadDuck.Scripts.Items.Data
{
    [CreateAssetMenu(fileName = "New Fishing Rod Item Data", menuName = "Fishing Rod/Fishing Rod Item")]
    public class FishingRodItemData : ItemData
    {
        [Title("References")]
        [field: SerializeField, InlineEditor, Required] public FishingRodStatsData BaseStats { get; private set; }
    }
}