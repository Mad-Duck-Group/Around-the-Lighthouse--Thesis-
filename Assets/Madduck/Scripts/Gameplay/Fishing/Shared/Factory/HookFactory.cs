using System;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Madduck.Fishing.Shared
{
    public interface IHookFactory : IGenericFactory<IHookProjectile>
    {
        GameObject CurrentGameObject { get; }
        void DestroyHook();
    }

    [Serializable]
    public class HookProjectileFactory : IHookFactory
    {
        [Required, AssetsOnly,
         OdinSerialize] private IHookProjectile prefab;
        [Required, 
         SerializeField] private Transform parent;
        public IHookProjectile Current { get; private set; }
        
        private GameObject _currentObject;
        public GameObject CurrentGameObject => _currentObject;

        public IHookProjectile Create()
        {
            if (_currentObject) return Current;
            Current = prefab.InstantiateAsInterface(new InstantiateParameters
            {
            }, out _currentObject);
            _currentObject.transform.position = parent.transform.position;
            if (Current is HookProjectile hook)
            {
                hook.SetUp(parent);
            }
            return Current;
        }
        
        public void DestroyHook()
        {
            if (!_currentObject) return;
            Object.Destroy(_currentObject);
            Current = null;
        }
    }

    public class HookProjectileFactoryMock : IHookFactory
    {
        public IHookProjectile Current { get; private set; }
        public GameObject CurrentGameObject { get; private set; }
        public IHookProjectile Create()
        {
            Current = new HookProjectileMock();
            CurrentGameObject = null;
            return Current;
        }

        public void DestroyHook()
        {
            Current = null;
        }
    }
}