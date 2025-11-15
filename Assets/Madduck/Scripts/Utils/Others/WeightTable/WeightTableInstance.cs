#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using R3;
using Sirenix.OdinInspector;
using VContainer;
using DisposableBag = R3.DisposableBag;

namespace Madduck.Utils
{
    public abstract class WeightTableInstance<TRecord, TModData, TItem> : 
        IWeightTableInstance<TRecord, TModData, TItem>, 
        IDisposable 
        where TRecord : class, IWeightRecord<TItem>, IStatModifiable<TRecord> 
        where TModData : BaseModifierData
    {
        #region Inspector

        [Title("Debug")] 
        protected List<TRecord> BaseRecords { get; set; }
        [Sirenix.OdinInspector.ReadOnly, 
         ShowInInspector] public Dictionary<string, IWeightFilter<TRecord>> PersistentFilters { get; private set; }
        [Sirenix.OdinInspector.ReadOnly, 
         ShowInInspector] public Dictionary<ModifierId, List<TModData>> PersistentModifiers { get; private set; }

        [Title("Debug")]
        [Sirenix.OdinInspector.ReadOnly, TableList,
         ShowInInspector] protected List<TRecord> modifiedRecords = new();
        
        public List<IWeightRecord> ModifiedRecords => modifiedRecords.Cast<IWeightRecord>().ToList();
        
        [Button("Refresh")]
        private void Refresh() => ApplyFiltersAndModifiers();

        #endregion

        #region Fields
        
        protected IModifierSource? modifierSource;
        protected IDisposable subscriptions = null!;
        protected DisposableBag modifierChangedSubscription;
        protected List<string> keys = new();

        #endregion

        #region Injection

        [Inject]
        protected WeightTableInstance(
            IWeightTable<TRecord> weightTable,
            [Key("ModifierContainer")] IModifierSource modifierSource)
        {
            BaseRecords = weightTable.Records.Select(x => x.Copy()).ToList();
            PersistentFilters = new Dictionary<string, IWeightFilter<TRecord>>();
            PersistentModifiers = new Dictionary<ModifierId, List<TModData>>();
            this.modifierSource = modifierSource;
            Subscribe();
        }

        #endregion

        #region Subscription

        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            SubscribeModifierSource();
            subscriptions = disposableBuilder.Build();
            SubscribeAdditional();
        }

        public void SetModifierSource(IModifierSource modifierSource)
        {
            this.modifierSource = modifierSource;
            SubscribeModifierSource();
        }

        protected virtual void SubscribeAdditional()
        {
            
        }
        
        public void SetKeys(params string[] keys)
        {
            this.keys = keys.ToList();
            if (modifierSource == null) return;
            modifierSource.Modifiers.OnModifierFirstSubscribe(PersistentModifiers, keys.ToArray());
            ApplyFiltersAndModifiers();
        }

        public virtual void Dispose()
        {
            subscriptions.Dispose();
            modifierChangedSubscription.Dispose();
            modifierChangedSubscription.Clear();
        }

        #endregion

        #region Events

        protected virtual void SubscribeModifierSource()
        {
            if (modifierSource == null) return;
            modifierSource.Modifiers.OnModifierFirstSubscribe(PersistentModifiers, keys.ToArray());
            ApplyFiltersAndModifiers();
            modifierSource.ModifiersView
                .ObserveChanged()
                .Subscribe(x =>
                {
                    x.OnModifierChanged(PersistentModifiers, keys.ToArray());
                    ApplyFiltersAndModifiers();
                })
                .AddTo(ref modifierChangedSubscription);
        }

        #endregion

        #region Utils

        protected abstract void ApplyFiltersAndModifiers();
        
        protected virtual TItem? GetRandomItemInternal(List<TRecord> records, out TRecord? resultRecord)
        {
            var totalWeight = records.Sum(record => record.Weight);
            var randomValue = UnityEngine.Random.Range(0f, totalWeight);
            var cumulativeWeight = 0f;
            foreach (var record in records)
            {
                cumulativeWeight += record.Weight;
                if (randomValue <= cumulativeWeight)
                {
                    resultRecord = record;
                    return record.Item;
                }
            }
            DebugUtils.LogWarning($"Cannot produce random item for {typeof(TItem)} of {typeof(TRecord)}");
            resultRecord = null;
            return default;
        }
        
        /// <summary>
        /// Get a random item from the weight table.
        /// </summary>
        /// <returns></returns>
        public virtual TItem? GetRandomItem()
        {
            ApplyFiltersAndModifiers();
            return GetRandomItemInternal(modifiedRecords, out _);
        }

        /// <summary>
        /// Get a list of items from the weight table. The results are not unique.
        /// </summary>
        /// <param name="array">The array to fill with random items.</param>
        /// <returns></returns>
        public virtual void GetRandomItems(TItem?[] array)
        {
            ApplyFiltersAndModifiers();
            var arrayCount = array.Length;
            for (int i = 0; i < arrayCount; i++)
            {
                var item = GetRandomItemInternal(modifiedRecords, out var resultRecord);
                if (resultRecord == null) break;
                array[i] = item;
            }
        }

        /// <summary>
        /// Get a list of unique items from the weight table.
        /// </summary>
        /// <param name="array">The array to fill with random unique items.</param>
        /// <param name="fallback">Fall back to not unique item?</param>
        /// <returns></returns>
        public virtual void GetRandomUniqueItems(TItem?[] array, bool fallback = false)
        {
            ApplyFiltersAndModifiers();
            var arrayCount = array.Length;
            var records = modifiedRecords.Select(x => x.Copy()).ToList();
            for (int i = 0; i < arrayCount; i++)
            {
                var item = GetRandomItemInternal(records, out var resultRecord);
                if (resultRecord == null)
                {
                    if (!fallback) break;
                    item = GetRandomItemInternal(modifiedRecords, out _);
                }
                else
                {
                    records.Remove(resultRecord);
                }
                array[i] = item;
            }
        }

        #endregion
    }
}