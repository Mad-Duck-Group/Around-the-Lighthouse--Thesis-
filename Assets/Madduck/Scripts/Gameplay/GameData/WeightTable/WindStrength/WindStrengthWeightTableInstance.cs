using System;
using System.Linq;
using Madduck.Utils;
using VContainer;

namespace Madduck.GameData
{
    [Serializable]
    public class WindStrengthWeightTableInstance : 
        WeightTableInstance<WindStrengthWeightRecord, WindStrengthWeightModifierData, WindStrength>
    {
        #region Injection

        [Inject]
        public WindStrengthWeightTableInstance(
            IWeightTable<WindStrengthWeightRecord> windStrengthWeightTable,
            IModifierSource modifierSource)
            : base(windStrengthWeightTable, modifierSource) { }

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
            modifiedRecords = flattenModifiers.ModifyBy(modifiedRecords, data => data.WindStrength, record => record.Item);
            var totalWeight = modifiedRecords.Sum(record => record.Weight);
            foreach (var record in modifiedRecords)
            {
                record.Probability = Percentage.FromFraction(record.Weight / totalWeight);
            }
        }
        #endregion
    }
}