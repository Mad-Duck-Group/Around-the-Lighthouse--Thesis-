using System;
using System.Runtime.CompilerServices;
using HasanSadikin.Carousel;
using Madduck.Utils;
using UnityEngine;


    [System.Serializable]
    public class LocationData
    {
        public Sprite sprite;
    }
    public class LocationCarousel : CarouselController<LocationData>
    {
        public void OnEnable()
        {
            OnItemSelected.AddListener(LogItem);
            OnCurrentItemUpdated.AddListener(LogItem);
        }

        private void LogItem(LocationData data)
        {
            //DebugUtils.Log(data.sprite);
        }
    }
