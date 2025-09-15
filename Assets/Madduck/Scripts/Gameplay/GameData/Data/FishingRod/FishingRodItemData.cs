using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [CreateAssetMenu(fileName = "New Fishing Rod Item Data", menuName = "Madduck/Fishing Rod/Fishing Rod Item Data")]
    public class FishingRodItemData : ItemData
    {
        [Title("References"), 
         HideLabel,
         ShowInInspector] private InspectorVoid _referenceTitle;
        [field: InlineEditor,
                Required,
                SerializeField] public FishingRodStatsData BaseStats { get; private set; }
    }
}