#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace Madduck.Utils
{
    public static class WeightTableUtils
    {
        /// <summary>
        /// Modifies the weights of the given records by applying the given modifiers with matching keys.
        /// </summary>
        /// <typeparam name="TMod">The type of modifier, which must be a subclass of <see cref="BaseModifierData"/>.</typeparam>
        /// <typeparam name="TRecord">The type of record, which must implement <see cref="IWeightRecord"/> and <see cref="IStatModifiable{TRecord}"/>.</typeparam>
        /// <typeparam name="TKey">The type of key used to group records and modifiers.</typeparam>
        /// <param name="modifiers">The modifiers to apply to the records.</param>
        /// <param name="records">The records to modify.</param>
        /// <param name="modifierKeySelector">A function to select the key from a modifier.</param>
        /// <param name="recordKeySelector">A function to select the key from a record.</param>
        /// <returns>A list of modified records.</returns>
        public static List<TRecord> ModifyBy<TMod, TRecord, TKey>(this List<TMod> modifiers, List<TRecord> records,
            Func<TMod, TKey> modifierKeySelector, Func<TRecord, TKey> recordKeySelector)
            where TMod : BaseModifierData
            where TRecord : class, IWeightRecord, IStatModifiable<TRecord>
        {
            var recordGroup = records
                .GroupBy(recordKeySelector)
                .ToDictionary(x => x.Key, 
                    x => x.Select(r => r.Copy()).ToList());
            var modifierGroup = modifiers
                .GroupBy(modifierKeySelector)
                .ToDictionary(x => x.Key, x => x.ToList());
            foreach (var modifier in modifierGroup)
            {
                if (!recordGroup.TryGetValue(modifier.Key, out var value)) continue;
                foreach (var record in value)
                {
                    record.Weight = modifier.Value.Calculate(record.Weight);
                }
            }
            return recordGroup.SelectMany(x => x.Value).ToList();
        }
    }
}