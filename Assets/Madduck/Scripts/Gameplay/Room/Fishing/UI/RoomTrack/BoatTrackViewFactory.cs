using System;
using Madduck.Shared;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Madduck.Room
{
    [Serializable]
    public class BoatTrackViewFactory : IGenericFactory<BoatTrackView>
    {
        [Required,
         SerializeField] private BoatTrackView _prefab;
        [Required,
         SerializeField] private Transform _parent;
        [Required,
        SerializeField] private Sprite _defaultBoatSprite;
        


        public BoatTrackView Current { get; private set; }
        public BoatTrackView Create()
        {
            Current = Object.Instantiate(_prefab, _parent);
            Current.SetUp(_defaultBoatSprite);
            return Current;
        }
       
    }
}

