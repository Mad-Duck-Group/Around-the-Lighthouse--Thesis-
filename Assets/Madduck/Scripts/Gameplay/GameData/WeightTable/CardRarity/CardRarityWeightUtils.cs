using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [Serializable]
    public record CardRarityWeightRecord : IWeightRecord<CardRarity>, IStatModifiable<CardRarityWeightRecord>
    {
        [field: Required, 
                SerializeField] public CardRarity Item { get; internal set; }
        [field: MinValue(0f), 
                SerializeField] public UFloat Weight { get; set; } = 1f;
        [field: DisplayAsString(TextAlignment.Center), 
                ShowInInspector] public Percentage Probability { get; internal set; }

        public CardRarityWeightRecord Copy() => this with {};
    }
    
    public class CardRarityWeightFilter : IWeightFilter<CardRarityWeightRecord>
    {
        private readonly Func<CardRarityWeightRecord, bool> _predicate;

        public CardRarityWeightFilter(Func<CardRarityWeightRecord, bool> predicate)
        {
            _predicate = predicate;
        }

        public List<CardRarityWeightRecord> Filter(List<CardRarityWeightRecord> records)
        {
            return records
                .Where(_predicate)
                .Select(x => x.Copy())
                .ToList();
        }
    }
}