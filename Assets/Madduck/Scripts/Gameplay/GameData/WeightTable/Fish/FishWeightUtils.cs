using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using ObservableCollections;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using DisposableBag = R3.DisposableBag;
using Random = UnityEngine.Random;

namespace Madduck.GameData
{
    #region Data Structure

    [Serializable]
    public record FishWeightRecord : IWeightRecord<FishItemData>, IStatModifiable<FishWeightRecord>
    {
        [field: Required, 
                SerializeField] public FishItemData Item { get; internal set; }
        [field: MinValue(0f), 
                SerializeField] public UFloat Weight { get; set; } = 1f;
        [field: DisplayAsString(TextAlignment.Center), 
                ShowInInspector] public Percentage Probability { get; internal set; }

        public FishWeightRecord Copy() => this with {};
    }
    
    public class FishWeightFilter : IWeightFilter<FishWeightRecord>
    {
        private readonly Func<FishWeightRecord, bool> _predicate;

        public FishWeightFilter(Func<FishWeightRecord, bool> predicate)
        {
            _predicate = predicate;
        }

        public List<FishWeightRecord> Filter(List<FishWeightRecord> records)
        {
            return records
                .Where(_predicate)
                .Select(x => x.Copy())
                .ToList();
        }
    }

    #endregion
}