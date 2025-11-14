using System;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Room
{
    [Serializable]
    public class BaitButtonViewFactory : IFactory<BaitButtonView>
    {
        [Required, 
         SerializeField] private Transform baitButtonsParent;
        [Required, 
         SerializeField] private Canvas tooltipCanvas;
        [Required, 
         SerializeField] private BaitButtonView baitButtonViewPrefab;
        [Required, 
         SerializeField] private Transform tooltipParent;
        
        public BaitButtonView Current { get; private set; }
        public BaitButtonView Create()
        {
            Current = UnityEngine.Object.Instantiate(baitButtonViewPrefab, baitButtonsParent);
            Current.SetUp(tooltipCanvas, tooltipParent);
            return Current;
        }
    }
}
