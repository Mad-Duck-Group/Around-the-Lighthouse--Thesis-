using System.Collections.Generic;

namespace Madduck.Scripts.Utils.Others
{
    public interface IWeightRecord
    {
        public float Weight { get; }
        public float Probability { get; }
    }

    public interface IWeightRecord<out T> : IWeightRecord
    {
        public T Item { get; }
    }
    
    public interface IWeightTable<TRecord, out TItem> where TRecord : IWeightRecord<TItem>
    {
        public Dictionary<string, IWeightFilter<TRecord>> PersistentFilters { get; }
        public Dictionary<string, IWeightModifier<TRecord>> PersistentModifiers { get; }
        public TItem GetRandomItem();
    }

    public interface IWeightFilter<T> where T : IWeightRecord
    {
        public List<T> Filter(List<T> record);
    }

    public interface IWeightModifier<T> where T : IWeightRecord
    {
        public List<T> Modify(List<T> record);
    }
}