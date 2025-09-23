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
    public class CardRackView : IDisposable
    {
	    private readonly FishermanItemInstance _fisherman;
	    private readonly IGenericFactory<CardView> _cardViewFactory;
		private IDisposable _bindings;
		private readonly List<CardView> _cardViews = new();

		[Inject]
		public CardRackView(
			IGenericFactory<CardView> cardViewFactory,
			FishermanItemInstance fisherman)
		{
			_fisherman = fisherman;
			_cardViewFactory = cardViewFactory;
			Bind();
		}

		private void Bind()
		{
			var disposableBuilder = Disposable.CreateBuilder();
			_fisherman.CurrentCardsView
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
					cardView.SetCard(eventData.NewItem.Value);
					_cardViews.Add(cardView);
					break;
				case NotifyCollectionChangedAction.Move:
					break;
				case NotifyCollectionChangedAction.Remove:
					var cardToRemove = _cardViews.FirstOrDefault(x => x.Card == eventData.OldItem.Value);
					_cardViews.Remove(cardToRemove);
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