using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Madduck.GameData;
using Madduck.GameData.Fisherman;
using Madduck.Shared;
using Madduck.Utils;
using ObservableCollections;
using R3;
using VContainer;
using Object = UnityEngine.Object;

namespace Madduck.Room
{
    public class CardRackViewModel : IDisposable
    {
	    private readonly PlayerInventory _inventory;
	    private readonly IGenericFactory<CardView> _cardViewFactory;
		private IDisposable _bindings;
		private readonly Dictionary<Guid, CardView> _cardViewDictionary = new();

		[Inject]
		public CardRackViewModel(
			IGenericFactory<CardView> cardViewFactory,
			PlayerInventory inventory)
		{
			_inventory = inventory;
			_cardViewFactory = cardViewFactory;
			Bind();
		}

		private void Bind()
		{
			var disposableBuilder = Disposable.CreateBuilder();
			_inventory.CurrentCardsView
				.ObserveChanged()
				.Subscribe(OnCardChanged)
				.AddTo(ref disposableBuilder);
			_bindings = disposableBuilder.Build();
		}

		public void Dispose()
		{
			_bindings.Dispose();
		}
		
		private void OnCardChanged(ViewChangedEvent<CardItemInstance, CardItemInstance> eventData)
		{
			switch (eventData.Action)
			{
				case NotifyCollectionChangedAction.Add:
					var cardView = _cardViewFactory.Create();
					var addedInstance = eventData.NewItem.Value;
					cardView.SetCard(addedInstance);
					_cardViewDictionary.Add(addedInstance.ItemData.Guid, cardView);
					break;
				case NotifyCollectionChangedAction.Move:
					break;
				case NotifyCollectionChangedAction.Remove:
					var removedInstance = eventData.OldItem.Value;
					if (!_cardViewDictionary.Remove(removedInstance.ItemData.Guid, out var cardToRemove)) break;
					Object.Destroy(cardToRemove);
					break;
				case NotifyCollectionChangedAction.Replace:
					break;
				case NotifyCollectionChangedAction.Reset:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
    }
}