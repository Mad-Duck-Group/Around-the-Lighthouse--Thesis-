using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Madduck.Utils;
using ObservableCollections;

namespace Madduck.GameData
{
    public static class ItemModifierUtils
    {
        /// <summary>
        /// Updates the modifiers dictionary based on the provided collection changed event.
        /// </summary>
        /// <typeparam name="TInstance">
        /// The type of the <see cref="ItemInstance{TData}"/>.
        /// </typeparam>
        /// <typeparam name="TData">
        /// The type of the <see cref="ItemData"/>.
        /// </typeparam>
        /// <param name="modifiers">
        /// The dictionary of modifiers to update.
        /// </param>
        /// <param name="collectionChangedEvent">
        /// The collection changed event containing the new and old items.
        /// </param>
        /// <param name="idDisplayNameGetter">
        /// The function to get the display name for the <see cref="ModifierId"/>.
        /// </param>
        public static void OnItemInstanceCollectionChanged<TInstance, TData>(
            this IDictionary<ModifierId, List<BaseModifierData>> modifiers, 
            CollectionChangedEvent<TInstance> collectionChangedEvent, 
            Func<TInstance, string> idDisplayNameGetter = null) 
            where TInstance : ItemInstance<TData> 
            where TData : ItemData, IHasModifier
        {
            var newItem = collectionChangedEvent.NewItem;
            var oldItem = collectionChangedEvent.OldItem;
            var newId = collectionChangedEvent.NewItem != null 
                ? new ModifierId(newItem.InstanceGuid, idDisplayNameGetter?.Invoke(newItem)) 
                : null;
            var oldId = collectionChangedEvent.OldItem != null 
                ? new ModifierId(oldItem.InstanceGuid, idDisplayNameGetter?.Invoke(oldItem)) 
                : null;
            switch (collectionChangedEvent.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (newId == null) return;
                    modifiers.TryAdd(newId, newItem.ItemData.Modifiers);
                    break;
                case NotifyCollectionChangedAction.Move:
                    //Ignore because the modifiers are flattened
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (oldId == null) return;
                    modifiers.Remove(oldId);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (oldId == null || newId == null) return;
                    modifiers.Remove(oldId);
                    modifiers.TryAdd(newId, newItem.ItemData.Modifiers);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    modifiers.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        
        /// <summary>
        /// Updates the modifiers dictionary based on the provided previous and current item instances.
        /// </summary>
        /// <typeparam name="TInstance">The type of the item instance.</typeparam>
        /// <typeparam name="TData">The type of the item data.</typeparam>
        /// <param name="modifiers">The modifiers.</param>
        /// <param name="previous">The previous item instance.</param>
        /// <param name="current">The current item instance.</param>
        /// <param name="idDisplayNameGetter">The function to get the display name for the <see cref="ModifierId"/>.</param>
        public static void OnItemInstanceChanged<TInstance, TData>(
            this IDictionary<ModifierId, List<BaseModifierData>> modifiers,
            TInstance previous, 
            TInstance current, 
            Func<TInstance, string> idDisplayNameGetter = null) 
            where TInstance : ItemInstance<TData> 
            where TData : ItemData, IHasModifier
        {
            if (previous != null)
            {
                var previousId = new ModifierId(previous.InstanceGuid, idDisplayNameGetter?.Invoke(previous));
                modifiers.Remove(previousId);
            }
            if (current != null)
            {
                var currentId = new ModifierId(current.InstanceGuid, idDisplayNameGetter?.Invoke(current));
                modifiers.TryAdd(currentId, current.ItemData.Modifiers);
            }
        }
    }
}