using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using ObservableCollections;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using DisposableBag = R3.DisposableBag;
using Random = UnityEngine.Random;

namespace Madduck.GameData
{
    #region Data Structure

    [Serializable]
    public record WeatherWeightRecord : IWeightRecord<WeatherType>, IStatModifiable<WeatherWeightRecord>
    {
        [field: UnflagEnum, 
                Required,
                SerializeField]
        public WeatherType Item { get; internal set; }

        [field: MinValue(0f),
                SerializeField]
        public UFloat Weight { get; set; } = 1f;

        [field: DisplayAsString(TextAlignment.Center),
                ShowInInspector]
        public Percentage Probability { get; internal set; }

        public WeatherWeightRecord Copy() => this with {};
    }

    public class WeatherWeightFilter : IWeightFilter<WeatherWeightRecord>
    {
        private readonly Func<WeatherWeightRecord, bool> _predicate;

        public WeatherWeightFilter(Func<WeatherWeightRecord, bool> predicate)
        {
            _predicate = predicate;
        }

        public List<WeatherWeightRecord> Filter(List<WeatherWeightRecord> records)
        {
            return records
                .Where(_predicate)
                .Select(x => x.Copy())
                .ToList();
        }
    }

        #endregion
}