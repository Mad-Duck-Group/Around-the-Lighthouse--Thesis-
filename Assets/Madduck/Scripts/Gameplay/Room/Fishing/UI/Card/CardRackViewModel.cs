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
	    #region Fields

	    private readonly PlayerInventory _inventory;
	    private readonly IGenericFactory<CardView> _cardViewFactory;
	    private readonly Dictionary<Guid, CardView> _cardViewDictionary = new();
	    private IDisposable _bindings;

	    #endregion

	    #region Injection

	    [Inject]
	    public CardRackViewModel(
		    PlayerInventory inventory,
		    IGenericFactory<CardView> cardViewFactory)
	    {
		    _inventory = inventory;
		    _cardViewFactory = cardViewFactory;
		    Bind();
	    }

	    #endregion

	    #region Binding

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

	    #endregion

	    #region Events

	    private void OnCardChanged(ViewChangedEvent<CardItemInstance, CardItemInstance> eventData)
	    {
		    switch (eventData.Action)
		    {
			    case NotifyCollectionChangedAction.Add:
				    var cardView = _cardViewFactory.Create();
				    var addedInstance = eventData.NewItem.Value;
				    cardView.SetCard(addedInstance);
				    _cardViewDictionary.Add(addedInstance.InstanceGuid, cardView);
				    break;
			    case NotifyCollectionChangedAction.Move:
				    break;
			    case NotifyCollectionChangedAction.Remove:
				    var removedInstance = eventData.OldItem.Value;
				    if (!_cardViewDictionary.Remove(removedInstance.InstanceGuid, out var cardToRemove)) break;
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

	    #endregion
    }
}