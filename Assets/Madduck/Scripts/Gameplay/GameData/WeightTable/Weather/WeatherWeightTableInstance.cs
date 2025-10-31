using System;
using System.Linq;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using VContainer;

namespace Madduck.GameData
{
    [Serializable]
    public class WeatherWeightTableInstance : 
        WeightTableInstance<WeatherWeightRecord, WeatherWeightModifierData, WeatherType>
    {
        #region Injection

        [Inject]
        public WeatherWeightTableInstance(
            IWeightTable<WeatherWeightRecord> weatherWeightTable,
            IModifierSource modifierSource)
            : base(weatherWeightTable, modifierSource) { }

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
            modifiedRecords = flattenModifiers.ModifyBy(modifiedRecords, data => data.WeatherType, record => record.Item);
            var totalWeight = modifiedRecords.Sum(record => record.Weight);
            foreach (var record in modifiedRecords)
            {
                record.Probability = Percentage.FromFraction(record.Weight / totalWeight);
            }
        }
        #endregion
    }
}