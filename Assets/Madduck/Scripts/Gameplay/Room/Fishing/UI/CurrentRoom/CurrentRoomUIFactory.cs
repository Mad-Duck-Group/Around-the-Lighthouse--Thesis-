using Madduck.Shared;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.Room
{
    public class CurrentRoomUIFactory : IGenericFactory<CurrentRoomView>
    {
        [Required,
         SerializeField] private CurrentRoomView prefab;
        [Required,
         SerializeField] private Transform parent;


        public CurrentRoomView Current { get; private set; }
        public CurrentRoomView Create()
        {
            Current = Object.Instantiate(prefab, parent);
            Current.SetUp();
            return Current;
        }
    }
}
