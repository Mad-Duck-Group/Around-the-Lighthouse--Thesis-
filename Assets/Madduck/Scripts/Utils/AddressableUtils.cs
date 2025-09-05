using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Madduck.Scripts.Utils
{
    public static class AddressableUtils
    {
        public static async UniTask<T> LoadAssetUniTask<T>(this AssetReference assetReference) where T : class
        {
            var handle = assetReference.LoadAssetAsync<T>();
            await handle.ToUniTask();
            var asset = handle.Result;
            if (asset == null)
            {
                Debug.LogError($"Failed to load asset: {assetReference.RuntimeKey}");
            }
            handle.Release();
            return asset;
        }
        
        public static async UniTask<T> LoadComponentInAssetUniTask<T>(this AssetReference assetReference) where T : Component
        {
            var handle = assetReference.LoadAssetAsync<GameObject>();
            await handle.ToUniTask();
            var asset = handle.Result;
            if (!asset)
            {
                Debug.LogError($"Failed to load asset: {assetReference.RuntimeKey}");
                handle.Release();
                return null;
            }
            if (!asset.TryGetComponent<T>(out var component))
            {
                Debug.LogError($"Failed to get component {typeof(T).Name} from asset: {assetReference.RuntimeKey}");
                handle.Release();
                return null;
            }
            handle.Release();
            return component;
        }
    }
}