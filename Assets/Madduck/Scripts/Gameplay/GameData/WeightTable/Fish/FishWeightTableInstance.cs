using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using VContainer;

namespace Madduck.GameData
{
    [Serializable]
    public class FishWeightTableInstance : 
        WeightTableInstance<FishWeightRecord, FishWeightModifierData, FishItemData>
    {
        #region Injection

        [Inject]
        public FishWeightTableInstance(
            IWeightTable<FishWeightRecord> fishWeightTable,
            [Key(DIConstants.ModifierContainerKey)] IModifierSource modifierSource) 
            : base(fishWeightTable, modifierSource) { }

        #endregion

        #region Utils

        protected override void ApplyFiltersAndModifiers()
        {
            modifiedRecords = BaseRecords.Select(x => x.Copy()).ToList();
            foreach (var filter in PersistentFilters.Values)
            {
                modifiedRecords = filter.Filter(modifiedRecords);
            }
            modifiedRecords = Modify(modifiedRecords);
            var totalWeight = modifiedRecords.Sum(record => record.Weight);
            foreach (var record in modifiedRecords)
            {
                record.Probability = Percentage.FromFraction(record.Weight / totalWeight);
            }
        }
        
        private List<FishWeightRecord> Modify(List<FishWeightRecord> records)
        {
            var copy = records.Select(x => x.Copy()).ToList();
            var flattenedModifiers = PersistentModifiers.Values.SelectMany(x => x).ToList();
            var bucket = BucketModifiers(copy, flattenedModifiers);
            foreach (var pair in bucket)
            {
                pair.Key.Weight = pair.Value.Calculate(pair.Key.Weight);
            }
            return copy;
        }

        private static Dictionary<FishWeightRecord, List<FishWeightModifierData>> BucketModifiers(
            List<FishWeightRecord> records,
            List<FishWeightModifierData> modifiers)
        {
            var dictionary = records.Distinct().ToDictionary(x => x, _ => new List<FishWeightModifierData>());

            foreach (var modifier in modifiers)
            {
                foreach (var record in records)
                {
                    switch (modifier.ModifierType)
                    {
                        case FishModifierType.All:
                            dictionary[record].Add(modifier);
                            break;
                        case FishModifierType.Name:
                            if (!modifier.FishItemData.Guid.Equals(record.Item.Guid)) break;
                            dictionary[record].Add(modifier);
                            break;
                        case FishModifierType.Size:
                            if (modifier.FishSize != record.Item.Size) break;
                            dictionary[record].Add(modifier);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            return dictionary;
        }

        #endregion
    }
}