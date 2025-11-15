using System;
using System.Linq;
using Madduck.Shared;
using Madduck.Utils;
using VContainer;

namespace Madduck.GameData
{
    [Serializable]
    public class WindDirectionWeightTableInstance : 
        WeightTableInstance<WindDirectionWeightRecord, WindDirectionWeightModifierData, WindDirection>
    {
        #region Injection

        [Inject]
        public WindDirectionWeightTableInstance(
            IWeightTable<WindDirectionWeightRecord> windDirectionWeightTable,
            [Key(DIConstants.ModifierContainerKey)] IModifierSource modifierSource)
            : base(windDirectionWeightTable, modifierSource) { }

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
            modifiedRecords = flattenModifiers.ModifyBy(modifiedRecords, data => data.WindDirection, record => record.Item);
            var totalWeight = modifiedRecords.Sum(record => record.Weight);
            foreach (var record in modifiedRecords)
            {
                record.Probability = Percentage.FromFraction(record.Weight / totalWeight);
            }
        }
        #endregion
    }
}