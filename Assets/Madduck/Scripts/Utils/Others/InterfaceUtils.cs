using UnityEngine;

namespace Madduck.Scripts.Utils.Others
{
    public enum Sign
    {
        Negative = -1,
        Zero = 0,
        Positive = 1
    }
    
    public static class InterfaceUtils
    {
        /// <summary>
        /// Instantiate a prefab that implements an interface and return the instantiated object as that interface type.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parameters"></param>
        /// <param name="gameObject"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T InstantiateAsInterface<T>(this T prefab, InstantiateParameters parameters, out GameObject gameObject) where T : class
        {
            if (prefab is not MonoBehaviour monoBehaviour)
            {
                DebugUtils.LogError($"Prefab of type {typeof(T)} is not a MonoBehaviour. Cannot instantiate.");
                gameObject = null;
                return null;
            }
            var clone = Object.Instantiate(monoBehaviour, parameters);
            gameObject = clone.gameObject;
            return clone as T;
        }
    }
}