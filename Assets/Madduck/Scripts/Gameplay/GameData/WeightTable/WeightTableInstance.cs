using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using MessagePipe;
using ObservableCollections;
using R3;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using VContainer;
using DisposableBag = R3.DisposableBag;

namespace Madduck.GameData
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
         ShowInInspector] protected List<TRecord> modifiedRecords;
        [Button("Refresh")]
        private void Refresh() => ApplyFiltersAndModifiers();

        #endregion

        #region Fields

        protected readonly ISubscriber<ModifierSourceEvent> modifierPublisherEventSubscriber;
        protected IDisposable subscriptions;
        protected DisposableBag modifierChangedSubscription;

        #endregion

        #region Injection

        [Inject]
        protected WeightTableInstance(
            IWeightTable<TRecord> weightTable,
            ISubscriber<ModifierSourceEvent> modifierPublisherEventSubscriber)
        {
            BaseRecords = weightTable.Records.Select(x => x.Copy()).ToList();
            PersistentFilters = new Dictionary<string, IWeightFilter<TRecord>>();
            PersistentModifiers = new Dictionary<ModifierId, List<TModData>>();
            this.modifierPublisherEventSubscriber = modifierPublisherEventSubscriber;
            Subscribe();
        }

        #endregion

        #region Subscription

        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            modifierPublisherEventSubscriber.Subscribe(OnModifierPublished)
                .AddTo(ref disposableBuilder);
            subscriptions = disposableBuilder.Build();
            SubscribeAdditional();
        }

        protected virtual void SubscribeAdditional()
        {
            
        }

        public virtual void Dispose()
        {
            subscriptions?.Dispose();
            modifierChangedSubscription.Dispose();
            modifierChangedSubscription.Clear();
        }

        #endregion

        #region Events

        protected virtual void OnModifierPublished(ModifierSourceEvent eventData)
        {
            eventData.ModiferSource.ModifiersView.ObserveChanged()
                .Subscribe(x =>
                {
                    PersistentModifiers.OnModifierChanged(x);
                    ApplyFiltersAndModifiers();
                })
                .AddTo(ref modifierChangedSubscription);
        }

        #endregion

        #region Utils

        protected abstract void ApplyFiltersAndModifiers();

        public virtual TItem GetRandomItem()
        {
            ApplyFiltersAndModifiers();
            var totalWeight = modifiedRecords.Sum(record => record.Weight);
            var randomValue = UnityEngine.Random.Range(0f, totalWeight);
            var cumulativeWeight = 0f;
            foreach (var record in modifiedRecords)
            {
                cumulativeWeight += record.Weight;
                if (randomValue <= cumulativeWeight)
                {
                    return record.Item;
                }
            }
            return default;
        }

        #endregion
    }
}