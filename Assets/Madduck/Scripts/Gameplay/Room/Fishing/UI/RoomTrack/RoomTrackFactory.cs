using System;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Madduck.Room
{
    [Serializable]
    public class RoomTrackFactory : IGenericFactory<RoomTrackView>
    {
        [FormerlySerializedAs("_prefab")]
        [Required,
         SerializeField] private RoomTrackView prefab;
        [FormerlySerializedAs("_parent")]
        [Required,
         SerializeField] private Transform parent;
        public RoomTrackView Current { get; private set; }
        public RoomTrackView Create()
        {
            Current = Object.Instantiate(prefab, parent);
            return Current;
        }
       
    }
}
