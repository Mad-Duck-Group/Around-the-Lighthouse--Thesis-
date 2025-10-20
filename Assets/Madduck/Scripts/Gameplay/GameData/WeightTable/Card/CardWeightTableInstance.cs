using System.Linq;
using Madduck.Utils;
using MessagePipe;
using VContainer;

namespace Madduck.GameData
{
    public class CardWeightTableInstance : 
        WeightTableInstance<CardWeightRecord, CardWeightModifierData, CardItemData>
    {
        #region Injection

        [Inject]
        public CardWeightTableInstance(
            IWeightTable<CardWeightRecord> cardWeightTable,
            ISubscriber<ModifierSourceEvent> modifierPublisherEventSubscriber)
            : base(cardWeightTable, modifierPublisherEventSubscriber) { }

        #endregion

        #region Utils
        protected override void ApplyFiltersAndModifiers()
        {
            modifiedRecords = BaseRecords.Select(x => x.Copy()).ToList();
            foreach (var filter in PersistentFilters.Values)
            {
                modifiedRecords = filter.Filter(modifiedRecords);
            }
            var flattenModifiers = PersistentModifiers.SelectMany(x => x.Value).ToList();
            modifiedRecords = flattenModifiers.ModifyBy(modifiedRecords, data => data.ItemData, record => record.Item);
            var totalWeight = modifiedRecords.Sum(record => record.Weight);
            foreach (var record in modifiedRecords)
            {
                record.Probability = Percentage.FromFraction(record.Weight / totalWeight);
            }
        }
        #endregion
    }
}