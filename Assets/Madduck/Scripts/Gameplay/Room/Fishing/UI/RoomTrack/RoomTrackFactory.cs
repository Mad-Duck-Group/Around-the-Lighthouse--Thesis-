using System;
using Madduck.Shared;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Madduck.Room
{
    [Serializable]
    public class RoomTrackFactory : IGenericFactory<RoomTrackView>
    {
        [Required,
         SerializeField] private RoomTrackView _prefab;
        [Required,
         SerializeField] private Transform _parent;


        public RoomTrackView Current { get; private set; }
        public RoomTrackView Create()
        {
            Current = Object.Instantiate(_prefab, _parent);
            return Current;
        }

        public RoomTrackView SetUpUI(Sprite sprite)
        {
            Current.SetUp(sprite);
            return Current;
        }
    }
}
