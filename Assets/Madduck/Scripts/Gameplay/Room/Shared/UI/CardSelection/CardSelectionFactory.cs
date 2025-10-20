using System;
using Madduck.Shared;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Madduck.Room
{
    [Serializable]
    public class CardSelectionFactory : IGenericFactory<CardSelectionView>
    {
        [SerializeField] private CardSelectionView prefab;
        [SerializeField] private Transform parent;
        
        public CardSelectionView Current { get; private set; }
        public CardSelectionView Create()
        {
            var view = Object.Instantiate(prefab, parent);
            Current = view;
            return view;
        }
    }
}