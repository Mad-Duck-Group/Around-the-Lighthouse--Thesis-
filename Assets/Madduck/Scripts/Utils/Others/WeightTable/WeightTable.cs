#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;

namespace Madduck.Utils
{
    public interface IWeightRecord
    {
        object Item { get; }
        UFloat Weight { get; set; }
        Percentage Probability { get; }
    }

    public interface IWeightRecord<out T> : IWeightRecord
    {
        new T Item { get; }
        object IWeightRecord.Item => Item!;
    }

    public interface IWeightTable
    {
        List<IWeightRecord> Records { get; }
        IWeightTableInstance CreateInstance();
    }

    public interface IWeightTable<TRecord> : IWeightTable 
        where TRecord : IWeightRecord
    {
        new List<TRecord> Records { get; }
        List<IWeightRecord> IWeightTable.Records => Records.Cast<IWeightRecord>().ToList();
    }

    public interface IWeightTableInstance
    {
        List<IWeightRecord> ModifiedRecords { get; }
        void SetModifierSource(IModifierSource modifierSource);
        void SetKeys(string[] keys);
    }
    
    public interface IWeightTableInstance<TRecord, TModData, TItem> : IWeightTableInstance 
        where TRecord : IWeightRecord<TItem>
    {
        Dictionary<string, IWeightFilter<TRecord>> PersistentFilters { get; }
        Dictionary<ModifierId, List<TModData>> PersistentModifiers { get; }
        TItem? GetRandomItem();
        void GetRandomItems(TItem?[] array);
        void GetRandomUniqueItems(TItem?[] array, bool fallback = false);
    }

    public interface IWeightFilter<T> where T : IWeightRecord
    {
        List<T> Filter(List<T> record);
    }
}