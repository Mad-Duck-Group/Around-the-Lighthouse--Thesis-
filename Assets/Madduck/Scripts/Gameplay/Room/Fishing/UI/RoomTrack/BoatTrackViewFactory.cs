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
    public class BoatTrackViewFactory : IGenericFactory<BoatTrackView>
    {
        [FormerlySerializedAs("_prefab")]
        [Required,
         SerializeField] private BoatTrackView prefab;
        [FormerlySerializedAs("_parent")]
        [Required,
         SerializeField] private Transform parent;
        [FormerlySerializedAs("_defaultBoatSprite")]
        [Required,
         SerializeField] private Sprite defaultBoatSprite;
        public BoatTrackView Current { get; private set; }
        public BoatTrackView Create()
        {
            Current = Object.Instantiate(prefab, parent);
            Current.SetUp(defaultBoatSprite);
            return Current;
        }
       
    }
}

