using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using MessagePipe;
using ObservableCollections;
using R3;
using Sirenix.OdinInspector;
using VContainer;

namespace Madduck.GameData.Fisherman
{
    [Serializable]
    public class FishermanItemInstance : ItemInstance<FishermanItemData>, IModifierProvider, IDisposable
    {
        [Title("Fisherman Stats"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _fishermanStatsTitle;
        [field: ReadOnly,
                ShowInInspector] public FishermanStats CurrentStats { get; private set; }
        [field: ReadOnly, 
                ShowInInspector] public FishingRodItemInstance CurrentFishingRod { get; private set; }

        [field: ReadOnly,
                ShowInInspector] public ObservableList<CardItemInstance> CurrentCards { get; private set; } = new();

        public ISynchronizedView<CardItemInstance, CardItemInstance> CurrentCardsView { get; private set; }
        
        private readonly IPublisher<ModifierUpdatedEvent> _modifierUpdatedEventPublisher;
        private readonly ISubscriber<FishingRoomStartedEvent> _fishingRoomStartedEventSubscriber;
        private IDisposable _subscriptions;
        
        [Inject]
        public FishermanItemInstance(
            FishermanItemData itemData,
            IPublisher<ModifierUpdatedEvent> modifierUpdatedEventPublisher,
            ISubscriber<ModifierUpdatedEvent> modifierUpdatedEventSubscriber,
            ISubscriber<FishingRoomStartedEvent> fishingRoomStartedEventSubscriber)
            : base(itemData)
        {
            _modifierUpdatedEventPublisher = modifierUpdatedEventPublisher;
            _fishingRoomStartedEventSubscriber = fishingRoomStartedEventSubscriber;
            CurrentStats = new FishermanStats(itemData);
            CurrentCardsView = CurrentCards.CreateView(x => x);
            CurrentFishingRod = new FishingRodItemInstance(ItemData.FishingRod, modifierUpdatedEventSubscriber);
            Subscribe();
        }

        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _fishingRoomStartedEventSubscriber.Subscribe(_ => OnFishingRoomStarted())
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
            CurrentCardsView.Dispose();
            CurrentFishingRod.Dispose();
        }

        private void OnFishingRoomStarted()
        {
            CurrentCards.AddRange(ItemData.StartingCards.Select(x => new CardItemInstance(x)));
            NotifyUpdate();
        }

        private void NotifyUpdate()
        {
            _modifierUpdatedEventPublisher.Publish(new ModifierUpdatedEvent(this));
        }

        public Dictionary<ModifierId, List<T>> GetModifiers<T>() where T : BaseModifierData
        {
            var dictionary = new Dictionary<ModifierId, List<T>>();
            foreach (var card in CurrentCards)
            {
                var list = new List<T>();
                foreach (var modifier in card.ItemData.Modifiers)
                {
                    if (modifier is T mod)
                    {
                        list.Add(mod);
                    }
                }
                if (list.Count > 0)
                    dictionary.Add(new ModifierId(card.InstanceGuid, card.ItemData.CardName), list);
            }
            return dictionary;
        }
    }

    [Serializable]
    public record FishermanStats : IStatModifiable<FishermanStats>
    {
        [field: DisplayAsString, InlineProperty,
                ShowInInspector] public UFloat CurrentStamina { get; set; }
        
        public FishermanStats(FishermanItemData itemData)
        {
            CurrentStamina = itemData.MaxStamina;
        }
        
        public FishermanStats Copy() => this with { };
    }
}