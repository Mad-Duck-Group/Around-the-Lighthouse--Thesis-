using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.GameData.Bait;
using Madduck.Utils;
using MessagePipe;
using ObservableCollections;
using R3;
using Sirenix.OdinInspector;
using VContainer;

namespace Madduck.GameData
{
    [Serializable]
    public class PlayerInventory : IModifierSource, IDisposable
    {
        #region Inspector
        [Title("Debug"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _debugTitle;
        [field: ReadOnly, 
                ShowInInspector] public FishingRodItemInstance CurrentFishingRod { get; }

        [field: ReadOnly,
                ShowInInspector] private ObservableList<CardItemInstance> CurrentCards { get; } = new();

        [field: ReadOnly,
                ShowInInspector] private ObservableDictionary<BaitType, BaitItemInstance> CurrentBaits { get; } = new();

        [field: ReadOnly,
                ShowInInspector] private SerializableReactiveProperty<BaitItemInstance> CurrentBait { get; } = new();
        #endregion
        
        #region Properties
        public ISynchronizedView<CardItemInstance, CardItemInstance> CurrentCardsView { get; }
        public ISynchronizedView<KeyValuePair<BaitType, BaitItemInstance>, 
            KeyValuePair<BaitType, BaitItemInstance>> CurrentBaitsView { get; }
        public ISynchronizedView<KeyValuePair<ModifierId, List<BaseModifierData>>, 
            KeyValuePair<ModifierId, List<BaseModifierData>>> ModifiersView { get; }
        public ReadOnlyReactiveProperty<BaitItemInstance> CurrentBaitView { get; }
        #endregion

        #region Fields
        private readonly ObservableDictionary<ModifierId, List<BaseModifierData>> _currentModifiers = new();
        private readonly PlayerInventoryConfig _config;
        private readonly IPublisher<ModifierSourceEvent> _modifierSourceEventPublisher;
        private readonly ISubscriber<FishingRoomStartedEvent> _fishingRoomStartedEventSubscriber;
        private IDisposable _subscriptions;
        #endregion
        
        #region Injection
        [Inject]
        public PlayerInventory(
            PlayerInventoryConfig config,
            IPublisher<ModifierSourceEvent> modifierSourceEventPublisher,
            ISubscriber<ModifierSourceEvent> modifierSourceEventSubscriber,
            ISubscriber<FishingRoomStartedEvent> fishingRoomStartedEventSubscriber)
        {
            _config = config;
            _modifierSourceEventPublisher = modifierSourceEventPublisher;
            _fishingRoomStartedEventSubscriber = fishingRoomStartedEventSubscriber;
            CurrentCardsView = CurrentCards.CreateView(x => x);
            ModifiersView = _currentModifiers.CreateView(x => x);
            CurrentBaitsView = CurrentBaits.CreateView(x => x);
            CurrentBaitView = CurrentBait.ToReadOnlyReactiveProperty();
            CurrentFishingRod = new FishingRodItemInstance(_config.FishingRod, modifierSourceEventSubscriber);
            Subscribe();
        }
        #endregion
        
        #region Subscriptions
        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _fishingRoomStartedEventSubscriber
                .Subscribe(_ => OnFishingRoomStarted())
                .AddTo(ref disposableBuilder);
            CurrentCards
                .ObserveChanged()
                .Subscribe(x =>
                {
                    _currentModifiers.OnItemInstanceCollectionChanged<CardItemInstance, CardItemData>(x,
                        i => i.GetRarityData(),
                        i => i.GetRarityData().CardName);
                })
                .AddTo(ref disposableBuilder);
            CurrentBait
                .Pairwise()
                .Subscribe(x =>
                {
                    _currentModifiers.OnItemInstanceChanged<BaitItemInstance, BaitItemData>(
                        x.Previous, 
                        x.Current,
                        i => i.ItemData,
                        i => i.ItemData.BaitName);
                })
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            CurrentCardsView.Dispose();
            CurrentFishingRod.Dispose();
        }
        #endregion
        
        #region Events
        private void OnFishingRoomStarted()
        {
            CurrentBaits.Clear();
            _modifierSourceEventPublisher?.Publish(new ModifierSourceEvent(this));
            CurrentCards.AddRange(_config.StartingCards.Select(x => new CardItemInstance(x)));
            foreach (var bait in _config.StartingBaits)
            {
                CurrentBaits.Add(bait.Key, new BaitItemInstance(bait.Value.ItemData, bait.Value.Count));
            }
            SetCurrentBait(BaitType.None);
        }
        #endregion

        #region Bait
        public void SetCurrentBait(BaitType baitType)
        {
            if (baitType is BaitType.None)
            {
                CurrentBait.Value = null;
                return;
            }
            if (!CurrentBaits.TryGetValue(baitType, out var bait))
            {
                DebugUtils.LogError($"Bait type {baitType} not found");
                return;
            }
            CurrentBait.Value = bait;
        }

        public void ChangeCurrentBaitAmount(int change)
        {
            if (CurrentBait.Value is null) return;
            CurrentBait.Value.ChangeCurrentCount(change);
            if (CurrentBait.Value.CurrentCount == 0) 
                SetCurrentBait(BaitType.None);
        }
        #endregion

        #region Card

        public void AddCard(CardItemInstance cardItemInstance)
        {
            CurrentCards.Add(cardItemInstance);
        }

        public void RemoveCard(Guid cardInstanceGuid)
        {
            var cardToRemove = CurrentCards.FirstOrDefault(x => x.InstanceGuid == cardInstanceGuid);
            if (cardToRemove is null)
            {
                DebugUtils.LogWarning($"Card with instance guid {cardInstanceGuid} not found");
                return;
            }
            CurrentCards.Remove(cardToRemove);
        }

        #endregion
    }
}