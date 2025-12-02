using System;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Madduck.Fishing.Shared
{
    public interface IFishEyesFactory : IGameObjectFactory<IFishEyesView>
    {
        void DestroyFishEyes();
    }
    
    [Serializable]
    public class FishEyesFactory : IFishEyesFactory
    {
        [Required, 
         OdinSerialize] private IFishEyesView prefab;
        
        public IFishEyesView Current { get; private set; }
        public GameObject CurrentGameObject { get; private set; }
        private GameObject _currentObject;
        public IFishEyesView Create()
        {
            Current = prefab.InstantiateAsInterface(new InstantiateParameters
            {
            }, out _currentObject);
            CurrentGameObject = _currentObject;
            return Current;
        }
        
        public IFishEyesView Create(out GameObject gameObject)
        {
            var fishEyes = Create();
            gameObject = _currentObject;
            CurrentGameObject = _currentObject;
            return fishEyes;
        }

        public void DestroyFishEyes()
        {
            if (_currentObject) Object.Destroy(_currentObject);
            Current = null;
        }
    }
}