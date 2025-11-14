using System;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Madduck.Fishing.Shared
{
    public interface IBubbleViewFactory : IFactory<IBubbleView>
    {
        IBubbleView Prototype { get; }
    }
    [Serializable]
    public class BubbleFactory : IBubbleViewFactory
    {
        [Required, 
         OdinSerialize] private IBubbleView bubbleViewPrefab;
        public IBubbleView Current { get; private set; }
        public IBubbleView Prototype => bubbleViewPrefab;
        private GameObject _bubbleViewGameObject;
        public IBubbleView Create()
        {
            var view = bubbleViewPrefab.InstantiateAsInterface(new InstantiateParameters(){}, out _bubbleViewGameObject);
            Current = view;
            return view;
        }
    }
}