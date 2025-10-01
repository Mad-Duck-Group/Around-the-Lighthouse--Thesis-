using System;
using Madduck.Shared;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Madduck.Room
{
    [Serializable]
    public class CardViewFactory : IGenericFactory<CardView>
    {
        [Required, 
         SerializeField] private CardView cardViewPrefab;
        [Required,
         SerializeField] private Transform parent;
        [Required,
         SerializeField] private Canvas tooltipCanvas;
        [Required,
         SerializeField] private Transform tooltipParent;
        
        public CardView Current { get; private set; }
        public CardView Create()
        {
            Current = Object.Instantiate(cardViewPrefab, parent);
            Current.SetUp(tooltipCanvas, tooltipParent);
            return Current;
        }
    }
}