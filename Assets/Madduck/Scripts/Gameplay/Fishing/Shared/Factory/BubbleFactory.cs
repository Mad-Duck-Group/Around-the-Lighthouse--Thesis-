using System;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Madduck.Fishing.Shared
{
    public interface IBubbleViewFactory : IGameObjectFactory<IBubbleView>
    {
        IBubbleView Prototype { get; }
    }
    [Serializable]
    public class BubbleFactory : IBubbleViewFactory
    {
        [Required, 
         OdinSerialize] private IBubbleView bubbleViewPrefab;
        public IBubbleView Current { get; private set; }
        public GameObject CurrentGameObject { get; private set; }
        public IBubbleView Prototype => bubbleViewPrefab;
        private GameObject _bubbleViewGameObject;
        public IBubbleView Create()
        {
            var view = bubbleViewPrefab.InstantiateAsInterface(new InstantiateParameters(){}, out _bubbleViewGameObject);
            Current = view;
            CurrentGameObject = _bubbleViewGameObject;
            return view;
        }
        
        public IBubbleView Create(out GameObject gameObject)
        {
            var view = Create();
            gameObject = _bubbleViewGameObject;
            CurrentGameObject = _bubbleViewGameObject;
            return view;
        }
    }
}