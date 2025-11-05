using System;
using System.Runtime.CompilerServices;
using HasanSadikin.Carousel;
using Madduck.Utils;
using R3;
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
            OnItemSelected.Subscribe(LogItem).AddTo(this);
            OnCurrentItemUpdated.Subscribe(LogItem).AddTo(this);
        }

        private void LogItem(LocationData data)
        {
            //DebugUtils.Log(data.sprite);
        }
    }


