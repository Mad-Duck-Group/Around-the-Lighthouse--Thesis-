#nullable enable
using System.Collections.Generic;

namespace Madduck.Utils
{
    public interface IWeightRecord
    {
        public UFloat Weight { get; set; }
        public Percentage Probability { get; }
    }

    public interface IWeightRecord<out T> : IWeightRecord
    {
        public T Item { get; }
    }

    public interface IWeightTable<TRecord>
    {
        public List<TRecord> Records { get; }
    }
    
    public interface IWeightTableInstance<TRecord, TModData, TItem> where TRecord : IWeightRecord<TItem>
    {
        public Dictionary<string, IWeightFilter<TRecord>> PersistentFilters { get; }
        public Dictionary<ModifierId, List<TModData>> PersistentModifiers { get; }
        public TItem? GetRandomItem();
        public void GetRandomItems(TItem?[] array);
        public void GetRandomUniqueItems(TItem?[] array, bool fallback = false);
    }

    public interface IWeightFilter<T> where T : IWeightRecord
    {
        public List<T> Filter(List<T> record);
    }
}