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
    
    public interface IWeightTable<TRecord, TModData, out TItem> where TRecord : IWeightRecord<TItem>
    {
        public Dictionary<string, IWeightFilter<TRecord>> PersistentFilters { get; }
        public Dictionary<ModifierId, List<TModData>> PersistentModifiers { get; }
        public TItem GetRandomItem();
    }

    public interface IWeightFilter<T> where T : IWeightRecord
    {
        public List<T> Filter(List<T> record);
    }
}