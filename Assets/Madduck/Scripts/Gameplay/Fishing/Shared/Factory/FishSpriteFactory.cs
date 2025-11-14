using System;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Madduck.Fishing.Shared
{
    public interface IFishSpriteFactory : IFactory<IFishSpriteView>
    {
        void DestroyFishSprite();
    }
    
    [Serializable]
    public class FishSpriteFactory : IFishSpriteFactory
    {
        [Required, AssetsOnly,
         OdinSerialize] private IFishSpriteView prefab;
        
        public IFishSpriteView Current { get; private set; }
        private GameObject _currentObject;
        public IFishSpriteView Create()
        {
            if (_currentObject) return Current;
            Current = prefab.InstantiateAsInterface(new InstantiateParameters
            {
            }, out _currentObject);
            return Current;
        }

        public void DestroyFishSprite()
        {
            if (!_currentObject) return;
            Object.Destroy(_currentObject);
            Current = null;
        }
    }
}