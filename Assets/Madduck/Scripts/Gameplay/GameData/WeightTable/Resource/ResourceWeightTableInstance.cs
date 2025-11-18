using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Shared;
using Madduck.Utils;
using VContainer;

namespace Madduck.GameData
{
    [Serializable]
    public class ResourceWeightTableInstance : 
        WeightTableInstance<ResourceWeightRecord, ResourceWeightModifierData, ResourceItemData>
    {
        #region Injection

        [Inject]
        public ResourceWeightTableInstance(
            IWeightTable<ResourceWeightRecord> resourceWeightTable,
            [Key(DIConstants.ModifierContainerKey)] IModifierSource modifierSource) 
            : base(resourceWeightTable, modifierSource) { }

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
        
        private List<ResourceWeightRecord> Modify(List<ResourceWeightRecord> records)
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

        public static Dictionary<ResourceWeightRecord, List<ResourceWeightModifierData>> BucketModifiers(
            List<ResourceWeightRecord> records,
            List<ResourceWeightModifierData> modifiers)
        {
            var dictionary = records.Distinct().ToDictionary(x => x, _ => new List<ResourceWeightModifierData>());

            foreach (var modifier in modifiers)
            {
                foreach (var record in records)
                {
                    switch (modifier.ModifierType)
                    {
                        case ResourceModifierType.All:
                            dictionary[record].Add(modifier);
                            break;
                        case ResourceModifierType.Name:
                            if (!modifier.ResourceItemData.Guid.Equals(record.Item.Guid)) break;
                            dictionary[record].Add(modifier);
                            break;
                        case ResourceModifierType.Type:
                            if (modifier.ResourceType != record.Item.ResourceType) break;
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